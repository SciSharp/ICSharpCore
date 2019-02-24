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
        public Kernel(ConnInfo conn)
        {
            var exit = false;

            // catch CTRL+C as exit command
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                exit = true;
            };

            // https://netmq.readthedocs.io/en/latest/router-dealer/
            string serverAddress = $"@tcp://{conn.IP}:{conn.ShellPort}";
            string iopubAddress = $"@tcp://{conn.IP}:{conn.IOPubPort}";
            
            using (var server = new RouterSocket(serverAddress))
            using (var iopub = new PublisherSocket(iopubAddress))
            using (var poller = new NetMQPoller())
            {
                var iopubSender = new MessageSender(conn.Key, iopub);

                // Handler for messages coming in to the frontend
                server.ReceiveReady += (s, e) =>
                {
                    var raw = e.Socket.ReceiveMultipartMessage();
                    var header = JsonConvert.DeserializeObject<Header>(raw[3].ConvertToString());
                    Console.WriteLine($"{header.MessageType}: [{raw.ToString()}]");

                    switch (header.MessageType)
                    {
                        case "kernel_info_request":
                            new KernelInfoHandler<ContentOfKernelInfoRequest>(iopubSender)
                                .Process(new Message<ContentOfKernelInfoRequest>(header, raw));
                            break;
                        case "execute_request":
                            new ExecuteHandler<ContentOfExecuteRequest>(iopubSender)
                                .Process(new Message<ContentOfExecuteRequest>(header, raw));
                            break;
                    }
                };

                poller.Add(server);
                poller.RunAsync();

                // var heartbeat = new HeartBeat(conn);
                Console.WriteLine($"Listening Shell {serverAddress}");
                Console.WriteLine($"Listening IOPub {iopubAddress}");

                // hit CRTL+C to stop the while loop
                while (!exit)
                    Thread.Sleep(100);
            }
        }
    }
}
