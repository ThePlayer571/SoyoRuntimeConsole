using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Soyo.SoyoRuntimeConsole
{
    public readonly struct Suggestion
    {
        public readonly struct CommandInfo
        {
            [NotNull] public string Name { get; }
            [AllowNull] public string HelpText { get; }
            [MaybeNull] public IReadOnlyList<IParameterHandler.Description> ParameterDescriptions { get; }
            public ConsoleCommandDesc AnalyzeResult { get; }

            public CommandInfo(
                [DisallowNull] string name, [AllowNull] string helpText,
                [AllowNull] IReadOnlyList<IParameterHandler.Description> parameterDescriptions,
                in ConsoleCommandDesc analyzeResult)
            {
                Name = name;
                HelpText = helpText;
                ParameterDescriptions = parameterDescriptions;
                AnalyzeResult = analyzeResult;
            }

            [return: NotNull]
            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(AnalyzeResult.Executable ? "✓ ": "✗ ");
                
                
                if (ParameterDescriptions == null || ParameterDescriptions.Count == 0)
                {
                    stringBuilder.Append(Name);
                }
                else
                {
                    stringBuilder.Append($"{Name} ");
                    stringBuilder.Append(string.Join(' ', ParameterDescriptions.Select(d => $"<{d.Name}: {d.Type}>")));
                }

                stringBuilder.Append($" - {HelpText}");

                return stringBuilder.ToString();
            }
        }

        public enum CompletionState
        {
            TypingCommandName,
            TypingParameters
        }


        [MaybeNull] IReadOnlyList<CommandInfo> CandidateCommands { get; }
        [MaybeNull] IReadOnlyList<string> CandidateParameters { get; }
        private CompletionState State { get; }

        public Suggestion(
            [AllowNull] IReadOnlyList<CommandInfo> candidateCommands,
            [AllowNull] IReadOnlyList<string> candidateParameters,
            CompletionState state)
        {
            CandidateCommands = candidateCommands;
            CandidateParameters = candidateParameters;
            State = state;
        }

        [return: NotNull]
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            // CompletionState
            stringBuilder.AppendLine($"CompletionState: {State.ToString()}");

            // CandidateCommands
            if (CandidateCommands == null || CandidateCommands.Count == 0)
            {
                stringBuilder.AppendLine("CandidateCommands: none");
            }
            else
            {
                stringBuilder.AppendLine($"CandidateCommands: ");
                foreach (var candidateCommand in CandidateCommands)
                {
                    stringBuilder.AppendLine($" - {candidateCommand.ToString()}");
                }
            }

            // CandidateParameters
            if (CandidateParameters == null || CandidateParameters.Count == 0)
            {
                stringBuilder.AppendLine("CandidateParameters: none");
            }
            else
            {
                stringBuilder.AppendLine($"CandidateParameters: {string.Join(", ", CandidateParameters)}");
            }

            return stringBuilder.ToString();
        }
    }
}