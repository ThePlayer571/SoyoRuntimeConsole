using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候命令：传入一个 Vec3 复合参数（圆括号包裹的 int, int, bool）。
    /// 示例输入：hello_vec3 (3, 5, true)
    /// </summary>
    public class HelloVec3Command : ConsoleCommandDefinition
    {
        public HelloVec3Command() : base("hello_vec3",
            new Vec3Handler())
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var vec3 = (Vec3)parameters[0];
            Debug.Log($"Hello Vec3: {vec3}");
        }
    }
}
