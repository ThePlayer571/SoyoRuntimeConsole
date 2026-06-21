using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 固定字段参数处理器。要求输入精确匹配一个固定的字符串（不允许包含空白字符）。
    /// 通常用于命令关键字或固定子命令，如子命令名 <c>"list"</c>、<c>"add"</c>、<c>"remove"</c>。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="StringOptionParameterHandler"/> 的区别：
    /// 本类仅匹配一个固定值，而 <see cref="StringOptionParameterHandler"/> 支持从多个选项中匹配。
    /// 解析始终返回 null——固定字段的值在命令结构层面就已确定，不需要将解析结果传递给执行逻辑。
    /// </remarks>
    public class FixedFieldParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly string _fixedField;

        /// <summary>
        /// 使用指定的固定字段值构造处理器（字段值同时作为参数名称）。
        /// </summary>
        /// <param name="name">固定字段的值，同时用作参数名称（用于提示）。
        /// 不能为 null、空字符串或包含空白字符，否则初始化失败。</param>
        public FixedFieldParameterHandler([DisallowNull] string name)
            : base(name, null)
        {
            _fixedField = name;

            // 检查 null 或纯空白
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError("FixedFieldParameterHandler: fixedField cannot be null, empty, or whitespace.");
                IsInitialized = false;
                return;
            }

            // 检查是否包含任何空白字符
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsWhiteSpace(name[i]))
                {
                    Debug.LogError(
                        $"FixedStringParameterHandler: fixedString contains whitespace at index {i}. Input: \"{name}\"");
                    IsInitialized = false;
                    return;
                }
            }

            IsInitialized = true;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 仅当固定字段包含当前输入（作为子串）时返回该固定字段作为候选项。
        /// 空输入时固定字段会包含空串，因此返回固定字段本身。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            parameter = parameter.Trim();
            if (_fixedField.Contains(parameter))
            {
                yield return _fixedField;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 输入去掉首尾空白后必须与固定字段完全相等。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            return parameter.Trim() == _fixedField;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 固定字段的值在命令结构层面已确定，解析始终返回 null。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return null;
        }

        /// <inheritdoc />
        public override bool IsInitialized { get; }
    }
}