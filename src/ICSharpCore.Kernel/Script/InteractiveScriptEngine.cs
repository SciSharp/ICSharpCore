using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dotnet.Script.DependencyModel.Context;
using Dotnet.Script.DependencyModel.NuGet;
using Dotnet.Script.DependencyModel.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.Extensions.Logging;
using ScriptLogLevel = Dotnet.Script.DependencyModel.Logging.LogLevel;

namespace ICSharpCore.Script
{
    public class InteractiveScriptEngine
    {
        private ScriptState<object> scriptState;

        private ScriptOptions scriptOptions;

        private InteractiveScriptGlobals globals;

        private StringBuilder interactiveOutput;

        private RuntimeDependencyResolver runtimeDependencyResolver;

        private ILogger logger;

        private string currentDirectory;

        public InteractiveScriptEngine(string currentDir, ILogger logger)
        {
            this.currentDirectory = currentDir;
            this.logger = logger;

            this.scriptOptions = CreateScriptOptions();

            this.runtimeDependencyResolver = new RuntimeDependencyResolver((t) => (level, m, e) =>
            {
                logger.Log(MapLogLevel(level), m, e);
            }, true);

            interactiveOutput = new StringBuilder();
            globals = new InteractiveScriptGlobals(new StringWriter(interactiveOutput), CSharpObjectFormatter.Instance);
        }

        private LogLevel MapLogLevel(ScriptLogLevel logLevel)
        {
            switch (logLevel)
            {
                case (ScriptLogLevel.Critical):
                    return LogLevel.Critical;
                case (ScriptLogLevel.Error):
                    return LogLevel.Error;
                case (ScriptLogLevel.Warning):
                    return LogLevel.Warning;
                case (ScriptLogLevel.Trace):
                    return LogLevel.Trace;
                case (ScriptLogLevel.Debug):
                    return LogLevel.Debug;
                default:
                    return LogLevel.Information;
            }
        }

        public async Task<string> ExecuteAsync(string statement)
        {
            statement = PrepareStatement(statement);

            if (scriptState == null)
            {
                scriptState = await CSharpScript.RunAsync("using Console = ICSharpCore.Script.FakeConsole;", scriptOptions, globals: globals);
                scriptState = await scriptState.ContinueWithAsync(statement, scriptOptions);
            }
            else
            {
                scriptState = await scriptState.ContinueWithAsync(statement, scriptOptions);
            }

            if (scriptState.ReturnValue == null)
                return string.Empty;

            globals.Print(scriptState.ReturnValue);

            var output = interactiveOutput.ToString();
            interactiveOutput.Clear();

            return output;
        }

        public string Execute(string statement)
        {
            return ExecuteAsync(statement).Result;
        }

        private bool TryLoadReferenceFromScript(string statement)
        {
            if (!statement.StartsWith("#r ") && !statement.StartsWith("#load "))
                return false;

            var lineRuntimeDependencies = runtimeDependencyResolver.GetDependencies(currentDirectory, ScriptMode.REPL, new string[0], statement);
            var lineDependencies = lineRuntimeDependencies.SelectMany(rtd => rtd.Assemblies).Distinct();
            var scriptMap = lineRuntimeDependencies.ToDictionary(rdt => rdt.Name, rdt => rdt.Scripts);

            if (scriptMap.Count > 0)
            {
                scriptOptions =
                    scriptOptions.WithSourceResolver(
                        new NuGetSourceReferenceResolver(
                            new SourceFileResolver(ImmutableArray<string>.Empty, currentDirectory), scriptMap));
            }

            foreach (var runtimeDependency in lineDependencies)
            {
                logger.LogInformation("Adding reference to a runtime dependency => " + runtimeDependency);
                scriptOptions = scriptOptions.AddReferences(MetadataReference.CreateFromFile(runtimeDependency.Path));
            }

            return true;
        }

        private string PrepareStatement(string statement)
        {
            TryLoadReferenceFromScript(statement);
            return statement;
        }

        private ScriptOptions CreateScriptOptions()
        {
            var dir = AppContext.BaseDirectory;

            var options = ScriptOptions.Default;
            options = AddDefaultImports(options);
            
            var mscorlib = typeof(object).GetTypeInfo().Assembly;
            var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;

            var references = new[]
                {
                    mscorlib,
                    systemCore,
                    Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),// System.Code
                    Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),// Microsoft.CSharp
                    Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)),// System.Dynamic
                    Assembly.GetAssembly(typeof(FakeConsole)) //ICSharpCore
                };

            options = options.AddReferences(references);

            return options;
        }

        private ScriptOptions AddDefaultImports(ScriptOptions scriptOptions)
        {
            var workingDir = AppContext.BaseDirectory;

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
            }).WithSourceResolver(new SourceFileResolver(ImmutableArray<string>.Empty, workingDir))
                .WithMetadataResolver(new NuGetMetadataReferenceResolver(ScriptMetadataResolver.Default.WithBaseDirectory(workingDir)))
                .WithEmitDebugInformation(true)
                .WithFileEncoding(Encoding.UTF8);
        }
    }
}
