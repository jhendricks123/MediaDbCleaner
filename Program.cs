using CommandLine;
using System.IO;

namespace MediaDbCleaner
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        private static void Run(Options opts)
        {
            Logger.Init(opts.ReportPath);
            Logger.Instance.Log("Starting. . .");
            if (!Directory.Exists(opts.BankPath))
            {
                Logger.Instance.Log($"Directory does not exist: {opts.BankPath}. For paths with spaces in the name, surround the value with quotes.");
                Stop();
                return;
            }
            var cleaner = new BankCleaner(opts.BankPath);
            cleaner.Clean().Wait();
            Stop();
        }

        private static void Stop()
        {
            Logger.Instance.Log("Stopping. . .");
            Logger.Instance.Close();
        }
    }
}
