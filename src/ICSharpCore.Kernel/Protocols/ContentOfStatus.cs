using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    public class ContentOfStatus
    {
        [JsonProperty("execution_state")]
        public string ExecutionState { get; set; }
    }
}
