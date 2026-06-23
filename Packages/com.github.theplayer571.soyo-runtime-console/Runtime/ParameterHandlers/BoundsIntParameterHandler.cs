using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="BoundsInt"/> 的复合参数处理器。
    /// 支持两种输入格式：6 个整数直接指定、或 2 个 Vector3Int 分别表示位置和大小。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="CompositeParameterHandler"/>，内部包含两个私有的 <see cref="TupleParameterHandler"/> 实现。
    /// 示例输入：
    /// <c>(1, 2, 3, 4, 5, 6)</c>（6 个整数：x, y, z, sizeX, sizeY, sizeZ）；
    /// <c>((1, 2, 3), (4, 5, 6))</c>（Vector3Int position, Vector3Int size）。
    /// </remarks>
    public class BoundsIntParameterHandler : CompositeParameterHandler
    {
        /// <summary>
        /// 构造 <see cref="BoundsInt"/> 参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public BoundsIntParameterHandler([DisallowNull] string name)
            : base(name, "BoundsInt",
                new BoundsIntInt6Handler(name + "_i6"),
                new BoundsIntVector3IntHandler(name + "_v3i"))
        {
        }

        /// <summary>
        /// 6 个整数格式：<c>(x, y, z, sizeX, sizeY, sizeZ)</c>。
        /// </summary>
        private sealed class BoundsIntInt6Handler : TupleParameterHandler
        {
            public BoundsIntInt6Handler([DisallowNull] string name)
                : base(name, "BoundsInt", BracketType.Braces,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y"),
                    new IntegerParameterHandler("z"),
                    new IntegerParameterHandler("sizeX"),
                    new IntegerParameterHandler("sizeY"),
                    new IntegerParameterHandler("sizeZ"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new BoundsInt(
                    (int)parts[0], (int)parts[1], (int)parts[2],
                    (int)parts[3], (int)parts[4], (int)parts[5]);
            }
        }

        /// <summary>
        /// 2 个 Vector3Int 格式：<c>(position, size)</c>。
        /// </summary>
        private sealed class BoundsIntVector3IntHandler : TupleParameterHandler
        {
            public BoundsIntVector3IntHandler([DisallowNull] string name)
                : base(name, "BoundsInt", BracketType.Braces,
                    new Vector3IntParameterHandler("position"),
                    new Vector3IntParameterHandler("size"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new BoundsInt((Vector3Int)parts[0], (Vector3Int)parts[1]);
            }
        }
    }
}
