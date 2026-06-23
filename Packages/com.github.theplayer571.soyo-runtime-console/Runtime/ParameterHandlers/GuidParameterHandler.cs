using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Guid"/> 的参数处理器。
    /// 接受标准 GUID 字符串格式（支持带或不带花括号/圆括号的格式）。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="SpaceSplitParameterHandlerBase"/>，与整数、浮点数等其他简单类型处理器保持一致。
    /// 示例输入：<c>12345678-1234-1234-1234-123456789abc</c>。
    /// <para>注意：<see cref="Guid"/> 无法基于 <see cref="TupleParameterHandler"/> 实现，
    /// 因为它是一个纯字符串值，不需要括号包裹或逗号分隔。</para>
    /// </remarks>
    public class GuidParameterHandler : SpaceSplitParameterHandlerBase
    {
        /// <summary>
        /// 用于空输入时的候选项提示。
        /// </summary>
        private static readonly string ZeroGuidString = Guid.Empty.ToString();

        /// <summary>
        /// 构造 <see cref="Guid"/> 参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public GuidParameterHandler([DisallowNull] string name)
            : base(name, "Guid")
        {
        }

        /// <inheritdoc />
        public override bool IsInitialized => true;

        /// <inheritdoc />
        /// <remarks>
        /// 使用 <see cref="Guid.TryParse(string, out Guid)"/> 验证输入是否为有效的 GUID 字符串。
        /// 支持以下格式：<c>00000000-0000-0000-0000-000000000000</c>、
        /// <c>{00000000-0000-0000-0000-000000000000}</c>、
        /// <c>(00000000-0000-0000-0000-000000000000)</c>。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            return !string.IsNullOrEmpty(parameter) && Guid.TryParse(parameter.Trim(), out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="Guid"/> 值。调用 <see cref="Guid.Parse(string)"/> 完成解析。
        /// 调用前应先通过 <see cref="IsValid"/> 确认输入合法。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return Guid.Parse(parameter.Trim());
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回零 GUID 字符串作为提示。
        /// 其他情况不提供候选项，因为 GUID 是任意字符串无法自动补全。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ZeroGuidString;
            }
        }
    }
}
