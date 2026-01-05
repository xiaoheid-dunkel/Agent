using System;
using System.IO;

namespace xiaohei.Scripts
{
    public static class GlobalContext
    {
        // Default working directory: Workspace folder under program execution directory
        public static string WorkingDirectory { get; private set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workspace");

        public static void SetWorkingDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            WorkingDirectory = Path.GetFullPath(path);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[System] Working directory locked to: {WorkingDirectory}");
            Console.ResetColor();
        }
    }
}
