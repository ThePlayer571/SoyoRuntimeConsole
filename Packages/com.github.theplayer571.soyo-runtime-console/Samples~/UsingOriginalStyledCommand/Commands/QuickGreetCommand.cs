using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class QuickGreetCommand : ConsoleCommandDefinition
    {
        public QuickGreetCommand() : base("quick_greet")
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            Debug.Log("Hello");
        }
    }
}