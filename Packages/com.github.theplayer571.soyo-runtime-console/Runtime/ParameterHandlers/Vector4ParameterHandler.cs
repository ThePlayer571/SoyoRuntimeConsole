using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Vector4"/> 的元组参数处理器。
    /// 使用圆括号 <c>()</c> 包裹、逗号分隔，内部依次为 <c>float x</c>、<c>float y</c>、<c>float z</c>、<c>float w</c>。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="TupleParameterHandler"/>，使用默认的固定数量模式（4 个子参数）。
    /// 示例输入：<c>(1.0, 2.0, 3.0, 4.0)</c>
    /// </remarks>
    public class Vector4ParameterHandler : TupleParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="Vector4"/> 参数处理器。
        /// 内部使用四个 <see cref="FloatParameterHandler"/> 分别处理 x、y、z、w 分量。
        /// </summary>
        public Vector4ParameterHandler()
            : base("vector4", "Vector4", BracketType.Parentheses,
                new FloatParameterHandler("x"),
                new FloatParameterHandler("y"),
                new FloatParameterHandler("z"),
                new FloatParameterHandler("w"))
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="Vector4"/> 值。调用 <see cref="BracketParameterHandler.GetParsedSubParameters"/>
        /// 获取各子参数解析结果后构造 Vector4。
        /// </remarks>
        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vector4((float)parts[0], (float)parts[1], (float)parts[2], (float)parts[3]);
        }
    }
}
