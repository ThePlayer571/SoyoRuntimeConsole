using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 字符串参数处理器。支持两种输入模式：
    /// 1. 无引号模式：任意不含空格的字符串，如 <c>"hello"</c>；
    /// 2. 双引号模式：用双引号包裹的字符串，支持内含空格，如 <c>"hello world"</c>。
    /// </summary>
    /// <remarks>
    /// 引号模式的选择由输入首字符是否为双引号决定。
    /// 无引号模式下，<see cref="ShouldAdvance"/> 在遇到空格时判断参数完整；
    /// 双引号模式下，遇到闭合引号后的空格时判断参数完整。
    /// 解析时将去除外层双引号。
    /// </remarks>
    public class StringParameterHandler : ParameterHandlerBase
    {
        /// <summary>
        /// 使用指定的参数名称构造字符串参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public StringParameterHandler([DisallowNull] string name) : base(name, "String")
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回 <c>""</c> 提示用户输入引号。
        /// 双引号模式下：未闭合时自动补全闭合引号，已闭合时返回当前输入。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return @"""""";
            }
            else if (parameter.StartsWith('"'))
            {
                if (parameter.Length == 1)
                {
                    yield return @"""""";
                }
                else if (parameter[^1] == '"')
                {
                    yield return parameter;
                }
                else
                {
                    yield return parameter + '"';
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 无引号模式：以空格结尾即认为参数完整。
        /// 双引号模式：需要至少 3 个字符（开引号 + 内容 + 闭引号）且以 <c>" </c>（引号+空格）结尾。
        /// </remarks>
        public override bool ShouldAdvance(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            parameter = ParameterHandlerParsingUtility.NormalizeSpaceSplitParameter(parameter);

            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter.Length >= 3 && parameter[^1] == ' ' && parameter[^2] == '"';
            }
            else
            {
                return parameter.EndsWith(' ');
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 无引号模式：任意非空字符串均视为合法。
        /// 双引号模式：长度至少为 2（开引号 + 闭引号）且首尾均为双引号时合法。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            parameter = parameter.Trim();
            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter.Length >= 2 && parameter[^1] == '"' && parameter.EndsWith('"');
            }
            else
            {
                return true;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="string"/>。双引号模式下会去除外层引号后返回内容；
        /// 无引号模式下直接返回输入。
        /// </remarks>
        public override object Parse(string parameter)
        {
            parameter = parameter.Trim();
            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter.TrimEnd()[1..^1];
            }
            else
            {
                return parameter;
            }
        }

        /// <inheritdoc />
        public override bool IsInitialized => true;
    }
}