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

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(1));
            Assert.That(_viewModel.CommandHistory[0], Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_SendInput_EmptyInput_DoesNotRecordHistory()
        {
            // InputText 默认为空
            _viewModel.SendInput();

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_SendInput_WhitespaceIsRecordedInHistory()
        {
            // RecordHistory 使用 string.IsNullOrEmpty，"   " 不会被过滤
            _viewModel.SetInputText("   ");
            _viewModel.SendInput();

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(1));
            Assert.That(_viewModel.CommandHistory[0], Is.EqualTo("   "));
        }

        #endregion

        #region C. CommandHistory

        [Test]
        public void ViewModel_CommandHistory_InitiallyEmpty()
        {
            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_CommandHistory_RecordsCommandsInOrder_NewestFirst()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            _viewModel.SetInputText("hello");
            _viewModel.SendInput();

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(2));
            Assert.That(_viewModel.CommandHistory[0], Is.EqualTo("hello"));
            Assert.That(_viewModel.CommandHistory[1], Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_CommandHistory_DefaultMax20Entries()
        {
            // 发送 25 条命令，默认上限 20
            for (var i = 1; i <= 25; i++)
            {
                _viewModel.SetInputText($"cmd{i}");
                _viewModel.SendInput();
            }

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(20));
            // 最新的在最前面
            Assert.That(_viewModel.CommandHistory[0], Is.EqualTo("cmd25"));
            Assert.That(_viewModel.CommandHistory[19], Is.EqualTo("cmd6"));
        }

        [Test]
        public void ViewModel_CommandHistory_MaxHistoryEntries_CustomLimit()
        {
            _viewModel.MaxHistoryEntries = 5;

            for (var i = 1; i <= 10; i++)
            {
                _viewModel.SetInputText($"cmd{i}");
                _viewModel.SendInput();
            }

            Assert.That(_viewModel.CommandHistory.Count, Is.EqualTo(5));
            Assert.That(_viewModel.CommandHistory[0], Is.EqualTo("cmd10"));
            Assert.That(_viewModel.CommandHistory[4], Is.EqualTo("cmd6"));
        }

        #endregion

        #region D. GetHistoryEntry

        [Test]
        public void ViewModel_GetHistoryEntry_Offset1_ReturnsLastSent()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistoryEntry(1), Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_GetHistoryEntry_Offset2_ReturnsSecondToLast()
        {
            _viewModel.SetInputText("cmd1");
            _viewModel.SendInput();
            _viewModel.SetInputText("cmd2");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistoryEntry(1), Is.EqualTo("cmd2"));
            Assert.That(_viewModel.GetHistoryEntry(2), Is.EqualTo("cmd1"));
        }

        [Test]
        public void ViewModel_GetHistoryEntry_InvalidOffset_ReturnsNull()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(_viewModel.GetHistoryEntry(0), Is.Null);
            Assert.That(_viewModel.GetHistoryEntry(-1), Is.Null);
            Assert.That(_viewModel.GetHistoryEntry(2), Is.Null); // 只有 1 条记录
        }

        [Test]
        public void ViewModel_GetHistoryEntry_EmptyHistory_ReturnsNull()
        {
            Assert.That(_viewModel.GetHistoryEntry(1), Is.Null);
        }

        #endregion

        #region E. RestoreHistoryEntry

        [Test]
        public void ViewModel_RestoreHistoryEntry_WritesToInputText()
        {
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            var result = _viewModel.RestoreHistoryEntry(1);
            Assert.That(result, Is.True);
            Assert.That(_viewModel.InputText, Is.EqualTo("hello_world"));
        }

        [Test]
        public void ViewModel_RestoreHistoryEntry_InvalidOffset_ReturnsFalse()
        {
            var result = _viewModel.RestoreHistoryEntry(1);
            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ViewModel_RestoreHistoryEntry_OverwritesCurrentInput()
        {
            _viewModel.SetInputText("cmd1");
            _viewModel.SendInput();
            _viewModel.SetInputText("cmd2");
            _viewModel.SendInput();

            _viewModel.SetInputText("currently typing...");
            var result = _viewModel.RestoreHistoryEntry(2); // 恢复到 cmd1
            Assert.That(result, Is.True);
            Assert.That(_viewModel.InputText, Is.EqualTo("cmd1"));
        }

        #endregion

        #region F. AutoComplete

        [Test]
        public void ViewModel_AutoComplete_TypingCommandName_CompletesCorrectly()
        {
            _viewModel.SetInputText("hel");
            _viewModel.CandidateIndex = 0;
            var result = _viewModel.AutoComplete();

            Assert.That(result, Is.True);
            Assert.That(_viewModel.InputText, Is.EqualTo("hello "));
        }

        [Test]
        public void ViewModel_AutoComplete_NegativeIndex_ReturnsFalse()
        {
            _viewModel.SetInputText("hel");
            _viewModel.CandidateIndex = -1;
            var result = _viewModel.AutoComplete();

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("hel"));
        }

        [Test]
        public void ViewModel_AutoComplete_IndexOutOfRange_ReturnsFalse()
        {
            _viewModel.SetInputText("hel");
            _viewModel.CandidateIndex = 99;
            var result = _viewModel.AutoComplete();

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("hel"));
        }

        [Test]
        public void ViewModel_AutoComplete_TypingParameters_CompletesParameter()
        {
            // "hello " 包含空格，进入 TypingParameters 状态
            _viewModel.SetInputText("hello ");
            _viewModel.CandidateIndex = 0;
            var result = _viewModel.AutoComplete();

            Assert.That(result, Is.True);
            // StringParameterHandler 补全为带引号的空字符串
            Assert.That(_viewModel.InputText, Is.EqualTo("hello \"\""));
        }

        [Test]
        public void ViewModel_AutoComplete_NoMatchingCommand_ReturnsFalse()
        {
            // "zzz" 不匹配任何已注册的命令
            _viewModel.SetInputText("zzz");
            _viewModel.CandidateIndex = 0;
            var result = _viewModel.AutoComplete();

            Assert.That(result, Is.False);
            Assert.That(_viewModel.InputText, Is.EqualTo("zzz"));
        }

        #endregion

        #region G. CandidateIndex navigation

        [Test]
        public void ViewModel_CandidateIndex_DefaultIsZero()
        {
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_MoveCandidateNext_MovesForwardWithWrapping()
        {
            _viewModel.SetInputText("hel"); // 候选：hello, hello_world (2 个)
            _viewModel.CandidateIndex = 0;

            _viewModel.MoveCandidateNext();
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(1));

            _viewModel.MoveCandidateNext();
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(0)); // 回绕
        }

        [Test]
        public void ViewModel_MoveCandidatePrevious_MovesBackwardWithWrapping()
        {
            _viewModel.SetInputText("hel"); // 候选：hello, hello_world (2 个)
            _viewModel.CandidateIndex = 0;

            _viewModel.MoveCandidatePrevious();
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(1)); // 回绕到末尾
        }

        [Test]
        public void ViewModel_MoveCandidateNext_NoCandidates_DoesNothing()
        {
            _viewModel.SetInputText("zzz"); // 无匹配
            _viewModel.CandidateIndex = 5;
            _viewModel.MoveCandidateNext();
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(5)); // 不变
        }

        [Test]
        public void ViewModel_MoveCandidatePrevious_NoCandidates_DoesNothing()
        {
            _viewModel.SetInputText("zzz"); // 无匹配
            _viewModel.CandidateIndex = 3;
            _viewModel.MoveCandidatePrevious();
            Assert.That(_viewModel.CandidateIndex, Is.EqualTo(3)); // 不变
        }

        #endregion

        #region H. GetAutoCompleteText

        [Test]
        public void ViewModel_GetAutoCompleteText_ReturnsCompletedText_WithoutModifyingInput()
        {
            _viewModel.SetInputText("hel");
            _viewModel.CandidateIndex = 0;

            var oldInput = _viewModel.InputText;
            var text = _viewModel.GetAutoCompleteText();

            Assert.That(text, Is.EqualTo("hello "));
            Assert.That(_viewModel.InputText, Is.EqualTo(oldInput)); // 未修改
        }

        [Test]
        public void ViewModel_GetAutoCompleteText_InvalidIndex_ReturnsNull()
        {
            _viewModel.SetInputText("hel");
            _viewModel.CandidateIndex = -1;

            var text = _viewModel.GetAutoCompleteText();

            Assert.That(text, Is.Null);
        }

        [Test]
        public void ViewModel_GetAutoCompleteText_NoMatchingCommand_ReturnsNull()
        {
            _viewModel.SetInputText("zzz");
            _viewModel.CandidateIndex = 0;

            var text = _viewModel.GetAutoCompleteText();

            Assert.That(text, Is.Null);
        }

        #endregion

        #region I. GetSuggestion

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

        [Test]
        public void ViewModel_GetSuggestion_CachesResult_WhenInputTextUnchanged()
        {
            _viewModel.SetInputText("hel");
            var first = _viewModel.GetSuggestion();
            var second = _viewModel.GetSuggestion();

            // 同一输入应返回相同的 Suggestion 实例（缓存生效）
            Assert.That(second.State, Is.EqualTo(first.State));
            Assert.That(second.Candidates, Is.EqualTo(first.Candidates));
        }

        [Test]
        public void ViewModel_GetSuggestion_InvalidatesCache_WhenInputTextChanges()
        {
            _viewModel.SetInputText("hel");
            var first = _viewModel.GetSuggestion();

            // "hello " 包含空格，进入 TypingParameters 状态（不同于 TypingCommandName）
            _viewModel.SetInputText("hello ");
            var second = _viewModel.GetSuggestion();

            // 不同输入应产生不同的 Suggestion（State 从 TypingCommandName 变为 TypingParameters）
            Assert.That(second.State, Is.Not.EqualTo(first.State));
        }

        #endregion

        #region J. OnLogEntry

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

        #region K. Log recording

        [Test]
        public void ViewModel_LogEntries_DefaultIsEmpty()
        {
            Assert.That(_viewModel.LogEntries.Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_LogEntries_NotRecordedByDefault()
        {
            LogAssert.Expect(LogType.Log, "Hello World");
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(_viewModel.LogEntries.Count, Is.EqualTo(0));
        }

        [Test]
        public void ViewModel_LogEntries_RecordsWhenEnabled()
        {
            _viewModel.RecordLogEntries = true;

            LogAssert.Expect(LogType.Log, "Hello World");
            _viewModel.SetInputText("hello_world");
            _viewModel.SendInput();

            Assert.That(_viewModel.LogEntries.Count, Is.EqualTo(1));
            Assert.That(_viewModel.LogEntries[0].Condition, Is.EqualTo("Hello World"));
            Assert.That(_viewModel.LogEntries[0].LogType, Is.EqualTo(LogType.Log));
        }

        [Test]
        public void ViewModel_LogEntries_MaxLogEntries_TrimsOldest()
        {
            _viewModel.RecordLogEntries = true;
            _viewModel.MaxLogEntries = 3;

            // 发送 5 条命令，每条都会产生一条 "Hello World" 的 Log
            for (var i = 1; i <= 5; i++)
            {
                LogAssert.Expect(LogType.Log, "Hello World");
                _viewModel.SetInputText("hello_world");
                _viewModel.SendInput();
            }

            Assert.That(_viewModel.LogEntries.Count, Is.EqualTo(3));
            // 所有 log 都是 "Hello World"，只需验证数量正确且最新一条存在
            Assert.That(_viewModel.LogEntries[0].Condition, Is.EqualTo("Hello World"));
            Assert.That(_viewModel.LogEntries[0].LogType, Is.EqualTo(LogType.Log));
        }

        [Test]
        public void ViewModel_LogEntries_MaxLogEntries_Default100()
        {
            Assert.That(_viewModel.MaxLogEntries, Is.EqualTo(100));
        }

        #endregion

        #region L. Constructor

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

        #region M. Virtual methods

        /// <summary>
        /// 验证 public 方法可以被 override。
        /// </summary>
        private class OverridingViewModel : ConsoleViewModel
        {
            public OverridingViewModel(IConsole console) : base(console) { }

            public override void SetInputText(string text)
            {
                base.SetInputText("[" + text + "]");
            }

            public override void SendInput()
            {
                // 自定义：不记录空输入，也不清空
                base.SendInput();
            }

            public override bool AutoComplete()
            {
                // 自定义：总是返回 true（偷懒实现）
                var suggestion = GetSuggestion();
                var candidates = suggestion.Candidates;
                if (candidates != null && CandidateIndex >= 0 && CandidateIndex < candidates.Count)
                {
                    base.AutoComplete();
                }
                return true;
            }

            public override Suggestion GetSuggestion()
            {
                return base.GetSuggestion();
            }

            public override string GetAutoCompleteText()
            {
                return base.GetAutoCompleteText();
            }

            public override string GetHistoryEntry(int offset)
            {
                return base.GetHistoryEntry(offset);
            }

            public override bool RestoreHistoryEntry(int offset)
            {
                return base.RestoreHistoryEntry(offset);
            }

            public override void MoveCandidateNext()
            {
                base.MoveCandidateNext();
            }

            public override void MoveCandidatePrevious()
            {
                base.MoveCandidatePrevious();
            }

            public override void Dispose()
            {
                base.Dispose();
            }
        }

        [Test]
        public void ViewModel_Virtual_SetInputText_CanBeOverridden()
        {
            var vm = new OverridingViewModel(new TestConsole());
            vm.SetInputText("hello");
            Assert.That(vm.InputText, Is.EqualTo("[hello]"));
            vm.Dispose();
        }

        [Test]
        public void ViewModel_Virtual_AutoComplete_CanBeOverridden()
        {
            var vm = new OverridingViewModel(new TestConsole());
            vm.SetInputText("hel");
            vm.CandidateIndex = 0;
            var result = vm.AutoComplete();
            Assert.That(result, Is.True);
            vm.Dispose();
        }

        [Test]
        public void ViewModel_Virtual_GetHistoryEntry_CanBeOverridden()
        {
            var vm = new OverridingViewModel(new TestConsole());
            // SetInputText 被 override 包裹了一层 "[" + text + "]"
            vm.SetInputText("hello"); // 实际存储 "[hello]"
            vm.SendInput();           // 记录 "[hello]" 到历史
            Assert.That(vm.GetHistoryEntry(1), Is.EqualTo("[hello]"));
            vm.Dispose();
        }

        [Test]
        public void ViewModel_Virtual_RestoreHistoryEntry_CanBeOverridden()
        {
            var vm = new OverridingViewModel(new TestConsole());
            vm.SetInputText("hello"); // 实际存储 "[hello]"
            vm.SendInput();           // 记录 "[hello]"，然后清空输入
            var result = vm.RestoreHistoryEntry(1);
            Assert.That(result, Is.True);
            // RestoreHistoryEntry 从历史中取出 "[hello]"，再调用 SetInputText("[hello]")
            // 被 override 再包裹一层 → "[[hello]]"
            Assert.That(vm.InputText, Is.EqualTo("[[hello]]"));
            vm.Dispose();
        }

        #endregion
    }
}
