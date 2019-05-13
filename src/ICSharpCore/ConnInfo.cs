using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore
{
    /// <summary>
    /// The transport, ip and five _port fields specify five ports which the kernel should bind to using ZeroMQ.
    /// </summary>
    public class ConnInfo
    {
        /// <summary>
        /// for request/reply calls to the kernel.
        /// </summary>
        [JsonProperty("shell_port")]
        public int ShellPort { get; set; }

        /// <summary>
        /// for the kernel to publish results to frontends.
        /// </summary>
        [JsonProperty("iopub_port")]
        public int IOPubPort { get; set; }

        /// <summary>
        /// for frontends to reply to raw_input calls in the kernel.
        /// </summary>
        [JsonProperty("stdin_port")] 
        public int StdInPort { get; set; }

        [JsonProperty("control_port")]
        public int ControlPort { get; set; }

        /// <summary>
        /// for monitoring the kernel’s heartbeat.
        /// </summary>
        [JsonProperty("hb_port")]
        public int HBPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Used to cryptographically sign messages
        /// So that other users on the system can’t send code to run in this kernel
        /// </summary>
        public string Key { get; set; }

        public string Transport { get; set; }

        /// <summary>
        /// Used to cryptographically sign messages
        /// </summary>
        [JsonProperty("signature_scheme")]
        public string SignatureScheme { get; set; }

        [JsonProperty("kernel_name")]
        public string KernelName { get; set; }
    }
}
