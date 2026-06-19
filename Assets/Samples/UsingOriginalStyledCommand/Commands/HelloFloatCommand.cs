using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloFloatCommand : ConsoleCommandDefinition
    {
        public HelloFloatCommand() : base("hello", new FloatParameterHandler("content"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            Debug.Log("Hello float: " + parameters[0]);
        }
    }
}