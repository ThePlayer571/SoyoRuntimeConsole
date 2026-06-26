using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using ConsoleKey = Soyo.SoyoRuntimeConsole.ValueObjects.ConsoleKey;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 对IConsole的封装，提供View相关的接口。推荐View层不与IConsole直接交互，而是通过这个类。
    /// </summary>
    public class ConsoleViewModel : IDisposable
    {
        #region 事件

        /// <summary>
        /// 输出条目，绑定了unity的日志系统
        /// </summary>
        public event Action<LogEntry> OnLogEntry;

        #endregion

        #region 输入文本

        /// <summary>
        /// 当前输入文本。
        /// </summary>
        [NotNull]
        public string InputText => _console.InputText;

        /// <summary>
        /// 设置输入文本。
        /// </summary>
        /// <param name="text"></param>
        public virtual void SetInputText([DisallowNull] string text)
        {
            CandidateIndex = 0;
            _console.SetInputText(text);
            _suggestionCache = null;
        }

        #endregion

        #region 发送输入

        /// <summary>
        /// 发送输入，执行命令。发送后输入条会被清空。
        /// </summary>
        public virtual void SendInput()
        {
            var inputText = _console.InputText;
            RecordHistory(InputText);       
            var success = _console.SendInput();
            _console.SetInputText(string.Empty);
            _suggestionCache = null;

            if (!success)
            {
                Debug.LogWarning($"Console: Failed to send input: '{inputText}'");
            }
        }

        #endregion

        #region AutoComplete

        /// <summary>
        /// 自动补全的候选索引。设置此值后调用 <see cref="AutoComplete"/> 会使用该索引对应的候选进行补全。
        /// </summary>
        public int CandidateIndex { get; set; }

        /// <summary>
        /// 将 <see cref="CandidateIndex"/> 安全地向前移动（+1）。超出范围时会回绕到 0。
        /// </summary>
        public virtual void MoveCandidateNext()
        {
            var suggestion = GetSuggestion();
            var count = suggestion.Candidates?.Count ?? 0;
            if (count > 0)
            {
                CandidateIndex = (CandidateIndex + 1) % count;
            }
        }

        /// <summary>
        /// 将 <see cref="CandidateIndex"/> 安全地向后移动（-1）。超出范围时会回绕到末尾。
        /// </summary>
        public virtual void MoveCandidatePrevious()
        {
            var suggestion = GetSuggestion();
            var count = suggestion.Candidates?.Count ?? 0;
            if (count > 0)
            {
                CandidateIndex = (CandidateIndex - 1 + count) % count;
            }
        }

        /// <summary>
        /// 使用 <see cref="CandidateIndex"/> 对应的候选进行自动补全。
        /// </summary>
        /// <returns>补全是否成功。</returns>
        /// <remarks>
        /// 调用此方法会修改 <see cref="InputText"/>。如果你自定义了 UI，记得在调用后更新输入框的显示文本。
        /// </remarks>
        public virtual bool AutoComplete()
        {
            var suggestion = GetSuggestion();
            var candidateIndex = CandidateIndex;

            // 提前退出
            var candidates = suggestion.Candidates;
            if (candidates == null || candidateIndex < 0 || candidateIndex >= candidates.Count ||
                suggestion.CandidateCommands == null)
            {
                return false;
            }

            var chosenCandidate = candidates[candidateIndex];

            switch (suggestion.State)
            {
                case Suggestion.CompletionState.TypingCommandName:
                {
                    // 补全命令名
                    _console.SetInputText(chosenCandidate + " ");
                    return true;
                }
                case Suggestion.CompletionState.TypingParameters:
                {
                    var referenceCommand = suggestion.CandidateCommands.FirstOrDefault(command =>
                        command.Candidates != null && command.Candidates.Contains(chosenCandidate));

                    var commandStringBuilder = new StringBuilder();
                    commandStringBuilder.Append(referenceCommand.AnalyzeResult.Definition.CommandName.Name);
                    commandStringBuilder.Append(' ');

                    for (int i = 0; i < referenceCommand.AnalyzeResult.Parameters.Count - 1; i++)
                    {
                        commandStringBuilder.Append(referenceCommand.AnalyzeResult.Parameters[i]);
                    }

                    commandStringBuilder.Append(chosenCandidate);

                    _console.SetInputText(commandStringBuilder.ToString());
                    return true;
                }
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取自动补全后的完整命令文本，但不修改 <see cref="InputText"/>。
        /// 可用于在 UI 中预览补全结果。
        /// </summary>
        /// <returns>补全后的文本；如果无法补全则返回 null。</returns>
        [return: MaybeNull]
        public virtual string GetAutoCompleteText()
        {
            var suggestion = GetSuggestion();

            var candidates = suggestion.Candidates;
            if (candidates == null || CandidateIndex < 0 || CandidateIndex >= candidates.Count ||
                suggestion.CandidateCommands == null)
            {
                return null;
            }

            var chosenCandidate = candidates[CandidateIndex];

            switch (suggestion.State)
            {
                case Suggestion.CompletionState.TypingCommandName:
                    return chosenCandidate + " ";
                case Suggestion.CompletionState.TypingParameters:
                {
                    var referenceCommand = suggestion.CandidateCommands.FirstOrDefault(command =>
                        command.Candidates != null && command.Candidates.Contains(chosenCandidate));

                    var commandStringBuilder = new StringBuilder();
                    commandStringBuilder.Append(referenceCommand.AnalyzeResult.Definition.CommandName.Name);
                    commandStringBuilder.Append(' ');

                    for (int i = 0; i < referenceCommand.AnalyzeResult.Parameters.Count - 1; i++)
                    {
                        commandStringBuilder.Append(referenceCommand.AnalyzeResult.Parameters[i]);
                    }

                    commandStringBuilder.Append(chosenCandidate);

                    return commandStringBuilder.ToString();
                }
                default:
                    return null;
            }
        }

        #endregion

        #region Suggestion

        /// <summary>
        /// 获取输入文本的建议。结果会被缓存——如果 <see cref="InputText"/> 没有变化则直接返回上次的计算结果。
        /// </summary>
        /// <returns></returns>
        public virtual Suggestion GetSuggestion()
        {
            var currentInput = InputText;

            // 如果 InputText 没变且有缓存，直接返回缓存
            if (_suggestionCache.HasValue && currentInput == _lastInputTextForSuggestion)
            {
                return _suggestionCache.Value;
            }

            _lastInputTextForSuggestion = currentInput;

            var isTypingCommandName = !currentInput.Contains(' ');
            var result = _console.CommandLineAnalyzer.Analyze(currentInput);


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
                            candidates: new[] { desc.Definition.CommandName.Name },
                            analyzeResult: desc
                        );
                    }).ToList();
                _suggestionCache = new Suggestion(commandCandidates, Suggestion.CompletionState.TypingCommandName);
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
                            candidates: candidateParameters,
                            analyzeResult: desc
                        );
                    }).ToList();

                _suggestionCache = new Suggestion(commandCandidates, Suggestion.CompletionState.TypingParameters);
            }

            return _suggestionCache.Value;
        }

        #endregion

        #region 历史记录

        /// <summary>
        /// 历史记录最大条数。默认 20。超过此数量时最旧的记录会被丢弃。
        /// </summary>
        public int MaxHistoryEntries { get; set; } = 20;

        /// <summary>
        /// 已发送命令的历史记录。按发送时间从新到旧排列，[0] 是最近发送的命令。
        /// </summary>
        [NotNull]
        public IReadOnlyList<string> CommandHistory => _history.AsReadOnly();

        /// <summary>
        /// 获取指定偏移量的历史命令。offset 必须为正数（1 代表上一条发送的命令）。
        /// </summary>
        /// <param name="offset">偏移量，1 代表上一条。</param>
        /// <returns>历史命令文本；如果 offset 无效则返回 null。</returns>
        [return: MaybeNull]
        public virtual string GetHistoryEntry(int offset)
        {
            if (offset <= 0 || offset > _history.Count)
                return null;
            return _history[offset - 1];
        }

        /// <summary>
        /// 将指定偏移量的历史命令恢复到 <see cref="InputText"/> 中。
        /// </summary>
        /// <param name="offset">偏移量，1 代表上一条。</param>
        /// <returns>是否成功恢复。</returns>
        /// <remarks>
        /// 调用此方法会修改 <see cref="InputText"/>。如果你自定义了 UI，记得在调用后更新输入框的显示文本。
        /// </remarks>
        public virtual bool RestoreHistoryEntry(int offset)
        {
            var entry = GetHistoryEntry(offset);
            if (entry == null)
                return false;

            SetInputText(entry);
            return true;
        }

        #endregion

        #region Log 记录

        /// <summary>
        /// 是否记录 <see cref="LogEntry"/>。默认 true。
        /// </summary>
        public bool RecordLogEntries { get; set; } = true;

        /// <summary>
        /// Log 记录最大条数。默认 100。
        /// </summary>
        public int MaxLogEntries { get; set; } = 100;

        /// <summary>
        /// 已记录的 Log 条目，按时间从新到旧排列。仅在 <see cref="RecordLogEntries"/> 为 true 时才会记录。
        /// </summary>
        [NotNull]
        public IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();

        #endregion

        #region 私有字段

        private readonly IConsole _console;
        private readonly List<string> _history = new List<string>(20);
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        private Suggestion? _suggestionCache;
        private string _lastInputTextForSuggestion = string.Empty;

        #endregion

        #region 构造与析构

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="console"></param>
        public ConsoleViewModel([DisallowNull] IConsole console)
        {
            _console = console;
            Application.logMessageReceived += HandleLog;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="consoleKey"></param>
        public ConsoleViewModel(ConsoleKey consoleKey) : this(Console.Create(consoleKey))
        {
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="consoleKey"></param>
        public ConsoleViewModel([DisallowNull] string consoleKey) : this(new ConsoleKey(consoleKey))
        {
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public virtual void Dispose()
        {
            Application.logMessageReceived -= HandleLog;
        }

        #endregion

        #region 私有方法

        private void RecordHistory(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _history.Insert(0, text);
            if (_history.Count > MaxHistoryEntries)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            var entry = new LogEntry(logString, stackTrace, type);
            OnLogEntry?.Invoke(entry);

            if (RecordLogEntries)
            {
                _logEntries.Insert(0, entry);
                if (_logEntries.Count > MaxLogEntries)
                {
                    _logEntries.RemoveAt(_logEntries.Count - 1);
                }
            }
        }

        #endregion
    }
}