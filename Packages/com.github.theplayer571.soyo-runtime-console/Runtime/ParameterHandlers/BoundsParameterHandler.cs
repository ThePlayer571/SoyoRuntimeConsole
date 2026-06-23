using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Bounds"/> 的复合参数处理器。
    /// 支持两种输入格式：6 个浮点数直接指定、或 2 个 Vector3 分别表示中心和大小。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="CompositeParameterHandler"/>，内部包含两个私有的 <see cref="TupleParameterHandler"/> 实现。
    /// 示例输入：
    /// <c>(1.0, 2.0, 3.0, 4.0, 5.0, 6.0)</c>（6 个浮点数：cx, cy, cz, sx, sy, sz）；
    /// <c>((1.0, 2.0, 3.0), (4.0, 5.0, 6.0))</c>（Vector3 center, Vector3 size）。
    /// </remarks>
    public class BoundsParameterHandler : CompositeParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="Bounds"/> 参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public BoundsParameterHandler([DisallowNull] string name)
            : base(name, "Bounds",
                new BoundsFloat6Handler(name + "_f6"),
                new BoundsVector3Handler(name + "_v3"))
        {
        }

        /// <summary>
        /// 6 个浮点数格式：<c>(cx, cy, cz, sx, sy, sz)</c>。
        /// </summary>
        private sealed class BoundsFloat6Handler : TupleParameterHandler
        {
            public BoundsFloat6Handler([DisallowNull] string name)
                : base(name, "Bounds", BracketType.Braces,
                    new FloatParameterHandler("cx"),
                    new FloatParameterHandler("cy"),
                    new FloatParameterHandler("cz"),
                    new FloatParameterHandler("sx"),
                    new FloatParameterHandler("sy"),
                    new FloatParameterHandler("sz"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new Bounds(
                    new Vector3((float)parts[0], (float)parts[1], (float)parts[2]),
                    new Vector3((float)parts[3], (float)parts[4], (float)parts[5]));
            }
        }

        /// <summary>
        /// 2 个 Vector3 格式：<c>(center, size)</c>。
        /// </summary>
        private sealed class BoundsVector3Handler : TupleParameterHandler
        {
            public BoundsVector3Handler([DisallowNull] string name)
                : base(name, "Bounds", BracketType.Braces,
                    new Vector3ParameterHandler("center"),
                    new Vector3ParameterHandler("size"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new Bounds((Vector3)parts[0], (Vector3)parts[1]);
            }
        }
    }
}
