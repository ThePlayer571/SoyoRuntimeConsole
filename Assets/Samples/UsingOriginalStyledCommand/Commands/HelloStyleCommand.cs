using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloStyleCommand : ConsoleCommandDefinition
    {
        public HelloStyleCommand() : base("hello", new StringOptionParameterHandler("style", "casual", "formal", "excited"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var style = (string)parameters[0];
            var suffix = style switch
            {
                "casual" => "~",
                "formal" => ".",
                "excited" => "!",
                _ => ""
            };
            Debug.Log("Hello" + suffix);
        }
    }
}
