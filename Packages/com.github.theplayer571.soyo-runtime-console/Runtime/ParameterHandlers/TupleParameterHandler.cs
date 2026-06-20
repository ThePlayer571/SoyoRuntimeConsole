using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 元组参数处理器。组合多个 IParameterHandler 来解析形如 (1, 2.5, true) 或 {1.1, "hahaha"} 的括号包裹、
    /// 逗号分隔的固定长度参数输入。
    /// </summary>
    /// <remarks>
    /// 本类为抽象类，子类需实现 <see cref="Parse"/> 方法来将子参数组合为目标类型（如 Vector3、Color 等）。
    /// 所有子参数必须全部匹配，且数量固定——如需支持多种参数格式的"选择"语义，请使用 <see cref="CompositeParameterHandler"/>。
    /// </remarks>
    public abstract class TupleParameterHandler : ParameterHandlerBase
    {
        private readonly BracketType _bracketType;
        private readonly IReadOnlyList<IParameterHandler> _handlers;

        /// <summary>
        /// 使用指定的名称、类型、括号类型和子处理器集合构造元组参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="type">参数类型名（用于提示，如 "Vector3"）</param>
        /// <param name="bracketType">括号类型</param>
        /// <param name="handlers">子参数处理器集合（允许为空，表示空括号语法如 {}）</param>
        protected TupleParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            BracketType bracketType,
            [DisallowNull] IEnumerable<IParameterHandler> handlers)
            : base(name, type)
        {
            _bracketType = bracketType;
            _handlers = handlers?.ToArray() ?? Array.Empty<IParameterHandler>();
        }

        /// <summary>
        /// 使用指定的名称、类型、括号类型和子处理器构造元组参数处理器（params 便捷重载）。
        /// </summary>
        protected TupleParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            BracketType bracketType,
            [DisallowNull] params IParameterHandler[] handlers)
            : this(name, type, bracketType, (IEnumerable<IParameterHandler>)handlers)
        {
        }

        /// <summary>
        /// 子参数处理器列表（只读）。
        /// </summary>
        protected IReadOnlyList<IParameterHandler> Handlers => _handlers;

        /// <summary>
        /// 当前使用的括号类型。
        /// </summary>
        protected BracketType BracketType => _bracketType;

        /// <summary>
        /// 判断该实例是否成功初始化。当所有子处理器均已初始化时返回 true。
        /// 注意：空子处理器集合视为已初始化（支持 {} 空括号语法）。
        /// </summary>
        public override bool IsInitialized => _handlers.All(h => h.IsInitialized);

        /// <inheritdoc />
        public override bool ShouldAdvance(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            parameter = parameter.TrimStart();
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            var (open, close) = GetBracketChars();

            // 必须以开括号开头
            if (parameter[0] != open)
            {
                return false;
            }

            // 必须以空格结尾（与其他 handler 一致的 advance 约定）
            if (!parameter.EndsWith(' '))
            {
                return false;
            }

            // 空格之前的字符必须是闭括号
            var withoutTrailingSpaces = parameter.TrimEnd();
            return withoutTrailingSpaces.Length >= 2 && withoutTrailingSpaces[^1] == close;
        }

        /// <inheritdoc />
        public override bool IsValid(string parameter)
        {
            parameter = parameter.Trim();

            var (open, close) = GetBracketChars();

            // 长度至少为 2（开括号 + 闭括号）
            if (parameter.Length < 2)
            {
                return false;
            }

            // 必须以开括号开头，闭括号结尾
            if (parameter[0] != open || parameter[^1] != close)
            {
                return false;
            }

            // 提取括号内部内容
            var inner = parameter[1..^1];

            // 无子处理器：内部必须为空或纯空白
            if (_handlers.Count == 0)
            {
                return string.IsNullOrWhiteSpace(inner);
            }

            // 按逗号分割
            var parts = inner.Split(',');
            if (parts.Length != _handlers.Count)
            {
                return false;
            }

            // 每个部分去除首尾空白后分别校验
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (!_handlers[i].IsValid(part))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 抽象方法，子类必须实现此方法来将子参数组合为目标类型。
        /// 子类可调用 <see cref="GetParsedSubParameters"/> 获取子处理器的解析结果。
        /// </remarks>
        public abstract override object Parse(string parameter);

        /// <inheritdoc />
        /// <remarks>
        /// 补全规则：候选项需要包含完整的前缀（开括号 + 已完成子参数 + 逗号），
        /// 因为自动补全会用候选项替换最后一个参数（即当前正在输入的子参数）。
        /// 例如输入 "(" 时返回 "(0"，输入 "(1," 时返回 "(1,0"。
        ///
        /// 除当前子参数的局部补全外，始终在列表末尾提供一个"一键填充"完整结果：
        /// 剩余未填写的子参数均使用默认值，并附带闭括号。例如输入 "(" 或 "(0" 时，
        /// 在候选项末尾补充 "(0, 0, 0)"（假设三个 int 子参数，默认值均为 "0"）。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            // 去除前导空格
            parameter = parameter.TrimStart();
            var (open, close) = GetBracketChars();

            // 空输入或仅有空白：给出开括号作为提示，以及完整填充结果
            if (string.IsNullOrEmpty(parameter))
            {
                if (_handlers.Count > 0)
                {
                    yield return open.ToString();

                    var complete = BuildCompleteResult(open, close,
                        Array.Empty<string>(), -1, string.Empty);
                    if (complete != null)
                    {
                        yield return complete;
                    }
                }

                yield break;
            }

            // 必须以开括号开头
            if (parameter[0] != open)
            {
                yield break;
            }

            // 去掉开括号
            var inner = parameter[1..];

            // 如果末尾有闭括号，去掉（可能后面还跟了空格），同时记录是否存在闭括号
            inner = inner.TrimEnd();
            var hasCloseBracket = inner.EndsWith(close.ToString());
            if (hasCloseBracket)
            {
                inner = inner[..^1];
            }

            // 计算逗号数量，确定当前正在输入第几个子参数
            var commaCount = 0;
            var lastCommaIndex = -1;
            for (var i = 0; i < inner.Length; i++)
            {
                if (inner[i] == ',')
                {
                    commaCount++;
                    lastCommaIndex = i;
                }
            }

            var currentHandlerIndex = commaCount;
            if (currentHandlerIndex >= _handlers.Count)
            {
                yield break;
            }

            // 前缀：开括号 + 已完成参数（含逗号），规范化逗号后带一个空格
            var prefix = open.ToString();
            if (lastCommaIndex >= 0)
            {
                prefix += inner[..(lastCommaIndex + 1)]; // 包含最后一个逗号
                prefix = prefix.TrimEnd() + ' '; // 规范化：确保逗号后恰好一个空格
            }

            // 提取最后一个逗号之后的当前部分输入（去除前导空格）
            var currentPartial = lastCommaIndex >= 0
                ? inner[(lastCommaIndex + 1)..].TrimStart()
                : inner;

            // 仅当输入尚未包含闭括号时才提供局部补全候选项
            if (!hasCloseBracket)
            {
                var hasYielded = false;
                var candidates = _handlers[currentHandlerIndex].GetCandidates(currentPartial);
                if (candidates != null)
                {
                    foreach (var candidate in candidates)
                    {
                        hasYielded = true;
                        yield return prefix + candidate;
                    }
                }

                // 子处理器无候选项时，回退到用户已输入的原始文本
                if (!hasYielded)
                {
                    var trimmed = currentPartial.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        yield return prefix + trimmed;
                    }
                }
            }

            // 输入已包含闭括号：用户已表明完结意图，仅返回输入自身
            if (hasCloseBracket)
            {
                yield return parameter.Trim();
                yield break;
            }

            // 提取已完成子参数（当前正在输入的子参数之前的部分）
            var completedParts = new string[currentHandlerIndex];
            if (currentHandlerIndex > 0)
            {
                var beforeLastComma = inner[..lastCommaIndex];
                var parts = beforeLastComma.Split(',');
                for (var i = 0; i < parts.Length && i < currentHandlerIndex; i++)
                {
                    completedParts[i] = parts[i].Trim();
                }
            }

            // 始终在末尾提供"一键填充"完整结果
            var completeResult = BuildCompleteResult(open, close,
                completedParts, currentHandlerIndex, currentPartial);
            if (completeResult != null)
            {
                yield return completeResult;
            }
        }

        /// <summary>
        /// 构建"一键填充"完整结果：已输入的部分保持原样，剩余未填写的子参数使用默认值填充，
        /// 末尾附带闭括号。
        /// </summary>
        /// <returns>完整的结果字符串；若无法获取某个位置的默认值则返回 null。</returns>
        private string BuildCompleteResult(char open, char close,
            string[] completedParts, int currentHandlerIndex, string currentPartial)
        {
            var sb = new StringBuilder();
            sb.Append(open);

            for (var i = 0; i < _handlers.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                string value;
                if (i < currentHandlerIndex)
                {
                    // 已完成的子参数，直接使用
                    value = i < completedParts.Length ? completedParts[i] : string.Empty;
                }
                else if (i == currentHandlerIndex)
                {
                    // 正在输入的子参数：优先使用子处理器的最佳匹配，无匹配则保留原始输入
                    value = currentPartial.Trim();
                    if (string.IsNullOrEmpty(value))
                    {
                        value = GetDefaultForHandler(i);
                        if (value == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        var bestMatch = _handlers[i].GetCandidates(value)?.FirstOrDefault();
                        if (bestMatch != null)
                        {
                            value = bestMatch;
                        }
                    }
                }
                else
                {
                    // 尚未开始的子参数，使用默认值
                    value = GetDefaultForHandler(i);
                    if (value == null)
                    {
                        return null;
                    }
                }

                sb.Append(value);
            }

            sb.Append(close);
            return sb.ToString();
        }

        /// <summary>
        /// 获取指定子处理器的默认值（<see cref="IParameterHandler.GetCandidates"/> 对空输入的第一个候选项）。
        /// </summary>
        /// <returns>默认值；若无候选项则返回 null。</returns>
        private string GetDefaultForHandler(int index)
        {
            if (index < 0 || index >= _handlers.Count)
            {
                return null;
            }

            var candidates = _handlers[index].GetCandidates(string.Empty);
            return candidates?.FirstOrDefault();
        }

        /// <summary>
        /// 获取子参数的解析结果。将括号内部按逗号分割、去除空白后，
        /// 依次调用每个子处理器的 <see cref="IParameterHandler.Parse"/> 方法。
        /// </summary>
        /// <param name="parameter">已通过 <see cref="IsValid"/> 验证的参数字符串</param>
        /// <returns>各子处理器的解析结果数组</returns>
        protected object[] GetParsedSubParameters([DisallowNull] string parameter)
        {
            var normalized = parameter.Trim();
            var (open, close) = GetBracketChars();
            var inner = normalized[1..^1];

            if (_handlers.Count == 0)
            {
                return Array.Empty<object>();
            }

            var parts = inner.Split(',');
            var results = new object[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                results[i] = _handlers[i].Parse(parts[i].Trim());
            }

            return results;
        }

        private (char open, char close) GetBracketChars()
        {
            return _bracketType switch
            {
                BracketType.Parentheses => ('(', ')'),
                BracketType.Braces => ('{', '}'),
                BracketType.Brackets => ('[', ']'),
                _ => throw new ArgumentOutOfRangeException(nameof(_bracketType), _bracketType, null)
            };
        }
    }
}
