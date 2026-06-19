using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class HelloContentToneCommand : ConsoleCommandDefinition
    {
        public HelloContentToneCommand() : base("hello", new StringParameterHandler("content"),
            new EnumParameterHandler<HelloTone>("tone"))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 2)
            {
                return;
            }

            var content = (string)parameters[0];
            var tone = (HelloTone)parameters[1];
            Debug.Log("Hello " + content + tone.ToDisplayString());
        }
    }
}