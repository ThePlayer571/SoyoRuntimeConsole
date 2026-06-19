using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand
{
    public class CustomConsole : ConsoleBase
    {
        private static IEnumerable<ConsoleCommandDefinition> GetCommands()
        {
            yield return new AmazingHelloCommand();
            yield return new HelloBoldCommand();
            yield return new HelloCommand();
            yield return new HelloContentToneCommand();
            yield return new HelloCuteCommand();
            yield return new HelloFloatCommand();
            yield return new HelloIntCommand();
        }


        public CustomConsole() : base(GetCommands())
        {
        }
    }
}