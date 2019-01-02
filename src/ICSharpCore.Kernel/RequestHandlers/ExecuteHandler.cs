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
    public class ExecuteHandler
    {
        private string key;
        private PublisherSocket iopub;
        private RouterSocket server;

        public ExecuteHandler(string key, RouterSocket server, PublisherSocket iopub)
        {
            this.key = key;
            this.server = server;
            this.iopub = iopub;
        }

        public void Process(Message<ContentOfKernelInfoRequest> message)
        {
            
        }
    }
}
