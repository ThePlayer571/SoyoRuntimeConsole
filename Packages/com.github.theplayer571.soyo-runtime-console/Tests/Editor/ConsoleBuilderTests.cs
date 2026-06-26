using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Helpers;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="ConsoleBuilder"/> 的单元测试。
    /// </summary>
    [TargetConsoleKey("Tests")]
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

        #region 同时包含 [ConsoleCommand] 和 [ConsoleParameterHandler] 的测试 Fixture

        /// <summary>
        /// 用于验证自定义参数类型的简单结构体。
        /// </summary>
        public struct BuilderTestPoint
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// 同时包含 [ConsoleCommand] 和 [ConsoleParameterHandler] 的 Fixture，
        /// 用于验证 RegisterFromClass 一次性扫描两种 Attribute。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DualAttributeFixture
        {
            public static BuilderTestPoint LastPoint { get; set; }

            /// <summary>
            /// [ConsoleParameterHandler] — 注册 BuilderTestPoint 的参数处理器。
            /// </summary>
            [ConsoleParameterHandler]
            private static BuilderTestPoint MakePoint(int x, int y)
            {
                return new BuilderTestPoint { X = x, Y = y };
            }

            /// <summary>
            /// [ConsoleCommand] — 使用 BuilderTestPoint 作为参数类型的命令。
            /// </summary>
            [ConsoleCommand("dual_test")]
            [CommandHelpText("Dual attribute test command.")]
            private static void DualCommand(BuilderTestPoint point)
            {
                LastPoint = point;
                Debug.Log($"DualCommand: ({point.X}, {point.Y})");
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

            Assert.DoesNotThrow(() => { new ConsoleBuilder().RegisterConsoleConfig(invalidConfig); });
        }

        #endregion

        #region RegisterFromClass

        [Test]
        public void RegisterFromClass_AttributeCommand_FoundAndExecutable()
        {
            s_wasCalled = false;

            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
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

        /// <summary>
        /// 验证 RegisterFromClass 同时扫描了 [ConsoleCommand] 和 [ConsoleParameterHandler]。
        /// DualAttributeFixture 包含自定义类型 BuilderTestPoint 的处理器和一条使用该类型的命令。
        /// </summary>
        [Test]
        public void RegisterFromClass_WithParameterHandlers_RegistersBoth()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterFromClass<DualAttributeFixture>()
                .Build();

            // 验证命令已注册
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "dual_test"), Is.True);

            // 验证帮助文本
            Assert.That(console.CommandHelpText[new CommandName("dual_test")],
                Is.EqualTo("Dual attribute test command."));

            // 验证参数处理器已通过 [ConsoleParameterHandler] 注册（自定义类型 BuilderTestPoint）
            var cmd = console.Commands.First(c => c.CommandName.Name == "dual_test");
            Assert.That(cmd.ParameterHandlers.Count, Is.EqualTo(1));
            Assert.That(cmd.ParameterHandlers[0], Is.InstanceOf<TupleParameterHandler>());
        }

        /// <summary>
        /// 验证两个 ConsoleBuilder 实例的 ParameterHandlerRegistry 互相隔离。
        /// </summary>
        [Test]
        public void RegisterFromClass_BuilderIsolation_TwoBuildersDoNotShareState()
        {
            // Builder 1: 注册自定义类型处理器
            var builder1 = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterFromClass<DualAttributeFixture>();
            var console1 = builder1.Build();

            // Builder 2: 不注册任何自定义处理器
            var builder2 = new ConsoleBuilder()
                .SetConsoleKey("Tests");
            var console2 = builder2.Build();

            // console1 应有 dual_test 命令
            Assert.That(console1.Commands.Any(c => c.CommandName.Name == "dual_test"), Is.True);

            // console2 不应有 dual_test 命令（未扫描 DualAttributeFixture）
            Assert.That(console2.Commands.Any(c => c.CommandName.Name == "dual_test"), Is.False);
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

        /// <summary>
        /// BuildConfig 多次调用返回相同结果（幂等性）。
        /// </summary>
        [Test]
        public void BuildConfig_CalledMultipleTimes_ReturnsSameResult()
        {
            var builder = new ConsoleBuilder()
                .SetConsoleKey("idem_test")
                .RegisterCommand(new TestCommand("idem_cmd"));

            var config1 = builder.BuildConfig();
            var config2 = builder.BuildConfig();

            Assert.That(config1.Key, Is.EqualTo(config2.Key));
            Assert.That(config1.CommandDefinitions.Count, Is.EqualTo(config2.CommandDefinitions.Count));
        }

        #endregion

        #region 动态处理器

        [Test]
        public void RegisterDynamicHandler_FluentApi_ReturnsBuilder()
        {
            var builder = new ConsoleBuilder()
                .SetConsoleKey("chain_test")
                .RegisterDynamicHandler((type, name) => null);

            // 验证流畅链式调用不崩溃
            Assert.DoesNotThrow(() => builder.Build());
        }

        [Test]
        public void RegisterDynamicHandler_BuildSucceeds()
        {
            var builder = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterDynamicHandler((type, name) =>
                    type == typeof(System.Version)
                        ? new StringParameterHandler(name ?? "Version")
                        : null);

            var console = builder.Build();

            Assert.IsNotNull(console);
            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("Tests")));
        }

        #endregion

        #region Register&lt;T&gt; — 类型绑定注册

        /// <summary>
        /// 用于类型绑定注册测试的自定义类型。
        /// </summary>
        public struct RegisterTestValue
        {
            public int Value;
        }

        /// <summary>
        /// 用于类型绑定注册测试的自定义参数处理器。
        /// </summary>
        private class RegisterTestValueHandler : SpaceSplitParameterHandlerBase
        {
            public RegisterTestValueHandler(string name) : base(name, "RegisterTestValueHandler")
            {
            }

            public override IEnumerable<string> GetCandidates(string parameter)
            {
                yield break;
            }

            public override bool IsValid(string parameter)
            {
                return int.TryParse(parameter, out _);
            }

            public override object Parse(string parameter)
            {
                return new RegisterTestValue { Value = int.Parse(parameter) };
            }

            public override bool IsInitialized => true;
        }

        /// <summary>
        /// 包含使用 RegisterTestValue 类型的命令的 Fixture。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class RegisterTestCommands
        {
            public static RegisterTestValue LastValue { get; set; }

            [ConsoleCommand("register_test_cmd")]
            [CommandHelpText("Test command using RegisterTestValue.")]
            private static void TestCommand(RegisterTestValue value)
            {
                LastValue = value;
                Debug.Log($"RegisterTestCommand: value={value.Value}");
            }
        }

        [Test]
        public void RegisterT_HandlerFactory_FluentApi_ReturnsBuilder()
        {
            var builder = new ConsoleBuilder()
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name));

            Assert.DoesNotThrow(() => builder.Build());
        }

        [Test]
        public void RegisterT_HandlerFactory_CommandResolvesHandlerCorrectly()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name))
                .RegisterFromClass<RegisterTestCommands>()
                .Build();

            // 验证命令已注册
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "register_test_cmd"), Is.True);

            // 验证参数处理器类型正确（不是降级的 StringParameterHandler）
            var cmd = console.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd.ParameterHandlers.Count, Is.EqualTo(1));
            Assert.That(cmd.ParameterHandlers[0], Is.InstanceOf<RegisterTestValueHandler>());
        }

        [Test]
        public void RegisterT_HandlerFactory_ExecutesSuccessfully()
        {
            RegisterTestCommands.LastValue = default;

            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name))
                .RegisterFromClass<RegisterTestCommands>()
                .Build();

            LogAssert.Expect(LogType.Log, "RegisterTestCommand: value=42");
            console.SetInputText("register_test_cmd 42");
            Assert.IsTrue(console.SendInput());
            Assert.That(RegisterTestCommands.LastValue.Value, Is.EqualTo(42));
        }

        [Test]
        public void RegisterT_HandlerFactory_MultipleRegistrations_ComposedAsComposite()
        {
            // 同一类型注册多个工厂 — 自动组合为 CompositeParameterHandler
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name))
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name + "_alt"))
                .RegisterFromClass<RegisterTestCommands>()
                .Build();

            var cmd = console.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd.ParameterHandlers[0], Is.InstanceOf<CompositeParameterHandler>());
        }

        [Test]
        public void Register_NonGeneric_TypeParameter_WorksSameAsGeneric()
        {
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register(typeof(RegisterTestValue), (type, name) => new RegisterTestValueHandler(name))
                .RegisterFromClass<RegisterTestCommands>()
                .Build();

            var cmd = console.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd.ParameterHandlers[0], Is.InstanceOf<RegisterTestValueHandler>());
        }

        [Test]
        public void RegisterT_HandlerInstance_ConvenienceOverload()
        {
            var handlerInstance = new RegisterTestValueHandler("my_value");

            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register<RegisterTestValue>(handlerInstance)
                .RegisterFromClass<RegisterTestCommands>()
                .Build();

            var cmd = console.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd.ParameterHandlers[0], Is.SameAs(handlerInstance));
        }

        [Test]
        public void RegisterT_BuilderIsolation_DoesNotLeakToOtherBuilder()
        {
            // Builder 1: 注册自定义类型处理器
            var builder1 = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name))
                .RegisterFromClass<RegisterTestCommands>();
            var console1 = builder1.Build();

            // Builder 2: 不注册自定义处理器
            var builder2 = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterFromClass<RegisterTestCommands>();
            var console2 = builder2.Build();

            // 两个 Builder 都有 register_test_cmd 命令
            Assert.That(console1.Commands.Any(c => c.CommandName.Name == "register_test_cmd"), Is.True);
            Assert.That(console2.Commands.Any(c => c.CommandName.Name == "register_test_cmd"), Is.True);

            // console1 使用自定义处理器
            var cmd1 = console1.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd1.ParameterHandlers[0], Is.InstanceOf<RegisterTestValueHandler>());

            // console2 降级为 StringParameterHandler（未注册自定义处理器）
            var cmd2 = console2.Commands.First(c => c.CommandName.Name == "register_test_cmd");
            Assert.That(cmd2.ParameterHandlers[0], Is.InstanceOf<StringParameterHandler>());
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