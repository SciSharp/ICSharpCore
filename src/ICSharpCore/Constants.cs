using Microsoft.Jupyter.Core;

namespace ICSharpCore
{

    internal static class Constants
    {
        internal static KernelProperties PROPERTIES = new KernelProperties
        {
            FriendlyName = "ICSharpCore",
            KernelName = "icsharpcore",
            KernelVersion = typeof(Program).Assembly.GetName().Version.ToString(),
            DisplayName = "ICSharpCore",

            LanguageName = "C#",
            LanguageVersion = "7.2",
            LanguageMimeType = "text/plain",
            LanguageFileExtension = ".cs",

            Description = "Runs C# using Roslyn within Jupyter Notebook."
        };
    }
}