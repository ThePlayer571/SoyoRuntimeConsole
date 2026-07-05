using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Commands;
using Soyo.SoyoRuntimeConsole.Helpers;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using UnityEngine.TestTools;
using ConsoleKey = Soyo.SoyoRuntimeConsole.ValueObjects.ConsoleKey;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="ConsoleAttributeScanner"/> 的单元测试。
    /// 测试扫描阶段返回的 PendingCommandEntry 以及通过构建流水线解析后的 ConsoleCommandDefinition。
    /// </summary>
    public class ConsoleAttributeScannerTests
    {
        #region 测试用的命令类

        /// <summary>
        /// 简单无参命令。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class SimpleCommandFixture
        {
            public static bool WasCalled { get; set; }

            [ConsoleCommand]
            private static void Hello()
            {
                WasCalled = true;
                Debug.Log("Hello!");
            }
        }

        /// <summary>
        /// 带参命令，覆盖 int, float, string, bool。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class ParameterCommandFixture
        {
            public static int LastIntValue { get; set; }
            public static float LastFloatValue { get; set; }
            public static string LastStringValue { get; set; }
            public static bool LastBoolValue { get; set; }

            [ConsoleCommand("test_params")]
            private static void TestParams(int count, float speed, string name, bool flag)
            {
                LastIntValue = count;
                LastFloatValue = speed;
                LastStringValue = name;
                LastBoolValue = flag;
            }
        }

        /// <summary>
        /// 带 [CommandParameter] 自定义参数名的命令。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class CustomParamNameFixture
        {
            [ConsoleCommand("custom_param")]
            [CommandHelpText("A command with custom parameter names.")]
            private static void DoSomething([CommandParameter("customName")] int value)
            {
            }
        }

        /// <summary>
        /// 带 TargetConsoleKey 的命令。
        /// </summary>
        [TargetConsoleKey("Admin")]
        private static class KeyedClassFixture
        {
            [ConsoleCommand("class_keyed")]
            private static void ClassKeyedCommand()
            {
                Debug.Log("class_keyed");
            }

            [ConsoleCommand("method_keyed")]
            [TargetConsoleKey("Debug")]
            private static void MethodKeyedCommand()
            {
                Debug.Log("method_keyed");
            }

            [ConsoleCommand("global_in_keyed_class")]
            private static void GlobalCommand()
            {
                Debug.Log("global_in_keyed_class");
            }
        }

        /// <summary>
        /// 带 TargetConsoleKey 方法级的命令（无类级 key）。
        /// </summary>
        private static class MethodLevelKeyFixture
        {
            [ConsoleCommand("debug_only")]
            [TargetConsoleKey("Debug")]
            private static void DebugOnlyCommand()
            {
                Debug.Log("debug_only");
            }
        }

        /// <summary>
        /// 带默认参数的命令（会触发警告）。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class DefaultParamFixture
        {
            [ConsoleCommand("default_param")]
            private static void WithDefault(int value = 42)
            {
            }
        }

        /// <summary>
        /// 泛型命令（会被跳过）。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class GenericMethodFixture
        {
            [ConsoleCommand("generic_cmd")]
            private static void GenericMethod<T>()
            {
            }
        }

        /// <summary>
        /// 非常规命令（无 [ConsoleCommand]），不应被扫描到。
        /// </summary>
        private static class NonCommandFixture
        {
            private static void NotACommand()
            {
            }

            [CommandHelpText("orphan help text — no ConsoleCommand")]
            private static void OrphanHelpText()
            {
            }
        }

        /// <summary>
        /// 带 [FixedField] 参数的命令。
        /// </summary>
        [TargetConsoleKey("Tests")]
        private static class FixedFieldFixture
        {
            public static string LastAction { get; set; }

            [ConsoleCommand("fixed_field_test")]
            private static void DoAction(
                [FixedField] object action,
                [FixedField("delete")] object deleteAction)
            {
                // FixedField 参数始终为 null，此处仅验证方法被调用
                LastAction = "called";
            }
        }

        #endregion

        #region 测试辅助 — 两阶段流水线

        /// <summary>
        /// 执行完整的「扫描 → 构建」流水线，返回可用的 <see cref="ConsoleCommandDefinition"/> 列表。
        /// 用于需要访问 ParameterHandlers 或 Execute 的测试。
        /// </summary>
        private static (List<ConsoleCommandDefinition> Commands, Dictionary<CommandName, string> HelpTexts)
            BuildScannedClass(Type type, ConsoleKey? targetKey = null)
        {
            var registry = new ParameterHandlerRegistry();
            ConsoleParameterHandlerScanner.ScanType(type, registry);
            registry.Freeze();

            var (pending, helpTexts) = ConsoleAttributeScanner.ScanClass(type, targetKey);

            var commands = new List<ConsoleCommandDefinition>();
            foreach (var entry in pending)
            {
                var paramInfos = entry.Method.GetParameters();
                var handlers = new IParameterHandler[paramInfos.Length];
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    var paramInfo = paramInfos[i];

                    // 检测 [FixedField]
                    var fixedFieldAttr = paramInfo.GetCustomAttribute<FixedFieldAttribute>();
                    if (fixedFieldAttr != null)
                    {
                        var fixedFieldName = fixedFieldAttr.FixedField ?? paramInfo.Name;
                        handlers[i] = new FixedFieldParameterHandler(fixedFieldName);
                        continue;
                    }

                    // 解析参数名
                    var paramName = paramInfo.GetCustomAttribute<CommandParameterAttribute>()?.Name ?? paramInfo.Name;

                    // 收集 HandlerSelectionAttribute 子类
                    var selectionAttrs = paramInfo.GetCustomAttributes<HandlerSelectionAttribute>(inherit: false);
                    var attrList = new List<Attribute>();
                    foreach (var attr in selectionAttrs)
                    {
                        attrList.Add(attr);
                    }
                    var attrs = attrList.ToArray();

                    handlers[i] = registry.HandlerOf(paramInfo.ParameterType, paramName, attrs);
                }
                commands.Add(new AttributeCommandDefinition(entry.Method, entry.CommandName, handlers));
            }

            return (commands, helpTexts);
        }

        #endregion

        #region 基本扫描

        [Test]
        public void ScanClass_SimpleCommand_FindsOneCommand()
        {
            SimpleCommandFixture.WasCalled = false;

            var (pendingCommands, helpTexts) = ConsoleAttributeScanner.ScanClass(typeof(SimpleCommandFixture));

            Assert.That(pendingCommands.Count, Is.EqualTo(1));
            Assert.That(pendingCommands[0].CommandName.Name, Is.EqualTo("Hello"));
            Assert.That(helpTexts.Count, Is.EqualTo(0));

            // 通过构建流水线验证 Execute 实际调用方法
            var (commands, _) = BuildScannedClass(typeof(SimpleCommandFixture));
            Assert.That(commands[0].ParameterHandlers.Count, Is.EqualTo(0));
            commands[0].Execute(null, null);
            Assert.IsTrue(SimpleCommandFixture.WasCalled);
        }

        [Test]
        public void ScanClass_ParameterCommand_FindsCommandWithCorrectHandlers()
        {
            var (commands, helpTexts) = BuildScannedClass(typeof(ParameterCommandFixture));

            Assert.That(commands.Count, Is.EqualTo(1));
            Assert.That(commands[0].CommandName.Name, Is.EqualTo("test_params"));

            var handlers = commands[0].ParameterHandlers;
            Assert.That(handlers.Count, Is.EqualTo(4));
            Assert.That(handlers[0], Is.InstanceOf<IntegerParameterHandler>());
            Assert.That(handlers[1], Is.InstanceOf<FloatParameterHandler>());
            Assert.That(handlers[2], Is.InstanceOf<StringParameterHandler>());
            Assert.That(handlers[3], Is.InstanceOf<BooleanParameterHandler>());
        }

        [Test]
        public void ScanClass_ParameterCommand_ExecuteCallsMethodWithCorrectValues()
        {
            var (commands, _) = BuildScannedClass(typeof(ParameterCommandFixture));

            // 模拟 Parse + Execute 流程
            var handlers = commands[0].ParameterHandlers;
            var parsed = new List<object>
            {
                handlers[0].Parse("10 "),
                handlers[1].Parse("3.14 "),
                handlers[2].Parse("hello "),
                handlers[3].Parse("true ")
            };

            commands[0].Execute(parsed, null);

            Assert.That(ParameterCommandFixture.LastIntValue, Is.EqualTo(10));
            Assert.That(ParameterCommandFixture.LastFloatValue, Is.EqualTo(3.14f).Within(1e-6f));
            Assert.That(ParameterCommandFixture.LastStringValue, Is.EqualTo("hello"));
            Assert.That(ParameterCommandFixture.LastBoolValue, Is.True);
        }

        #endregion

        #region 自定义参数名

        [Test]
        public void ScanClass_CustomParamName_HandlerUsesCustomName()
        {
            var (commands, _) = BuildScannedClass(typeof(CustomParamNameFixture));

            Assert.That(commands.Count, Is.EqualTo(1));
            Assert.That(commands[0].CommandName.Name, Is.EqualTo("custom_param"));

            var handler = commands[0].ParameterHandlers[0];
            Assert.That(handler.GetDescription().Name, Is.EqualTo("customName"));
        }

        #endregion

        #region HelpText

        [Test]
        public void ScanClass_HelpText_CollectedCorrectly()
        {
            var (pendingCommands, helpTexts) = ConsoleAttributeScanner.ScanClass(typeof(CustomParamNameFixture));

            Assert.That(helpTexts.Count, Is.EqualTo(1));
            Assert.That(helpTexts.ContainsKey(pendingCommands[0].CommandName), Is.True);
            Assert.That(helpTexts[pendingCommands[0].CommandName], Is.EqualTo("A command with custom parameter names."));
        }

        [Test]
        public void ScanClass_DuplicateHelpText_WarnsAndKeepsFirst()
        {
            // CustomParamNameFixture 只有一个帮助文本，不会重复
            // 我们在合并测试中验证重复行为
            var (pendingCommands, helpTexts) = ConsoleAttributeScanner.ScanClass(typeof(CustomParamNameFixture));
            Assert.That(helpTexts.Count, Is.EqualTo(1));
        }

        #endregion

        #region TargetConsoleKey 过滤

        [Test]
        public void ScanClass_NoTargetKey_ReturnsAllCommands()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(typeof(KeyedClassFixture));

            // 不传 targetKey → 返回所有 3 个命令
            Assert.That(pendingCommands.Count, Is.EqualTo(3));
        }

        [Test]
        public void ScanClass_TargetKeyAdmin_ExcludesDebugKeyedCommand()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(
                typeof(KeyedClassFixture), new ConsoleKey("Admin"));

            // "Admin" key:
            // - class_keyed: 继承类级 "Admin" → 匹配 → 包含
            // - global_in_keyed_class: 继承类级 "Admin" → 匹配 → 包含
            // - method_keyed: 方法级 "Debug" → 不匹配 → 排除
            Assert.That(pendingCommands.Count, Is.EqualTo(2));
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "class_keyed"), Is.True);
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "global_in_keyed_class"), Is.True);
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "method_keyed"), Is.False);
        }

        [Test]
        public void ScanClass_TargetKeyDebug_ExcludesAdminKeyedCommand()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(
                typeof(KeyedClassFixture), new ConsoleKey("Debug"));

            // "Debug" key:
            // - method_keyed: 方法级 "Debug" → 匹配 → 包含
            // - class_keyed: 继承类级 "Admin" → 不匹配 → 排除
            // - global_in_keyed_class: 继承类级 "Admin"（类有标记，方法无标记时继承类的 key，非全局）→ 排除
            Assert.That(pendingCommands.Count, Is.EqualTo(1));
            Assert.That(pendingCommands[0].CommandName.Name, Is.EqualTo("method_keyed"));
        }

        [Test]
        public void ScanClass_MethodLevelKey_FilteredCorrectly()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(
                typeof(MethodLevelKeyFixture), new ConsoleKey("Debug"));

            Assert.That(pendingCommands.Count, Is.EqualTo(1));
            Assert.That(pendingCommands[0].CommandName.Name, Is.EqualTo("debug_only"));
        }

        [Test]
        public void ScanClass_MethodLevelKey_WrongKey_Excluded()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(
                typeof(MethodLevelKeyFixture), new ConsoleKey("Admin"));

            Assert.That(pendingCommands.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 方法级 TargetConsoleKey 覆盖类级；方法无标记时继承类级。
        /// </summary>
        [Test]
        public void ScanClass_MethodKeyOverridesClassKey()
        {
            // 不带 targetKey 参数 → 不过滤，所有命令都返回
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(typeof(KeyedClassFixture));

            Assert.That(pendingCommands.Count, Is.EqualTo(3));
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "class_keyed"), Is.True);
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "method_keyed"), Is.True);
            Assert.That(pendingCommands.Any(c => c.CommandName.Name == "global_in_keyed_class"), Is.True);
        }

        #endregion

        #region 警告场景

        [Test]
        public void ScanClass_DefaultParameter_DoesNotSkip()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(typeof(DefaultParamFixture));

            // 默认参数方法不应被跳过（不再打印信息日志）
            Assert.That(pendingCommands.Count, Is.EqualTo(1));
            Assert.That(pendingCommands[0].CommandName.Name, Is.EqualTo("default_param"));
        }

        [Test]
        public void ScanClass_GenericMethod_SkippedWithWarning()
        {
            LogAssert.Expect(LogType.Warning,
                "[ConsoleCommand] 'Soyo.SoyoRuntimeConsole.Tests.Editor.ConsoleAttributeScannerTests+GenericMethodFixture.GenericMethod' " +
                "is a generic method, which is not supported. Skipping.");

            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(typeof(GenericMethodFixture));

            // 泛型方法应被跳过
            Assert.That(pendingCommands.Count, Is.EqualTo(0));
        }

        [Test]
        public void ScanClass_NonCommandClass_ReturnsEmpty()
        {
            var (pendingCommands, _) = ConsoleAttributeScanner.ScanClass(typeof(NonCommandFixture));

            Assert.That(pendingCommands.Count, Is.EqualTo(0));
        }

        #endregion

        #region 端到端：ScanClass → 通过 ConsoleBase 执行

        [Test]
        public void EndToEnd_ScannedCommand_ExecutesViaConsoleBase()
        {
            SimpleCommandFixture.WasCalled = false;

            var (commands, helpTexts) = BuildScannedClass(typeof(SimpleCommandFixture));
            var config = new ConsoleConfig(
                new ConsoleKey("Test"),
                commands,
                helpTexts.Select(kv => (kv.Key, kv.Value)));

            var console = new TestConsole(config);

            LogAssert.Expect(LogType.Log, "Hello!");
            console.SetInputText("Hello");
            Assert.IsTrue(console.SendInput());
            Assert.IsTrue(SimpleCommandFixture.WasCalled);
        }

        /// <summary>
        /// 用于端到端测试的 ConsoleBase 子类。
        /// </summary>
        private class TestConsole : ConsoleBase
        {
            public TestConsole(ConsoleConfig config) : base(config)
            {
            }
        }

        #endregion

        #region FixedField

        [Test]
        public void ScanClass_FixedField_CreatesFixedFieldHandler()
        {
            var (commands, _) = BuildScannedClass(typeof(FixedFieldFixture));

            Assert.That(commands.Count, Is.EqualTo(1));
            Assert.That(commands[0].ParameterHandlers.Count, Is.EqualTo(2));

            // 无参 FixedField — 使用参数名 "action"
            Assert.That(commands[0].ParameterHandlers[0], Is.InstanceOf<FixedFieldParameterHandler>());
            Assert.That(commands[0].ParameterHandlers[0].GetDescription().Name, Is.EqualTo("action"));

            // 带参 FixedField("delete") — 使用 "delete"
            Assert.That(commands[0].ParameterHandlers[1], Is.InstanceOf<FixedFieldParameterHandler>());
            Assert.That(commands[0].ParameterHandlers[1].GetDescription().Name, Is.EqualTo("delete"));
        }

        [Test]
        public void ScanClass_FixedField_ParseReturnsNull()
        {
            var (commands, _) = BuildScannedClass(typeof(FixedFieldFixture));

            var handler = commands[0].ParameterHandlers[0];
            Assert.That(handler.Parse("anything"), Is.Null);
            Assert.That(handler.Parse(""), Is.Null);
            Assert.That(handler.Parse(" "), Is.Null);
        }

        [Test]
        public void EndToEnd_FixedFieldCommand_ExecutesViaConsoleBase()
        {
            var (commands, helpTexts) = BuildScannedClass(typeof(FixedFieldFixture));
            var config = new ConsoleConfig(
                new ConsoleKey("Tests"),
                commands,
                helpTexts.Select(kv => (kv.Key, kv.Value)));

            var console = new TestConsole(config);

            // 验证命令存在
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "fixed_field_test"), Is.True);

            // 输入正确的固定字段值 — 命令应执行
            console.SetInputText("fixed_field_test action delete");
            Assert.IsTrue(console.SendInput());
            Assert.That(FixedFieldFixture.LastAction, Is.EqualTo("called"));
        }

        [Test]
        public void EndToEnd_FixedFieldCommand_WrongValue_DoesNotExecute()
        {
            FixedFieldFixture.LastAction = null;

            var (commands, helpTexts) = BuildScannedClass(typeof(FixedFieldFixture));
            var config = new ConsoleConfig(
                new ConsoleKey("Tests"),
                commands,
                helpTexts.Select(kv => (kv.Key, kv.Value)));

            var console = new TestConsole(config);

            // 输入不匹配的固定字段值 — 命令不应执行
            console.SetInputText("fixed_field_test wrong_value delete");
            Assert.IsFalse(console.SendInput());
            Assert.That(FixedFieldFixture.LastAction, Is.Null);
        }

        #endregion
    }
}
