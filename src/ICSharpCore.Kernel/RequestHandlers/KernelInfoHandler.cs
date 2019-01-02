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
    public class KernelInfoHandler
    {
        private string key;
        private PublisherSocket iopub;
        private RouterSocket server;

        public KernelInfoHandler(string key, RouterSocket server, PublisherSocket iopub)
        {
            this.key = key;
            this.server = server;
            this.iopub = iopub;
        }

        public void Process(Message<ContentOfKernelInfoRequest> message)
        {
            var msgSender = new MessageSender(key, iopub);
            msgSender.Send(message, new ContentOfStatus { ExecutionState = Status.Busy }, MessageType.Status);

            msgSender.Send(message, new ContentOfKernelInfoReply(), MessageType.KernelInfoReply);

            msgSender.Send(message, new ContentOfStatus { ExecutionState = Status.Idle }, MessageType.Status);
        }
    }
}
