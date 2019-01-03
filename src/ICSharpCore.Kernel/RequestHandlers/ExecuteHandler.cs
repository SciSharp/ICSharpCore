using ICSharpCore.Kernels;
using ICSharpCore.Protocols;
using ICSharpCore.RequestHandlers;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ICSharpCore.RequestHandlers
{
    public class ExecuteHandler<T> : IRequestHandler<T> where T : ContentOfExecuteRequest
    {
        private MessageSender sender;

        public ExecuteHandler(MessageSender sender)
        {
            this.sender = sender;
        }

        public void Process(Message<T> message)
        {
            sender.Send(message, new ContentOfStatus { ExecutionState = Status.Busy }, MessageType.Status);

            sender.Send(message, new ContentOfKernelInfoReply(), MessageType.KernelInfoReply);

            sender.Send(message, new ContentOfStatus { ExecutionState = Status.Idle }, MessageType.Status);
        }
    }
}
