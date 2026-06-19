using System.Collections.Generic;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloCommand : ConsoleCommandDefinition
    {
        public HelloCommand() : base("hello", null)
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            Debug.Log("Hello");
        }
    }
}