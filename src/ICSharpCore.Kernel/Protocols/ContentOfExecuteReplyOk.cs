using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    /// <summary>
    /// https://jupyter-client.readthedocs.io/en/stable/messaging.html#execution-results
    /// </summary>
    public class ContentOfExecuteReplyOk : ContentOfExecuteReply
    {
        public ContentOfExecuteReplyOk()
        {
            Status = Protocols.Status.Ok;
        }

        [JsonProperty("payload")]
        public List<Dictionary<string, string>> Payload { get; set; }

        [JsonProperty("user_expressions")]
        public Dictionary<string, string> UserExpressions { get; set; }
    }
}
