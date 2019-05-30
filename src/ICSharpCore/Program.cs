using ICSharpCore.Script;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ICSharpCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // We introduced DependencyInjection only for logging;
            // It is not needed in .NET Core 3.0
            IServiceCollection serviceCollection = new ServiceCollection();            
            serviceCollection.AddLogging(builder => builder.AddConsole());
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Connection files

            // When Jupyter starts a kernel, it passes it a connection file.
            // This specifies how to set up communications with the frontend.
            Console.WriteLine("Kernel connecting...");
            for (int i = 0; i < args.Length; i++)
                Console.WriteLine($"arg {i}: {args[i]}");

            // Create the connection model
            string json = File.ReadAllText(args[0]);
            var connInfo = JsonConvert.DeserializeObject<ConnInfo>(json);
            Console.WriteLine(JsonConvert.SerializeObject(connInfo));

            if (args.Length > 1)
            {
                InteractiveScriptEngine.RefsFilePath = args[1];
            }

            // Handling messages

            // After reading the connection file and binding to the necessary sockets, 
            // the kernel should go into an event loop, 
            // listening on the hb (heartbeat), 
            // control and shell sockets.
            var kernel = new Kernel(connInfo, loggerFactory);
            kernel.Start();
        }
    }
}
