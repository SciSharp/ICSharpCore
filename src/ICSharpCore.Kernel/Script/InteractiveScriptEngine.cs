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
using ICSharpCore.Protocols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ScriptLogLevel = Dotnet.Script.DependencyModel.Logging.LogLevel;

namespace ICSharpCore.Script
{
    public class InteractiveScriptEngine
    {
        private ScriptState<object> _scriptState;

        private ScriptOptions _scriptOptions;

        private InteractiveScriptGlobals _globals;

        private StringBuilder _interactiveOutput;

        private RuntimeDependencyResolver _runtimeDependencyResolver;

        private ILogger _logger;

        private string _currentDirectory;

        public InteractiveScriptEngine(string currentDir, ILogger logger)
        {
            _currentDirectory = currentDir;
            _logger = logger;

            _scriptOptions = CreateScriptOptions();

            _runtimeDependencyResolver = new RuntimeDependencyResolver((t) => (level, m, e) =>
            {
                logger.Log(MapLogLevel(level), m, e);
            }, true);

            _interactiveOutput = new StringBuilder();
            _globals = new InteractiveScriptGlobals(new StringWriter(_interactiveOutput), CSharpObjectFormatter.Instance);
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

        public async Task<object> ExecuteAsync(string statement)
        {
            statement = PrepareStatement(statement);

            if (_scriptState == null)
            {
                _scriptState = await CSharpScript.RunAsync("using Console = ICSharpCore.Script.FakeConsole;", _scriptOptions, globals: _globals);
                _scriptState = await _scriptState.ContinueWithAsync(statement, _scriptOptions);
            }
            else
            {
                _scriptState = await _scriptState.ContinueWithAsync(statement, _scriptOptions);
            }

            if (_scriptState.ReturnValue == null)
                return string.Empty;

            var displayData = _scriptState.ReturnValue as JObject;

            if (displayData != null)
                return displayData;

            _globals.Print(_scriptState.ReturnValue);

            var output = _interactiveOutput.ToString();
            _interactiveOutput.Clear();

            return new DisplayData(output);
        }

        public object Execute(string statement)
        {
            return ExecuteAsync(statement).Result;
        }

        private bool TryLoadReferenceFromScript(string statement)
        {
            if (!statement.StartsWith("#r ") && !statement.StartsWith("#load "))
                return false;

            var lineRuntimeDependencies = _runtimeDependencyResolver.GetDependenciesForCode(_currentDirectory, ScriptMode.REPL, new string[0], statement);
            var lineDependencies = lineRuntimeDependencies.SelectMany(rtd => rtd.Assemblies).Distinct();
            var scriptMap = lineRuntimeDependencies.ToDictionary(rdt => rdt.Name, rdt => rdt.Scripts);

            if (scriptMap.Count > 0)
            {
                _scriptOptions =
                    _scriptOptions.WithSourceResolver(
                        new NuGetSourceReferenceResolver(
                            new SourceFileResolver(ImmutableArray<string>.Empty, _currentDirectory), scriptMap));
            }

            foreach (var runtimeDependency in lineDependencies)
            {
                _logger.LogInformation("Adding reference to a runtime dependency => " + runtimeDependency);
                _scriptOptions = _scriptOptions.AddReferences(MetadataReference.CreateFromFile(runtimeDependency.Path));
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
