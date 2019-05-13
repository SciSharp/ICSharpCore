using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    /// <summary>
    /// This type of message is used to bring back data that 
    /// should be displayed (text, html, svg, etc.) in the frontends.
    /// https://jupyter-client.readthedocs.io/en/stable/messaging.html#display-data
    /// </summary>
    public class DisplayData
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        [JsonProperty("metadata")]
        public JObject MetaData { get; set; }

        [JsonProperty("transient")]
        public JObject Transient { get; set; }

        public DisplayData()
        {
            Source = string.Empty;
            Data = new JObject();
            MetaData = new JObject();
            Transient = new JObject();
        }

        public DisplayData(string text)
            : this(text, text)
        {
            
        }

        public DisplayData(string text, string html)
            : this()
        {
            Data = new JObject
            {
                { "text/plain", text },
                { "text/html", html }
            };
        }
    }
}
