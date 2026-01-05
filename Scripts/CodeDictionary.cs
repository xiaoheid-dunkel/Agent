using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly string _savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedScripts");

        private class CodeEntry
        {
            public string Code { get; set; } = "";
            public object? CompiledDelegate { get; set; }
            public DateTime SavedAt { get; set; }
            public int ExecutionCount { get; set; }
            public string Description { get; set; } = "";
        }

        public CodeDictionary()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            var files = Directory.GetFiles(_savePath, "*.cs");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[System] Found {files.Length} saved scripts. Loading...");

            foreach (var file in files)
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string code = File.ReadAllText(file);
                    string description = ExtractDescription(code);

                    _codes[name] = new CodeEntry
                    {
                        Code = code,
                        SavedAt = File.GetLastWriteTime(file),
                        ExecutionCount = 0,
                        Description = description
                    };
                    Console.WriteLine($"  + Loaded: {name} ({description})");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ! Failed to load {file}: {ex.Message}");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private string ExtractDescription(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "No description";

            using (var reader = new StringReader(code))
            {
                string? firstLine = reader.ReadLine()?.Trim();
                if (firstLine != null && firstLine.StartsWith("//"))
                {
                    return firstLine.TrimStart('/', ' ').Trim();
                }
            }
            return "No function description";
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

            string description = ExtractDescription(code);
            _codes[name] = new CodeEntry
            {
                Code = code,
                SavedAt = DateTime.Now,
                ExecutionCount = 0,
                Description = description
            };

            string filePath = Path.Combine(_savePath, $"{name}.cs");
            File.WriteAllText(filePath, code);
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

            string description = ExtractDescription(newCode);
            _codes[name].Code = newCode;
            _codes[name].Description = description;
            _codes[name].SavedAt = DateTime.Now;

            string filePath = Path.Combine(_savePath, $"{name}.cs");
            File.WriteAllText(filePath, newCode);
        }

        /// <summary>
        /// Delete code by name
        /// </summary>
        public void Delete(string name)
        {
            if (!_codes.Remove(name))
                throw new KeyNotFoundException($"Code '{name}' not found");

            string filePath = Path.Combine(_savePath, $"{name}.cs");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Validate code compilation and return detailed error messages
        /// </summary>
        public string ValidateCode(string code)
        {
            try
            {
                // 1. Security Check: Ban unsafe classes
                CheckSecurity(code);

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

                    // Print errors to console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessages.ToString());
                    Console.ResetColor();

                    throw new InvalidOperationException(errorMessages.ToString());
                }

                if (warnings.Count > 0)
                {
                    var warningMsg = $"? Code compiled successfully with {warnings.Count} warning(s)";

                    // Print warnings to console
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(warningMsg);
                    foreach (var warning in warnings)
                    {
                        Console.WriteLine($"  Warning: {warning.GetMessage()}");
                    }
                    Console.ResetColor();

                    return warningMsg;
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

        private void CheckSecurity(string code)
        {
            // Simple keyword check (can be improved with Roslyn SyntaxTree)
            // Ban direct use of File or Directory classes
            if (code.Contains("System.IO.File") || code.Contains("System.IO.Directory") || 
                code.Contains(" File.") || code.Contains(" Directory.")) // Note spaces to avoid false positives like Path or FileInfo
            {
                throw new InvalidOperationException("Security Policy Block: Direct use of System.IO.File or Directory is prohibited.\nPlease use 'SafeIO' class instead. Example: SafeIO.WriteAllText(...)");
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

                // Fix: Add System.ComponentModel.Component reference (required for Process)
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.Component).Assembly.Location),

                // Fix: Add System.Runtime reference (required for MarshalByRefObject)
                MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location),

                // System.IO (for File operations)
                MetadataReference.CreateFromFile(typeof(System.IO.File).Assembly.Location),

                // System.Collections
                MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),

                    // System.Linq.Expressions (for advanced LINQ scenarios)
                    MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),

                    // Add SafeIO reference
                    MetadataReference.CreateFromFile(typeof(SafeIO).Assembly.Location),
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

            entry.ExecutionCount++;
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
                    typeof(System.ComponentModel.Component).Assembly, // Fix: Add Component reference
                    System.Reflection.Assembly.Load("System.Runtime"), // Fix: Add System.Runtime reference
                    typeof(System.IO.File).Assembly,
                        typeof(System.Collections.Generic.List<>).Assembly,
                        typeof(System.Linq.Expressions.Expression).Assembly,
                        typeof(SafeIO).Assembly, // Add SafeIO reference
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
            var sb = new StringBuilder();
            sb.AppendLine("System Loaded Custom Functions (Saved Functions):");

            if (_codes.Count == 0)
            {
                sb.AppendLine("  (No saved scripts)");
            }
            else
            {
                foreach (var kvp in _codes)
                {
                    sb.AppendLine($"  - {kvp.Key}: {kvp.Value.Description}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Available Code Management Functions (System Tools):");
            sb.AppendLine("1. save_code(name, code)");
            sb.AppendLine("   - Save C# code with a unique name");
            sb.AppendLine("   - Code is immediately compiled and validated");
            sb.AppendLine("   - IMPORTANT: First line of code MUST be a // comment describing the function");
            sb.AppendLine("");
            sb.AppendLine("2. run_code(nameOrCode)");
            sb.AppendLine("   - Execute code by name (if saved) or directly");
            sb.AppendLine("   - Returns execution result with timing info");
            sb.AppendLine("   - Has 30-second timeout for safety");
            sb.AppendLine("");
            sb.AppendLine("3. list_codes()");
            sb.AppendLine("   - Show all currently saved code snippets");
            sb.AppendLine("   - Displays the names only");
            sb.AppendLine("");
            sb.AppendLine("4. get_code(name)");
            sb.AppendLine("   - Retrieve the full source code of a saved snippet");
            sb.AppendLine("   - Useful for reviewing or modifying code");
            sb.AppendLine("");
            sb.AppendLine("5. update_code(name, newCode)");
            sb.AppendLine("   - Modify existing saved code");
            sb.AppendLine("   - New code is immediately compiled");
            sb.AppendLine("   - Use when you want to improve or fix existing code");
            sb.AppendLine("");
            sb.AppendLine("6. delete_code(name)");
            sb.AppendLine("   - Remove a saved code snippet");
            sb.AppendLine("   - Permanently deletes from this session");
            sb.AppendLine("");
            sb.AppendLine("7. test_code(code)");
            sb.AppendLine("   - Validate if code compiles without running it");
            sb.AppendLine("   - Use before saving to catch syntax errors early");
            sb.AppendLine("");
            sb.AppendLine("WORKFLOW TIPS:");
            sb.AppendLine("- Always test_code() first to catch syntax errors");
            sb.AppendLine("- Then save_code() to store the code");
            sb.AppendLine("- Use run_code() to execute and verify results");
            sb.AppendLine("- Use update_code() to improve or fix code");
            sb.AppendLine("- Use list_codes() to see what you have");

            return sb.ToString();
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
