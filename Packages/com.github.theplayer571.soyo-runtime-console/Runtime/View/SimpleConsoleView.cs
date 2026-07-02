using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Soyo.SoyoRuntimeConsole.View
{
    public class SimpleConsoleView : MonoBehaviour
    {
        #region OutputView

        private class OutputView
        {
            public void ConsiderInput()
            {
                var isInputEmpty = string.IsNullOrEmpty(Self._viewModel.InputText);

                if (isInputEmpty)
                {
                    if (_showState != ShowState.ShowLog)
                    {
                        _showState = ShowState.ShowLog;
                        RebuildView();
                    }
                }
                else
                {
                    _showState = ShowState.ShowSuggestion;
                    RebuildView();
                }
            }

            public bool ShowStackTrace
            {
                get => _showStackTrace;
                set
                {
                    if (_showStackTrace == value) return;

                    _showStackTrace = value;
                    RebuildView();
                }
            }

            public void RebuildView(bool scrollToBottom = true)
            {
                var sb = new StringBuilder();

                // 构造sb
                switch (_showState)
                {
                    case ShowState.ShowLog:
                    {
                        for (int i = Self._viewModel.LogEntries.Count - 1; i >= 0; i--)
                        {
                            var logEntry = Self._viewModel.LogEntries[i];
                            sb.Append(LogEntry2String(logEntry, _showStackTrace));
                        }

                        break;
                    }
                    case ShowState.ShowSuggestion:
                    {
                        var suggestion = Self._viewModel.GetSuggestion();
                        var autoCompletionIndex = Self._viewModel.CandidateIndex;

                        if (suggestion.State == Suggestion.CompletionState.TypingCommandName)
                        {
                            if (suggestion.CandidateCommands == null || suggestion.Candidates == null) break;
                            var shownNames = new HashSet<string>();
                            var input = Self._viewModel.InputText;

                            // 添加每个命令
                            for (var index = 0; index < suggestion.Candidates.Count; index++)
                            {
                                var commandName = suggestion.Candidates[index];
                                if (!shownNames.Add(commandName)) continue;

                                var commandInfo = suggestion.CandidateCommands.First(c => c.Name == commandName);

                                // 前缀
                                if (index == autoCompletionIndex)
                                {
                                    sb.Append("<color=white>");
                                    sb.Append("> ");
                                }
                                else
                                {
                                    sb.Append("<color=grey>");
                                    sb.Append("- ");
                                }

                                // 命令名
                                sb.Append("<color=grey>");
                                sb.Append(commandInfo.Name.Replace(input, $"<color=white>{input}<color=grey>"));

                                // HelpText
                                if (commandInfo.HelpText != null)
                                {
                                    sb.Append(" - ");
                                    sb.Append("<color=grey><i>");
                                    sb.Append(commandInfo.HelpText);
                                    sb.Append("</i></color>");
                                }

                                sb.AppendLine();
                            }

                            // 提示
                            sb.Append("<color=grey><i>Tab for completion</i></color>");

                            break;
                        }
                        else if (suggestion.State == Suggestion.CompletionState.TypingParameters)
                        {
                            if (suggestion.CandidateCommands == null || suggestion.Candidates == null) break;

                            var lastInput = suggestion.CandidateCommands
                                .Select(c => c.AnalyzeResult.Parameters.LastOrDefault()?.TrimEnd())
                                .Where(p => !string.IsNullOrEmpty(p))
                                .Distinct().ToList();

                            // 添加补全参数
                            for (var index = 0; index < suggestion.Candidates.Count; index++)
                            {
                                var candidate = suggestion.Candidates[index];

                                // 前缀
                                if (index == autoCompletionIndex)
                                {
                                    sb.Append("<color=white>");
                                    sb.Append("> ");
                                }
                                else
                                {
                                    sb.Append("<color=grey>");
                                    sb.Append("- ");
                                }

                                // 参数
                                sb.Append("<color=grey>");

                                var coloredParameter = Highlight(candidate, lastInput);
                                sb.Append(coloredParameter);

                                sb.AppendLine();
                            }

                            // 添加命令
                            sb.AppendLine();
                            var activeCommandMarked = false;
                            for (var candidateCommandIndex = 0;
                                 candidateCommandIndex < suggestion.CandidateCommands.Count;
                                 candidateCommandIndex++)
                            {
                                var commandInfo = suggestion.CandidateCommands[candidateCommandIndex];

                                // 前缀
                                if (!activeCommandMarked && commandInfo.AnalyzeResult.Executable)
                                {
                                    sb.Append("<color=white>");
                                    sb.Append("> ");
                                    activeCommandMarked = true;
                                }
                                else
                                {
                                    sb.Append("<color=grey>");
                                    sb.Append("- ");
                                }

                                // 命令内容
                                sb.Append("<color=white>");
                                sb.Append(commandInfo.Name);
                                sb.Append(' ');

                                if (commandInfo.ParameterDescriptions != null)
                                {
                                    for (var parameterIndex = 0;
                                         parameterIndex < commandInfo.ParameterDescriptions.Count;
                                         parameterIndex++)
                                    {
                                        var beforeAppendInValidParameter =
                                            parameterIndex == commandInfo.AnalyzeResult.ValidParameterCount;
                                        if (beforeAppendInValidParameter)
                                        {
                                            sb.Append("<color=grey>");
                                        }

                                        var parameterDescription = commandInfo.ParameterDescriptions[parameterIndex];

                                        if (parameterDescription.Type == null)
                                        {
                                            sb.Append($"{parameterDescription.Name} ");
                                        }
                                        else
                                        {
                                            sb.Append($"<{parameterDescription.Type}: {parameterDescription.Name}> ");
                                        }
                                    }
                                }

                                sb.AppendLine();
                            }

                            // 提示
                            sb.Append("<color=grey><i>Tab for completion.</i></color>");
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // 显示文本
                _outputContent = sb.ToString();
                Self.outputField.text = _outputContent;
                if (scrollToBottom)
                {
                    Self.StartCoroutine(ScrollToBottom());
                }
            }

            public void AppendLog(in LogEntry logEntry)
            {
                if (_showState != ShowState.ShowLog) return;

                var logString = LogEntry2String(logEntry, _showStackTrace);
                _outputContent += logString;
                Self.outputField.text = _outputContent;
            }


            // 引用
            private readonly SimpleConsoleView Self;

            // 变量
            private string _outputContent;
            private bool _showStackTrace = false;
            private ShowState _showState = ShowState.ShowLog;

            public OutputView(SimpleConsoleView self)
            {
                Self = self;
            }

            private IEnumerator ScrollToBottom()
            {
                yield return new WaitForEndOfFrame();
                Self.outputScrollRect.verticalNormalizedPosition = 0f;
            }

            private enum ShowState
            {
                ShowLog,
                ShowSuggestion,
            }
        }

        #endregion

        // 配置
        [Header("Configs")] [SerializeField] private bool showOnStart = false;
        [SerializeField] private bool isSingleton = false;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private KeyCode toggleVisibilityKey = KeyCode.F1;
        [SerializeField] private int recordLogEntriesCount = 10;
        [SerializeField] private bool showStackTraceOnStart = false;
        [SerializeField] private bool allowTabForStackTrace = true;
        [SerializeField] private ConsoleProvider consoleProvider;


        // 引用
        [Header("References(Don't Edit)")] [SerializeField]
        private Transform view;

        [SerializeField] private TMP_InputField inputField;

        [SerializeField] private TMP_InputField outputField;
        [SerializeField] private ScrollRect outputScrollRect;


        // 变量
        private ConsoleViewModel _viewModel;
        private OutputView _outputView;
        private bool _showConsole = false;

        public static SimpleConsoleView Instance { get; private set; }

        private void Awake()
        {
            if (isSingleton)
            {
                if (Instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                Instance = this;
            }

            _showConsole = showOnStart;
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

            var console = CreateConsole();
            _viewModel = new ConsoleViewModel(console);
            _viewModel.RecordLogEntries = true;
            _viewModel.MaxLogEntries = recordLogEntriesCount;

            _outputView = new OutputView(this);

            inputField.onValueChanged.AddListener(OnValueChanged);
            _viewModel.OnLogEntry += OnLogEntry;

            _outputView.RebuildView();
            _outputView.ShowStackTrace = showStackTraceOnStart;

            view.gameObject.SetActive(_showConsole);
        }

        private void Update()
        {
            var hasInput = !string.IsNullOrEmpty(_viewModel.InputText);

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (hasInput)
                {
                    _viewModel.AutoComplete();
                    inputField.SetTextWithoutNotify(_viewModel.InputText);
                    inputField.caretPosition = inputField.text.Length;
                    _outputView.ConsiderInput();
                }
                else
                {
                    if (allowTabForStackTrace)
                    {
                        _outputView.ShowStackTrace = !_outputView.ShowStackTrace;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (hasInput)
                {
                    _viewModel.MoveCandidatePrevious();
                    _outputView.RebuildView(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (hasInput)
                {
                    _viewModel.MoveCandidateNext();
                    _outputView.RebuildView(false);
                }
            }

            if (Input.GetKeyDown(toggleVisibilityKey))
            {
                _showConsole = !_showConsole;
                inputField.SetTextWithoutNotify(string.Empty);
                view.gameObject.SetActive(_showConsole);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            inputField.onValueChanged.RemoveListener(OnValueChanged);
            _viewModel.OnLogEntry -= OnLogEntry;
        }

        private void OnValueChanged(string value)
        {
            if (value.Contains('\t'))
            {
                inputField.SetTextWithoutNotify(value.Replace("\t", string.Empty));
                return;
            }

            // 防止递归，不放到后面处理
            if (string.IsNullOrEmpty(value))
            {
                _viewModel.SetInputText(string.Empty);
                _outputView.ConsiderInput();
                return;
            }

            // 该发送命令
            if (value.Contains('\n'))
            {
                _viewModel.SendInput();
                inputField.SetTextWithoutNotify(string.Empty);
                _outputView.ConsiderInput();
                return;
            }

            // 正常的修改
            _viewModel.SetInputText(value);
            _outputView.ConsiderInput();
        }

        private void OnLogEntry(LogEntry logEntry)
        {
            _outputView.AppendLog(logEntry);
        }

        private IConsole CreateConsole()
        {
            return consoleProvider == null
                ? Console.Create()
                : consoleProvider.CreateConsole();
        }

        private static string LogEntry2String(in LogEntry logEntry, bool showStackTrace)
        {
            var sb = new StringBuilder();

            // Condition
            sb.Append(LogType2Color(logEntry.LogType));
            sb.Append('>');
            sb.Append("<space=0.5em>");
            sb.Append(LogType2Prefix(logEntry.LogType));
            sb.Append("<space=0.5em>");

            sb.Append(logEntry.Condition);
            sb.Append("</color>");
            sb.AppendLine();

            // StackTrace
            if (showStackTrace)
            {
                sb.Append("<size=60%>");
                sb.Append("<color=grey>");
                sb.Append(logEntry.StackTrace);

                sb.Append("</color>");
                sb.Append("</size>");
            }

            return sb.ToString();

            static string LogType2Color(LogType logType)
            {
                return logType switch
                {
                    LogType.Error => "<color=red>",
                    LogType.Assert => "<color=red>",
                    LogType.Warning => "<color=yellow>",
                    LogType.Log => "<color=white>",
                    LogType.Exception => "<color=red>",
                    _ => string.Empty
                };
            }

            static string LogType2Prefix(LogType logType)
            {
                return logType switch
                {
                    LogType.Error => "[Error]",
                    LogType.Assert => "[Assert]",
                    LogType.Warning => "[Warning]",
                    LogType.Log => "[Log]",
                    LogType.Exception => "[Exception]",
                    _ => string.Empty
                };
            }
        }

        private static string Highlight(string text, IEnumerable<string> keywords)
        {
            if (string.IsNullOrEmpty(text) || keywords == null)
                return text;

            var validKeywords = keywords
                .Where(k => !string.IsNullOrEmpty(k))
                .OrderByDescending(k => k.Length)
                .ToArray();

            if (validKeywords.Length == 0)
                return text;

            string pattern = string.Join("|", validKeywords.Select(Regex.Escape));
            return Regex.Replace(text, pattern, m => $"<color=white>{m.Value}<color=grey>");
        }
    }
}