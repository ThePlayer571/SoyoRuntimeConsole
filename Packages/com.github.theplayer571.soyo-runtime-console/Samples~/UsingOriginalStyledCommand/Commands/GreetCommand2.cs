using System.Collections.Generic;
using System.Linq;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class GreetCommand2 : ConsoleCommandDefinition
    {
        public GreetCommand2() : base("greet", new GreetStyleHandler("style"),
            new ArrayParameterHandler("words", new StringParameterHandler("word")))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            var greetStyle = (ValueObjects.GreetStyle)parameters![0];
            var words = (object[])parameters[1];
            var greetConfig = new ValueObjects.GreetConfig(greetStyle, words.Select(w => (string)w).ToArray());
            Debug.Log(greetConfig.ToString());
        }
    }
}