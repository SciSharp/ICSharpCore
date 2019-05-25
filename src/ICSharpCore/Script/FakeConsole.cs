using System;
using ICSharpCore.Primitives;

namespace ICSharpCore.Script
{
    public static class FakeConsole
    {
        public static void WriteLine(string value)
        {
            OnLineReceived(value);
        }

        public static void WriteLine(string format, params object[] args)
        {
            OnLineReceived(string.Format(format, args));
        }

        public static void WriteLine(char[] buffer, int index, int count)
        {
            OnLineReceived(new string(buffer, index, count));
        }

        public static void WriteLine(char[] buffer)
        {
            OnLineReceived(new string(buffer));
        }

        public static void WriteLine(char value)
        {
            OnLineReceived(value.ToString());
        }

        private static void OnLineReceived(string line)
        {
            DisplayDataEmitter.EmitText(line);
        }
    }
}
