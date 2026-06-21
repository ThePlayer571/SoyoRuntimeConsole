using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 布尔参数处理器。匹配不区分大小写的 <c>"true"</c> / <c>"false"</c> 字符串。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 候选项规则：空输入时返回 <c>"true"</c> 和 <c>"false"</c> 两个候选项；
    /// 非空时返回以当前输入为前缀的候选项（不区分大小写）。
    /// </remarks>
    public class BooleanParameterHandler : SpaceSplitParameterHandlerBase
    {
        /// <summary>
        /// 使用指定的参数名称构造布尔参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public BooleanParameterHandler([DisallowNull] string name) : base(name, "Boolean")
        {
        }

        private string True => "true";
        private string False => "false";

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回 <c>"true"</c> 和 <c>"false"</c> 两个候选项；
        /// 非空时按前缀匹配（不区分大小写）返回匹配的候选项。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return True;
                yield return False;
                yield break;
            }

            parameter = parameter.Trim();

            if (True.StartsWith(parameter, StringComparison.OrdinalIgnoreCase))
            {
                yield return True;
            }

            if (False.StartsWith(parameter, StringComparison.OrdinalIgnoreCase))
            {
                yield return False;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 仅当输入去掉首尾空白后严格等于 <c>"true"</c> 或 <c>"false"</c>（不区分大小写）时返回 true。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            parameter = parameter.Trim();
            return parameter == True || parameter == False;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="bool"/> 值。输入不匹配时返回 null。
        /// </remarks>
        public override object Parse(string parameter)
        {
            parameter = parameter.Trim();
            if (parameter == True)
            {
                return true;
            }

            if (parameter == False)
            {
                return false;
            }

            return null;
        }

        /// <inheritdoc />
        public override bool IsInitialized => true;
    }
}