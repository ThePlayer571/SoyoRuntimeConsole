using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand
{
    // 定义控制台
    public class CustomConsole : ConsoleBase
    {
        private static IEnumerable<ConsoleCommandDefinition> GetCommands()
        {
            yield return new QuickGreetCommand();
            yield return new GreetCommand();
            yield return new GreetCommand2();
            yield return new GreetCommand3();
        }


        public CustomConsole() : base(new ConsoleConfig(new ConsoleKey(""), GetCommands(), null))
        {
        }
    }
}