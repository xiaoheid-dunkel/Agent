using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace xiaohei.Scripts
{
    /// <summary>
    /// Code management methods exposed to AI
    /// </summary>
    public class CodeMethods
    {
        private readonly CodeDictionary _codeDictionary;

        public CodeMethods(CodeDictionary codeDictionary)
        {
            _codeDictionary = codeDictionary;
        }

        [KernelFunction("save_code")]
        [Description("Save C# code with a name. Code will be compiled and stored for execution. Returns compilation errors if code is invalid.")]
        public string SaveCode(
            [Description("Unique name for the code snippet")] string name,
            [Description("C# code to save")] string code)
        {
            try
            {
                // First validate the code
                var validationResult = _codeDictionary.ValidateCode(code);

                // If validation passed (no exception thrown), save it
                _codeDictionary.Save(name, code);
                return $"? Code '{name}' saved and compiled successfully\n{validationResult}";
            }
            catch (Exception ex)
            {
                // Return detailed error message so AI can fix it
                return $"? Cannot save code '{name}':\n{ex.Message}";
            }
        }

        [KernelFunction("list_codes")]
        [Description("List all saved code snippets")]
        public string ListCodes()
        {
            var codes = _codeDictionary.GetAll();
            if (codes.Count == 0)
                return "No saved code snippets yet";

            return "Saved code snippets:\n" + string.Join("\n", codes.Keys.Select(k => $"  - {k}"));
        }

        [KernelFunction("get_code")]
        [Description("Get code by name")]
        public string GetCode([Description("Name of the code snippet")] string name)
        {
            var code = _codeDictionary.Get(name);
            if (code == null)
                return $"? Code '{name}' not found";

            return code;
        }

        [KernelFunction("run_code")]
        [Description("Compile and run C# code. Returns execution results, output, and any errors encountered.")]
        public string RunCode(
            [Description("Name of the code snippet to run, or C# code directly")] string codeOrName)
        {
            try
            {
                // Try to get from dictionary first
                var code = _codeDictionary.Get(codeOrName);
                if (code != null)
                {
                    var result = _codeDictionary.Execute(codeOrName);
                    return result;
                }

                // If not found, treat as direct code
                var directResult = _codeDictionary.ExecuteDirectly(codeOrName);
                return directResult;
            }
            catch (Exception ex)
            {
                return $"? Error executing code:\n{ex.Message}";
            }
        }

        [KernelFunction("delete_code")]
        [Description("Delete a saved code snippet")]
        public string DeleteCode([Description("Name of the code snippet")] string name)
        {
            try
            {
                _codeDictionary.Delete(name);
                return $"? Code '{name}' deleted";
            }
            catch (Exception ex)
            {
                return $"? Error deleting code: {ex.Message}";
            }
        }

        [KernelFunction("update_code")]
        [Description("Update existing code")]
        public string UpdateCode(
            [Description("Name of the code snippet")] string name,
            [Description("New C# code")] string newCode)
        {
            try
            {
                _codeDictionary.Update(name, newCode);
                return $"? Code '{name}' updated and recompiled";
            }
            catch (Exception ex)
            {
                return $"? Error updating code: {ex.Message}";
            }
        }

        [KernelFunction("test_code")]
        [Description("Test if code compiles without errors. Returns compilation errors if any.")]
        public string TestCode([Description("C# code to test")] string code)
        {
            try
            {
                // This will return detailed error messages from Roslyn
                return _codeDictionary.ValidateCode(code);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
