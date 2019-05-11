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
        private MessageSender ioPub;
        private MessageSender shell;
        private int executionCount = 0;
        private InteractiveScriptEngine scriptEngine;
        private ILogger logger;

        public ExecuteHandler(MessageSender ioPub, MessageSender shell, ILoggerFactory loggerFactory)
        {
            this.ioPub = ioPub;
            this.shell = shell;
            this.scriptEngine = new InteractiveScriptEngine(AppContext.BaseDirectory, loggerFactory.CreateLogger(nameof(InteractiveScriptEngine)));
            this.logger = loggerFactory.CreateLogger(nameof(ExecuteHandler<T>));
        }

        private void SendErrorMessage(Message<T> message, string error)
        {
            var content = new DisplayData
            {
                Data = new JObject
                {
                    { "text/plain", error},
                    { "text/html", $"<p style=\"color:red;\">{error}</p>"}
                }
            };

            ioPub.Send(message, content, MessageType.DisplayData);
        }

        private void SendDisplayData(Message<T> message, string text)
        {
            // send execute result message to IOPub
            var content = new DisplayData(text);
            ioPub.Send(message, content, MessageType.DisplayData);
        }

        public async void Process(Message<T> message)
        {
            object result = null;

            try
            {
                FakeConsole.LineHandler = (line) =>
                {
                    ioPub.Send(message, new DisplayData(line), MessageType.DisplayData);
                };

                result = await scriptEngine.ExecuteAsync(message.Content.Code);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to run the code: " + message.Content.Code);

                var error = e.Message + Environment.NewLine + e.StackTrace;
                ioPub.Send(message, new DisplayData(error, $"<p style=\"color:red;\">{error}</p>"), MessageType.DisplayData);
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

            ioPub.Send(message, result, MessageType.DisplayData);

            // send execute reply to shell socket
            var executeReply = new ExecuteReplyOk
            {
                ExecutionCount = executionCount++,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };

            shell.Send(message, executeReply, MessageType.ExecuteReply);
        }
    }
}
