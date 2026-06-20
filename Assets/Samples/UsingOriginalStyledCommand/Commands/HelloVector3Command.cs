using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloVector3Command : ConsoleCommandDefinition
    {
        public HelloVector3Command() : base("hello", new Vector3ParameterHandler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var vector3 = (Vector3)parameters[0];
            Debug.Log($"Hello Vector3: {vector3}");
        }
    }
}