using ICSharpCore.Kernels;
using ICSharpCore.Protocols;
using ICSharpCore.RequestHandlers;
using ICSharpCore.Script;
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

        public ExecuteHandler(MessageSender ioPub, MessageSender shell)
        {
            this.ioPub = ioPub;
            this.shell = shell;
            this.scriptEngine = new InteractiveScriptEngine();
        }

        public async void Process(Message<T> message)
        {
            var executeResult = await scriptEngine.ExecuteAsync(message.Content.Code);

            if (executeResult == null)
            {
                // nothing?
                return;
            }

            var result = executeResult.ToString();

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
