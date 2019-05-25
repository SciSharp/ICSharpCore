using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Jupyter.Core;
using static ICSharpCore.Constants;


namespace ICSharpCore
{
    public class Program
    {
        public static void Init(ServiceCollection serviceCollection) =>
            serviceCollection
                .AddSingleton<IExecutionEngine, ICSharpCoreEngine>();

        public static int Main(string[] args) {
            var app = new KernelApplication(
                PROPERTIES,
                Init
            );

            return app.WithDefaultCommands().Execute(args);
        }
    }
}
