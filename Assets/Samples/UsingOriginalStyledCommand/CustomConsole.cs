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

        private static IEnumerable<(CommandName, string)> GetCommandHelpText()
        {
            yield return (new CommandName("quick_greet"), "A brief greet");
            yield return (new CommandName("greet"), "greet with detailed config");
        }


        public CustomConsole() : base(new ConsoleConfig(new ConsoleKey(""), GetCommands(), GetCommandHelpText()))
        {
        }
    }
}