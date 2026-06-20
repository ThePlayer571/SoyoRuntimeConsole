using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// Vector4 的元组参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 float x、float y、float z、float w。
    /// 示例输入：(1.0, 2.0, 3.0, 4.0)
    /// </summary>
    public class Vector4ParameterHandler : TupleParameterHandler
    {
        public Vector4ParameterHandler()
            : base("vector4", "Vector4", BracketType.Parentheses,
                new FloatParameterHandler("x"),
                new FloatParameterHandler("y"),
                new FloatParameterHandler("z"),
                new FloatParameterHandler("w"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector4((float)parts[0], (float)parts[1], (float)parts[2], (float)parts[3]);
        }
    }
}
