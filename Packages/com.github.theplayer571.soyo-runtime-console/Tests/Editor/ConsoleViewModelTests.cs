using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public partial class ConsoleBaseTests
    {
        #region A. InputText / SetInputText

        [Test]
        public void ViewModel_InputText_Default_IsEmptyString()
        {
            Assert.That(_viewModel.InputText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ViewModel_SetInputText_String_ReturnsValue()
        {
            _viewModel.SetInputText("hello");
            Assert.That(_viewModel.InputText, Is.EqualTo("hello"));
        }

        [Test]
        public void ViewModel_SetInputText_OverwritesPreviousValue()
        {
            _viewModel.SetInputText("first");
            _viewModel.SetInputText("second");
            Assert.That(_viewModel.InputText, Is.EqualTo("second"));
        }

        [Test]
        public void ViewModel_SetInputText_EmptyString_ClearsInput()
        {
            _viewModel.SetInputText("hello");
            _viewModel.SetInputText(string.Empty);
            Assert.That(_viewModel.InputText, Is.EqualTo(string.Empty));
        }

        #endregion

        #region B. SendInput

        [Test]
        public void ViewModel_SendInput_ExecutesCommand()
        {
            LogAssert.Expect(LogType.Log, "Hello World");
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();
        }

        [Test]
        public void ViewModel_SendInput_ClearsInputAfterExecution()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();
            Assert.That(_viewModel.InputText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ViewModel_SendInput_RecordsHistory()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(1));
            Assert.That(_viewModel.GetHistory()[0], Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_SendInput_EmptyInput_DoesNotRecordHistory()
        {
            // InputText 默认为空
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_SendInput_WhitespaceIsRecordedInHistory()
        {
            // RecordHistory 使用 string.IsNullOrEmpty，"   " 不会被过滤
            _viewModel.SetInputText("   ");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(1));
            Assert.That(_viewModel.GetHistory()[0], Is.EqualTo("   "));
        }

        #endregion

        #region C. GetHistory

        [Test]
        public void ViewModel_GetHistory_InitiallyEmpty()
        {
            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_GetHistory_RecordsCommandsInOrder_NewestFirst()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            _viewModel.SetInputText("hello");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(2));
            Assert.That(_viewModel.GetHistory()[0], Is.EqualTo("hello"));
            Assert.That(_viewModel.GetHistory()[1], Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_GetHistory_Maximum10Entries()
        {
            // 发送 12 条命令
            for (var i = 1; i <= 12; i++)
            {
                _viewModel.SetInputText($"cmd{i}");
                _viewModel.SendInput();
            }

            Assert.That(_viewModel.GetHistory().Count, Is.EqualTo(10));
            // 最新的在最前面
            Assert.That(_viewModel.GetHistory()[0], Is.EqualTo("cmd12"));
            Assert.That(_viewModel.GetHistory()[9], Is.EqualTo("cmd3"));
        }

        #endregion

        #region D. AutoComplete

        [Test]
        public void ViewModel_AutoComplete_TypingCommandName_CompletesCorrectly()
        {
            _viewModel.SetInputText("hel");
            var result = _viewModel.AutoComplete(0);

            Assert.That(result, Is.True);
            Assert.That(_viewModel.InputText, Is.EqualTo("hello "));
        }

        [Test]
        public void ViewModel_AutoComplete_NegativeIndex_ReturnsFalse()
        {
            _viewModel.SetInputText("hel");
            var result = _viewModel.AutoComplete(-1);

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("hel"));
        }

        [Test]
        public void ViewModel_AutoComplete_IndexOutOfRange_ReturnsFalse()
        {
            _viewModel.SetInputText("hel");
            var result = _viewModel.AutoComplete(99);

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("hel"));
        }

        [Test]
        public void ViewModel_AutoComplete_TypingParameters_CompletesParameter()
        {
            // "hello " 包含空格，进入 TypingParameters 状态
            _viewModel.SetInputText("hello ");
            var result = _viewModel.AutoComplete(0);

            Assert.That(result, Is.True);
            // StringParameterHandler 补全为带引号的空字符串
            Assert.That(_viewModel.InputText, Is.EqualTo("hello \"\""));
        }

        [Test]
        public void ViewModel_AutoComplete_NoMatchingCommand_ReturnsFalse()
        {
            // "zzz" 不匹配任何已注册的命令
            _viewModel.SetInputText("zzz");
            var result = _viewModel.AutoComplete(0);

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("zzz"));
        }

        #endregion

        #region E. GetSuggestion

        [Test]
        public void ViewModel_GetSuggestion_TypingCommandName_ReturnsCommandCandidates()
        {
            _viewModel.SetInputText("hel");
            var suggestion = _viewModel.GetSuggestion();

            Assert.That(suggestion.State, Is.EqualTo(Suggestion.CompletionState.TypingCommandName));
            Assert.That(suggestion.CandidateCommands, Is.Not.Null);
            Assert.That(suggestion.CandidateCommands.Count, Is.GreaterThan(0));
            Assert.That(suggestion.Candidates, Does.Contain("hello"));
            Assert.That(suggestion.Candidates, Does.Contain("hello_world"));
        }

        [Test]
        public void ViewModel_GetSuggestion_TypingParameters_ReturnsParameterCandidates()
        {
            // "hello " 包含空格，进入 TypingParameters 状态
            _viewModel.SetInputText("hello ");
            var suggestion = _viewModel.GetSuggestion();

            Assert.That(suggestion.State, Is.EqualTo(Suggestion.CompletionState.TypingParameters));
            Assert.That(suggestion.CandidateCommands, Is.Not.Null);
            Assert.That(suggestion.CandidateCommands.Count, Is.GreaterThan(0));
            // 参数候选项不应为空（StringParameterHandler 返回 ["\"\""]）
            Assert.That(suggestion.Candidates, Is.Not.Null);
            Assert.That(suggestion.Candidates.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ViewModel_GetSuggestion_UnknownCommand_ReturnsNoCandidates()
        {
            _viewModel.SetInputText("zzz");
            var suggestion = _viewModel.GetSuggestion();

            Assert.That(suggestion.State, Is.EqualTo(Suggestion.CompletionState.TypingCommandName));
            // 不匹配任何命令时 CandidateCommands 为空列表
            Assert.That(suggestion.CandidateCommands, Is.Not.Null);
            Assert.That(suggestion.CandidateCommands.Count, Is.EqualTo(0));
            Assert.That(suggestion.Candidates, Is.Not.Null);
            Assert.That(suggestion.Candidates.Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_GetSuggestion_EmptyInput_ReturnsAllCommands()
        {
            _viewModel.SetInputText(string.Empty);
            var suggestion = _viewModel.GetSuggestion();

            Assert.That(suggestion.State, Is.EqualTo(Suggestion.CompletionState.TypingCommandName));
            // 空输入匹配所有已注册的命令
            Assert.That(suggestion.Candidates, Does.Contain("hello"));
            Assert.That(suggestion.Candidates, Does.Contain("hello_world"));
        }

        #endregion

        #region F. OnLogEntry

        [Test]
        public void ViewModel_OnLogEntry_FiresOnCommandExecution()
        {
            LogEntry? capturedEntry = null;
            _viewModel.OnLogEntry += entry => capturedEntry = entry;

            LogAssert.Expect(LogType.Log, "Hello World");
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(capturedEntry.HasValue, Is.True);
            if (capturedEntry.HasValue)
            {
                Assert.That(capturedEntry.Value.Condition, Is.EqualTo("Hello World"));
                Assert.That(capturedEntry.Value.LogType, Is.EqualTo(LogType.Log));
            }
        }

        #endregion

        #region G. Constructor

        [Test]
        public void ViewModel_Constructor_AcceptsCustomIConsole()
        {
            var customConsole = new TestConsole();
            var viewModel = new ConsoleViewModel(customConsole);

            Assert.That(viewModel.InputText, Is.EqualTo(string.Empty));

            viewModel.SetInputText("hello_world");
            Assert.That(viewModel.InputText, Is.EqualTo("hello_world"));

            viewModel.Dispose();
        }

        #endregion
    }
}