using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 整数参数处理器。匹配标准的十进制整数输入（正负号 + 数字），
    /// 如 <c>"0"</c>、<c>"-1"</c>、<c>"42"</c>。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 候选项规则：空输入时或当前输入可解析为 0 时返回 <c>"0"</c>；
    /// 非空且不可解析为 0 时不返回候选项（用户输入尚未形成合法数字）。
    /// </remarks>
    public class IntegerParameterHandler : SpaceSplitParameterHandlerBase
    {
        /// <summary>
        /// 使用指定的参数名称构造整数参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public IntegerParameterHandler([DisallowNull] string name) : base(name, "Integer")
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入或当前输入可解析为 0 时返回 <c>"0"</c> 作为候选项。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            parameter = parameter.Trim();
            if (string.IsNullOrEmpty(parameter) || int.TryParse(parameter, out int result) && result == 0)
            {
                yield return "0";
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 使用 <see cref="int.TryParse(string, out int)"/> 验证输入是否为合法整数。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            return int.TryParse(parameter.Trim(), out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="int"/> 值。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return int.Parse(parameter.Trim());
        }

        /// <inheritdoc />
        public override bool IsInitialized => true;
    }
}