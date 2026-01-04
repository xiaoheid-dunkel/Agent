using xiaohei.Scripts;
using System;
using System.Threading.Tasks;

namespace xiaohei.Scripts
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var orchestrator = new SingleAgentOrchestrator();
                await orchestrator.RunAsync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
