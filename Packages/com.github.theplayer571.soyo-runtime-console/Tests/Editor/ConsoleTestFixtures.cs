using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public partial class ConsoleBaseTests
    {
        #region 内部测试命令

        /// <summary>
        /// 无参命令：hello —— 在控制台输出 "Hello!"
        /// </summary>
        private class HelloCommand : ConsoleCommandDefinition
        {
            public HelloCommand() : base("hello", null)
            {
            }

            public override void Execute(IReadOnlyList<object> parameters, IConsole console)
            {
                UnityEngine.Debug.Log("Hello!");
            }
        }

        /// <summary>
        /// 带 string 参数的命令：hello &lt;name&gt; —— 向指定人名问好
        /// 与 HelloCommand 共享命令名 "hello"，形成歧义以测试 chosenCommandIndex 行为。
        /// </summary>
        private class HelloWithNameCommand : ConsoleCommandDefinition
        {
            public HelloWithNameCommand() : base("hello", new StringParameterHandler("name"))
            {
            }

            public override void Execute(IReadOnlyList<object> parameters, IConsole console)
            {
                if (parameters == null || parameters.Count <= 0)
                {
                    return;
                }

                UnityEngine.Debug.Log("Hello " + parameters[0]);
            }
        }

        /// <summary>
        /// 无参命令：hello_world —— 在控制台输出 "Hello World"
        /// </summary>
        private class HelloWorldCommand : ConsoleCommandDefinition
        {
            public HelloWorldCommand() : base("hello_world", null)
            {
            }

            public override void Execute(IReadOnlyList<object> parameters, IConsole console)
            {
                UnityEngine.Debug.Log("Hello World");
            }
        }

        #endregion

        #region 内部测试 Console

        /// <summary>
        /// 测试用 ConsoleBase 子类，注册 3 个命令，不带 helpText。
        /// </summary>
        private class TestConsole : ConsoleBase
        {
            private static IEnumerable<ConsoleCommandDefinition> GetCommands()
            {
                yield return new HelloCommand();
                yield return new HelloWithNameCommand();
                yield return new HelloWorldCommand();
            }

            public TestConsole() : base(new ConsoleConfig(new ConsoleKey("TestConsole"),GetCommands(), null))
            {
            }
        }

        /// <summary>
        /// 测试用 ConsoleBase 子类，注册 3 个命令并附带 helpText。
        /// </summary>
        private class TestConsoleWithHelp : ConsoleBase
        {
            private static IEnumerable<ConsoleCommandDefinition> GetCommands()
            {
                yield return new HelloCommand();
                yield return new HelloWithNameCommand();
                yield return new HelloWorldCommand();
            }

            private static IEnumerable<(CommandName, string)> GetHelpText()
            {
                yield return (new CommandName("hello"), "Says hello");
                yield return (new CommandName("hello_world"), "Says hello world");
            }

            public TestConsoleWithHelp() : base(new ConsoleConfig(new ConsoleKey("TestConsoleWithHelp"),GetCommands(), GetHelpText()))
            {
            }
        }

        #endregion
    }
}
