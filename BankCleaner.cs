using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace MediaDbCleaner
{
    internal class BankCleaner
    {
        private readonly BlockingCollection<DirectoryInfo> _dirtyTables = new BlockingCollection<DirectoryInfo>();
        private readonly Regex _tableNameRegex = new Regex(@"[a-fA-F0-9]{8}-(?:[a-fA-F0-9]{4}-){3}[a-fA-F0-9]{12}(?:_.+)?");

        private readonly DirectoryInfo _bank;
        private readonly bool _readOnly;

        public BankCleaner(string bankPath, bool readOnly = false)
        {
            _bank = new DirectoryInfo(bankPath);
            _readOnly = readOnly;
        }

        public async Task Clean()
        {
            if (!Validate()) return; 
            var deleteTask = DeleteDirtyTables();
            await EnumerateTables();
            await deleteTask;
            Logger.Instance.Log("Operation completed.");
        }

        private bool Validate()
        {
            if (!_tableNameRegex.IsMatch(_bank.Name))
            {
                Logger.Instance.Log($"Path is not a media database bank. Path: {_bank.FullName}");
                return false;
            }

            if (!File.Exists(Path.Combine(_bank.FullName, "config.xml")))
            {
                Logger.Instance.Log($"Expected to find 'config.xml' in {_bank.FullName}. If this file is missing, it may indicate an incorrect media database bank path. Path: {_bank.FullName}");
                return false;
            }

            return true;
        }

        private async Task EnumerateTables()
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (var table in _bank.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        Logger.Instance.Log($"Checking table '{table.Name}'");
                        if (IsTableDirty(table))
                            _dirtyTables.Add(table);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"An error occurred while enumerating tables in bank path. Error: {ex.Message}");
            }
            finally
            {
                _dirtyTables.CompleteAdding();
            }
        }

        private bool IsTableDirty(DirectoryInfo table)
        {
            try
            {
                if (!_tableNameRegex.IsMatch(table.Name))
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

        private async Task DeleteDirtyTables()
        {
            await Task.Run(() =>
            {
                foreach (var dirtyTable in _dirtyTables.GetConsumingEnumerable())
                {
                    if (_readOnly)
                    {
                        Logger.Instance.Log($"Table {dirtyTable.Name} would be deleted if not running with --listonly flag");
                        continue;
                    }
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
            });
        }
    }
}