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
    public class KernelInfoHandler<T> : IRequestHandler<T> where T : KernelInfoRequest
    {
        private MessageSender ioPub;
        private MessageSender shell;

        public KernelInfoHandler(MessageSender ioPub, MessageSender shell)
        {
            this.ioPub = ioPub;
            this.shell = shell;
        }

        public void Process(Message<T> message)
        {
            shell.Send(message, new KernelInfoReply(), MessageType.KernelInfoReply);
        }
    }
}
