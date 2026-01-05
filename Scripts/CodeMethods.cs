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

        // ============================================================================
        // 代码管理函数 - 保存、编译和运行C#代码
        // ============================================================================

        [KernelFunction("save_code")]
        [Description("保存C#代码并使用唯一的名称以供将来重用。代码将被编译和验证。返回成功消息或详细的编译错误。")]
        public string SaveCode(
            [Description("代码片段的唯一名称")] string name,
            [Description("要保存的C#代码（仅使用顶级语句，不能有class Program或static Main）")] string code)
        {
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
            try
            {
                var code = _codeDictionary.Get(codeOrName);
                if (code != null)
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

        [KernelFunction("test_code")]
        [Description("测试C#代码是否能编译而不执行它。用于在运行前检查语法和修复编译错误。如果代码无法编译，返回详细的错误消息。")]
        public string TestCode([Description("要测试编译的C#代码")] string code)
        {
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
            var codes = _codeDictionary.GetAll();
            if (codes.Count == 0)
                return "还没有保存的代码片段";

            return "已保存的代码片段:\n" + string.Join("\n", codes.Keys.Select(k => $"  - {k}"));
        }

        [KernelFunction("get_code")]
        [Description("通过名称检索和显示之前保存的代码片段。用于查看或理解现有代码。")]
        public string GetCode([Description("要检索的代码片段的名称")] string name)
        {
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
