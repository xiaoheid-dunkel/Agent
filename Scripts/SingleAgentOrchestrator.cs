#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace xiaohei.Scripts
{
    public class SingleAgentOrchestrator
    {
        private readonly IConfiguration _config;
                                                                                                                                                                                                                                                                                                                                        private const string SYSTEM_PROMPT = @"你是一个运行在Windows 11上的AI代码助手，具备代码执行能力。

                                                                                                                                                                                                                                                                                                                                系统信息：
                                                                                                                                                                                                                                                                                                                                - 操作系统：Windows 11
                                                                                                                                                                                                                                                                                                                                - 运行时：.NET 10
                                                                                                                                                                                                                                                                                                                                - 你拥有7个代码管理函数

                                                                                                                                                                                                                                                                                                                                关键代码规范（非常重要）：
                                                                                                                                                                                                                                                                                                                                1. 你的所有代码必须是顶级语句。
                                                                                                                                                                                                                                                                                                                                2. **代码的第一行必须是双斜杠注释，简要描述这段代码的功能。**
                                                                                                                                                                                                                                                                                                                                   这对你记忆该代码的功能至关重要。

                                                                                                                                                                                                                                                                                                                                正确示例：
                                                                                                                                                                                                                                                                                                                                   // 打开B站网页并搜索
                                                                                                                                                                                                                                                                                                                                   using System.Diagnostics;
                                                                                                                                                                                                                                                                                                                                   var url = ""https://www.bilibili.com"";
                                                                                                                                                                                                                                                                                                                                   Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                                                                                                                                                                                                                                                                                                                                   return ""已打开网页"";

                                                                                                                                                                                                                                                                                                                                工作流程：
                                                                                                                                                                                                                                                                                                                                1. 用户请求任务
                                                                                                                                                                                                                                                                                                                                2. 编写代码（记得第一行写注释！）
                                                                                                                                                                                                                                                                                                                                3. save_code() -> 这会将代码保存到本地 'SavedScripts' 文件夹
                                                                                                                                                                                                                                                                                                                                4. run_code()
                                                                                                                                                                                                                                                                                                                                5. 将结果返回给用户

                                                                                                                                                                                                                                                                                                                                关键指示：

                                                                                                                                                                                                                                                                                                                                当用户请求任何操作时：
                                                                                                                                                                                                                                                                                                                                1. 你必须编写C#代码来完成它
                                                                                                                                                                                                                                                                                                                                2. 使用save_code()保存你的代码
                                                                                                                                                                                                                                                                                                                                3. 使用run_code()执行你的代码
                                                                                                                                                                                                                                                                                                                                4. 将结果返回给用户

                                                                                                                                                                                                                                                                                                                                你不能拒绝或说你做不了某事。
                                                                                                                                                                                                                                                                                                                                你必须编写代码来完成请求。

                                                                                                                                                                                                                                                                                                                                可用函数：
                                                                                                                                                                                                                                                                                                                                1. save_code(name, code) - 保存C#代码
                                                                                                                                                                                                                                                                                                                                2. run_code(codeOrName) - 执行代码
                                                                                                                                                                                                                                                                                                                                3. test_code(code) - 测试编译
                                                                                                                                                                                                                                                                                                                                4. list_codes() - 列出代码
                                                                                                                                                                                                                                                                                                                                5. get_code(name) - 获取代码
                                                                                                                                                                                                                                                                                                                                6. update_code(name, newCode) - 更新代码
                                                                                                                                                                                                                                                                                                                                7. delete_code(name) - 删除代码

                                                                                                                                                                                                                                                                                                                                代码风格规则：

                                                                                                                                                                                                                                                                                                                                正确 - 使用顶级语句：
                                                                                                                                                                                                                                                                                                                                using System.Diagnostics;
                                                                                                                                                                                                                                                                                                                                Process.Start(new ProcessStartInfo { FileName = ""https://www.bilibili.com"", UseShellExecute = true });
                                                                                                                                                                                                                                                                                                                                return ""网页已打开"";

                                                                                                                                                                                                                                                                                                                                错误 - 永远不要使用入口点：
                                                                                                                                                                                                                                                                                                                                class Program { static void Main() { } }
                                                                                                                                                                                                                                                                                                                                static void MyMethod() { }

                                                                                                                                                                                                                                                                                                                                你可以在代码中做的事情：

                                                                                                                                                                                                                                                                                                                                打开网页：
                                                                                                                                                                                                                                                                                                                                  using System.Diagnostics;
                                                                                                                                                                                                                                                                                                                                  Process.Start(new ProcessStartInfo { FileName = ""url"", UseShellExecute = true });

                                                                                                                                                                                                                                                                                                                                创建文件：
                                                                                                                                                                                                                                                                                                                                  using System.IO;
                                                                                                                                                                                                                                                                                                                                  File.WriteAllText(@""C:\path\file.txt"", ""content"");

                                                                                                                                                                                                                                                                                                                                读取文件：
                                                                                                                                                                                                                                                                                                                                  using System.IO;
                                                                                                                                                                                                                                                                                                                                  var content = File.ReadAllText(@""C:\path\file.txt"");
                                                                                                                                                                                                                                                                                                                                  return content;

                                                                                                                                                                                                                                                                                                                                运行命令：
                                                                                                                                                                                                                                                                                                                                  using System.Diagnostics;
                                                                                                                                                                                                                                                                                                                                  var p = Process.Start(""cmd.exe"", ""/c dir"");
                                                                                                                                                                                                                                                                                                                                  p.WaitForExit();

                                                                                                                                                                                                                                                                                                                                计算：
                                                                                                                                                                                                                                                                                                                                  var result = Enumerable.Range(1, 100).Sum();
                                                                                                                                                                                                                                                                                                                                  return result.ToString();

                                                                                                                                                                                                                                                                                                                                你的工作流程：

                                                                                                                                                                                                                                                                                                                                步骤1：用户给你一个请求
                                                                                                                                                                                                                                                                                                                                步骤2：编写C#代码来做它
                                                                                                                                                                                                                                                                                                                                步骤3：保存代码：save_code('描述性名称', your_code)
                                                                                                                                                                                                                                                                                                                                步骤4：运行代码：run_code('描述性名称')
                                                                                                                                                                                                                                                                                                                                步骤5：把结果显示给用户

                                                                                                                                                                                                                                                                                                                                记住：
                                                                                                                                                                                                                                                                                                                                - 仅使用顶级语句
                                                                                                                                                                                                                                                                                                                                - 无class定义
                                                                                                                                                                                                                                                                                                                                - 无static方法
                                                                                                                                                                                                                                                                                                                                - 代码立即执行
                                                                                                                                                                                                                                                                                                                                - 使用System.IO处理文件
                                                                                                                                                                                                                                                                                                                                - 使用System.Diagnostics处理进程
                                                                                                                                                                                                                                                                                                                                - 完整访问.NET 10 API

                                                                                                                                                                                                                                                                                                                                文件操作重要规则：
                                                                                                                                                                                                                                                                                                                                1. 你被限制在用户指定的工作目录中。
                                                                                                                                                                                                                                                                                                                                2. 禁止使用 System.IO.File 或 System.IO.Directory。
                                                                                                                                                                                                                                                                                                                                3. 必须使用 'SafeIO' 工具类进行文件操作。
                                                                                                                                                                                                                                                                                                                                   - 写文件：SafeIO.WriteAllText(""filename.txt"", ""content"")
                                                                                                                                                                                                                                                                                                                                   - 读文件：SafeIO.ReadAllText(""filename.txt"")
                                                                                                                                                                                                                                                                                                                                   - 删文件：SafeIO.DeleteFile(""filename.txt"")
                                                                                                                                                                                                                                                                                                                                   - 建目录：SafeIO.CreateDirectory(""foldername"")
                                                                                                                                                                                                                                                                                                                                4. 路径只需写相对路径（如 ""test.txt""），系统会自动映射到安全目录。

                                                                                                                                                                                                                                                                                                                                你是一个代码生成器。生成并执行代码。
                                                                                                                                                                                                                                                                                                                                不要仅仅解释能做什么 - 去做它！";

        public SingleAgentOrchestrator()
        {
            var builder = new ConfigurationBuilder();

            // Try multiple paths to find appsettings.json
            var paths = new[]
            {
                "appsettings.json",
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")
            };

            string? configPath = null;
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    configPath = path;
                    break;
                }
            }

            if (configPath == null)
            {
                throw new FileNotFoundException(
                    "appsettings.json not found in any expected location. " +
                    "Please ensure appsettings.json exists in the project root and is set to copy to output directory.");
            }

            var fullPath = Path.GetFullPath(configPath);
            var directory = Path.GetDirectoryName(fullPath);

            builder.SetBasePath(directory);
            builder.AddJsonFile(Path.GetFileName(fullPath), optional: false, reloadOnChange: true);
            _config = builder.Build();
        }

        public async Task RunAsync()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("  XiaoHei - AI Code Assistant");
                Console.WriteLine("========================================\n");

                var kernel = InitializeKernel();
                var agent = CreateAgent(kernel);

                // Display tool capabilities to user and AI
                DisplayToolCapabilities();

                // Display loaded methods
                PrintLoadedMethods(kernel);

                await RunConversationLoopAsync(agent);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] {ex.Message}");
                Console.ResetColor();
            }
        }

        // New command handling method
        private void HandleCommand(string input)
        {
            var parts = input.Split(' ', 2);
            var command = parts[0].ToLower();
            var arg = parts.Length > 1 ? parts[1].Trim() : "";

            switch (command)
            {
                case "/exit":
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                    break;

                case "/cd":
                case "/workdir":
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        Console.WriteLine("Error: Please provide a path. Example: /cd D:\\Test");
                    }
                    else
                    {
                        try { GlobalContext.SetWorkingDirectory(arg); }
                        catch (Exception ex) { Console.WriteLine($"Set failed: {ex.Message}"); }
                    }
                    break;

                case "/help":
                    Console.WriteLine("Available Commands:");
                    Console.WriteLine("  /exit           - Exit program");
                    Console.WriteLine("  /cd <path>      - Set AI working/safe directory (alias: /workdir)");
                    Console.WriteLine("  /reset          - Clear conversation history (not implemented)");
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }

        private Kernel InitializeKernel()
        {
            var apiKey = _config["Doubao:ApiKey"] ?? throw new Exception("ApiKey missing in appsettings.json");
            var modelId = _config["Doubao:EndpointId"] ?? "doubao-pro-32k";
            var endpoint = _config["Doubao:Endpoint"] ?? "https://ark.cn-beijing.volces.com/api/v3";

            var builder = Kernel.CreateBuilder();
            var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            httpClient.BaseAddress = new Uri(endpoint);

            builder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                httpClient: httpClient);

            return builder.Build();
        }

        private ChatCompletionAgent CreateAgent(Kernel kernel)
        {
            // Create code dictionary and register methods
            var codeDictionary = new CodeDictionary();
            var codeMethods = new CodeMethods(codeDictionary);
            kernel.Plugins.AddFromObject(codeMethods, "CodeTools");

            // Get available functions description from CodeDictionary
            var functionsDescription = codeDictionary.GetAvailableFunctions();

            // Get tool capabilities information
            var toolCapabilities = ToolCapabilities.GetQuickReference();

            var config = new AgentConfig
            {
                Name = "CodeAssistant",
                Instructions = SYSTEM_PROMPT + "\n\n" + toolCapabilities + "\n\n" + functionsDescription,
                Temperature = 0.7
            };

            var agent = new ChatCompletionAgent
            {
                Name = config.Name,
                Instructions = config.Instructions,
                Kernel = kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
                {
                    Temperature = config.Temperature,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                })
            };

            return agent;
        }

        private void DisplayToolCapabilities()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(ToolCapabilities.GetCapabilitiesSummary());
            Console.ResetColor();
        }

        private void PrintLoadedMethods(Kernel kernel)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[System] Loaded Functions:");
            Console.ResetColor();

            var plugins = kernel.Plugins;
            int totalFunctions = 0;

            foreach (var plugin in plugins)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nPlugin: {plugin.Name}");
                Console.ResetColor();

                var functions = plugin.Where(f => f.Metadata.Name != null).ToList();
                totalFunctions += functions.Count;

                foreach (var function in functions)
                {
                    var description = function.Metadata.Description ?? "No description";
                    Console.WriteLine($"  ? {function.Metadata.Name}");
                    Console.WriteLine($"    → {description}");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[System] {totalFunctions} functions loaded and ready\n");
            Console.ResetColor();

            // Show that AI has been informed
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[System] AI has been informed about all available functions\n");
            Console.ResetColor();
        }

        private async Task RunConversationLoopAsync(ChatCompletionAgent agent)
        {
            var history = new ChatHistory();

            // Initialize default directory
            GlobalContext.SetWorkingDirectory(GlobalContext.WorkingDirectory);

            Console.WriteLine("Tip: Type '/help' to see available commands");
            Console.WriteLine("Type 'exit' to quit\n");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"You ({GlobalContext.WorkingDirectory}) > ");
                Console.ResetColor();

                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                // === Command Mode Interception ===
                if (input.StartsWith("/"))
                {
                    HandleCommand(input);
                    continue; // Skip AI processing, go to next loop
                }

                if (input.ToLower() == "exit")
                    break;

                history.AddUserMessage(input);

                try
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("Assistant: ");
                    Console.ResetColor();

                    var chatService = agent.Kernel.GetRequiredService<IChatCompletionService>();

                    // 用来拼接完整的回复，以便存入历史记录
                    var fullResponse = new StringBuilder();

                    // === 关键修改开始：使用 await foreach 循环 ===
                    // 注意：不要在前面加 var results = await ...
                    await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(
                        history,
                        new OpenAIPromptExecutionSettings
                        {
                            Temperature = 0.7,
                            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                        },
                        agent.Kernel))
                    {
                        // 实时打印每一个片段
                        if (!string.IsNullOrEmpty(update.Content))
                        {
                            Console.Write(update.Content);
                            fullResponse.Append(update.Content);
                        }
                    }
                    // === 关键修改结束 ===

                    Console.WriteLine("\n");

                    // 将完整回复存入历史，否则下一轮对话 AI 会“失忆”
                    if (fullResponse.Length > 0)
                    {
                        history.AddAssistantMessage(fullResponse.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[Error] {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[Exit] Goodbye!");
            Console.ResetColor();
        }
    }
}
