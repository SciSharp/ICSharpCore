using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCore.Protocols
{
    public class ContentOfLanguageInfo
    {
        [JsonProperty("file_extension")]
        public string FileExtension { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
        [JsonProperty("pygments_lexer")]
        public string PygmentsLexer { get; set; }
        public string Version { get; set; }

        public ContentOfLanguageInfo()
        {
            FileExtension = ".cs";
            MimeType = "text/x-csharp";
            Name = ".netstandard";
            PygmentsLexer = "C#";
            Version = "2.0";
        }
    }
}
