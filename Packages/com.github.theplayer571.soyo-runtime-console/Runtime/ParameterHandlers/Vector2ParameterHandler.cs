using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// Vector2 的复合参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 float x、float y。
    /// 示例输入：(1.5, 2.0)
    /// </summary>
    public class Vector2ParameterHandler : CompositeParameterHandler
    {
        public Vector2ParameterHandler()
            : base("vector2", "Vector2", BracketType.Parentheses,
                new FloatParameterHandler("x"),
                new FloatParameterHandler("y"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector2((float)parts[0], (float)parts[1]);
        }
    }
}
