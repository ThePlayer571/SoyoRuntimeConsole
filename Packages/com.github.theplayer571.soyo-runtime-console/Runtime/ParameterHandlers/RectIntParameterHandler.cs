using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="RectInt"/> 的复合参数处理器。
    /// 支持两种输入格式：4 个整数直接指定、或 2 个 Vector2Int 分别表示位置和大小。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="CompositeParameterHandler"/>，内部包含两个私有的 <see cref="TupleParameterHandler"/> 实现。
    /// 示例输入：
    /// <c>(1, 2, 3, 4)</c>（4 个整数：x, y, width, height）；
    /// <c>((1, 2), (3, 4))</c>（Vector2Int position, Vector2Int size）。
    /// </remarks>
    public class RectIntParameterHandler : CompositeParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="RectInt"/> 参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public RectIntParameterHandler([DisallowNull] string name)
            : base(name, "RectInt",
                new RectIntInt4Handler(name + "_i4"),
                new RectIntVector2IntHandler(name + "_v2i"))
        {
        }

        /// <summary>
        /// 4 个整数格式：<c>(x, y, width, height)</c>。
        /// </summary>
        private sealed class RectIntInt4Handler : TupleParameterHandler
        {
            public RectIntInt4Handler([DisallowNull] string name)
                : base(name, "RectInt", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y"),
                    new IntegerParameterHandler("width"),
                    new IntegerParameterHandler("height"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new RectInt((int)parts[0], (int)parts[1], (int)parts[2], (int)parts[3]);
            }
        }

        /// <summary>
        /// 2 个 Vector2Int 格式：<c>(position, size)</c>。
        /// </summary>
        private sealed class RectIntVector2IntHandler : TupleParameterHandler
        {
            public RectIntVector2IntHandler([DisallowNull] string name)
                : base(name, "RectInt", BracketType.Parentheses,
                    new Vector2IntParameterHandler("position"),
                    new Vector2IntParameterHandler("size"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new RectInt((Vector2Int)parts[0], (Vector2Int)parts[1]);
            }
        }
    }
}
