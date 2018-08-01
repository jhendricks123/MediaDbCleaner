using CommandLine;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaDbCleaner
{
    internal class Program
    {
        private static readonly BlockingCollection<DirectoryInfo> DirtyTables = new BlockingCollection<DirectoryInfo>();
        private static readonly Regex TableNameRegex = new Regex(@"[a-fA-F0-9]{8}-(?:[a-fA-F0-9]{4}-){3}[a-fA-F0-9]{12}(?:_.+)?");

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

            Task.Run(() => EnumerateTables(opts));
            var deleteTask = Task.Run(() => DeleteDirtyTables(opts.ListOnly));
            deleteTask.Wait();
            Logger.Instance.Log("Operation completed.");
            Stop();
        }

        private static void EnumerateTables(Options opts)
        {
            try
            {
                var di = new DirectoryInfo(opts.BankPath);
                foreach (var table in di.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    Logger.Instance.Log($"Checking table '{table.Name}'");
                    if (IsTableDirty(table))
                        DirtyTables.Add(table);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"An error occurred while enumerating tables in bank path. Error: {ex.Message}");
            }
            finally
            {
                DirtyTables.CompleteAdding();
            }
        }

        private static bool IsTableDirty(DirectoryInfo table)
        {
            try
            {
                if (!TableNameRegex.IsMatch(table.Name))
                {
                    Logger.Instance.Log(
                        $"Table with name '{table.Name}' does not match the valid format for a media database table.");
                    return true;
                }

                if (table.GetFiles("*.*", SearchOption.AllDirectories).Count(f => !f.Attributes.HasFlag(FileAttributes.Hidden)) == 0)
                {
                    Logger.Instance.Log($"Table with name '{table.Name}' contains no files.");
                    return true;
                }

                if (table.Name.Contains("_") && table.GetFiles("*.blk", SearchOption.AllDirectories).Length == 0)
                {
                    Logger.Instance.Log($"Table with name '{table.Name}' does not contain any block files (*.blk).");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"An error occurred while evaluating table '{table.Name}'. Error: {ex.Message}");
            }

            return false;
        }

        private static void DeleteDirtyTables(bool listOnly)
        {
            foreach (var dirtyTable in DirtyTables.GetConsumingEnumerable())
            {
                if (listOnly) continue;
                try
                {
                    Logger.Instance.Log($"Deleting table '{dirtyTable.Name}'");
                    dirtyTable.Delete(true);
                    Logger.Instance.Log($"Successfully deleted table '{dirtyTable.Name}'");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Failed to delete {dirtyTable.FullName}. Error: {ex.Message}");
                }
            }
        }

        private static void Stop()
        {
            Logger.Instance.Log("Stopping. . .");
            Logger.Instance.Close();
        }
    }
}
