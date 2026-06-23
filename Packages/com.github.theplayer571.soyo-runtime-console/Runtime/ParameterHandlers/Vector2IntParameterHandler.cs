using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Vector2Int"/> 的元组参数处理器。
    /// 使用圆括号 <c>()</c> 包裹、逗号分隔，内部依次为 <c>int x</c>、<c>int y</c>。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="TupleParameterHandler"/>，使用默认的固定数量模式（2 个子参数）。
    /// 示例输入：<c>(1, 2)</c>
    /// </remarks>
    public class Vector2IntParameterHandler : TupleParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="Vector2Int"/> 参数处理器。
        /// 内部使用两个 <see cref="IntegerParameterHandler"/> 分别处理 x 和 y 分量。
        /// </summary>
        public Vector2IntParameterHandler()
            : base("vector2int", "Vector2Int", BracketType.Parentheses,
                new IntegerParameterHandler("x"),
                new IntegerParameterHandler("y"))
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="Vector2Int"/> 值。调用 <see cref="BracketParameterHandler.GetParsedSubParameters"/>
        /// 获取各子参数解析结果后构造 Vector2Int。
        /// </remarks>
        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector2Int((int)parts[0], (int)parts[1]);
        }
    }
}
