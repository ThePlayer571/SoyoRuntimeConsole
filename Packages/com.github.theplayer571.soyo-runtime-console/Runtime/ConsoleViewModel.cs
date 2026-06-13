using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    public class ConsoleViewModel : IDisposable
    {
        #region 对外接口

        public event Action<LogEntry> OnLogEntry;

        public void SetInputText(string text)
        {
            _console.SetInputText(text);
        }

        public void SendInput()
        {
            _console.SendInput();
        }

        public string InputText => _console.InputText;

        public Suggestion GetSuggestion()
        {
            var isTypingCommandName = !InputText.Contains(' ');
            var result = _console.CommandLineAnalyzer.Analyze(InputText);

            var commandCandidates = result.CandidateCommandDescs?
                .Select(desc =>
                {
                    return new Suggestion.CommandInfo(
                        desc.Definition.CommandName.Name,
                        _console.CommandHelpText.GetValueOrDefault(desc.Definition.CommandName),
                        desc.Definition.ParameterHandlers.Select(handler => handler.GetDescription()).ToList(),
                        desc);
                }).ToList();

            if (isTypingCommandName)
            {
                return new Suggestion(commandCandidates, null, Suggestion.CompletionState.TypingCommandName);
            }
            else // isEnteringParameters
            {
                // 获取candidateParameters
                var candidateParameters =
                    from commandDesc in result.CandidateCommandDescs
                    // 只操作有参命令
                    where commandDesc.Parameters.Count > 0
                    from candidateParameterString in
                        // 获取最后一个参数的候选字符串
                        commandDesc.Definition.ParameterHandlers[commandDesc.Parameters.Count - 1]
                            .GetCandidates(commandDesc.Parameters[^1])
                    select candidateParameterString;

                return new Suggestion(
                    commandCandidates, candidateParameters.ToList(), Suggestion.CompletionState.TypingParameters);
            }
        }

        #endregion

        private readonly IConsole _console;

        public ConsoleViewModel() : this(new GlobalConsole())
        {
        }

        public ConsoleViewModel([DisallowNull] IConsole console)
        {
            _console = console;
            Application.logMessageReceived += HandleLog;
        }

        public void Dispose()
        {
            Application.logMessageReceived -= HandleLog;
        }


        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            var entry = new LogEntry(logString, stackTrace, type);
            OnLogEntry?.Invoke(entry);
        }
    }
}