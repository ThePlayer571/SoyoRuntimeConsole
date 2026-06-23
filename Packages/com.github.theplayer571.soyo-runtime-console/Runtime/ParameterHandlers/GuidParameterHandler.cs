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
        /// 部分输入时，将用户输入的合法字符覆盖到零 GUID 字符串对应位置，剩余部分
        /// 保持为零，提供完整的 GUID 预览，让用户在输入过程中始终能看到补全结果。
        /// 支持花括号 <c>{}</c> / 圆括号 <c>()</c> 包裹格式，自动补全闭括号。
        /// 按零 GUID 模板逐位校验：连字符位置必须为 <c>-</c>，数字位置必须为
        /// 十六进制数字。格式不匹配时不提供候选项。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ZeroGuidString;
                yield break;
            }

            var trimmed = parameter.Trim();

            // 检测并剥离开括号
            var hasOpenBrace = trimmed.Length > 0 && trimmed[0] == '{';
            var hasOpenParen = trimmed.Length > 0 && trimmed[0] == '(';

            if (hasOpenBrace || hasOpenParen)
            {
                trimmed = trimmed.Substring(1);
            }

            // 剥离闭括号（如果用户已输入）
            if (trimmed.Length > 0 && trimmed[trimmed.Length - 1] == '}')
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }
            else if (trimmed.Length > 0 && trimmed[trimmed.Length - 1] == ')')
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            // 超过标准 GUID 长度（36 字符）不提供候选项
            if (trimmed.Length > ZeroGuidString.Length)
            {
                yield break;
            }

            // 按零 GUID 模板逐位校验：连字符位置必须为 '-'，数字位置必须为十六进制数字
            for (var i = 0; i < trimmed.Length; i++)
            {
                var inputChar = trimmed[i];
                if (ZeroGuidString[i] == '-')
                {
                    if (inputChar != '-')
                    {
                        yield break;
                    }
                }
                else
                {
                    if (!IsHexDigit(inputChar))
                    {
                        yield break;
                    }
                }
            }

            // 将用户输入覆盖到 ZeroGuidString 上
            var chars = ZeroGuidString.ToCharArray();
            for (var i = 0; i < trimmed.Length; i++)
            {
                chars[i] = trimmed[i];
            }
            var padded = new string(chars);

            // 补回括号包裹（自动补全闭括号）
            if (hasOpenBrace)
            {
                padded = '{' + padded + '}';
            }
            else if (hasOpenParen)
            {
                padded = '(' + padded + ')';
            }

            yield return padded;
        }

        /// <summary>
        /// 判断字符是否为合法十六进制数字（<c>0-9</c>、<c>a-f</c>、<c>A-F</c>）。
        /// </summary>
        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9')
                   || (c >= 'a' && c <= 'f')
                   || (c >= 'A' && c <= 'F');
        }
    }
}
