using ICSharpCore.Kernels;
using ICSharpCore.Primitives;
using ICSharpCore.Protocols;
using ICSharpCore.RequestHandlers;
using ICSharpCore.Script;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICSharpCore.RequestHandlers
{
    public class ExecuteHandler<T> : IRequestHandler<T> where T : ExecuteRequest
    {
        private MessageSender _ioPub;
        private MessageSender _shell;
        private int _executionCount = 0;
        private InteractiveScriptEngine _scriptEngine;
        private ILogger _logger;

        public ExecuteHandler(MessageSender ioPub, MessageSender shell, ILoggerFactory loggerFactory)
        {
            this._ioPub = ioPub;
            this._shell = shell;
            this._scriptEngine = new InteractiveScriptEngine(AppContext.BaseDirectory, loggerFactory.CreateLogger(nameof(InteractiveScriptEngine)));
            this._logger = loggerFactory.CreateLogger(nameof(ExecuteHandler<T>));
        }

        private ConsoleProxy CreateConsoleProxy(Action<DisplayData> displayDataHandler)
        {
            return new ConsoleProxy((line) =>
            {
                var data = new DisplayData
                {
                    Data = new JObject
                    {
                        { "text/plain", line }
                    }
                };

                displayDataHandler(data);
            });
        }

        public async void Process(Message<T> message)
        {
            object result = null;

            try
            {
                var displayDataHandler = new Action<DisplayData>((data) =>
                {
                    _ioPub.Send(message, data, MessageType.DisplayData);
                });

                DisplayDataEmitter.DisplayDataHandler = displayDataHandler;

                using (var consoleProxy = CreateConsoleProxy(displayDataHandler))
                {
                    consoleProxy.StartRedirect();
                    result = await _scriptEngine.ExecuteAsync(message.Content.Code);
                }                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to run the code: " + message.Content.Code);

                var error = e.Message + Environment.NewLine + e.StackTrace;
                _ioPub.Send(message, new DisplayData(error, $"<p style=\"color:red;\">{error}</p>"), MessageType.DisplayData);
                return;
            }
            finally
            {
                DisplayDataEmitter.DisplayDataHandler = null;
            }

            if (result == null)
            {
                return;
            }

            _ioPub.Send(message, result, MessageType.DisplayData);

            // send execute reply to shell socket
            var executeReply = new ExecuteReplyOk
            {
                ExecutionCount = _executionCount++,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            _shell.Send(message, executeReply, MessageType.ExecuteReply);
        }
    }
}
