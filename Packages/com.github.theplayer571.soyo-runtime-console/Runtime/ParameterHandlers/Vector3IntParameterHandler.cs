using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// Vector3Int 的元组参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 int x、int y、int z。
    /// 示例输入：(1, 2, 3)
    /// </summary>
    public class Vector3IntParameterHandler : TupleParameterHandler
    {
        public Vector3IntParameterHandler()
            : base("vector3int", "Vector3Int", BracketType.Parentheses,
                new IntegerParameterHandler("x"),
                new IntegerParameterHandler("y"),
                new IntegerParameterHandler("z"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector3Int((int)parts[0], (int)parts[1], (int)parts[2]);
        }
    }
}
