using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Vector2"/> 的元组参数处理器。
    /// 使用圆括号 <c>()</c> 包裹、逗号分隔，内部依次为 <c>float x</c>、<c>float y</c>。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="TupleParameterHandler"/>，使用默认的固定数量模式（2 个子参数）。
    /// 示例输入：<c>(1.5, 2.0)</c>
    /// </remarks>
    public class Vector2ParameterHandler : TupleParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="Vector2"/> 参数处理器。
        /// 内部使用两个 <see cref="FloatParameterHandler"/> 分别处理 x 和 y 分量。
        /// </summary>
        public Vector2ParameterHandler()
            : base("vector2", "Vector2", BracketType.Parentheses,
                new FloatParameterHandler("x"),
                new FloatParameterHandler("y"))
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="Vector2"/> 值。调用 <see cref="BracketParameterHandler.GetParsedSubParameters"/>
        /// 获取各子参数解析结果后构造 Vector2。
        /// </remarks>
        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector2((float)parts[0], (float)parts[1]);
        }
    }
}
