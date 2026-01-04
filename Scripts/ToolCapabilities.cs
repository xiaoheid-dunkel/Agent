using System;
using System.Collections.Generic;
using System.Text;

namespace xiaohei.Scripts
{
    /// <summary>
    /// Defines all available KernelFunction tools and their capabilities
    /// AI uses this to understand what it can do directly vs what requires code
    /// </summary>
    public static class ToolCapabilities
    {
        public static string GetCapabilitiesSummary()
        {
            var capabilities = new StringBuilder();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("SYSTEM INFORMATION");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine($"Operating System: Windows 11");
            capabilities.AppendLine($"Runtime: .NET 10");
            capabilities.AppendLine($"Language: C#");
            capabilities.AppendLine($"Execution Mode: Local (Code runs on this Windows 11 machine)");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("AVAILABLE KERNEL FUNCTIONS (Direct Tools)");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine();

            // Code Management Functions
            capabilities.AppendLine("?? CODE MANAGEMENT FUNCTIONS:");
            capabilities.AppendLine();

            capabilities.AppendLine("1. save_code(name: string, code: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Save and compile C# code with a unique name");
            capabilities.AppendLine("   Use when: You want to store code for reuse");
            capabilities.AppendLine("   Returns: Compilation status or detailed error messages");
            capabilities.AppendLine();

            capabilities.AppendLine("2. run_code(codeOrName: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Execute C# code immediately on Windows 11");
            capabilities.AppendLine("   Use when: You need to test or get results from code");
            capabilities.AppendLine("   Input: Either saved code name or direct C# code");
            capabilities.AppendLine("   Returns: Execution result or runtime error details");
            capabilities.AppendLine("   Environment: Your code runs on Windows 11, you can use Windows APIs");
            capabilities.AppendLine();

            capabilities.AppendLine("3. test_code(code: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Check if code compiles without errors");
            capabilities.AppendLine("   Use when: You want to validate syntax before saving");
            capabilities.AppendLine("   Returns: Compilation status with error details");
            capabilities.AppendLine();

            capabilities.AppendLine("4. get_code(name: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Retrieve previously saved code");
            capabilities.AppendLine("   Use when: You need to see or modify existing code");
            capabilities.AppendLine("   Returns: Full source code of saved snippet");
            capabilities.AppendLine();

            capabilities.AppendLine("5. list_codes() ¡ú string");
            capabilities.AppendLine("   Purpose: Show all saved code snippet names");
            capabilities.AppendLine("   Use when: You want to see what's available");
            capabilities.AppendLine("   Returns: List of saved code names");
            capabilities.AppendLine();

            capabilities.AppendLine("6. update_code(name: string, newCode: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Modify and recompile existing saved code");
            capabilities.AppendLine("   Use when: You want to improve or fix saved code");
            capabilities.AppendLine("   Returns: Update status or error messages");
            capabilities.AppendLine();

            capabilities.AppendLine("7. delete_code(name: string) ¡ú string");
            capabilities.AppendLine("   Purpose: Remove saved code snippet");
            capabilities.AppendLine("   Use when: You no longer need saved code");
            capabilities.AppendLine("   Returns: Deletion confirmation or error");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("YOUR CAPABILITIES ON WINDOWS 11:");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine();
            capabilities.AppendLine("? Write and execute C# code");
            capabilities.AppendLine("? Access Windows 11 system APIs");
            capabilities.AppendLine("? Open and manage files on disk");
            capabilities.AppendLine("? Open web browsers and URLs");
            capabilities.AppendLine("? Run system commands");
            capabilities.AppendLine("? Create processes");
            capabilities.AppendLine("? Read/Write files");
            capabilities.AppendLine("? Interact with the file system");
            capabilities.AppendLine("? And much more using C# and .NET 10");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("DECISION TREE:");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine();

            capabilities.AppendLine("User Request");
            capabilities.AppendLine("    ©¦");
            capabilities.AppendLine("    ©À©¤ Need to manage saved code?");
            capabilities.AppendLine("    ©¦  ©À©¤ YES ¡ú Use list_codes() / get_code() / delete_code()");
            capabilities.AppendLine("    ©¦  ©¸©¤ NO ¡ú Continue");
            capabilities.AppendLine("    ©¦");
            capabilities.AppendLine("    ©À©¤ Need to create/modify code?");
            capabilities.AppendLine("    ©¦  ©À©¤ Create ¡ú Use save_code(name, code)");
            capabilities.AppendLine("    ©¦  ©¸©¤ Modify ¡ú Use update_code(name, newCode)");
            capabilities.AppendLine("    ©¦");
            capabilities.AppendLine("    ©À©¤ Need to check if code compiles?");
            capabilities.AppendLine("    ©¦  ©¸©¤ YES ¡ú Use test_code(code)");
            capabilities.AppendLine("    ©¦");
            capabilities.AppendLine("    ©¸©¤ Need to execute code?");
            capabilities.AppendLine("       ©¸©¤ YES ¡ú Use run_code(name) or run_code(code)");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("WHEN TO WRITE CODE vs USE EXISTING FUNCTIONS:");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine();

            capabilities.AppendLine("? USE EXISTING FUNCTIONS WHEN:");
            capabilities.AppendLine("  ? Managing code snippets (save, list, get, delete, update)");
            capabilities.AppendLine("  ? Testing code compilation");
            capabilities.AppendLine("  ? Executing saved code");
            capabilities.AppendLine();

            capabilities.AppendLine("? WRITE CODE (via save_code/run_code) WHEN:");
            capabilities.AppendLine("  ? User wants to open websites/URLs (use Process.Start)");
            capabilities.AppendLine("  ? User wants to interact with files on disk (use System.IO)");
            capabilities.AppendLine("  ? User wants to perform system operations");
            capabilities.AppendLine("  ? User wants custom logic not covered by existing functions");
            capabilities.AppendLine("  ? User wants to see output or results from custom code");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine("EXAMPLE WORKFLOWS:");
            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");
            capabilities.AppendLine();

            capabilities.AppendLine("Workflow 1: User asks 'Open a website'");
            capabilities.AppendLine("  1. Check: No existing function for this");
            capabilities.AppendLine("  2. Action: Write C# code using Process.Start() on Windows 11");
            capabilities.AppendLine("  3. Execute: run_code(code) to open browser");
            capabilities.AppendLine("  4. Example code:");
            capabilities.AppendLine("     using System.Diagnostics;");
            capabilities.AppendLine("     var psi = new ProcessStartInfo");
            capabilities.AppendLine("     {");
            capabilities.AppendLine("         FileName = \"https://www.google.com\",");
            capabilities.AppendLine("         UseShellExecute = true");
            capabilities.AppendLine("     };");
            capabilities.AppendLine("     Process.Start(psi);");
            capabilities.AppendLine();

            capabilities.AppendLine("Workflow 2: User asks 'Save my code'");
            capabilities.AppendLine("  1. Check: save_code() exists for this");
            capabilities.AppendLine("  2. Action: Call save_code(name, userCode)");
            capabilities.AppendLine("  3. Result: Code saved and compiled");
            capabilities.AppendLine();

            capabilities.AppendLine("Workflow 3: User asks 'List saved code'");
            capabilities.AppendLine("  1. Check: list_codes() exists for this");
            capabilities.AppendLine("  2. Action: Call list_codes()");
            capabilities.AppendLine("  3. Result: Show all saved code names");
            capabilities.AppendLine();

            capabilities.AppendLine("¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T");

            return capabilities.ToString();
        }

                /// <summary>
                /// Quick reference for AI
                /// </summary>
                public static string GetQuickReference()
                {
                    return @"
        ¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T
        SYSTEM CONTEXT FOR AI
        ¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T¨T

        You are running on: Windows 11
        Runtime: .NET 10
        Language: C#

        QUICK REFERENCE:

        Direct Functions (Use These First):
          save_code(name, code)      ¡ú Save code
          run_code(nameOrCode)       ¡ú Execute code on Windows 11
          test_code(code)            ¡ú Check syntax
          get_code(name)             ¡ú Get saved code
          list_codes()               ¡ú List saved code
          update_code(name, newCode) ¡ú Update code
          delete_code(name)          ¡ú Delete code

        Decision Rule:
          - Have existing function? ¡ú Use it
          - No existing function? ¡ú Write C# code
          - Need to execute code? ¡ú Use run_code()
          - Running on Windows 11? ¡ú YES, use Windows APIs

        Example: Open Website on Windows 11
          using System.Diagnostics;
          var psi = new ProcessStartInfo 
          { 
              FileName = ""https://www.google.com"", 
              UseShellExecute = true 
          };
          Process.Start(psi);
        ";
                }
    }
}
