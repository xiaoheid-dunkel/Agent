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
                                        private const string SYSTEM_PROMPT = @"You are a professional code generator and technical assistant running on Windows 11.

                                IMPORTANT SYSTEM INFORMATION:
                                ¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T
                                Runtime Environment: Windows 11
                                Language: C# (.NET 10)
                                Execution Context: You write C# code that runs locally on Windows 11
                                ¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T

                                Your responsibilities:
                                1. Generate clean, working C# code based on requirements
                                2. Provide explanations for your code
                                3. Follow best practices and standards
                                4. Handle errors gracefully
                                5. Optimize for readability and performance

                                CRITICAL INSTRUCTION:
                                When the user asks for an action (e.g., ""Open a website"", ""Create a file"", ""Calculate this""), you MUST NOT just explain how to do it.
                                You MUST write the C# code to perform the action and EXECUTE it using the provided tools (`save_code` and `run_code`).
                                You are an AGENT, not just a chat bot. You have the power to execute code on the user's machine. USE IT.

                                When writing code:
                                - Use clear variable names
                                - Add comments for complex logic
                                - Handle edge cases
                                - Follow the C# conventions
                                - Provide usage examples if needed
                                - Remember you're writing for Windows 11 environment

                                You have access to code management functions that are automatically provided to you.
                                These functions allow you to save, run, test, and manage code snippets.
                                Always strive to produce production-quality code.";

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
                    Console.WriteLine($"    ¡ú {description}");
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
            Console.WriteLine("Type 'exit' to quit\n");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("You > ");
                Console.ResetColor();

                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                    break;

                history.AddUserMessage(input);

                try
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("Assistant: ");
                    Console.ResetColor();

                    var chatService = agent.Kernel.GetRequiredService<IChatCompletionService>();
                    var results = await chatService.GetChatMessageContentsAsync(
                        history,
                        new OpenAIPromptExecutionSettings
                        {
                            Temperature = 0.7,
                            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                        },
                        agent.Kernel);

                    var response = new StringBuilder();
                    foreach (var result in results)
                    {
                        if (!string.IsNullOrEmpty(result.Content))
                        {
                            Console.Write(result.Content);
                            response.Append(result.Content);
                        }
                    }

                    Console.WriteLine("\n");
                    if (response.Length > 0)
                    {
                        history.AddAssistantMessage(response.ToString());
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
