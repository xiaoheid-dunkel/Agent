using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace xiaohei.Scripts
{
    /// <summary>
    /// 暴露给AI的代码管理方法
    /// </summary>
    public class CodeMethods
    {
        private readonly CodeDictionary _codeDictionary;

        public CodeMethods(CodeDictionary codeDictionary)
        {
            _codeDictionary = codeDictionary;
        }

        private void LogAction(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n[System] {message}");
            Console.ResetColor();
        }

        // ============================================================================
        // 代码管理函数 - 保存、编译和运行C#代码
        // ============================================================================

        [KernelFunction("save_code")]
        [Description("保存C#代码并使用唯一的名称以供将来重用。代码将被编译和验证。返回成功消息或详细的编译错误。")]
        public string SaveCode(
            [Description("代码片段的唯一名称")] string name,
            [Description("要保存的C#代码（仅使用顶级语句，不能有class Program或static Main）")] string code)
        {
            LogAction($"Saving code snippet: '{name}' ...");
            try
            {
                var validationResult = _codeDictionary.ValidateCode(code);
                _codeDictionary.Save(name, code);
                return $"成功：代码'{name}'已保存并编译成功\n{validationResult}";
            }
            catch (Exception ex)
            {
                return $"错误：无法保存代码'{name}':\n{ex.Message}";
            }
        }

        [KernelFunction("run_code")]
        [Description("执行C#代码。可以通过名称运行之前保存的代码，或直接执行新代码。返回执行结果或错误消息。")]
        public string RunCode(
            [Description("之前保存的代码片段的名称，或要直接执行的C#代码（仅顶级语句）")] string codeOrName)
        {
            // 1. 获取完整的代码内容（如果是名称则从字典获取，否则直接就是代码）
            string codeToRun = _codeDictionary.Get(codeOrName) ?? codeOrName;

            // 2. 风险检测与审批
            if (RequiresApproval(codeToRun))
            {
                // 播放提示音（可选）
                Console.Beep();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n==================================================");
                Console.WriteLine(" [?? 敏感操作审批] AI 请求执行具有潜在风险的代码");
                Console.WriteLine("==================================================");

                // 显示代码内容（限制长度以免刷屏）
                Console.ForegroundColor = ConsoleColor.Cyan;
                string preview = codeToRun.Length > 1000 ? codeToRun.Substring(0, 1000) + "...(剩余部分省略)" : codeToRun;
                Console.WriteLine(preview);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n此代码包含文件系统或进程操作（如 Git/部署）。");
                Console.Write("是否允许执行？(输入 'y' 允许，其他键拒绝): ");

                // 等待用户输入
                var input = Console.ReadLine();
                Console.ResetColor();

                if (input?.Trim().ToLower() != "y")
                {
                    Console.WriteLine("[系统] 用户拒绝了执行请求。");
                    return "错误：用户拒绝了该代码的执行请求。请修改代码或放弃任务。";
                }
            }

            // 3. 用户批准或无风险，继续执行
            LogAction($"Executing code: '{codeOrName}' (please wait)...");
            try
            {
                // 如果是已保存的名称，用 Execute 增加计数；否则直接运行
                var storedCode = _codeDictionary.Get(codeOrName);
                if (storedCode != null)
                {
                    return _codeDictionary.Execute(codeOrName);
                }

                return _codeDictionary.ExecuteDirectly(codeOrName);
            }
            catch (Exception ex)
            {
                return $"错误：代码执行失败:\n{ex.Message}";
            }
        }

        // 辅助方法：检测敏感关键字
        private bool RequiresApproval(string code)
        {
            var sensitiveKeywords = new[] {
                "System.Diagnostics.Process", "Process.Start",  // 运行程序/CMD/Git
                "System.IO", "File.", "Directory.",             // 文件读写
                "WebClient", "HttpClient", "WebRequest",        // 网络请求
                "registry", "Registry"                          // 注册表操作
            };

            // 只要代码中包含上述任意关键字，就触发审批
            return sensitiveKeywords.Any(k => code.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        [KernelFunction("test_code")]
        [Description("测试C#代码是否能编译而不执行它。用于在运行前检查语法和修复编译错误。如果代码无法编译，返回详细的错误消息。")]
        public string TestCode([Description("要测试编译的C#代码")] string code)
        {
            LogAction("Testing code compilation...");
            try
            {
                return _codeDictionary.ValidateCode(code);
            }
            catch (Exception ex)
            {
                return $"错误：编译测试失败:\n{ex.Message}";
            }
        }

        [KernelFunction("list_codes")]
        [Description("显示所有之前保存的代码片段及其名称。用于查看可用的代码。")]
        public string ListCodes()
        {
            LogAction("Listing saved codes...");
            var codes = _codeDictionary.GetAll();
            if (codes.Count == 0)
                return "还没有保存的代码片段";

            return "已保存的代码片段:\n" + string.Join("\n", codes.Keys.Select(k => $"  - {k}"));
        }

        [KernelFunction("get_code")]
        [Description("通过名称检索和显示之前保存的代码片段。用于查看或理解现有代码。")]
        public string GetCode([Description("要检索的代码片段的名称")] string name)
        {
            LogAction($"Retrieving code content: '{name}'...");
            var code = _codeDictionary.Get(name);
            if (code == null)
                return $"错误：找不到代码'{name}'";

            return code;
        }

        [KernelFunction("update_code")]
        [Description("更新并重新编译现有保存的代码片段。新代码将在保存前被验证。")]
        public string UpdateCode(
            [Description("要更新的现有代码片段的名称")] string name,
            [Description("用于替换旧代码的新C#代码（仅顶级语句）")] string newCode)
        {
            LogAction($"Updating code: '{name}'...");
            try
            {
                _codeDictionary.Update(name, newCode);
                return $"成功：代码'{name}'已更新并重新编译";
            }
            catch (Exception ex)
            {
                return $"错误：无法更新代码:\n{ex.Message}";
            }
        }

        [KernelFunction("delete_code")]
        [Description("删除之前保存的代码片段。此操作无法撤销。")]
        public string DeleteCode([Description("要删除的代码片段的名称")] string name)
        {
            LogAction($"Deleting code: '{name}'...");
            try
            {
                _codeDictionary.Delete(name);
                return $"成功：代码'{name}'已删除";
            }
            catch (Exception ex)
            {
                return $"错误：无法删除代码:\n{ex.Message}";
            }
        }
    }
}
