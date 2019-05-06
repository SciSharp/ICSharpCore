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

        public async void Process(Message<T> message)
        {
            string result = null;

            try
            {
                result = await scriptEngine.ExecuteAsync(message.Content.Code);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to run the code: " + message.Content.Code, e);
                return;
            }            

            if (string.IsNullOrEmpty(result))
            {
                return;
            }

            // send execute result message to IOPub
            var content = new DisplayData
            {
                Data = new JObject
                {
                    {"text/plain", result},
                    {"text/html", result}
                }
            };
            ioPub.Send(message, content, MessageType.DisplayData);

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
