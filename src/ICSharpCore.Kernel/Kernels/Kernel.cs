using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
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

            using (var server = new RouterSocket(serverAddress))
            using (var poller = new NetMQPoller())
            {
                // Handler for messages coming in to the frontend
                server.ReceiveReady += (s, e) =>
                {
                    var msg = e.Socket.ReceiveMultipartMessage();
                    Console.WriteLine($"Received: {msg.ToString()}");
                };

                poller.Add(server);
                poller.RunAsync();

                // hit CRTL+C to stop the while loop
                while (!exit)
                    Thread.Sleep(100);
            }
        }
    }
}
