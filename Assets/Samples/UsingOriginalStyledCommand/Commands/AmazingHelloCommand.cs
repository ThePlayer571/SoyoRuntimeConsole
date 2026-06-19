using System.Collections.Generic;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class AmazingHelloCommand : ConsoleCommandDefinition
    {
        public AmazingHelloCommand() : base("amazing_hello")
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            Debug.Log("Amazing Hello");
        }
    }
}