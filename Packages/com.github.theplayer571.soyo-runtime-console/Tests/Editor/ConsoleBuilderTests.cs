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
            var console = new ConsoleBuilder("Test")
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
            var console = new ConsoleBuilder("chain_test")
                .RegisterCommand(new TestCommand("cmd1"))
                .RegisterCommand(new TestCommand("cmd2"))
                .RegisterHelpText(new CommandName("cmd1"), "First command")
                .Build();

            Assert.That(console.Commands.Count, Is.EqualTo(2));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(1));
            Assert.That(console.CommandHelpText[new CommandName("cmd1")], Is.EqualTo("First command"));
        }


        #endregion

        #region HelpText

        [Test]
        public void RegisterHelpText_Duplicate_WarnsAndKeepsFirst()
        {
            var builder = new ConsoleBuilder("test");
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

            var console = new ConsoleBuilder("BuilderKey")
                .RegisterCommand(new TestCommand("manual"))
                .RegisterConsoleConfig(config1)
                .Build();

            Assert.That(console.Commands.Count, Is.EqualTo(2));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(1));
            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("BuilderKey")));
        }

        [Test]
        public void RegisterConsoleConfig_KeyAlreadySet_KeyNotOverwritten()
        {
            var config = new ConsoleConfig(
                new ConsoleKey("ConfigKey"),
                null, null);

            var console = new ConsoleBuilder("MyKey")
                .RegisterConsoleConfig(config)
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("MyKey")));
        }

        [Test]
        public void RegisterConsoleConfig_InvalidConfig_Ignored()
        {
            var invalidConfig = default(ConsoleConfig); // IsValid = false

            Assert.DoesNotThrow(() => { new ConsoleBuilder("test").RegisterConsoleConfig(invalidConfig); });
        }

        #endregion

        #region RegisterFromClass

        [Test]
        public void RegisterFromClass_AttributeCommand_FoundAndExecutable()
        {
            s_wasCalled = false;

            var console = new ConsoleBuilder("Tests")
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
            var console = new ConsoleBuilder("Tests")
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
            var console = new ConsoleBuilder("Tests")
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
            var builder1 = new ConsoleBuilder("Tests")
                .RegisterFromClass<DualAttributeFixture>();
            var console1 = builder1.Build();

            // Builder 2: 不注册任何自定义处理器
            var builder2 = new ConsoleBuilder("Tests");
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
            var config = new ConsoleBuilder("cfg_test")
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
            var builder = new ConsoleBuilder("idem_test")
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
            var builder = new ConsoleBuilder("chain_test")
                .RegisterDynamicHandler((type, name, _) => null);

            // 验证流畅链式调用不崩溃
            Assert.DoesNotThrow(() => builder.Build());
        }

        [Test]
        public void RegisterDynamicHandler_BuildSucceeds()
        {
            var builder = new ConsoleBuilder("Tests")
                .RegisterDynamicHandler((type, name, _) =>
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
            var builder = new ConsoleBuilder("test")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name));

            Assert.DoesNotThrow(() => builder.Build());
        }

        [Test]
        public void RegisterT_HandlerFactory_CommandResolvesHandlerCorrectly()
        {
            var console = new ConsoleBuilder("Tests")
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

            var console = new ConsoleBuilder("Tests")
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
            var console = new ConsoleBuilder("Tests")
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
            var console = new ConsoleBuilder("Tests")
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

            var console = new ConsoleBuilder("Tests")
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
            var builder1 = new ConsoleBuilder("Tests")
                .Register<RegisterTestValue>((type, name) => new RegisterTestValueHandler(name))
                .RegisterFromClass<RegisterTestCommands>();
            var console1 = builder1.Build();

            // Builder 2: 不注册自定义处理器
            var builder2 = new ConsoleBuilder("Tests")
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
            var console = new ConsoleBuilder("test").Build();

            Assert.That(console.Commands.Count, Is.EqualTo(0));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(0));

            console.SetInputText("nothing");
            Assert.IsFalse(console.SendInput());
        }

        #endregion

        #region FixedField

        /// <summary>
        /// 带 [FixedField] 参数的命令 Fixture。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class BuilderFixedFieldFixture
        {
            public static bool WasCalled { get; set; }

            [ConsoleCommand("builder_fixed")]
            private static void BuilderFixed(
                [FixedField("list")] object action)
            {
                WasCalled = true;
                Debug.Log("BuilderFixedField executed");
            }
        }

        [Test]
        public void RegisterFromClass_FixedField_CreatesFixedFieldHandler()
        {
            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<BuilderFixedFieldFixture>()
                .Build();

            var cmd = console.Commands.FirstOrDefault(c => c.CommandName.Name == "builder_fixed");
            Assert.IsNotNull(cmd);
            Assert.That(cmd.ParameterHandlers.Count, Is.EqualTo(1));
            Assert.That(cmd.ParameterHandlers[0], Is.InstanceOf<FixedFieldParameterHandler>());
            Assert.That(cmd.ParameterHandlers[0].GetDescription().Name, Is.EqualTo("list"));
        }

        [Test]
        public void RegisterFromClass_FixedField_ExecutesSuccessfully()
        {
            BuilderFixedFieldFixture.WasCalled = false;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<BuilderFixedFieldFixture>()
                .Build();

            LogAssert.Expect(LogType.Log, "BuilderFixedField executed");
            console.SetInputText("builder_fixed list");
            Assert.IsTrue(console.SendInput());
            Assert.IsTrue(BuilderFixedFieldFixture.WasCalled);
        }

        [Test]
        public void RegisterFromClass_FixedField_WrongValue_DoesNotExecute()
        {
            BuilderFixedFieldFixture.WasCalled = false;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<BuilderFixedFieldFixture>()
                .Build();

            // 输入不匹配固定字段 "list"
            console.SetInputText("builder_fixed wrong");
            Assert.IsFalse(console.SendInput());
            Assert.IsFalse(BuilderFixedFieldFixture.WasCalled);
        }

        #endregion

        #region 默认参数

        /// <summary>
        /// 带默认参数的命令 Fixture：cmd(int a, int b = 1, int c = 2)。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DefaultParamTwoDefaultsFixture
        {
            public static int LastA { get; set; }
            public static int LastB { get; set; }
            public static int LastC { get; set; }

            [ConsoleCommand("default_test")]
            private static void DefaultTest(int a, int b = 1, int c = 2)
            {
                LastA = a;
                LastB = b;
                LastC = c;
            }
        }

        /// <summary>
        /// 全部参数都有默认值：cmd(int a = 10, int b = 20)。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DefaultParamAllOptionalFixture
        {
            public static int LastA { get; set; }
            public static int LastB { get; set; }

            [ConsoleCommand("default_all_opt")]
            private static void DefaultAllOpt(int a = 10, int b = 20)
            {
                LastA = a;
                LastB = b;
            }
        }

        /// <summary>
        /// 单个参数带默认值：cmd(int value = 42)。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DefaultParamSingleFixture
        {
            public static int LastValue { get; set; }

            [ConsoleCommand("default_single")]
            private static void DefaultSingle(int value = 42)
            {
                LastValue = value;
            }
        }

        /// <summary>
        /// 混合类型默认参数：cmd(int count, string name = "default", float ratio = 1.0f)。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DefaultParamMixedFixture
        {
            public static int LastCount { get; set; }
            public static string LastName { get; set; }
            public static float LastRatio { get; set; }

            [ConsoleCommand("default_mixed")]
            private static void DefaultMixed(int count, string name = "default", float ratio = 1.0f)
            {
                LastCount = count;
                LastName = name;
                LastRatio = ratio;
            }
        }

        /// <summary>
        /// 无默认参数的控制组 Fixture。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private class DefaultParamNoDefaultsFixture
        {
            public static bool WasCalled { get; set; }

            [ConsoleCommand("default_no_def")]
            private static void DefaultNoDefaults(int a, int b)
            {
                WasCalled = true;
            }
        }

        [Test]
        public void DefaultParam_ExpandsToMultipleVariants()
        {
            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamTwoDefaultsFixture>()
                .Build();

            // cmd(int a, int b = 1, int c = 2) 应生成 3 个变体
            // 变体 1: 1 handler (a)
            // 变体 2: 2 handlers (a, b)
            // 变体 3: 3 handlers (a, b, c)
            var commands = console.Commands
                .Where(c => c.CommandName.Name == "default_test")
                .ToList();

            Assert.That(commands.Count, Is.EqualTo(3),
                "Should generate 3 variants for method with 2 default parameters.");

            var handlerCounts = commands.Select(c => c.ParameterHandlers.Count).OrderBy(n => n).ToList();
            Assert.That(handlerCounts, Is.EqualTo(new[] { 1, 2, 3 }),
                "Variants should have 1, 2, and 3 handlers respectively.");
        }

        [Test]
        public void DefaultParam_UsesDefaults()
        {
            DefaultParamTwoDefaultsFixture.LastA = 0;
            DefaultParamTwoDefaultsFixture.LastB = 0;
            DefaultParamTwoDefaultsFixture.LastC = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamTwoDefaultsFixture>()
                .Build();

            // 只提供第一个参数
            console.SetInputText("default_test 100");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamTwoDefaultsFixture.LastA, Is.EqualTo(100));
            Assert.That(DefaultParamTwoDefaultsFixture.LastB, Is.EqualTo(1),
                "Second parameter should use default value 1.");
            Assert.That(DefaultParamTwoDefaultsFixture.LastC, Is.EqualTo(2),
                "Third parameter should use default value 2.");
        }

        [Test]
        public void DefaultParam_AllArgsProvided()
        {
            DefaultParamTwoDefaultsFixture.LastA = 0;
            DefaultParamTwoDefaultsFixture.LastB = 0;
            DefaultParamTwoDefaultsFixture.LastC = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamTwoDefaultsFixture>()
                .Build();

            // 提供全部三个参数
            console.SetInputText("default_test 10 20 30");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamTwoDefaultsFixture.LastA, Is.EqualTo(10));
            Assert.That(DefaultParamTwoDefaultsFixture.LastB, Is.EqualTo(20));
            Assert.That(DefaultParamTwoDefaultsFixture.LastC, Is.EqualTo(30));
        }

        [Test]
        public void DefaultParam_AllOptional_NoArgsProvided()
        {
            DefaultParamAllOptionalFixture.LastA = 0;
            DefaultParamAllOptionalFixture.LastB = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamAllOptionalFixture>()
                .Build();

            // 不提供参数
            console.SetInputText("default_all_opt");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamAllOptionalFixture.LastA, Is.EqualTo(10),
                "Should use default value 10.");
            Assert.That(DefaultParamAllOptionalFixture.LastB, Is.EqualTo(20),
                "Should use default value 20.");
        }

        [Test]
        public void DefaultParam_AllOptional_SomeArgsProvided()
        {
            DefaultParamAllOptionalFixture.LastA = 0;
            DefaultParamAllOptionalFixture.LastB = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamAllOptionalFixture>()
                .Build();

            // 只提供第一个参数
            console.SetInputText("default_all_opt 99");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamAllOptionalFixture.LastA, Is.EqualTo(99));
            Assert.That(DefaultParamAllOptionalFixture.LastB, Is.EqualTo(20),
                "Should use default value 20.");
        }

        [Test]
        public void DefaultParam_SingleOptional_NoArgs()
        {
            DefaultParamSingleFixture.LastValue = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamSingleFixture>()
                .Build();

            // 无参
            console.SetInputText("default_single");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamSingleFixture.LastValue, Is.EqualTo(42),
                "Should use default value 42.");
        }

        [Test]
        public void DefaultParam_SingleOptional_WithArg()
        {
            DefaultParamSingleFixture.LastValue = 0;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamSingleFixture>()
                .Build();

            // 提供参数
            console.SetInputText("default_single 77");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamSingleFixture.LastValue, Is.EqualTo(77));
        }

        [Test]
        public void DefaultParam_MixedTypes_UsesDefaults()
        {
            DefaultParamMixedFixture.LastCount = 0;
            DefaultParamMixedFixture.LastName = null;
            DefaultParamMixedFixture.LastRatio = 0f;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamMixedFixture>()
                .Build();

            // 只提供必选参数
            console.SetInputText("default_mixed 5");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamMixedFixture.LastCount, Is.EqualTo(5));
            Assert.That(DefaultParamMixedFixture.LastName, Is.EqualTo("default"),
                "Should use default value.");
            Assert.That(DefaultParamMixedFixture.LastRatio, Is.EqualTo(1.0f),
                "Should use default value.");
        }

        [Test]
        public void DefaultParam_MixedTypes_AllArgsProvided()
        {
            DefaultParamMixedFixture.LastCount = 0;
            DefaultParamMixedFixture.LastName = null;
            DefaultParamMixedFixture.LastRatio = 0f;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamMixedFixture>()
                .Build();

            // 提供全部参数
            console.SetInputText("default_mixed 3 hello 2.5");
            Assert.IsTrue(console.SendInput());

            Assert.That(DefaultParamMixedFixture.LastCount, Is.EqualTo(3));
            Assert.That(DefaultParamMixedFixture.LastName, Is.EqualTo("hello"));
            Assert.That(DefaultParamMixedFixture.LastRatio, Is.EqualTo(2.5f));
        }

        [Test]
        public void DefaultParam_NonDefaultMethod_Unaffected()
        {
            DefaultParamNoDefaultsFixture.WasCalled = false;

            var console = new ConsoleBuilder("Tests")
                .RegisterFromClass<DefaultParamNoDefaultsFixture>()
                .Build();

            // 无默认参数的方法应只有 1 个命令定义
            var commands = console.Commands
                .Where(c => c.CommandName.Name == "default_no_def")
                .ToList();

            Assert.That(commands.Count, Is.EqualTo(1),
                "Method without default parameters should have exactly 1 command definition.");
            Assert.That(commands[0].ParameterHandlers.Count, Is.EqualTo(2));

            // 执行验证
            console.SetInputText("default_no_def 1 2");
            Assert.IsTrue(console.SendInput());
            Assert.IsTrue(DefaultParamNoDefaultsFixture.WasCalled);
        }

        #endregion
    }
}