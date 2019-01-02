using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    public class ContentOfExecuteRequest
    {
        public string Code { get; set; }

        public bool Silent { get; set; }

        [JsonProperty("store_history")]
        public bool StoreHistory { get; set; }

        [JsonProperty("user_expressions")]
        public JObject UserExpressions { get; set; }

        [JsonProperty("allow_stdin")]
        public bool AllowStdin { get; set; }

        [JsonProperty("stop_on_error")]
        public bool StopOnError { get; set; }
    }
}
