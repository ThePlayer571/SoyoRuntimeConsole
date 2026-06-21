using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class GreetCommand : ConsoleCommandDefinition
    {
        public GreetCommand() : base("greet", new GreetConfigHandler("config"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            var greetConfig = (ValueObjects.GreetConfig)parameters![0];
            Debug.Log(greetConfig.ToString());
        }
    }
}