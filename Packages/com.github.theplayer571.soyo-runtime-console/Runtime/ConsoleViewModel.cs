using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 对IConsole的封装，提供View相关的接口。推荐View层不与IConsole直接交互，而是通过这个类。
    /// </summary>
    public class ConsoleViewModel : IDisposable
    {
        #region 对外接口

        /// <summary>
        /// 输入条目，绑定了unity的日志系统
        /// </summary>
        public event Action<LogEntry> OnLogEntry;

        /// <summary>
        /// 设置输入文本。
        /// </summary>
        /// <param name="text"></param>
        public void SetInputText(string text)
        {
            _console.SetInputText(text);
        }

        /// <summary>
        /// 发送输入，执行命令。发送后输入条会被清空。
        /// </summary>
        public void SendInput()
        {
            RecordHistory(InputText);
            _console.SendInput();
            _console.SetInputText(string.Empty);
        }

        /// <summary>
        /// 获取最近发送出去的命令历史。最多10条，按发送时间从新到旧排列，[0]是上一条发送的命令。
        /// </summary>
        public IReadOnlyList<string> GetHistory() => _history.AsReadOnly();

        /// <summary>
        /// 自动补全指定索引的候选参数。
        /// </summary>
        /// <param name="candidateIndex"></param>
        public bool AutoComplete(int candidateIndex = 0)
        {
            var suggestion = GetSuggestion();
            var candidateParameters = suggestion.CandidateParameters;

            // 提前退出
            if (candidateParameters == null || candidateIndex < 0 || candidateIndex >= candidateParameters.Count ||
                suggestion.CandidateCommands == null)
            {
                return false;
            }

            var chosenCandidateParameter = candidateParameters[candidateIndex];
            var referenceCommand = suggestion.CandidateCommands.FirstOrDefault(command =>
                command.CandidateParameters != null && command.CandidateParameters.Contains(chosenCandidateParameter));

            var commandStringBuilder = new StringBuilder();
            commandStringBuilder.Append(referenceCommand.AnalyzeResult.Definition.CommandName.Name);
            commandStringBuilder.Append(' ');

            for (int i = 0; i < referenceCommand.AnalyzeResult.Parameters.Count - 1; i++)
            {
                commandStringBuilder.Append(referenceCommand.AnalyzeResult.Parameters[i]);
            }

            commandStringBuilder.Append(chosenCandidateParameter);

            _console.SetInputText(commandStringBuilder.ToString());
            return true;
        }

        /// <summary>
        /// 当前输入文本。
        /// </summary>
        public string InputText => _console.InputText;

        /// <summary>
        /// 获取输入文本的建议。
        /// </summary>
        /// <returns></returns>
        public Suggestion GetSuggestion()
        {
            var isTypingCommandName = !InputText.Contains(' ');
            var result = _console.CommandLineAnalyzer.Analyze(InputText);


            if (isTypingCommandName)
            {
                var commandCandidates = result.CandidateCommandDescs?
                    .Select(desc =>
                    {
                        return new Suggestion.CommandInfo(
                            name: desc.Definition.CommandName.Name,
                            helpText: _console.CommandHelpText.GetValueOrDefault(desc.Definition.CommandName),
                            parameterDescriptions: desc.Definition.ParameterHandlers
                                .Select(handler => handler.GetDescription()).ToList(),
                            candidateParameters: null,
                            analyzeResult: desc
                        );
                    }).ToList();
                return new Suggestion(commandCandidates, Suggestion.CompletionState.TypingCommandName);
            }
            else // isEnteringParameters
            {
                var commandCandidates = result.CandidateCommandDescs?
                    .Select(desc =>
                    {
                        var candidateParameters = desc.Parameters.Count == 0
                            ? null
                            : desc.Definition.ParameterHandlers[desc.Parameters.Count - 1]
                                .GetCandidates(desc.Parameters[^1])?.ToList();

                        return new Suggestion.CommandInfo(
                            name: desc.Definition.CommandName.Name,
                            helpText: _console.CommandHelpText.GetValueOrDefault(desc.Definition.CommandName),
                            parameterDescriptions: desc.Definition.ParameterHandlers
                                .Select(handler => handler.GetDescription()).ToList(),
                            candidateParameters: candidateParameters,
                            analyzeResult: desc
                        );
                    }).ToList();

                return new Suggestion(commandCandidates, Suggestion.CompletionState.TypingParameters);
            }
        }

        #endregion

        private readonly IConsole _console;
        private readonly List<string> _history = new List<string>(10);

        private void RecordHistory(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _history.Insert(0, text);
            if (_history.Count > 10)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

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