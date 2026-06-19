using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloCuteCommand : ConsoleCommandDefinition
    {
        public HelloCuteCommand() : base("hello", new BooleanParameterHandler("cute"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var cute = (bool)parameters[0];
            Debug.Log(cute ? "Hello qwq" : "Hello");
        }
    }
}