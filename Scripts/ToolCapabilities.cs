using System;
using System.Collections.Generic;
using System.Text;

namespace xiaohei.Scripts
{
    public static class ToolCapabilities
    {
        public static string GetCapabilitiesSummary()
        {
            var capabilities = new StringBuilder();

            capabilities.AppendLine("系统信息");
            capabilities.AppendLine("- 操作系统：Windows 11");
            capabilities.AppendLine("- 运行时：.NET 10");
            capabilities.AppendLine("- 编程语言：C#");
            capabilities.AppendLine();

            capabilities.AppendLine("可用的代码管理函数：");
            capabilities.AppendLine();
            capabilities.AppendLine("1. save_code(name, code)");
            capabilities.AppendLine("   保存C#代码并使用唯一名称");
            capabilities.AppendLine("   代码会立即被编译和验证");
            capabilities.AppendLine("   仅使用顶级语句（无class Program或Main）");
            capabilities.AppendLine();
            capabilities.AppendLine("2. run_code(codeOrName)");
            capabilities.AppendLine("   执行C#代码或通过名称运行之前保存的代码");
            capabilities.AppendLine("   返回执行结果、输出和执行时间");
            capabilities.AppendLine();
            capabilities.AppendLine("3. test_code(code)");
            capabilities.AppendLine("   测试C#代码是否能编译而不执行");
            capabilities.AppendLine("   返回详细的编译错误信息");
            capabilities.AppendLine();
            capabilities.AppendLine("4. list_codes()");
            capabilities.AppendLine("   显示所有之前保存的代码片段");
            capabilities.AppendLine();
            capabilities.AppendLine("5. get_code(name)");
            capabilities.AppendLine("   检索并显示保存的代码片段");
            capabilities.AppendLine();
            capabilities.AppendLine("6. update_code(name, newCode)");
            capabilities.AppendLine("   更新现有的代码片段");
            capabilities.AppendLine();
            capabilities.AppendLine("7. delete_code(name)");
            capabilities.AppendLine("   删除保存的代码片段");
            capabilities.AppendLine();

            capabilities.AppendLine("执行环境：");
            capabilities.AppendLine("- 代码运行在 Roslyn 脚本上下文中");
            capabilities.AppendLine("- 使用顶级语句（直接代码）");
            capabilities.AppendLine("- 无需入口点（无 Main 方法、无 Program 类）");
            capabilities.AppendLine("- 代码立即执行");
            capabilities.AppendLine();

            capabilities.AppendLine("你可以做的事情：");
            capabilities.AppendLine("- 打开网页（使用 Process.Start）");
            capabilities.AppendLine("- 创建、读取、写入文件（使用 System.IO）");
            capabilities.AppendLine("- 运行系统命令（使用 Process）");
            capabilities.AppendLine("- 执行计算和数据处理");
            capabilities.AppendLine("- 访问 Windows 11 系统 API");
            capabilities.AppendLine("- 执行任何 C# .NET 10 代码");
            capabilities.AppendLine();

            capabilities.AppendLine("如何完成用户请求：");
            capabilities.AppendLine();
            capabilities.AppendLine("步骤1：用户给你一个请求");
            capabilities.AppendLine("步骤2：编写C#代码来实现它");
            capabilities.AppendLine("步骤3：使用 save_code() 保存代码");
            capabilities.AppendLine("步骤4：使用 run_code() 执行它");
            capabilities.AppendLine("步骤5：将结果返回给用户");
            capabilities.AppendLine();

            capabilities.AppendLine("代码示例：");
            capabilities.AppendLine();
            capabilities.AppendLine("打开网页：");
            capabilities.AppendLine("  using System.Diagnostics;");
            capabilities.AppendLine("  Process.Start(new ProcessStartInfo { FileName = \"https://www.bilibili.com\", UseShellExecute = true });");
            capabilities.AppendLine("  return \"网页已打开\";");
            capabilities.AppendLine();
            capabilities.AppendLine("创建文件：");
            capabilities.AppendLine("  using System.IO;");
            capabilities.AppendLine("  File.WriteAllText(@\"C:\\Users\\Desktop\\test.txt\", \"Hello World\");");
            capabilities.AppendLine("  return \"文件已创建\";");
            capabilities.AppendLine();
            capabilities.AppendLine("读取文件：");
            capabilities.AppendLine("  using System.IO;");
            capabilities.AppendLine("  var content = File.ReadAllText(@\"C:\\Users\\Desktop\\test.txt\");");
            capabilities.AppendLine("  return content;");
            capabilities.AppendLine();
            capabilities.AppendLine("计算：");
            capabilities.AppendLine("  var sum = Enumerable.Range(1, 100).Sum();");
            capabilities.AppendLine("  return $\"1到100的和：{sum}\";");
            capabilities.AppendLine();

            return capabilities.ToString();
        }

        public static string GetQuickReference()
        {
            return @"系统上下文 - Windows 11、.NET 10、C#

你有7个代码管理函数：

1. save_code(name, code) - 保存代码
2. run_code(codeOrName) - 执行代码
3. test_code(code) - 测试编译
4. list_codes() - 列出代码
5. get_code(name) - 获取代码
6. update_code(name, newCode) - 更新代码
7. delete_code(name) - 删除代码

你的工作流程：
1. 用户给你一个请求
2. 你必须编写C#代码来完成它
3. 使用save_code()保存代码
4. 使用run_code()执行代码
5. 将结果返回给用户

记住：
- 仅使用顶级语句
- 无class Program或static Main
- 代码立即执行
- 使用System.IO处理文件
- 使用System.Diagnostics处理进程
- 你可以访问所有.NET 10 API

重要：
不要拒绝任何请求
必须编写代码来完成任务
";
        }
    }
}
