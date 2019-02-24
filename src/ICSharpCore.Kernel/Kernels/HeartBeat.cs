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
    /// This socket allows for simple bytestring messages to be sent between 
    /// the frontend and the kernel to ensure that they are still connected.
    /// </summary>
    public class HeartBeat
    {
        public HeartBeat(ConnInfo conn)
        {
            string heartbeatAddress = $"@tcp://{conn.IP}:{conn.HBPort}";
            using (var responseSocket = new ResponseSocket(heartbeatAddress))
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var message = responseSocket.ReceiveFrameString();

                        Console.WriteLine("ResponseSocket : Server Received '{0}'", message);

                        responseSocket.SendFrame("OK");
                        Console.WriteLine("Response OK.");

                        Thread.Sleep(1000);
                    }
                });
            }
        }
    }
}
