using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候命令：传入一个 GreetStyle 参数（圆括号包裹的 tone 和 cute）。
    /// 示例输入：hello_style (Shout, true)
    /// </summary>
    public class HelloGreetStyleCommand : ConsoleCommandDefinition
    {
        public HelloGreetStyleCommand() : base("hello_style",
            new GreetStyleHandler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var style = (GreetStyle)parameters[0];
            Debug.Log($"Hello{style.Tone.ToDisplayString()} {(style.Cute ? "qwq" : "")}");
        }
    }
}
