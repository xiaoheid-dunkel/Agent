using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace xiaohei.Scripts
{
    /// <summary>
    /// Code dictionary - stores, compiles, and executes C# code using Roslyn
    /// </summary>
    public class CodeDictionary
    {
        private readonly Dictionary<string, CodeEntry> _codes = new();

        private class CodeEntry
        {
            public string Code { get; set; } = "";
            public object? CompiledDelegate { get; set; }
            public DateTime SavedAt { get; set; }
            public int ExecutionCount { get; set; }
        }

        /// <summary>
        /// Save and compile code
        /// </summary>
        public void Save(string name, string code)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Code name cannot be empty");

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be empty");

            ValidateCode(code);

            _codes[name] = new CodeEntry
            {
                Code = code,
                SavedAt = DateTime.Now,
                ExecutionCount = 0
            };
        }

        /// <summary>
        /// Get code by name
        /// </summary>
        public string? Get(string name)
        {
            return _codes.TryGetValue(name, out var entry) ? entry.Code : null;
        }

        /// <summary>
        /// Get all saved codes
        /// </summary>
        public Dictionary<string, string> GetAll()
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in _codes)
            {
                result[kvp.Key] = kvp.Value.Code;
            }
            return result;
        }

        /// <summary>
        /// Update existing code
        /// </summary>
        public void Update(string name, string newCode)
        {
            if (!_codes.ContainsKey(name))
                throw new KeyNotFoundException($"Code '{name}' not found");

            ValidateCode(newCode);

            _codes[name].Code = newCode;
        }

        /// <summary>
        /// Delete code by name
        /// </summary>
        public void Delete(string name)
        {
            if (!_codes.Remove(name))
                throw new KeyNotFoundException($"Code '{name}' not found");
        }

        /// <summary>
        /// Validate code compilation and return detailed error messages
        /// </summary>
        public string ValidateCode(string code)
        {
            try
            {
                var compilation = CreateCompilation(code);
                var diagnostics = compilation.GetDiagnostics();

                var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
                var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();

                if (errors.Count > 0)
                {
                    var errorMessages = new StringBuilder();
                    errorMessages.AppendLine($"? Compilation failed with {errors.Count} error(s):");
                    errorMessages.AppendLine();

                    foreach (var error in errors)
                    {
                        errorMessages.AppendLine($"Error: {error.GetMessage()}");
                        var location = error.Location.GetLineSpan();
                        errorMessages.AppendLine($"  Line {location.StartLinePosition.Line + 1}: {location.StartLinePosition.Character}");
                        errorMessages.AppendLine();
                    }

                    throw new InvalidOperationException(errorMessages.ToString());
                }

                if (warnings.Count > 0)
                {
                    return $"? Code compiled successfully with {warnings.Count} warning(s)";
                }

                return "? Code is valid and compiles successfully";
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    throw;

                throw new InvalidOperationException($"Compilation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create Roslyn compilation from code
        /// </summary>
        private Compilation CreateCompilation(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Add all necessary .NET 10 assemblies for proper compilation
            var references = new[]
            {
                // Core runtime
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),

                // System (includes basics like Uri, etc.)
                MetadataReference.CreateFromFile(typeof(System.Uri).Assembly.Location),

                // LINQ support
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),

                // Console I/O
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),

                // System.Diagnostics (for Process, File operations, etc.)
                MetadataReference.CreateFromFile(typeof(System.Diagnostics.Process).Assembly.Location),

                // System.IO (for File operations)
                MetadataReference.CreateFromFile(typeof(System.IO.File).Assembly.Location),

                // System.Collections
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),

                // System.Linq.Expressions (for advanced LINQ scenarios)
                MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create("DynamicCode")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            return compilation;
        }

        /// <summary>
        /// Execute saved code by name
        /// </summary>
        public string Execute(string name)
        {
            if (!_codes.TryGetValue(name, out var entry))
                throw new KeyNotFoundException($"Code '{name}' not found");

            return ExecuteCode(entry.Code);
        }

        /// <summary>
        /// Execute code directly (not saved)
        /// </summary>
        public string ExecuteDirectly(string code)
        {
            return ExecuteCode(code);
        }

        private string ExecuteCode(string code)
        {
            try
            {
                // First validate compilation
                var validationResult = ValidateCode(code);

                var stopwatch = Stopwatch.StartNew();
                var globals = new CodeGlobals();

                // Fix: Add all necessary references to match CreateCompilation
                var references = new[]
                {
                    typeof(object).Assembly,
                    typeof(System.Uri).Assembly,
                    typeof(Enumerable).Assembly,
                    typeof(Console).Assembly,
                    typeof(System.Diagnostics.Process).Assembly,
                    typeof(System.IO.File).Assembly,
                    typeof(System.Collections.Generic.List<>).Assembly,
                    typeof(System.Linq.Expressions.Expression).Assembly,
                };

                var task = CSharpScript.RunAsync(
                    code,
                    ScriptOptions.Default
                        .WithReferences(references)
                        .WithImports("System", "System.IO", "System.Collections.Generic", "System.Linq", "System.Diagnostics"),
                    globals);

                task.Wait(TimeSpan.FromSeconds(30)); // 30 second timeout

                if (task.IsFaulted)
                {
                    stopwatch.Stop();
                    var ex = task.Exception?.InnerException ?? new Exception("Code execution failed");

                    // Format runtime error for AI to understand
                    var errorMessage = new StringBuilder();
                    errorMessage.AppendLine("? Runtime Error:");
                    errorMessage.AppendLine(ex.GetType().Name + ": " + ex.Message);

                    if (!string.IsNullOrEmpty(ex.StackTrace))
                    {
                        errorMessage.AppendLine("\nStackTrace:");
                        var stackLines = ex.StackTrace.Split('\n').Take(5); // First 5 stack frames
                        foreach (var line in stackLines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                                errorMessage.AppendLine("  " + line.Trim());
                        }
                    }

                    return errorMessage.ToString();
                }

                stopwatch.Stop();

                var result = task.Result.ReturnValue ?? "null";
                var output = globals.GetOutput();

                var successMessage = new StringBuilder();
                successMessage.AppendLine("? Execution successful");

                if (!string.IsNullOrEmpty(output))
                    successMessage.AppendLine(output);

                successMessage.AppendLine($"Return value: {result}");
                successMessage.AppendLine($"Executed in: {stopwatch.ElapsedMilliseconds}ms");

                return successMessage.ToString();
            }
            catch (OperationCanceledException)
            {
                return "? Code execution timeout (30 seconds limit exceeded)";
            }
            catch (InvalidOperationException ex)
            {
                // Validation errors
                return ex.Message;
            }
            catch (Exception ex)
            {
                return $"? Execution error: {ex.GetType().Name}\n{ex.Message}";
            }
        }

        /// <summary>
        /// Get code statistics
        /// </summary>
        public (int Total, int MostUsed) GetStatistics()
        {
            if (_codes.Count == 0)
                return (0, 0);

            var mostUsed = _codes.Values.Max(e => e.ExecutionCount);
            return (_codes.Count, mostUsed);
        }

        /// <summary>
        /// Clear all codes
        /// </summary>
        public void Clear()
        {
            _codes.Clear();
        }

        /// <summary>
        /// Get descriptions of all available code management functions
        /// </summary>
        public string GetAvailableFunctions()
        {
            var functions = new List<string>
            {
                "Available Code Management Functions:",
                "",
                "1. save_code(name, code)",
                "   - Save C# code with a unique name",
                "   - Code is immediately compiled and validated",
                "   - Use this to store important code snippets for reuse",
                "",
                "2. run_code(nameOrCode)",
                "   - Execute code by name (if saved) or directly",
                "   - Returns execution result with timing info",
                "   - Has 30-second timeout for safety",
                "",
                "3. list_codes()",
                "   - Show all currently saved code snippets",
                "   - Displays the names only",
                "",
                "4. get_code(name)",
                "   - Retrieve the full source code of a saved snippet",
                "   - Useful for reviewing or modifying code",
                "",
                "5. update_code(name, newCode)",
                "   - Modify existing saved code",
                "   - New code is immediately compiled",
                "   - Use when you want to improve or fix existing code",
                "",
                "6. delete_code(name)",
                "   - Remove a saved code snippet",
                "   - Permanently deletes from this session",
                "",
                "7. test_code(code)",
                "   - Validate if code compiles without running it",
                "   - Use before saving to catch syntax errors early",
                "",
                "WORKFLOW TIPS:",
                "- Always test_code() first to catch syntax errors",
                "- Then save_code() to store the code",
                "- Use run_code() to execute and verify results",
                "- Use update_code() to improve or fix code",
                "- Use list_codes() to see what you have"
            };

            return string.Join("\n", functions);
        }

        /// <summary>
        /// Export all codes as a summary
        /// </summary>
        public string Export()
        {
            if (_codes.Count == 0)
                return "No saved code snippets";

            var lines = new List<string> { "=== Code Dictionary Export ===" };
            
            foreach (var kvp in _codes)
            {
                lines.Add($"\n[{kvp.Key}]");
                lines.Add($"Saved: {kvp.Value.SavedAt:yyyy-MM-dd HH:mm:ss}");
                lines.Add($"Executions: {kvp.Value.ExecutionCount}");
                lines.Add("Code:");
                var codeLines = kvp.Value.Code.Split('\n');
                foreach (var line in codeLines.Take(10))
                {
                    lines.Add($"  {line}");
                }
                if (codeLines.Length > 10)
                    lines.Add($"  ... ({codeLines.Length - 10} more lines)");
            }

            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// Global context for script execution
    /// </summary>
    public class CodeGlobals
    {
        private readonly List<string> _output = new();

        public void WriteLine(object? obj = null)
        {
            var text = obj?.ToString() ?? "";

            // Real-time console output
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ResetColor();

            _output.Add(text);
        }

        public void Print(object? obj)
        {
            var text = obj?.ToString() ?? "";

            // Real-time console output
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(text);
            Console.ResetColor();

            _output.Add(text);
        }

        public string GetOutput()
        {
            if (_output.Count == 0)
                return "";
            return "Output:\n" + string.Join("\n", _output);
        }
    }
}
