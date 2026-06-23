using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Rect"/> 的复合参数处理器。
    /// 支持两种输入格式：4 个浮点数直接指定、或 2 个 Vector2 分别表示位置和大小。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="CompositeParameterHandler"/>，内部包含两个私有的 <see cref="TupleParameterHandler"/> 实现。
    /// 示例输入：
    /// <c>(1.0, 2.0, 3.0, 4.0)</c>（4 个浮点数：x, y, width, height）；
    /// <c>((1.0, 2.0), (3.0, 4.0))</c>（Vector2 position, Vector2 size）。
    /// </remarks>
    public class RectParameterHandler : CompositeParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="Rect"/> 参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public RectParameterHandler([DisallowNull] string name)
            : base(name, "Rect",
                new RectFloat4Handler(name + "_f4"),
                new RectVector2Handler(name + "_v2"))
        {
        }

        /// <summary>
        /// 4 个浮点数格式：<c>(x, y, width, height)</c>。
        /// </summary>
        private sealed class RectFloat4Handler : TupleParameterHandler
        {
            public RectFloat4Handler([DisallowNull] string name)
                : base(name, "Rect", BracketType.Braces,
                    new FloatParameterHandler("x"),
                    new FloatParameterHandler("y"),
                    new FloatParameterHandler("width"),
                    new FloatParameterHandler("height"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new Rect((float)parts[0], (float)parts[1], (float)parts[2], (float)parts[3]);
            }
        }

        /// <summary>
        /// 2 个 Vector2 格式：<c>(position, size)</c>。
        /// </summary>
        private sealed class RectVector2Handler : TupleParameterHandler
        {
            public RectVector2Handler([DisallowNull] string name)
                : base(name, "Rect", BracketType.Braces,
                    new Vector2ParameterHandler("position"),
                    new Vector2ParameterHandler("size"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new Rect((Vector2)parts[0], (Vector2)parts[1]);
            }
        }
    }
}
