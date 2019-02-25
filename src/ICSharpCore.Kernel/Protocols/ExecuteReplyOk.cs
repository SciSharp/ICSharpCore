using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    /// <summary>
    /// https://jupyter-client.readthedocs.io/en/stable/messaging.html#execution-results
    /// </summary>
    public class ExecuteReplyOk : ExecuteReply
    {
        public ExecuteReplyOk()
        {
            Status = Protocols.StatusType.Ok;
        }

        [JsonProperty("payload")]
        public List<Dictionary<string, string>> Payload { get; set; }

        [JsonProperty("user_expressions")]
        public Dictionary<string, string> UserExpressions { get; set; }
    }
}
