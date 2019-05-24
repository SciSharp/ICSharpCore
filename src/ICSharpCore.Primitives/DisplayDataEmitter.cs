using System;
using Newtonsoft.Json.Linq;

namespace ICSharpCore.Primitives
{
    public static class DisplayDataEmitter
    {
        public static Action<DisplayData> DisplayDataHandler { get; set; }

        public static void Emit(DisplayData data)
        {
            DisplayDataHandler?.Invoke(data);
        }

        public static void EmitHtml(string html)
        {
            Emit(new DisplayData
            {
                Data = new JObject
                {
                    { "text/html", html }
                }
            });
        }

        public static void EmitText(string text)
        {
            Emit(new DisplayData
            {
                Data = new JObject
                {
                    { "text/plain", text }
                }
            });
        }
    }
}
