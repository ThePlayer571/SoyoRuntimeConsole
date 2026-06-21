using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 浮点数参数处理器。匹配标准的十进制浮点数输入（正负号 + 数字 + 可选小数点），
    /// 如 <c>"0"</c>、<c>"0.0"</c>、<c>"-1.5"</c>、<c>"3.14"</c>。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 候选项规则：空输入时返回 <c>"0"</c> 和 <c>"0.0"</c> 两个候选项；
    /// 当前输入可解析为近似 0 的浮点数时，根据输入是否包含 <c>"."</c> 返回 <c>"0"</c> 或 <c>"0.0"</c>。
    /// </remarks>
    public class FloatParameterHandler : SpaceSplitParameterHandlerBase
    {
        /// <summary>
        /// 使用指定的参数名称构造浮点数参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public FloatParameterHandler([DisallowNull] string name) : base(name, "Float")
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回 <c>"0"</c> 和 <c>"0.0"</c> 两个候选项；
        /// 非空且当前输入可解析为近似 0 的浮点数时，根据输入是否包含 <c>"."</c> 返回对应格式。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return "0";
                yield return "0.0";
                yield break;
            }

            parameter = parameter.Trim();
            if (float.TryParse(parameter, out var result) && Mathf.Approximately(result, 0f))
            {
                if (parameter.Contains("."))
                {
                    yield return "0.0";
                }
                else
                {
                    yield return "0";
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 使用 <see cref="float.TryParse(string, out float)"/> 验证输入是否为合法浮点数。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            return float.TryParse(parameter.Trim(), out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为 <see cref="float"/> 值。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return float.Parse(parameter.Trim());
        }

        /// <inheritdoc />
        public override bool IsInitialized => true;
    }
}