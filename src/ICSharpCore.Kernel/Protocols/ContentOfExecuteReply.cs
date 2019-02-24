using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    /// <summary>
    /// https://jupyter-client.readthedocs.io/en/stable/messaging.html#execution-results
    /// </summary>
    public abstract class ContentOfExecuteReply
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// The global kernel counter that increases by one with each request that
        /// stores history.  This will typically be used by clients to display
        /// prompt numbers to the user.  If the request did not store history, this will
        /// be the current value of the counter in the kernel.
        /// </summary>
        [JsonProperty("execution_count")]
        public int ExecutionCount { get; set; }
    }
}
