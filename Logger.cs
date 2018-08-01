using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MediaDbCleaner
{
    internal class Logger
    {
        public static Logger Instance;

        private readonly BlockingCollection<object> _messageQueue = new BlockingCollection<object>();
        private readonly Task _queueProcessor;
        private FileStream _fileStream;

        private Logger(string path)
        {
            Path = path;
            _queueProcessor = Task.Run(async () => await ProcessMessageQueue());
        }

        public string Path { get; }

        private async Task ProcessMessageQueue()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Path))
                    _fileStream = File.Open(Path, FileMode.Append, FileAccess.Write, FileShare.Read);


                foreach (var message in _messageQueue.GetConsumingEnumerable())
                {
                    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";
                    Console.Write(line);
                    await TryWriteToLog(line, _fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                _fileStream?.Dispose();
            }
        }

        private async Task TryWriteToLog(string line, FileStream fileStream)
        {
            if (fileStream == null) return;
            try
            {
                var buffer = Encoding.UTF8.GetBytes(line);
                await _fileStream.WriteAsync(buffer, 0, buffer.Length);
                await _fileStream.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to write to log file. Error: {e.Message}");
            }
        }

        public void Log(object message)
        {
            _messageQueue.Add(message);
        }

        public void Close()
        {
            _messageQueue.CompleteAdding();
            _queueProcessor.Wait(10000);
        }

        public static void Init(string path)
        {
            if (Instance == null)
                Instance = new Logger(path);
        }

        ~Logger()
        {
            _messageQueue.CompleteAdding();
            _queueProcessor.Wait(10000);
        }
    }
}