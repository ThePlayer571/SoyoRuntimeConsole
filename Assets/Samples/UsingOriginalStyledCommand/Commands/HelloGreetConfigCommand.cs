using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候命令：传入 string 内容和 GreetConfig 配置（花括号包裹）。
    /// 示例输入：hello "hi there" {Doubt, true}
    /// </summary>
    public class HelloGreetConfigCommand : ConsoleCommandDefinition
    {
        public HelloGreetConfigCommand() : base("hello",
            new StringParameterHandler("content"),
            new GreetConfigHandler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 2)
            {
                return;
            }

            var content = (string)parameters[0];
            var config = (GreetConfig)parameters[1];

            var cuteStr = config.Cute ? "qwq" : "";
            Debug.Log($"Hello {content}{config.Tone.ToDisplayString()}{cuteStr}");
        }
    }
}
