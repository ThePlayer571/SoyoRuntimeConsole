using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 多重问候命令 —— 展示 Tuple(Tuple, Tuple) 的嵌套能力。
    /// 使用方括号包裹两个 GreetStyle（各自是圆括号元组）。
    /// 示例输入：hello_multi [(Shout, true), (Normal, false)]
    /// </summary>
    public class HelloMultiGreetCommand : ConsoleCommandDefinition
    {
        public HelloMultiGreetCommand() : base("hello_multi",
            new MultiGreetConfigHandler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var parts = (object[])parameters[0];
            var style1 = (GreetStyle)parts[0];
            var style2 = (GreetStyle)parts[1];

            Debug.Log($"Hello{style1.Tone.ToDisplayString()} {(style1.Cute ? "qwq" : "")} " +
                      $"and Hello{style2.Tone.ToDisplayString()} {(style2.Cute ? "qwq" : "")}");
        }
    }
}
