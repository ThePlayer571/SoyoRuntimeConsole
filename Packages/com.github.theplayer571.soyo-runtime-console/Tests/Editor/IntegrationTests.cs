using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// 集成测试与边界场景覆盖。
    /// 覆盖多个子系统协同工作的端到端流程，以及尚未被单元测试覆盖的边界情况。
    /// </summary>
    public class IntegrationTests
    {
        private enum TestColor
        {
            Red,
            Green,
            Blue
        }

        #region 集成测试用的命令 Fixture

        [TargetConsoleKey("Tests")]
        private static class ComplexParamFixture
        {
            public static Vector3 LastVector3 { get; set; }
            public static TestColor LastColor { get; set; }
            public static int[] LastArray { get; set; }

            [ConsoleCommand("itest_complex")]
            [CommandHelpText("Complex parameter test command.")]
            private static void ComplexCommand(
                Vector3 pos,
                [CommandParameter("color")] TestColor color,
                int[] values)
            {
                LastVector3 = pos;
                LastColor = color;
                LastArray = values;
            }
        }

        [TargetConsoleKey("Tests")]
        private static class DuplicateNameFixture
        {
            public static string LastCalled { get; set; }

            [ConsoleCommand("itest_dup")]
            private static void DupVersion1()
            {
                LastCalled = "v1";
                Debug.Log("dup_v1");
            }

            [ConsoleCommand("itest_dup")]
            [CommandHelpText("Duplicate name version 2")]
            private static void DupVersion2(int value)
            {
                LastCalled = "v2";
                Debug.Log("dup_v2");
            }
        }

        [TargetConsoleKey("Tests")]
        private static class RefParamFixture
        {
            [ConsoleCommand("itest_ref")]
            private static void RefCommand(ref int value)
            {
            }
        }

        [TargetConsoleKey("Tests")]
        private static class ManyTypesFixture
        {
            public static string LastResult { get; set; }

            [ConsoleCommand("itest_all_types")]
            private static void AllTypesCommand(
                int count,
                float speed,
                bool flag,
                string name)
            {
                LastResult = $"{count},{speed},{flag},{name}";
                Debug.Log($"AllTypes: {LastResult}");
            }
        }

        #endregion

        #region 端到端：复杂参数类型

        [Test]
        public void EndToEnd_Vector3EnumArray_ParsedAndExecutedCorrectly()
        {
            var (commands, helpTexts) = ConsoleAttributeScanner.ScanClass(typeof(ComplexParamFixture));
            var config = new ConsoleConfig(new ConsoleKey("ITest"), commands, helpTexts.Select(kv => (kv.Key, kv.Value)));
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterConsoleConfig(config)
                .Build();

            // 验证命令已注册
            Assert.That(console.Commands.Count, Is.EqualTo(1));
            Assert.That(console.Commands[0].CommandName.Name, Is.EqualTo("itest_complex"));

            // 验证帮助文本
            Assert.That(console.CommandHelpText[new CommandName("itest_complex")],
                Is.EqualTo("Complex parameter test command."));

            // 验证参数处理器类型
            var handlers = console.Commands[0].ParameterHandlers;
            Assert.That(handlers.Count, Is.EqualTo(3));
            Assert.That(handlers[0], Is.InstanceOf<Vector3ParameterHandler>());
            Assert.That(handlers[1], Is.InstanceOf<EnumParameterHandler>());
            Assert.That(handlers[2], Is.InstanceOf<ArrayParameterHandler<int>>());

            // 端到端执行
            console.SetInputText("itest_complex (1, 2, 3) Red [10, 20]");
            Assert.IsTrue(console.SendInput());

            Assert.That(ComplexParamFixture.LastVector3, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(ComplexParamFixture.LastColor, Is.EqualTo(TestColor.Red));
            Assert.That(ComplexParamFixture.LastArray, Is.EqualTo(new[] { 10, 20 }));
        }

        #endregion

        #region 重名命令

        [Test]
        public void DuplicateCommandName_BothRegistered_ExecutedByIndex()
        {
            var (commands, _) = ConsoleAttributeScanner.ScanClass(typeof(DuplicateNameFixture));
            var config = new ConsoleConfig(new ConsoleKey("ITest"), commands, null);
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterConsoleConfig(config)
                .Build();

            // 两个命令同名
            Assert.That(commands.Count, Is.EqualTo(2));
            Assert.That(commands[0].CommandName.Name, Is.EqualTo("itest_dup"));
            Assert.That(commands[1].CommandName.Name, Is.EqualTo("itest_dup"));

            // 一个有参数一个无：输入无参数时 Executable 选择无参版本
            DuplicateNameFixture.LastCalled = null;
            LogAssert.Expect(LogType.Log, "dup_v1");
            console.SetInputText("itest_dup");
            Assert.IsTrue(console.SendInput());
            Assert.That(DuplicateNameFixture.LastCalled, Is.EqualTo("v1"));

            // 输入带参数时选择有参版本
            DuplicateNameFixture.LastCalled = null;
            LogAssert.Expect(LogType.Log, "dup_v2");
            console.SetInputText("itest_dup 42");
            Assert.IsTrue(console.SendInput());
            Assert.That(DuplicateNameFixture.LastCalled, Is.EqualTo("v2"));
        }

        #endregion

        #region ref 参数警告

        [Test]
        public void RefParameter_WarnsButDoesNotSkip()
        {
            LogAssert.Expect(LogType.Warning,
                "[ConsoleCommand] 'Soyo.SoyoRuntimeConsole.Tests.Editor.IntegrationTests+RefParamFixture.RefCommand' " +
                "has ref/out parameter 'value', which may cause unexpected behavior.");

            var (commands, _) = ConsoleAttributeScanner.ScanClass(typeof(RefParamFixture));

            // ref 参数的方法不应被跳过
            Assert.That(commands.Count, Is.EqualTo(1));
        }

        #endregion

        #region 混合手动 + 扫描命令

        [Test]
        public void MixedManualAndScanned_AllCommandsPresent()
        {
            var manualCmd = new TestManualCommand("manual_mixed");

            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterCommand(manualCmd)
                .RegisterFromClass(typeof(ManyTypesFixture))
                .Build();

            Assert.That(console.Commands.Any(c => c.CommandName.Name == "manual_mixed"), Is.True);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "itest_all_types"), Is.True);
        }

        private class TestManualCommand : ConsoleCommandDefinition
        {
            public TestManualCommand(string name) : base(name, null)
            {
            }

            public override void Execute(System.Collections.Generic.IReadOnlyList<object> parameters, IConsole console)
            {
                Debug.Log("manual_mixed executed");
            }
        }

        #endregion

        #region 多类型参数端到端

        [Test]
        public void EndToEnd_MultipleBasicTypes_AllParsedCorrectly()
        {
            var (commands, _) = ConsoleAttributeScanner.ScanClass(typeof(ManyTypesFixture));
            var config = new ConsoleConfig(new ConsoleKey("ITest"), commands, null);
            var console = new ConsoleBuilder()
                .SetConsoleKey("Tests")
                .RegisterConsoleConfig(config)
                .Build();

            ManyTypesFixture.LastResult = null;

            LogAssert.Expect(LogType.Log, "AllTypes: 5,3.14,True,hello");
            console.SetInputText(@"itest_all_types 5 3.14 true ""hello""");
            Assert.IsTrue(console.SendInput());

            Assert.That(ManyTypesFixture.LastResult, Is.EqualTo("5,3.14,True,hello"));
        }

        #endregion

        #region ConsoleBuilder + ConsoleConfig 综合

        [Test]
        public void Builder_MultipleConfigs_CommandsMerged()
        {
            var config1 = new ConsoleConfig(
                new ConsoleKey("k1"),
                new ConsoleCommandDefinition[] { new TestManualCommand("cmd_a") },
                new[] { (new CommandName("cmd_a"), "Help A") });

            var config2 = new ConsoleConfig(
                new ConsoleKey("k2"),
                new ConsoleCommandDefinition[] { new TestManualCommand("cmd_b") },
                new[] { (new CommandName("cmd_b"), "Help B") });

            var console = new ConsoleBuilder()
                .SetConsoleKey("my_key")
                .RegisterConsoleConfig(config1)
                .RegisterConsoleConfig(config2)
                .Build();

            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("my_key")));
            Assert.That(console.Commands.Count, Is.EqualTo(2));
            Assert.That(console.CommandHelpText.Count, Is.EqualTo(2));
        }

        #endregion

    }
}
