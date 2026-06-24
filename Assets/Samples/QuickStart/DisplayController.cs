using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Soyo.SoyoRuntimeConsole.Samples.QuickStart
{
    [ExecuteAlways]
    public class DisplayController : MonoBehaviour
    {
        public string input;
        public Text suggestionText;

        public ConsoleViewModel _viewModel;

        private void OnEnable()
        {
            // 创建对应key的Console
            var console = Console.Create(new ConsoleKey("QuickStartConsole"));
            _viewModel = new ConsoleViewModel(console);
        }

        private void OnValidate()
        {
            RefreshSuggestionDisplay();
        }

        public void RefreshSuggestionDisplay()
        {
            if (_viewModel == null)
            {
                return;
            }

            _viewModel.SetInputText(input ?? string.Empty);
            var suggestion = _viewModel.GetSuggestion();
            if (suggestionText != null)
            {
                suggestionText.text = suggestion.ToString();
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(DisplayController))]
        private class DisplayControllerEditor : Editor
        {
            private ConsoleViewModel _editorViewModel;
            private int _autoCompleteIndex = 0;
            private int _historyIndex = 0;

            private void OnEnable()
            {
                _editorViewModel = serializedObject.targetObject is DisplayController
                    ? new ConsoleViewModel(Console.Create(new ConsoleKey("QuickStartConsole")))
                    : null;
                EditorApplication.update += Repaint;
            }

            private void OnDisable()
            {
                EditorApplication.update -= Repaint;
                _editorViewModel = null;
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                // Draw the default inspector (shows `input` and `suggestionText` fields)
                DrawDefaultInspector();

                serializedObject.ApplyModifiedProperties();

                var dc = (DisplayController)target;

                if (_editorViewModel == null)
                {
                    EditorGUILayout.HelpBox("Editor ConsoleViewModel not available.", MessageType.Warning);
                    return;
                }

                // Keep editor view model in sync with the public `input` field on the component.
                _editorViewModel.SetInputText(dc.input ?? string.Empty);

                // Read-only field showing the current InputText from the view model
                EditorGUILayout.LabelField("InputText", _editorViewModel.InputText ?? string.Empty);

                // Horizontal layout: left int field for index, right button to trigger AutoComplete
                EditorGUILayout.BeginHorizontal();
                _autoCompleteIndex = EditorGUILayout.IntField(_autoCompleteIndex);
                if (GUILayout.Button("AutoComplete", GUILayout.Width(100)))
                {
                    // Try autocomplete and, if it produced a new input, write it back to the component
                    _editorViewModel.CandidateIndex = _autoCompleteIndex;
                    var success = _editorViewModel.AutoComplete();
                    if (success)
                    {
                        dc.input = _editorViewModel.InputText;
                        EditorUtility.SetDirty(dc);
                        dc.RefreshSuggestionDisplay();
                    }
                }

                EditorGUILayout.EndHorizontal();

                // History backtracking: index + button
                EditorGUILayout.BeginHorizontal();
                _historyIndex = EditorGUILayout.IntField(_historyIndex);
                if (GUILayout.Button("Recover History", GUILayout.Width(100)))
                {
                    var history = _editorViewModel.CommandHistory;
                    if (history != null && _historyIndex >= 0 && _historyIndex < history.Count)
                    {
                        dc.input = history[_historyIndex];
                        EditorUtility.SetDirty(dc);
                        dc.RefreshSuggestionDisplay();
                    }
                }

                EditorGUILayout.EndHorizontal();

                // SendInput button
                if (GUILayout.Button("Send Input"))
                {
                    _editorViewModel.SetInputText(dc.input ?? string.Empty);
                    _editorViewModel.SendInput();
                    dc.input = string.Empty;
                    EditorUtility.SetDirty(dc);
                    dc.RefreshSuggestionDisplay();
                }
            }
        }
#endif
    }
}