// 这些测试通过完整的 IConsole 管线（SetInputText -> SendInput -> CommandLineAnalyzer.Analyze -> Execute）
// 来测试 ConsoleBase，同时也间接覆盖了 CommandLineAnalyzer 的解析逻辑。
// 不直接测试 CommandLineAnalyzeResult，而是通过 ConsoleBase 的 SendInput 行为来验证解析和执行的正确性。

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public partial class ConsoleBaseTests
    {
        private TestConsole _console;
        private ConsoleViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _console = new TestConsole();
            _viewModel = new ConsoleViewModel(new TestConsole());
        }

        [TearDown]
        public void TearDown()
        {
            _viewModel?.Dispose();
        }

        #region A. InputText / SetInputText

        [Test]
        public void InputText_Default_IsEmptyString()
        {
            Assert.That(_console.InputText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SetInputText_String_InputTextReturnsIt()
        {
            _console.SetInputText("hello");
            Assert.That(_console.InputText, Is.EqualTo("hello"));
        }

        [Test]
        public void SetInputText_OverwritesPreviousValue()
        {
            _console.SetInputText("first");
            _console.SetInputText("second");
            Assert.That(_console.InputText, Is.EqualTo("second"));
        }

        [Test]
        public void SetInputText_EmptyString_Overwrites()
        {
            _console.SetInputText("hello");
            _console.SetInputText(string.Empty);
            Assert.That(_console.InputText, Is.EqualTo(string.Empty));
        }

        #endregion

        #region B. Commands

        [Test]
        public void Commands_ReturnsRegisteredCommands()
        {
            Assert.That(_console.Commands.Count, Is.EqualTo(3));

            var commandNames = _console.Commands.Select(c => c.CommandName.Name).ToList();
            Assert.That(commandNames, Does.Contain("hello"));
            Assert.That(commandNames, Does.Contain("hello_world"));
            // "hello" 应出现两次（HelloCommand 和 HelloWithNameCommand）
            Assert.That(commandNames.Count(n => n == "hello"), Is.EqualTo(2));
        }

        #endregion

        #region C. CommandHelpText

        [Test]
        public void CommandHelpText_DefaultNull_IsEmpty()
        {
            Assert.That(_console.CommandHelpText.Count, Is.EqualTo(0));
        }

        [Test]
        public void CommandHelpText_WithEntries_ContainsExpectedValues()
        {
            var consoleWithHelp = new TestConsoleWithHelp();

            Assert.That(consoleWithHelp.CommandHelpText.Count, Is.EqualTo(2));
            Assert.That(consoleWithHelp.CommandHelpText[new CommandName("hello")], Is.EqualTo("Says hello"));
            Assert.That(consoleWithHelp.CommandHelpText[new CommandName("hello_world")],
                Is.EqualTo("Says hello world"));
        }

        #endregion

        #region D. CommandLineAnalyzer

        [Test]
        public void CommandLineAnalyzer_IsNotNull()
        {
            Assert.That(_console.CommandLineAnalyzer, Is.Not.Null);
        }

        #endregion

        #region E. SendInput 成功场景

        [Test]
        public void SendInput_HelloCommand_LogsHelloAndReturnsTrue()
        {
            LogAssert.Expect(LogType.Log, "Hello!");
            _console.SetInputText("hello");
            var result = _console.SendInput();
            Assert.That(result, Is.True);
        }

        [Test]
        public void SendInput_HelloName_LogsGreetingAndReturnsTrue()
        {
            LogAssert.Expect(LogType.Log, "Hello World");
            _console.SetInputText("hello World");
            var result = _console.SendInput();
            Assert.That(result, Is.True);
        }

        [Test]
        public void SendInput_HelloNameQuoted_LogsGreetingAndReturnsTrue()
        {
            LogAssert.Expect(LogType.Log, "Hello John Doe");
            _console.SetInputText("hello \"John Doe\"");
            var result = _console.SendInput();
            Assert.That(result, Is.True);
        }

        [Test]
        public void SendInput_HelloWorldCommand_LogsHelloWorldAndReturnsTrue()
        {
            LogAssert.Expect(LogType.Log, "Hello World");
            _console.SetInputText("hello_world");
            var result = _console.SendInput();
            Assert.That(result, Is.True);
        }

        #endregion

        #region F. SendInput 失败 / 边界场景

        [Test]
        public void SendInput_EmptyInput_ReturnsFalse()
        {
            _console.SetInputText(string.Empty);
            var result = _console.SendInput();
            Assert.That(result, Is.False);
        }

        [Test]
        public void SendInput_NonexistentCommand_ReturnsFalse()
        {
            _console.SetInputText("nonexistent");
            var result = _console.SendInput();
            Assert.That(result, Is.False);
        }

        [Test]
        public void SendInput_PartialCommandName_ReturnsFalse()
        {
            // "hel" 是 "hello" 和 "hello_world" 的前缀，
            // 解析器会匹配到候选项但没有可执行的命令
            _console.SetInputText("hel");
            var result = _console.SendInput();
            Assert.That(result, Is.False);
        }

        #endregion

        #region G. chosenCommandIndex

        [Test]
        public void SendInput_ChosenCommandIndex0_UsesFirstCandidate()
        {
            LogAssert.Expect(LogType.Log, "Hello!");
            _console.SetInputText("hello");
            var result = _console.SendInput(chosenCommandIndex: 0);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SendInput_ChosenCommandIndexNonExecutable_FallsBackToFirstExecutable()
        {
            // "hello" 的候选项：[0] HelloCommand(可执行), [1] HelloWithNameCommand(不可执行)
            // 选择 index=1 时应回退到第一个可执行的命令（HelloCommand）
            LogAssert.Expect(LogType.Log, "Hello!");
            _console.SetInputText("hello");
            var result = _console.SendInput(chosenCommandIndex: 1);
            Assert.That(result, Is.True);
        }

        [Test]
        public void SendInput_ChosenCommandIndexOutOfRange_FallsBackToFirstExecutable()
        {
            // index 越界时 ElementAtOrDefault 返回 default(ConsoleCommandDesc)，Executable=false
            // 应回退到第一个可执行的命令
            LogAssert.Expect(LogType.Log, "Hello!");
            _console.SetInputText("hello");
            var result = _console.SendInput(chosenCommandIndex: 99);
            Assert.That(result, Is.True);
        }

        #endregion
    }
}