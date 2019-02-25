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
using System.Threading;
using System.Threading.Tasks;

namespace ICSharpCore
{
    /// <summary>
    /// A 'kernel' is a program that runs and introspects the user's code. 
    /// https://jupyter-client.readthedocs.io/en/stable/kernels.html
    /// </summary>
    public class Kernel
    {
        private ConnInfo _conn;
        private string _shellAddress;
        private string _iopubAddress;
        private bool exit = false;

        public Kernel(ConnInfo conn)
        {
            _conn = conn;
            // https://netmq.readthedocs.io/en/latest/router-dealer/
            _shellAddress = $"@tcp://{conn.IP}:{conn.ShellPort}";
            _iopubAddress = $"@tcp://{conn.IP}:{conn.IOPubPort}";
        }

        public void Start()
        {
            
            // catch CTRL+C as exit command
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                exit = true;
            };
            
            using (var shell = new RouterSocket(_shellAddress))
            using (var iopub = new PublisherSocket(_iopubAddress))
            using (var poller = new NetMQPoller())
            {
                var iopubSender = new MessageSender(_conn.Key, iopub);
                var shellSender = new MessageSender(_conn.Key, shell);

                // Handler for messages coming in to the frontend
                shell.ReceiveReady += (s, e) =>
                {
                    var raw = e.Socket.ReceiveMultipartMessage();
                    var header = JsonConvert.DeserializeObject<Header>(raw[3].ConvertToString());
                    Console.WriteLine($"{header.MessageType}: [{raw.ToString()}]");

                    switch (header.MessageType)
                    {
                        case "kernel_info_request":
                            new KernelInfoHandler<KernelInfoRequest>(iopubSender, shellSender)
                                .Process(new Message<KernelInfoRequest>(header, raw));
                            break;
                        case "execute_request":
                            new ExecuteHandler<ExecuteRequest>(iopubSender, shellSender)
                                .Process(new Message<ExecuteRequest>(header, raw));
                            break;
                    }
                };

                poller.Add(shell);
                poller.RunAsync();

                // var heartbeat = new HeartBeat(conn);
                Console.WriteLine($"Listening Shell {_shellAddress}");
                Console.WriteLine($"Listening IOPub {_iopubAddress}");

                // hit CRTL+C to stop the while loop
                while (!exit)
                    Thread.Sleep(100);
            }
        }
    }
}
