using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole
{
    public readonly struct CommandLineAnalyzeResult
    {
        /// <summary>
        /// The list of possible commands that match the analyzed command line input.
        /// </summary>
        [MaybeNull]
        public IReadOnlyList<ConsoleCommandDesc> CandidateCommandDescs { get; }

        public CommandLineAnalyzeResult([DisallowNull] IReadOnlyList<ConsoleCommandDesc> candidateCommands)
        {
            CandidateCommandDescs = candidateCommands;
        }
    }
}