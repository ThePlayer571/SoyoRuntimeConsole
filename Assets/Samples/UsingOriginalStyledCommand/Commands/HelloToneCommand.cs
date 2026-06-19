using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloToneCommand : ConsoleCommandDefinition
    {
        public HelloToneCommand() : base("hello", new EnumParameterHandler<HelloTone>("tone"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var tone = (HelloTone)parameters[0];
            Debug.Log("Hello: " + tone.ToDisplayString());
        }
    }
}