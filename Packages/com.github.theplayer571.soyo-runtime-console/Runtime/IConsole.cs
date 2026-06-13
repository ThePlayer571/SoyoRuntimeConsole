using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole
{
    public interface IConsole
    {
        void SetInputText(string text);
        bool SendInput(int chosenCommandIndex = 0);
        string InputText { get; }
        [NotNull] IReadOnlyList<ConsoleCommandDefinition> Commands { get; }
        [NotNull] IReadOnlyDictionary<CommandName, string> CommandHelpText { get; }
        [NotNull] CommandLineAnalyzer CommandLineAnalyzer { get; }
    }
}