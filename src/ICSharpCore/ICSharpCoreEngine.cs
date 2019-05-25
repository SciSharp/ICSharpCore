using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Jupyter.Core;

namespace ICSharpCore
{
    class ICSharpCoreEngine : BaseEngine
    {
        public ICSharpCoreEngine(IShellServer shell, IOptions<KernelContext> context, ILogger logger)
            : base(shell, context, logger)
        {

        }

        public override ExecutionResult ExecuteMundane(string input, IChannel channel)
        {
            throw new System.NotImplementedException();
        }
    }
}