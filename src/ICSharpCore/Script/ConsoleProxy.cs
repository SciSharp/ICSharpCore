using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using ICSharpCore.Primitives;

namespace ICSharpCore.Script
{
    public class ConsoleProxy : IDisposable
    {
        private TextWriter _originalOut = Console.Out;

        private AnonymousPipeServerStream _consoleOutPipe;

        private Action<string> _consoleHandler;
        
        private StreamWriter _consoleWriter;

        private Task _readTask;

        public ConsoleProxy(Action<string> consoleHandler)
        {
            _consoleOutPipe = new AnonymousPipeServerStream(PipeDirection.Out);
            _consoleWriter = new StreamWriter(_consoleOutPipe);
            _consoleWriter.AutoFlush = true;

            _consoleHandler = consoleHandler;

            Console.SetOut(_consoleWriter);
        }

        public void StartRedirect()
        {
            _readTask = ReadLineAsync();
        }

        private async Task ReadLineAsync()
        {
            using (var consoleOutClientPipe = new AnonymousPipeClientStream(PipeDirection.In, _consoleOutPipe.ClientSafePipeHandle))
            using (var reader = new StreamReader(consoleOutClientPipe))
            {
                while (true)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (line != null)
                        OnLineReceived(line);

                    if (line == null || reader.EndOfStream)
                        break;
                }
            }            
        }

        private void OnLineReceived(string line)
        {
            _consoleHandler(line);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _consoleWriter?.Flush();
                    _consoleWriter?.Dispose();
                }

                Console.SetOut(_originalOut);

                _readTask?.Wait();
                _readTask = null;

                disposedValue = true;
            }            
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
