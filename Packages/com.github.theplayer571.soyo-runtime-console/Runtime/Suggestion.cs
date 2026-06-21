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
            [MaybeNull] public IReadOnlyList<string> Candidates { get; }


            public ConsoleCommandDesc AnalyzeResult { get; }

            public CommandInfo(
                [DisallowNull] string name, [AllowNull] string helpText,
                [AllowNull] IReadOnlyList<IParameterHandler.Description> parameterDescriptions,
                [AllowNull] IReadOnlyList<string> candidates,
                in ConsoleCommandDesc analyzeResult)
            {
                Name = name;
                HelpText = helpText;
                ParameterDescriptions = parameterDescriptions;
                Candidates = candidates;
                AnalyzeResult = analyzeResult;
            }

            [return: NotNull]
            public override string ToString()
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(AnalyzeResult.Executable ? "✓" : "✗");

                stringBuilder.Append(AnalyzeResult.Parameters.Count + " ");


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
                
                // 不需要显示参数
                // stringBuilder.AppendLine();
                // stringBuilder.Append($"    - {string.Join(',', Candidates ?? new List<string>())}");

                return stringBuilder.ToString();
            }
        }

        public enum CompletionState
        {
            TypingCommandName,
            TypingParameters
        }

        [MaybeNull] public IReadOnlyList<CommandInfo> CandidateCommands { get; }
        [MaybeNull] public IReadOnlyList<string> Candidates { get; }
        public CompletionState State { get; }

        public Suggestion(
            [AllowNull] IReadOnlyList<CommandInfo> candidateCommands,
            CompletionState state)
        {
            CandidateCommands = candidateCommands;
            State = state;

            if (candidateCommands == null)
            {
                Candidates = null;
            }
            else
            {
                Candidates = candidateCommands
                    .SelectMany(commandInfo => commandInfo.Candidates ?? Enumerable.Empty<string>())
                    .Distinct().ToList();
            }
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
            
            // Candidates
            if (Candidates == null || Candidates.Count == 0)
            {
                stringBuilder.AppendLine("Candidates: none");
            }
            else
            {
                stringBuilder.AppendLine($"Candidates: {string.Join(" || ", Candidates)}");
            }

            return stringBuilder.ToString();
        }
    }
}