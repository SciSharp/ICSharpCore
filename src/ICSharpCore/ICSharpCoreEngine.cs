using System;
using ICSharpCore.Script;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Jupyter.Core;

namespace ICSharpCore
{
    class ICSharpCoreEngine : BaseEngine
    {
        private InteractiveScriptEngine _scriptEngine;
        
        public ICSharpCoreEngine(IShellServer shell, IOptions<KernelContext> context, ILogger logger)
            : base(shell, context, logger)
        {
            _scriptEngine = new InteractiveScriptEngine(AppContext.BaseDirectory, logger);
        }

        public override ExecutionResult ExecuteMundane(string input, IChannel channel)
        {
            try
            {
                var result = _scriptEngine.Execute(input);

                return new ExecutionResult
                    {
                        Status = ExecuteStatus.Ok,
                        Output = result
                    };
            }
            catch (Exception e)
            {
                return new ExecutionResult
                {
                    Status = ExecuteStatus.Error,
                    Output = e
                };
            }
        }
    }
}