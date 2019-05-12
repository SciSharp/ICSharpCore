using ICSharpCore.Kernels;
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

        public async void Process(Message<T> message)
        {
            object result = null;

            try
            {
                FakeConsole.LineHandler = (line) =>
                {
                    _ioPub.Send(message, new DisplayData(line), MessageType.DisplayData);
                };

                result = await _scriptEngine.ExecuteAsync(message.Content.Code);
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
                FakeConsole.LineHandler = null;
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
