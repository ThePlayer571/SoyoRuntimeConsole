using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 嵌套问候命令 —— 展示 Tuple(Tuple, simple) 的嵌套能力。
    /// 第一个参数是 GreetStyle（本身是圆括号元组），第二个参数是重复次数。
    /// 示例输入：hello_nested ((Normal, false), 3)
    /// </summary>
    public class HelloNestedGreetCommand : ConsoleCommandDefinition
    {
        public HelloNestedGreetCommand() : base("hello_nested",
            new NestedGreetHandler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var parts = (object[])parameters[0];
            var style = (GreetStyle)parts[0];
            var repeat = (int)parts[1];

            for (var i = 0; i < repeat; i++)
            {
                Debug.Log($"[{i + 1}/{repeat}] Hello{style.Tone.ToDisplayString()} {(style.Cute ? "qwq" : "")}");
            }
        }
    }
}
