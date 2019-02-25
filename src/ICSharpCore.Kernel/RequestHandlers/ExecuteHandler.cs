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

namespace ICSharpCore.RequestHandlers
{
    public class ExecuteHandler<T> : IRequestHandler<T> where T : ExecuteRequest
    {
        private MessageSender ioPub;
        private MessageSender shell;

        public ExecuteHandler(MessageSender ioPub, MessageSender shell)
        {
            this.ioPub = ioPub;
            this.shell = shell;
        }

        public async void Process(Message<T> message)
        {
            ioPub.Send(message, new Status { ExecutionState = StatusType.Busy }, MessageType.Status);

            var commands = new[]
            {
                message.Content.Code,
                "#exit"
            };

            var ctx = GetExecuteInteractiveCommand(commands);
            await ctx.Command.Execute(new ExecuteInteractiveCommandOptions(null, null, null));

            var result = ctx.Console.Out.ToString()
                .Split('\r', '\n')
                .FirstOrDefault()?
                .Substring(4);
            /*var content = new ContentOfExecuteReplyOk
            {
                ExecutionCount = 1,
                Payload = new List<Dictionary<string, string>>(),
                UserExpressions = new Dictionary<string, string>()
            };*/

            var content = new DisplayData
            {
                Data = new JObject
                {
                    {"text/plain", result},
                    { "text/html", result}
                }
            };
            ioPub.Send(message, content, MessageType.DisplayData);

            ioPub.Send(message, new Status { ExecutionState = StatusType.Idle }, MessageType.Status);
        }

        private (ExecuteInteractiveCommand Command, ScriptConsole Console) GetExecuteInteractiveCommand(string[] commands)
        {
            var reader = new StringReader(string.Join(Environment.NewLine, commands));
            var writer = new StringWriter();
            var error = new StringWriter();

            var console = new ScriptConsole(writer, reader, error);
            LogFactory logFactory = CreateLogFactory();
            return (new ExecuteInteractiveCommand(console, logFactory), console);
        }

        private static LogFactory CreateLogFactory()
        {
            return type => (level, message, exception) =>
            {
            };
        }
    }
}
