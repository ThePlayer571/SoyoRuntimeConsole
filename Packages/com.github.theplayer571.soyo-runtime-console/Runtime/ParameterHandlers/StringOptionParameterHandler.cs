using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 字符串选项参数处理器。从预定义的选项集合中匹配输入，要求输入的选项不能包含空白字符。
    /// 常用于限定值的参数，如颜色名（<c>"red"</c>、<c>"green"</c>、<c>"blue"</c>）、难度等级等。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 初始化时会对所有选项进行校验：不能为 null、空或包含空白字符，否则 <see cref="IsInitialized"/> 为 false。
    ///
    /// 候选项规则：空输入时返回所有选项；非空时按子串匹配（不区分大小写）返回匹配的选项。
    ///
    /// 与 <see cref="FixedFieldParameterHandler"/> 的区别：
    /// 本类支持从多个选项中匹配一个，而 <see cref="FixedFieldParameterHandler"/> 仅匹配一个固定值。
    /// 与 <see cref="EnumParameterHandler"/> 的区别：
    /// 本类使用手动指定的字符串列表，不依赖枚举类型反射。
    /// </remarks>
    public class StringOptionParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly string[] _options;

        /// <summary>
        /// 使用指定的参数名称和选项集合构造字符串选项参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="options">选项字符串集合。至少包含一个选项，每个选项不能为 null、空或包含空白字符。</param>
        public StringOptionParameterHandler(
            [DisallowNull] string name,
            [DisallowNull] IEnumerable<string> options)
            : base(name, "option")
        {
            var optionsList = options.ToList();

            if (optionsList.Count == 0)
            {
                Debug.LogError("StringOptionParameterHandler: options must contain at least one string.");
                IsInitialized = false;
                return;
            }

            for (var i = 0; i < optionsList.Count; i++)
            {
                var option = optionsList[i];
                if (string.IsNullOrWhiteSpace(option))
                {
                    Debug.LogError(
                        $"StringOptionParameterHandler: option at index {i} is null, empty, or whitespace.");
                    IsInitialized = false;
                    return;
                }

                for (var j = 0; j < option.Length; j++)
                {
                    if (char.IsWhiteSpace(option[j]))
                    {
                        Debug.LogError(
                            $"StringOptionParameterHandler: option at index {i} contains whitespace at char {j}. Input: \"{option}\"");
                        IsInitialized = false;
                        return;
                    }
                }
            }

            _options = optionsList.ToArray();
            IsInitialized = true;
        }

        /// <summary>
        /// 使用指定的参数名称和选项构造字符串选项参数处理器（params 便捷重载）。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="options">选项字符串数组</param>
        public StringOptionParameterHandler(
            [DisallowNull] string name,
            [DisallowNull] params string[] options)
            : this(name, (IEnumerable<string>)options)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回所有选项；非空时返回包含当前输入子串（不区分大小写）的选项。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (!IsInitialized)
            {
                yield break;
            }

            parameter = parameter.Trim();
            if (string.IsNullOrEmpty(parameter))
            {
                foreach (var option in _options)
                {
                    yield return option;
                }

                yield break;
            }

            foreach (var option in _options)
            {
                if (option.Contains(parameter, System.StringComparison.OrdinalIgnoreCase))
                {
                    yield return option;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 输入必须与某个选项精确匹配。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            if (!IsInitialized)
            {
                return false;
            }

            parameter = parameter.Trim();
            return _options.Any(t => t == parameter);
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为匹配的选项字符串。输入不匹配时返回 null。
        /// </remarks>
        public override object Parse(string parameter)
        {
            // IsValid相当于加速的Try检查
            if (IsValid(parameter))
            {
                return parameter.Trim();
            }

            return null;
        }

        /// <inheritdoc />
        public override bool IsInitialized { get; }
    }
}
