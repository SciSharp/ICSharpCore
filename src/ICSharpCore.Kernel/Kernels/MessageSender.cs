using ICSharpCore.Protocols;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ICSharpCore.Kernels
{
    public class MessageSender
    {
        private string key;
        private PublisherSocket iopub;

        public MessageSender(string key, PublisherSocket iopub)
        {
            this.key = key;
            this.iopub = iopub;
        }

        public bool Send<T, C>(Message<T> request, C content,  string msgType)
        {
            var ioPubMessage = new Message<C>
            {
                Identities = request.Identities,
                Delimiter = request.Delimiter,
                ParentHeader = request.Header,
                Header = new Header()
                {
                    UserName = request.Header.UserName,
                    Session = request.Header.Session,
                    Date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    MessageId = Guid.NewGuid().ToString(),
                    MessageType = msgType,
                    Version = request.Header.Version
                },
                Metadata = request.Metadata,
                Content = content
            };

            Console.WriteLine($"{msgType}: [{JsonConvert.SerializeObject(ioPubMessage.Content)}]");

            var encoder = new UTF8Encoding();
            List<string> messages = new List<string>();
            var signature = Sign(key, ioPubMessage, messages, iopub);

            // send
            foreach (var id in request.Identities)
            {
                iopub.TrySendFrame(encoder.GetBytes(id), true);
            }
            iopub.SendFrame(ioPubMessage.Delimiter, true);
            iopub.SendFrame(signature, true);
            for (int i = 0; i < messages.Count; i++)
            {
                iopub.SendFrame(messages[i], i < messages.Count - 1);
            }

            return true;
        }

        private string Sign<T>(string key, Message<T> ioPubMessage, List<string> messages, PublisherSocket iopub)
        {
            var encoder = new UTF8Encoding();
            var hMAC = new HMACSHA256(encoder.GetBytes(key));
            hMAC.Initialize();

            // https://jupyter-client.readthedocs.io/en/stable/messaging.html#the-wire-protocol
            
            messages.Add(JsonConvert.SerializeObject(ioPubMessage.Header));
            messages.Add(JsonConvert.SerializeObject(ioPubMessage.ParentHeader));
            messages.Add(JsonConvert.SerializeObject(ioPubMessage.Metadata));
            messages.Add(JsonConvert.SerializeObject(ioPubMessage.Content));

            // signature
            foreach (string item in messages)
            {
                var sourceBytes = encoder.GetBytes(item);
                hMAC.TransformBlock(sourceBytes, 0, sourceBytes.Length, null, 0);
            }

            hMAC.TransformFinalBlock(new byte[0], 0, 0);
            return BitConverter.ToString(hMAC.Hash).Replace("-", "").ToLower();
        }
    }
}
