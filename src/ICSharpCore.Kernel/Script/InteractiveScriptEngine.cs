using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace ICSharpCore.Script
{
    public class InteractiveScriptEngine
    {
        private ScriptState<object> scriptState;

        private ScriptOptions scriptOptions;

        public async Task<object> ExecuteAsync(string statement)
        {
            if (scriptOptions == null)
            {
                scriptOptions = CreateScriptOptions();
                statement = PrepareStatement(statement, scriptOptions);
            }

            if (scriptState == null)
            {
                scriptState = await CSharpScript.RunAsync(statement);
            }
            else
            {
                scriptState = await scriptState.ContinueWithAsync(statement);
            }

            return scriptState.ReturnValue;
        }

        public object Execute(string statement)
        {
            return ExecuteAsync(statement).Result;
        }

        private string PrepareStatement(string statement, ScriptOptions scriptOptions)
        {
            return statement;
        }

        private ScriptOptions CreateScriptOptions()
        {
            var dir = AppContext.BaseDirectory;

            var options = ScriptOptions.Default;
            options = AddDefaultImports(options);
            return options;
        }

        private ScriptOptions AddDefaultImports(ScriptOptions scriptOptions)
        {
            return scriptOptions.AddImports(new [] {
                "System",
                "System.IO",
                "System.Collections.Generic",
                "System.Console",
                "System.Diagnostics",
                "System.Dynamic",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Text",
                "System.Threading.Tasks"
            });
        }
    }
}
