using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand
{
    public class CustomConsole : ConsoleBase
    {
        private static IEnumerable<ConsoleCommandDefinition> GetCommands()
        {
            yield return new AmazingHelloCommand();
            yield return new HelloVector3Command();
            yield return new HelloBoldCommand();
            yield return new HelloCommand();
            yield return new HelloContentToneCommand();
            yield return new HelloCuteCommand();
            yield return new HelloFloatCommand();
            yield return new HelloGreetConfigCommand();
            yield return new HelloIntCommand();
            yield return new HelloVec3Command();
            yield return new HelloGreetStyleCommand();
            yield return new HelloNestedGreetCommand();
            yield return new HelloMultiGreetCommand();
            yield return new HelloIntArrayCommand();
        }


        public CustomConsole() : base(new ConsoleConfig(GetCommands(), null))
        {
        }
    }
}