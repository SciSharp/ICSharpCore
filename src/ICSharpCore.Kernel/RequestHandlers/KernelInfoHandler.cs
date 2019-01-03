using ICSharpCore.Kernels;
using ICSharpCore.Protocols;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ICSharpCore.RequestHandlers
{
    public class KernelInfoHandler<T> : IRequestHandler<T> where T : ContentOfKernelInfoRequest
    {
        private MessageSender sender;

        public KernelInfoHandler(MessageSender sender)
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
