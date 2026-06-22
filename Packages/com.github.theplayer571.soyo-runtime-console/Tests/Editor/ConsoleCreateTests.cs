using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="Console.Create(ConsoleKey)"/> 和 <see cref="Console.Create(string)"/> 的测试。
    /// 验证通过 ConsoleKey 扫描全程序集的命令过滤行为。
    /// </summary>
    /// <remarks>
    /// 使用唯一前缀 "cct_" 的命令名避免与其他测试 Fixture 冲突。
    /// </remarks>
    public class ConsoleCreateTests
    {
        #region 测试用的命令 Fixture

        [TargetConsoleKey("cct_alpha_key")]
        private static class AlphaGroup
        {
            [ConsoleCommand("cct_alpha_only")]
            private static void AlphaOnly()
            {
            }
        }

        [TargetConsoleKey("cct_beta_key")]
        private static class BetaGroup
        {
            [ConsoleCommand("cct_beta_only")]
            private static void BetaOnly()
            {
            }
        }

        private static class GlobalGroup
        {
            [ConsoleCommand("cct_global_anytime")]
            private static void GlobalAnytime()
            {
            }
        }

        #endregion

        #region 基本过滤

        [Test]
        public void Create_ConsoleKey_Alpha_IncludesAlphaAndGlobal_ExcludesBeta()
        {
            var console = Console.Create(new ConsoleKey("cct_alpha_key"));

            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_alpha_only"), Is.True);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_global_anytime"), Is.True);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_beta_only"), Is.False);
        }

        [Test]
        public void Create_ConsoleKey_Beta_IncludesBetaAndGlobal_ExcludesAlpha()
        {
            var console = Console.Create(new ConsoleKey("cct_beta_key"));

            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_beta_only"), Is.True);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_global_anytime"), Is.True);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_alpha_only"), Is.False);
        }

        [Test]
        public void Create_ConsoleKey_Nonexistent_OnlyGlobalCommands()
        {
            var console = Console.Create(new ConsoleKey("cct_nonexistent_key"));

            // 特定 key 的命令不会被包含
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_alpha_only"), Is.False);
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_beta_only"), Is.False);
            // 全局命令仍然被包含
            Assert.That(console.Commands.Any(c => c.CommandName.Name == "cct_global_anytime"), Is.True);
        }

        #endregion

        #region 便捷重载

        [Test]
        public void Create_StringKey_SameAsConsoleKey()
        {
            var console1 = Console.Create(new ConsoleKey("cct_alpha_key"));
            var console2 = Console.Create("cct_alpha_key");

            Assert.That(console1.Key, Is.EqualTo(console2.Key));
            Assert.That(console1.Commands.Count, Is.EqualTo(console2.Commands.Count));
        }

        #endregion

        #region Key 属性

        [Test]
        public void Create_ConsoleKey_ReturnsConsoleKey()
        {
            var console = Console.Create(new ConsoleKey("cct_my_key"));
            Assert.That(console.Key, Is.EqualTo(new ConsoleKey("cct_my_key")));
        }

        #endregion

        #region 多次调用独立性

        [Test]
        public void Create_CalledTwice_IndependentScans()
        {
            var console1 = Console.Create(new ConsoleKey("cct_alpha_key"));
            var console2 = Console.Create(new ConsoleKey("cct_beta_key"));

            // 两次扫描结果不同
            Assert.That(
                console1.Commands.Any(c => c.CommandName.Name == "cct_alpha_only"),
                Is.True);
            Assert.That(
                console2.Commands.Any(c => c.CommandName.Name == "cct_beta_only"),
                Is.True);

            // 两者不共享状态
            Assert.That(console1.Commands, Is.Not.SameAs(console2.Commands));
        }

        #endregion
    }
}
