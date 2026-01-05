using System;
using System.IO;

namespace xiaohei.Scripts
{
    public static class SafeIO
    {
        private static string ValidatePath(string path)
        {
            // Combine relative path with working directory to get absolute path
            string fullPath = Path.GetFullPath(Path.Combine(GlobalContext.WorkingDirectory, path));

            // Core check: Does the path start with the working directory?
            if (!fullPath.StartsWith(GlobalContext.WorkingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException($"[Security Block] Access denied to path outside working directory: {path}");
            }
            return fullPath;
        }

        // --- Wrappers for common file operations ---

        public static void WriteAllText(string path, string content)
        {
            File.WriteAllText(ValidatePath(path), content);
        }

        public static string ReadAllText(string path)
        {
            return File.ReadAllText(ValidatePath(path));
        }

        public static void DeleteFile(string path)
        {
            File.Delete(ValidatePath(path));
        }

        public static bool Exists(string path)
        {
            return File.Exists(ValidatePath(path));
        }

        public static void CreateDirectory(string path)
        {
            Directory.CreateDirectory(ValidatePath(path));
        }
        
        public static void AppendAllText(string path, string content)
        {
            File.AppendAllText(ValidatePath(path), content);
        }
        
        public static string[] GetFiles(string path, string searchPattern = "*")
        {
            return Directory.GetFiles(ValidatePath(path), searchPattern);
        }
    }
}
