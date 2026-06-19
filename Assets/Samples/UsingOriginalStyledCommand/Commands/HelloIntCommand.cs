using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloIntCommand : ConsoleCommandDefinition
    {
        public HelloIntCommand() : base("hello", new IntegerParameterHandler("content"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            Debug.Log("Hello int: " + parameters[0]);
        }
    }
}