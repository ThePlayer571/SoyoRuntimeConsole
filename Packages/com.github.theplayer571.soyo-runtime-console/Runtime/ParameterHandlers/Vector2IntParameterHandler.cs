using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// Vector2Int 的元组参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 int x、int y。
    /// 示例输入：(1, 2)
    /// </summary>
    public class Vector2IntParameterHandler : TupleParameterHandler
    {
        public Vector2IntParameterHandler()
            : base("vector2int", "Vector2Int", BracketType.Parentheses,
                new IntegerParameterHandler("x"),
                new IntegerParameterHandler("y"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector2Int((int)parts[0], (int)parts[1]);
        }
    }
}
