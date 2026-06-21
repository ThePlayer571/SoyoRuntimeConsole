using System.Collections.Generic;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 使用 ArrayParameterHandler 的示例命令。
    /// 输入格式：hello [1, 2, 3] 或 hello [42] 或 hello []
    /// </summary>
    public class HelloIntArrayCommand : ConsoleCommandDefinition
    {
        public HelloIntArrayCommand() : base("hello",
            new ArrayParameterHandler("values", new IntegerParameterHandler("value")))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            if (parameters == null || parameters.Count < 1)
            {
                return;
            }

            var array = (object[])parameters[0];
            if (array.Length == 0)
            {
                Debug.Log("Hello int array: (empty)");
                return;
            }

            var values = new int[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                values[i] = (int)array[i];
            }

            Debug.Log($"Hello int array: [{string.Join(", ", values)}] (length={values.Length})");
        }
    }
}
