using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="ConsoleBuilder"/> 的单元测试。
    /// </summary>
    public class ConsoleBuilderTests
    {
        #region 测试用的命令和 Fixture

        private static bool s_wasCalled;

        [ConsoleCommand("builder_test_cmd")]
        [CommandHelpText("A test command for ConsoleBuilder.")]
        private static void BuilderTestCommand()
        {
            s_wasCalled = true;
            Debug.Log("BuilderTestCommand executed");
        }

        private class TestCommand : ConsoleCommandDefinition
        {
            public TestCommand(string name) : base(name, null)
            {
            }

            public override void Execute(IReadOnlyList<object> parameters, IConsole console)
            {
                Debug.Log($"TestCommand '{CommandName.Name}' executed");
            }
        }

        #endregion

        [SetUp]
        public void SetUp()
        {
            s_wasCalled = false;
        }

        #region 基本构建

        [Test]
        public void Build_ManualCommand_ExecutesSuccessfully()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("Test")
                .RegisterCommand(new TestCommand("manual_cmd"))
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("Test")));
            Assert.That(console.Commands.Count, Is.EqualTo(1));
            Assert.That(console.Commands[0].CommandName.Name, Is.EqualTo("manual_cmd"));

            LogAssert.Expect(LogType.Log, "TestCommand 'manual_cmd' executed");
            console.SetInputText("manual_cmd");
            Assert.IsTrue(console.SendInput());
        }

        [Test]
        public void Build_ChainedCall_AllMethodsWork()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("chain_test")
                .RegisterCommand(new TestCommand("cmd1"))
                .RegisterCommand(new TestCommand("cmd2"))
                .RegisterHelpText(new CommandName("cmd1"), "First command")
                .Build();

            Assert.That(console.Commands.Count, Is.EqualTo(2));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(1));
            Assert.That(console.CommandHelpText[new CommandName("cmd1")], Is.EqualTo("First command"));
        }

        [Test]
        public void Build_NoConsoleKey_UsesEmptyKey()
        {
            var console = new ConsoleBuilder()
                .RegisterCommand(new TestCommand("no_key_cmd"))
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey(string.Empty)));
        }

        [Test]
        public void Build_MultipleSetConsoleKey_LastWins()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("first")
                .SetConsoleKey("second")
                .SetConsoleKey(new ConsoleKey("third"))
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("third")));
        }

        #endregion

        #region HelpText

        [Test]
        public void RegisterHelpText_Duplicate_WarnsAndKeepsFirst()
        {
            var builder = new ConsoleBuilder();
            builder.RegisterHelpText(new CommandName("dup_cmd"), "First help text");

            LogAssert.Expect(LogType.Warning,
                "[ConsoleBuilder] Help text for command 'dup_cmd' is already registered. Ignoring duplicate.");

            builder.RegisterHelpText(new CommandName("dup_cmd"), "Second help text");

            var config = builder.BuildConfig();
            Assert.That(config.CommandHelpText[new CommandName("dup_cmd")], Is.EqualTo("First help text"));
        }

        #endregion

        #region RegisterConsoleConfig 合并

        [Test]
        public void RegisterConsoleConfig_MergesCommandsAndHelpTexts()
        {
            var config1 = new ConsoleConfig(
                new ConsoleKey("ConfigKey"),
                new ConsoleCommandDefinition[] { new TestCommand("from_config") },
                new[] { (new CommandName("from_config"), "From config") });

            var console = new ConsoleBuilder()
                .RegisterCommand(new TestCommand("manual"))
                .RegisterConsoleConfig(config1)
                .Build();

            Assert.That(console.Commands.Count, Is.EqualTo(2));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(1));
            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("ConfigKey")));
        }

        [Test]
        public void RegisterConsoleConfig_KeyAlreadySet_KeyNotOverwritten()
        {
            var config = new ConsoleConfig(
                new ConsoleKey("ConfigKey"),
                null, null);

            var console = new ConsoleBuilder()
                .SetConsoleKey("MyKey")
                .RegisterConsoleConfig(config)
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("MyKey")));
        }

        [Test]
        public void RegisterConsoleConfig_InvalidConfig_Ignored()
        {
            var invalidConfig = default(ConsoleConfig); // IsValid = false

            Assert.DoesNotThrow(() =>
            {
                new ConsoleBuilder().RegisterConsoleConfig(invalidConfig);
            });
        }

        #endregion

        #region RegisterFromClass

        [Test]
        public void RegisterFromClass_AttributeCommand_FoundAndExecutable()
        {
            s_wasCalled = false;

            var console = new ConsoleBuilder()
                .SetConsoleKey("Test")
                .RegisterFromClass<ConsoleBuilderTests>()
                .Build();

            Assert.That(console.Commands.Any(c => c.CommandName.Name == "builder_test_cmd"), Is.True);

            // 验证帮助文本
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(1));
            Assert.That(console.CommandHelpText[new CommandName("builder_test_cmd")],
                Is.EqualTo("A test command for ConsoleBuilder."));

            // 端到端执行
            LogAssert.Expect(LogType.Log, "BuilderTestCommand executed");
            console.SetInputText("builder_test_cmd");
            Assert.IsTrue(console.SendInput());
            Assert.IsTrue(s_wasCalled);
        }

        [Test]
        public void RegisterFromClass_GenericOverload_WorksSame()
        {
            var console = new ConsoleBuilder()
                .RegisterFromClass(typeof(ConsoleBuilderTests))
                .Build();

            Assert.That(console.Commands.Any(c => c.CommandName.Name == "builder_test_cmd"), Is.True);
        }

        #endregion

        #region BuildConfig

        [Test]
        public void BuildConfig_ReturnsValidConfig()
        {
            var config = new ConsoleBuilder()
                .SetConsoleKey("cfg_test")
                .RegisterCommand(new TestCommand("cfg_cmd"))
                .BuildConfig();

            Assert.IsTrue(config.IsValid);
            Assert.That(config.Key, Is.EqualTo(new ConsoleKey("cfg_test")));
            Assert.That(config.CommandDefinitions.Count, Is.EqualTo(1));
            Assert.That(config.CommandDefinitions[0].CommandName.Name, Is.EqualTo("cfg_cmd"));
        }

        #endregion

        #region 边界情况

        [Test]
        public void Build_EmptyBuilder_CreatesValidConsole()
        {
            var console = new ConsoleBuilder().Build();

            Assert.That(console.Commands.Count, Is.EqualTo(0));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(0));

            console.SetInputText("nothing");
            Assert.IsFalse(console.SendInput());
        }

        #endregion
    }
}
