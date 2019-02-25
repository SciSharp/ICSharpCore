using Dotnet.Script.Core;
using Dotnet.Script.Core.Commands;
using Dotnet.Script.DependencyModel.Logging;
using ICSharpCore.Kernels;
using ICSharpCore.Protocols;
using ICSharpCore.RequestHandlers;
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
        private InteractiveRunner runner;
        private ScriptConsole console;
        private int outputOffset;

        public ExecuteHandler(MessageSender ioPub, MessageSender shell)
        {
            this.ioPub = ioPub;
            this.shell = shell;

            GetExecuteInteractiveCommand();
        }

        public async void Process(Message<T> message)
        {
            await runner.Execute(message.Content.Code);

            var result = console.Out.ToString().Substring(outputOffset);
            outputOffset += result.Length;
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

        private void GetExecuteInteractiveCommand()
        {
            var reader = new StringReader(Environment.NewLine);
            var writer = new StringWriter();
            var error = new StringWriter();

            console = new ScriptConsole(writer, reader, error);

            var _logFactory = CreateLogFactory();
            var compiler = new ScriptCompiler(_logFactory, useRestoreCache: false);
            runner = new InteractiveRunner(compiler, _logFactory, console, new string[0]);
        }

        private static LogFactory CreateLogFactory()
        {
            return type => (level, message, exception) =>
            {
            };
        }
    }
}
