using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soyo.SoyoRuntimeConsole.View
{
    /// <summary>
    /// 编辑器专用控制台视图。用于在编辑器内与控制台交互，主要用于示例制作和简单的快速使用。
    /// </summary>
    /// <remarks>
    /// 通过 <see cref="viewModel"/> 直接访问 <see cref="ConsoleViewModel"/> 的所有 API。
    /// 在 Inspector 中提供完整的控制面板：输入、自动补全、建议、历史记录、日志。
    /// </remarks>
    [ExecuteAlways]
    public class EditorOnlyConsoleView : MonoBehaviour
    {
        #region 序列化字段

        /// <summary>
        /// 控制台 Key。用于创建对应 Key 的 <see cref="ConsoleViewModel"/>。
        /// 修改此值会在 OnValidate 中自动重建 ViewModel。
        /// </summary>
        [DisallowNull]
        public string consoleKey = string.Empty;

        /// <summary>
        /// 可选的 <see cref="ConsoleProvider"/> ScriptableObject。设置此值后，控制台将通过
        /// <see cref="ConsoleProvider.CreateConsole"/> 创建，而非使用 <see cref="consoleKey"/>。
        /// 优先级高于 <see cref="consoleKey"/>。
        /// </summary>
        [AllowNull]
        public ConsoleProvider consoleProvider;

        /// <summary>
        /// 运行时 <see cref="ConsoleViewModel"/> 实例。通过此字段直接访问控制台的所有 API。
        /// </summary>
        [AllowNull]
        public ConsoleViewModel viewModel;

        #endregion

        #region 私有字段

        [AllowNull]
        private string _lastConsoleKey;

        [AllowNull]
        private ConsoleProvider _lastConsoleProvider;

        #endregion

        #region 生命周期

        private void Awake()
        {
            EnsureViewModel();
        }

        private void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.Dispose();
                viewModel = null;
            }
        }

        private void OnValidate()
        {
            // 检测 consoleKey / consoleProvider 变化并重建 ViewModel
            if (consoleKey != _lastConsoleKey || consoleProvider != _lastConsoleProvider)
            {
                if (viewModel != null)
                {
                    viewModel.Dispose();
                    viewModel = null;
                }

                EnsureViewModel();
                _lastConsoleKey = consoleKey;
                _lastConsoleProvider = consoleProvider;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建 <see cref="IConsole"/> 实例。子类可重写此方法以提供自定义控制台。
        /// </summary>
        /// <remarks>
        /// 优先级：<see cref="consoleProvider"/> &gt; <see cref="consoleKey"/>。
        /// 如果两者都未配置，返回 null。
        /// </remarks>
        /// <returns>控制台实例；如果未配置任何控制台源则返回 null。</returns>
        [return: MaybeNull]
        private IConsole CreateConsole()
        {
            if (consoleProvider != null)
            {
                return consoleProvider.CreateConsole();
            }

            if (!string.IsNullOrEmpty(consoleKey))
            {
                return Console.Create(consoleKey);
            }

            return null;
        }

        private void EnsureViewModel()
        {
            if (viewModel != null)
                return;

            var console = CreateConsole();
            if (console != null)
            {
                viewModel = new ConsoleViewModel(console);
                _lastConsoleKey = consoleKey;
                _lastConsoleProvider = consoleProvider;
            }
        }

        #endregion

#if UNITY_EDITOR

        #region Editor

        /// <summary>
        /// <see cref="EditorOnlyConsoleView"/> 的自定义 Inspector。
        /// 提供完整的控制台交互面板，涵盖 <see cref="ConsoleViewModel"/> 的全部功能。
        /// </summary>
        [CustomEditor(typeof(EditorOnlyConsoleView))]
        private class EditorOnlyConsoleViewEditor : Editor
        {
            #region 编辑器状态

            [AllowNull]
            private ConsoleViewModel _editorViewModel;

            [DisallowNull]
            private string _inputText = string.Empty;

            private int _candidateIndex;
            private int _historyOffset = 1;

            private bool _showSuggestion = true;
            private bool _showHistory = true;
            private bool _showLog = true;

            private Vector2 _logScrollPos;

            [AllowNull]
            private SerializedProperty _consoleKeyProp;

            [AllowNull]
            private SerializedProperty _consoleProviderProp;

            [AllowNull]
            private string _lastEditorConsoleKey;

            [AllowNull]
            private ConsoleProvider _lastEditorConsoleProvider;

            #endregion

            #region 编辑器生命周期

            private void OnEnable()
            {
                _consoleKeyProp = serializedObject.FindProperty("consoleKey");
                _consoleProviderProp = serializedObject.FindProperty("consoleProvider");

                _lastEditorConsoleKey = _consoleKeyProp?.stringValue ?? string.Empty;
                _lastEditorConsoleProvider =
                    (ConsoleProvider)(_consoleProviderProp?.objectReferenceValue ?? null);

                // 自动创建 Editor ViewModel
                CreateEditorViewModel();

                EditorApplication.update += Repaint;
            }

            private void OnDisable()
            {
                EditorApplication.update -= Repaint;
                DisposeEditorViewModel();
            }

            #endregion

            #region Inspector GUI

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                // 手动绘制 consoleKey 和 consoleProvider 字段
                EditorGUILayout.PropertyField(_consoleKeyProp);
                EditorGUILayout.PropertyField(_consoleProviderProp);

                // 显式生成 ViewModel 按钮
                var hasProvider = _consoleProviderProp?.objectReferenceValue != null;
                var hasKey = !string.IsNullOrEmpty(_consoleKeyProp?.stringValue ?? string.Empty);
                EditorGUI.BeginDisabledGroup(!hasProvider && !hasKey);
                if (GUILayout.Button("Create ViewModel"))
                {
                    CreateEditorViewModel();
                }
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                // 检测 consoleKey / consoleProvider 变化 → 废弃旧 ViewModel
                var currentKey = _consoleKeyProp?.stringValue ?? string.Empty;
                var currentProvider =
                    (ConsoleProvider)(_consoleProviderProp?.objectReferenceValue ?? null);
                if (currentKey != _lastEditorConsoleKey || currentProvider != _lastEditorConsoleProvider)
                {
                    DisposeEditorViewModel();
                    _lastEditorConsoleKey = currentKey;
                    _lastEditorConsoleProvider = currentProvider;
                }

                // ViewModel 不可用
                if (_editorViewModel == null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(
                        "点击上方 \"Create ViewModel\" 按钮以生成编辑器 ViewModel。",
                        MessageType.Info);
                    return;
                }

                EditorGUILayout.Space();

                // 实时同步输入到 ViewModel，使建议和预览随输入即时更新
                _editorViewModel.SetInputText(_inputText ?? string.Empty);

                // 各功能区域
                DrawInputSection();
                EditorGUILayout.Space();
                DrawAutoCompleteSection();
                EditorGUILayout.Space();
                DrawSuggestionSection();
                EditorGUILayout.Space();
                DrawHistorySection();
                EditorGUILayout.Space();
                DrawLogSection();
            }

            #endregion

            #region 输入区域

            private void DrawInputSection()
            {
                EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                _inputText = EditorGUILayout.TextField(_inputText ?? string.Empty);
                if (GUILayout.Button("Send Input", GUILayout.Width(100)))
                {
                    _editorViewModel.SetInputText(_inputText ?? string.Empty);
                    _editorViewModel.SendInput();
                    _inputText = string.Empty;

                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.EndHorizontal();

                // 调试显示：当前 ViewModel 中的 InputText
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("ViewModel InputText", _editorViewModel.InputText ?? string.Empty);
                EditorGUI.EndDisabledGroup();
            }

            #endregion

            #region 自动补全区域

            private void DrawAutoCompleteSection()
            {
                EditorGUILayout.LabelField("AutoComplete", EditorStyles.boldLabel);

                // 候选索引
                _candidateIndex = EditorGUILayout.IntField("Candidate Index", _candidateIndex);

                // Prev / Next 按钮
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("◀ Prev"))
                {
                    _editorViewModel.MoveCandidatePrevious();
                    _candidateIndex = _editorViewModel.CandidateIndex;
                }

                if (GUILayout.Button("Next ▶"))
                {
                    _editorViewModel.MoveCandidateNext();
                    _candidateIndex = _editorViewModel.CandidateIndex;
                }

                EditorGUILayout.EndHorizontal();

                // AutoComplete 按钮
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("AutoComplete"))
                {
                    _editorViewModel.CandidateIndex = _candidateIndex;
                    if (_editorViewModel.AutoComplete())
                    {
                        _inputText = _editorViewModel.InputText;
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUILayout.EndHorizontal();

                // 预览文本
                var previewText = _editorViewModel.GetAutoCompleteText();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Preview", previewText ?? "(无法补全)");
                EditorGUI.EndDisabledGroup();
            }

            #endregion

            #region 建议区域

            private void DrawSuggestionSection()
            {
                _showSuggestion = EditorGUILayout.Foldout(_showSuggestion, "Suggestion", true);
                if (!_showSuggestion)
                    return;

                EditorGUI.indentLevel++;

                var suggestion = _editorViewModel.GetSuggestion();

                // Completion State
                EditorGUILayout.LabelField("State", suggestion.State.ToString());

                // Candidates 列表
                var candidates = suggestion.Candidates;
                if (candidates != null && candidates.Count > 0)
                {
                    var candidatesText = string.Join("  |  ", candidates);
                    EditorGUILayout.LabelField(
                        $"Candidates ({candidates.Count})",
                        candidatesText,
                        EditorStyles.wordWrappedLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("Candidates", "(none)");
                }

                // CandidateCommands
                var commands = suggestion.CandidateCommands;
                if (commands != null && commands.Count > 0)
                {
                    EditorGUILayout.LabelField($"Commands ({commands.Count}):", EditorStyles.miniBoldLabel);
                    foreach (var cmd in commands)
                    {
                        EditorGUILayout.LabelField($"  {cmd.ToString()}", EditorStyles.wordWrappedLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Commands", "(none)");
                }

                EditorGUI.indentLevel--;
            }

            #endregion

            #region 历史记录区域

            private void DrawHistorySection()
            {
                _showHistory = EditorGUILayout.Foldout(_showHistory, "History", true);
                if (!_showHistory)
                    return;

                EditorGUI.indentLevel++;

                // MaxHistoryEntries
                var maxHistory = _editorViewModel.MaxHistoryEntries;
                maxHistory = EditorGUILayout.IntField("Max Entries", maxHistory);
                if (maxHistory < 0) maxHistory = 0;
                _editorViewModel.MaxHistoryEntries = maxHistory;

                // 恢复历史
                EditorGUILayout.BeginHorizontal();
                _historyOffset = EditorGUILayout.IntField("Offset (1=latest)", _historyOffset);
                if (GUILayout.Button("Restore", GUILayout.Width(80)))
                {
                    if (_editorViewModel.RestoreHistoryEntry(_historyOffset))
                    {
                        _inputText = _editorViewModel.InputText;
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUILayout.EndHorizontal();

                // 历史列表
                var history = _editorViewModel.CommandHistory;
                EditorGUILayout.LabelField($"Total: {history.Count} entries");

                if (history.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    // 限制显示条数避免 Inspector 过长
                    var displayCount = Mathf.Min(history.Count, 20);
                    for (var i = 0; i < displayCount; i++)
                    {
                        EditorGUILayout.LabelField($"[{i}] {history[i]}", EditorStyles.wordWrappedLabel);
                    }

                    if (history.Count > displayCount)
                    {
                        EditorGUILayout.LabelField($"... (还有 {history.Count - displayCount} 条)");
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            #endregion

            #region 日志区域

            private void DrawLogSection()
            {
                _showLog = EditorGUILayout.Foldout(_showLog, "Log", true);
                if (!_showLog)
                    return;

                EditorGUI.indentLevel++;

                // RecordLogEntries
                var recordLog = _editorViewModel.RecordLogEntries;
                recordLog = EditorGUILayout.Toggle("Record Log Entries", recordLog);
                _editorViewModel.RecordLogEntries = recordLog;

                // MaxLogEntries
                var maxLog = _editorViewModel.MaxLogEntries;
                maxLog = EditorGUILayout.IntField("Max Entries", maxLog);
                if (maxLog < 0) maxLog = 0;
                _editorViewModel.MaxLogEntries = maxLog;

                // 日志列表
                var logEntries = _editorViewModel.LogEntries;
                EditorGUILayout.LabelField($"Total: {logEntries.Count} entries");

                if (logEntries.Count > 0)
                {
                    _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(200));

                    var displayCount = Mathf.Min(logEntries.Count, _editorViewModel.MaxLogEntries);
                    for (var i = 0; i < displayCount; i++)
                    {
                        var entry = logEntries[i];
                        var originalColor = GUI.color;

                        // 根据 LogType 着色
                        switch (entry.LogType)
                        {
                            case LogType.Error:
                            case LogType.Exception:
                            case LogType.Assert:
                                GUI.color = Color.red;
                                break;
                            case LogType.Warning:
                                GUI.color = Color.yellow;
                                break;
                            default:
                                GUI.color = Color.white;
                                break;
                        }

                        EditorGUILayout.LabelField(
                            $"[{i}] [{entry.LogType}] {entry.Condition}",
                            EditorStyles.wordWrappedLabel);

                        if (!string.IsNullOrEmpty(entry.StackTrace))
                        {
                            EditorGUI.indentLevel++;
                            var stackStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                            {
                                fontStyle = FontStyle.Italic
                            };
                            EditorGUILayout.LabelField(entry.StackTrace, stackStyle, GUILayout.MaxHeight(60));
                            EditorGUI.indentLevel--;
                        }

                        GUI.color = originalColor;

                        // 分隔线
                        if (i < displayCount - 1)
                        {
                            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }

                EditorGUI.indentLevel--;
            }

            #endregion

            #region 私有方法

            private void CreateEditorViewModel()
            {
                DisposeEditorViewModel();

                var view = (EditorOnlyConsoleView)target;
                var console = view.CreateConsole();
                if (console != null)
                {
                    _editorViewModel = new ConsoleViewModel(console);
                }

                _inputText = string.Empty;
                _candidateIndex = 0;
                _historyOffset = 1;
            }

            private void DisposeEditorViewModel()
            {
                if (_editorViewModel != null)
                {
                    _editorViewModel.Dispose();
                    _editorViewModel = null;
                }
            }

            #endregion
        }

        #endregion

#endif
    }
}
