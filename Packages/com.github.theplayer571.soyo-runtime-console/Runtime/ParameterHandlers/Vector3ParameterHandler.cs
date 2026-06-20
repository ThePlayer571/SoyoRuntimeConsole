using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// Vector3 的复合参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 float x、float y、float z。
    /// 示例输入：(1.0, 2.0, 3.0)
    /// </summary>
    public class Vector3ParameterHandler : CompositeParameterHandler
    {
        public Vector3ParameterHandler()
            : base("vector3", "Vector3", BracketType.Parentheses,
                new FloatParameterHandler("x"),
                new FloatParameterHandler("y"),
                new FloatParameterHandler("z"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector3((float)parts[0], (float)parts[1], (float)parts[2]);
        }
    }
}
