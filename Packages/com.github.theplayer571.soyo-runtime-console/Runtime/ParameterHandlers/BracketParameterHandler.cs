using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 括号参数处理器基类。提供括号包裹、逗号分隔的参数输入解析基础设施，
    /// 包括树解析（<see cref="TupleInputNode"/>）、验证、补全和子参数解析。
    /// </summary>
    /// <remarks>
    /// 本类为抽象类，子类需实现 <see cref="Parse"/> 方法。
    /// 子类可通过重写 5 个虚方法来自定义元素数量限制和处理器分配策略：
    /// <list type="bullet">
    /// <item><see cref="TupleParameterHandler"/>：固定数量，按索引分配不同处理器（乘积类型）</item>
    /// <item><see cref="ArrayParameterHandler"/>：可变数量，所有位置使用同一处理器（数组类型）</item>
    /// </list>
    /// 如需支持多种参数格式的"选择"语义，请使用 <see cref="CompositeParameterHandler"/>。
    /// </remarks>
    public abstract class BracketParameterHandler : ParameterHandlerBase
    {
        private readonly BracketType _bracketType;
        private readonly IReadOnlyList<IParameterHandler> _handlers;

        /// <summary>
        /// 使用指定的名称、类型、括号类型和子处理器集合构造括号参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="type">参数类型名（用于提示，如 "Vector3"）</param>
        /// <param name="bracketType">括号类型</param>
        /// <param name="handlers">子参数处理器集合（允许为空，表示空括号语法如 {}）</param>
        protected BracketParameterHandler(
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
        /// 使用指定的名称、类型、括号类型和子处理器构造括号参数处理器（params 便捷重载）。
        /// </summary>
        protected BracketParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            BracketType bracketType,
            [DisallowNull] params IParameterHandler[] handlers)
            : this(name, type, bracketType, (IEnumerable<IParameterHandler>)handlers)
        {
        }

        // ==================== 虚方法：子类可重写以自定义行为 ====================

        /// <summary>
        /// 获取指定位置的子参数处理器。
        /// 默认按索引从处理器列表中获取（固定数量模式）。
        /// <see cref="ArrayParameterHandler"/> 重写此方法始终返回第一个处理器。
        /// </summary>
        protected virtual IParameterHandler GetHandlerForPosition(int index)
        {
            return _handlers[index];
        }

        /// <summary>
        /// 判断是否可以在指定位置创建子节点。
        /// 默认仅在索引不超出处理器数量时允许（固定数量模式）。
        /// <see cref="ArrayParameterHandler"/> 重写此方法始终返回 true。
        /// </summary>
        protected virtual bool CanCreateChildAt(int index)
        {
            return index < _handlers.Count;
        }

        /// <summary>
        /// 验证元素总数是否合法。
        /// 默认要求元素数量与处理器数量完全匹配（固定数量模式）。
        /// <see cref="ArrayParameterHandler"/> 重写此方法始终返回 true。
        /// </summary>
        protected virtual bool IsElementCountValid(int totalPartCount)
        {
            return totalPartCount == _handlers.Count;
        }

        /// <summary>
        /// 获取空输入时的默认填充元素数量。
        /// 默认填充全部处理器位置（固定数量模式）。
        /// <see cref="ArrayParameterHandler"/> 重写此方法返回 1（单元素默认数组）。
        /// </summary>
        protected virtual int GetEmptyFillCount()
        {
            return _handlers.Count;
        }

        /// <summary>
        /// 获取 <see cref="BuildCompleteRecursive"/> 中的填充元素数量。
        /// 默认填充全部处理器位置（固定数量模式）。
        /// <see cref="ArrayParameterHandler"/> 重写此方法根据已有的子节点数量动态确定。
        /// </summary>
        /// <param name="childrenCount">当前已完成的子节点数量</param>
        protected virtual int GetFillElementCount(int childrenCount)
        {
            return _handlers.Count;
        }

        // ==================== 属性 ====================

        /// <summary>
        /// 子参数处理器列表（只读）。对程序集内可见，供 <see cref="TupleInputNode"/> 递归解析时访问。
        /// </summary>
        protected internal IReadOnlyList<IParameterHandler> Handlers => _handlers;

        /// <summary>
        /// 当前使用的括号类型。对程序集内可见，供 <see cref="TupleInputNode"/> 递归解析时访问。
        /// </summary>
        protected internal BracketType BracketType => _bracketType;

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

            var startsWithOpen = parameter[0] == open;

            // 以开括号开头
            if (startsWithOpen)
            {
                // 必须以空格结尾（与其他 handler 一致的 advance 约定）
                if (!parameter.EndsWith(' '))
                {
                    return false;
                }

                // 空格之前的字符必须是闭括号
                var withoutTrailingSpaces = parameter.TrimEnd();
                return withoutTrailingSpaces.Length >= 2 && withoutTrailingSpaces[^1] == close;
            }
            else
            {
                // 不以开括号开头：这是完全不可理喻的参数，按照空格分隔
                return parameter.EndsWith(' ');
            }
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

            // 尾随逗号：最后一个非空白字符是逗号 → 非法（如 [1, 2,] 或 [1, 2, ]）
            var trimmedInner = inner.TrimEnd();
            if (trimmedInner.Length > 0 && trimmedInner[^1] == ',')
            {
                return false;
            }

            // 无子处理器：内部必须为空或纯空白
            if (_handlers.Count == 0)
            {
                return string.IsNullOrWhiteSpace(inner);
            }

            // 使用统一的树解析，然后递归验证
            var root = ParseInput(parameter);

            if (root.IsParseFailed || !root.IsTuple || !root.IsClosed)
            {
                return false;
            }

            return ValidateNode(root);
        }

        /// <summary>
        /// 递归验证树节点及其所有子节点。
        /// </summary>
        private bool ValidateNode(TupleInputNode node)
        {
            if (node.IsLeaf)
            {
                return node.Handler.IsValid(node.LeafText);
            }

            if (!node.IsTuple || !node.IsClosed)
            {
                return false;
            }

            var bh = (BracketParameterHandler)node.Handler;
            if (!bh.IsElementCountValid(node.TotalPartCount))
            {
                return false;
            }

            for (var i = 0; i < node.Children.Count; i++)
            {
                if (!ValidateNode(node.Children[i]))
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
        ///
        /// 输入解析委托给 <see cref="ParseInput"/>，其产出 <see cref="TupleInputNode"/> 树，
        /// 随后通过树遍历确定活跃子处理器、构建前缀并生成候选项。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            parameter = parameter.TrimStart();
            var (open, close) = GetBracketChars();

            var root = ParseInput(parameter);

            // 输入完全不成立 → 无候选项
            if (root.IsParseFailed)
            {
                yield break;
            }

            // 空输入：给出开括号提示 + 一键填充完整结果
            if (root == TupleInputNode.Empty)
            {
                if (_handlers.Count > 0)
                {
                    yield return open.ToString();

                    var complete = BuildCompleteFromTree(root, open, close);
                    if (complete != null)
                    {
                        yield return complete;
                    }
                }

                yield break;
            }

            // 不以开括号开头：无效
            if (!root.IsTuple)
            {
                yield break;
            }

            // 输入已包含闭括号：用户已表明完结意图，仅返回输入自身
            if (root.IsClosed)
            {
                yield return parameter.Trim();
                yield break;
            }

            // 找到当前层的活跃节点（不递归——递归由各层 BracketParameterHandler.GetCandidates 自行处理）
            var activeInfo = FindActiveNode(root);
            if (activeInfo == null)
            {
                yield break;
            }

            // 局部补全候选项
            {
                var activeHandler = activeInfo.Handler;
                var activeText = activeInfo.Text;
                var prefix = activeInfo.Prefix;

                var hasYielded = false;
                var candidates = activeHandler.GetCandidates(activeText);
                if (candidates != null)
                {
                    var isActiveTextEmpty = string.IsNullOrEmpty(activeText);
                    foreach (var candidate in candidates)
                    {
                        // 当前子参数输入为空且候选项是纯括号提示（如 "("、"{"、"["）时，
                        // 同时保留括号提示并展开以获取内层值候选项。
                        if (isActiveTextEmpty && IsSingleBracket(candidate))
                        {
                            hasYielded = true;
                            yield return prefix + candidate;

                            var deeperCandidates = activeHandler.GetCandidates(candidate);
                            if (deeperCandidates != null)
                            {
                                foreach (var dc in deeperCandidates)
                                {
                                    hasYielded = true;
                                    yield return prefix + dc;
                                }
                            }
                        }
                        else
                        {
                            hasYielded = true;
                            yield return prefix + candidate;
                        }
                    }
                }

                // 子处理器无候选项时，回退到用户已输入的原始文本
                if (!hasYielded)
                {
                    var trimmed = activeText.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        yield return prefix + trimmed;
                    }
                }
            }

            // 始终在末尾提供"一键填充"完整结果
            var completeResult = BuildCompleteFromTree(root, open, close);
            if (completeResult != null)
            {
                yield return completeResult;
            }
        }

        // ==================== 树解析 ====================

        /// <summary>
        /// 递归解析输入字符串为 <see cref="TupleInputNode"/> 树。
        /// 这是所有解析的单一入口：<see cref="GetCandidates"/>、<see cref="IsValid"/>、
        /// <see cref="GetParsedSubParameters"/> 均通过此方法获取结构化输入表示。
        /// </summary>
        /// <param name="parameter">已去除前导空格的输入字符串</param>
        /// <returns>解析后的树；输入为空时返回 <see cref="TupleInputNode.Empty"/>，不成立时返回 <see cref="TupleInputNode.Failed"/></returns>
        private TupleInputNode ParseInput(string parameter)
        {
            parameter = parameter.TrimStart();
            if (string.IsNullOrEmpty(parameter))
            {
                return TupleInputNode.Empty;
            }

            var (open, close) = GetBracketChars();

            // 不成立：非空但首字符不是本处理器的开括号
            if (parameter[0] != open)
            {
                return TupleInputNode.Failed(parameter);
            }

            // 去掉开括号
            var inner = parameter[1..];

            // 检测闭括号：若末尾有闭括号且移除后剩余内容括号平衡，
            // 则该闭括号属于本层元组而非内层嵌套元组
            inner = inner.TrimEnd();
            var isClosed = false;
            if (inner.Length > 0 && inner[^1] == close)
            {
                var withoutClose = inner[..^1];
                if (IsBracketBalanced(withoutClose))
                {
                    isClosed = true;
                    inner = withoutClose;
                }
            }

            // 一次遍历：统计顶层逗号并记录位置（替代 SplitByTopLevelComma 的二次遍历）
            var commaPositions = new List<int>();
            var depth = 0;
            for (var i = 0; i < inner.Length; i++)
            {
                var c = inner[i];
                if (c == '(' || c == '{' || c == '[')
                {
                    depth++;
                }
                else if (c == ')' || c == '}' || c == ']')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    commaPositions.Add(i);
                }
            }

            // 根据逗号位置分割并递归解析已完成子节点
            var children = new List<TupleInputNode>();
            var inProgressStart = 0;

            for (var i = 0; i < commaPositions.Count; i++)
            {
                var commaPos = commaPositions[i];
                var part = inner[inProgressStart..commaPos].Trim();

                if (CanCreateChildAt(i))
                {
                    children.Add(CreateChildNode(GetHandlerForPosition(i), part));
                }

                inProgressStart = commaPos + 1;
            }

            // 逗号之后的内容 = 正在输入的文本
            var inProgressTextRaw = inner[inProgressStart..];
            var inProgressText = inProgressTextRaw.TrimStart();

            // 递归解析 in-progress 文本
            var currentHandlerIndex = commaPositions.Count;
            TupleInputNode inProgressChild = null;
            if (!string.IsNullOrEmpty(inProgressText) && CanCreateChildAt(currentHandlerIndex))
            {
                inProgressChild = CreateChildNode(GetHandlerForPosition(currentHandlerIndex), inProgressText);
            }

            // 如果已闭合且 in-progress 非空，将其移入 children
            if (isClosed && inProgressChild != null)
            {
                children.Add(inProgressChild);
                inProgressChild = null;
                inProgressText = null;
            }
            else if (isClosed && !string.IsNullOrEmpty(inProgressText) && inProgressChild == null)
            {
                // in-progress 文本未能解析为子节点（如为空或 handler 索引越界）
                // 但在闭合状态下应视为完整子节点
                if (CanCreateChildAt(currentHandlerIndex))
                {
                    children.Add(CreateChildNode(GetHandlerForPosition(currentHandlerIndex), inProgressText));
                    inProgressText = null;
                }
            }
            else if (isClosed && string.IsNullOrEmpty(inProgressText) && inProgressTextRaw.Length > 0
                     && CanCreateChildAt(currentHandlerIndex))
            {
                // 闭合括号内仅有空白字符（如 "[ ]" 或 "[1,  ]"）：
                // TrimStart 后为空，说明原始文本全为空白，需将其作为子节点提交给处理器验证
                // （大多数处理器会拒绝纯空白，从而正确返回 IsValid = false）
                children.Add(CreateChildNode(GetHandlerForPosition(currentHandlerIndex), inProgressTextRaw));
                inProgressText = null;
            }

            return TupleInputNode.Tuple(
                handler: this,
                openChar: open,
                closeChar: close,
                isClosed: isClosed,
                children: children,
                inProgressText: inProgressText ?? string.Empty,
                inProgressChild: inProgressChild,
                totalPartCount: commaPositions.Count + 1);
        }

        /// <summary>
        /// 为子处理器创建对应的树节点。若子处理器是 BracketParameterHandler，递归解析；
        /// 否则创建叶节点。
        /// </summary>
        private static TupleInputNode CreateChildNode(IParameterHandler handler, string text)
        {
            if (handler is BracketParameterHandler bh)
            {
                return bh.ParseInput(text);
            }

            return TupleInputNode.Leaf(handler, text);
        }

        /// <summary>
        /// 在树中找到当前层的活跃节点——即用户正在输入的子参数位置。
        /// 不递归进入嵌套元组——嵌套元组由其自身的 GetCandidates 处理。
        /// 返回活跃子处理器的信息，以及用于候选项的完整前缀。
        /// </summary>
        private ActiveNodeInfo FindActiveNode(TupleInputNode root)
        {
            if (root == null || !root.IsTuple)
            {
                return null;
            }

            var bh = (BracketParameterHandler)root.Handler;
            var currentIndex = root.CompletedCount;

            // 超出处理器数量
            if (!bh.CanCreateChildAt(currentIndex))
            {
                return null;
            }

            return new ActiveNodeInfo
            {
                Handler = bh.GetHandlerForPosition(currentIndex),
                Text = root.InProgressText ?? string.Empty,
                Prefix = BuildNodePrefix(root),
            };
        }

        /// <summary>
        /// 活跃节点查找结果。
        /// </summary>
        private sealed class ActiveNodeInfo
        {
            /// <summary>应提供候选项的处理器。</summary>
            public IParameterHandler Handler;

            /// <summary>传递给 Handler.GetCandidates 的当前输入文本。</summary>
            public string Text;

            /// <summary>候选项前缀（从根到活跃节点之前的所有内容）。</summary>
            public string Prefix;
        }

        /// <summary>
        /// 构建元组节点的前缀字符串：开括号 + 已完成子节点的重建文本 + 逗号空格。
        /// 例如 "(1, " 或 "("（如果无已完成子节点）。
        /// </summary>
        private static string BuildNodePrefix(TupleInputNode node)
        {
            if (!node.IsTuple)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append(node.OpenChar);

            for (var i = 0; i < node.Children.Count; i++)
            {
                sb.Append(node.Children[i].Reconstruct());
                sb.Append(", ");
            }

            return sb.ToString();
        }

        // ==================== 一键填充（基于树） ====================

        /// <summary>
        /// 基于输入树构建"一键填充"完整结果字符串。
        /// 已输入的子参数保持原样，剩余未填写的使用默认值填充，末尾附带闭括号。
        /// </summary>
        private string BuildCompleteFromTree(TupleInputNode root, char open, char close)
        {
            if (root == TupleInputNode.Empty)
            {
                // 空输入 → 所有字段使用默认值
                var sb = new StringBuilder();
                sb.Append(open);

                var fillCount = GetEmptyFillCount();
                for (var i = 0; i < fillCount; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    var def = GetDefaultForHandler(GetHandlerForPosition(i));
                    if (def == null)
                    {
                        return null;
                    }

                    sb.Append(def);
                }

                sb.Append(close);
                return sb.ToString();
            }

            if (!root.IsTuple)
            {
                return null;
            }

            return BuildCompleteRecursive(root, open, close);
        }

        /// <summary>
        /// 递归构建"一键填充"结果字符串。
        /// </summary>
        private string BuildCompleteRecursive(TupleInputNode node, char open, char close)
        {
            var bh = (BracketParameterHandler)node.Handler;
            var sb = new StringBuilder();
            sb.Append(open);

            var elementCount = bh.GetFillElementCount(node.Children.Count);
            for (var i = 0; i < elementCount; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                if (i < node.Children.Count)
                {
                    // 已完成的子节点：直接使用重建文本
                    sb.Append(node.Children[i].Reconstruct());
                }
                else if (i == node.CompletedCount && node.InProgressChild != null)
                {
                    // 正在输入的子节点
                    if (node.InProgressChild.IsParseFailed)
                    {
                        // 解析不成立：使用原始文本
                        var rawText = (node.InProgressChild.LeafText ?? string.Empty).TrimStart();
                        if (!string.IsNullOrEmpty(rawText))
                        {
                            sb.Append(rawText);
                        }
                        else
                        {
                            var def = GetDefaultForHandler(bh.GetHandlerForPosition(i));
                            if (def == null) return null;
                            sb.Append(def);
                        }
                    }
                    else if (node.InProgressChild.IsLeaf)
                    {
                        var text = (node.InProgressChild.LeafText ?? string.Empty).TrimStart();
                        if (!string.IsNullOrEmpty(text))
                        {
                            // 尝试找到最佳匹配
                            var bestMatch = node.InProgressChild.Handler
                                .GetCandidates(text)?.FirstOrDefault();
                            sb.Append(bestMatch ?? text);
                        }
                        else
                        {
                            var def = GetDefaultForHandler(node.InProgressChild.Handler);
                            if (def == null)
                            {
                                return null;
                            }

                            sb.Append(def);
                        }
                    }
                    else
                    {
                        // In-progress 是嵌套元组 → 递归
                        var innerResult = BuildCompleteRecursive(
                            node.InProgressChild,
                            node.InProgressChild.OpenChar,
                            node.InProgressChild.CloseChar);
                        if (innerResult == null)
                        {
                            return null;
                        }

                        sb.Append(innerResult);
                    }
                }
                else
                {
                    // 尚未开始的子参数：使用默认值
                    var handler = bh.GetHandlerForPosition(i);
                    var def = GetDefaultForHandler(handler);
                    if (def == null)
                    {
                        return null;
                    }

                    sb.Append(def);
                }
            }

            sb.Append(close);
            return sb.ToString();
        }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// 获取指定处理器的默认值。
        /// 跳过仅包含单个括号字符的候选项（如 "("、"{"、"["），
        /// 因为这些是 UI 提示而非有效的完整默认值。
        /// </summary>
        /// <returns>默认值；若无有效候选项则返回 null。</returns>
        private static string GetDefaultForHandler(IParameterHandler handler)
        {
            var candidates = handler.GetCandidates(string.Empty);
            return candidates?.FirstOrDefault(c => !IsSingleBracket(c));
        }

        /// <summary>
        /// 判断字符串是否仅包含一个括号字符。
        /// </summary>
        private static bool IsSingleBracket(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 1) return false;
            var c = s[0];
            return c == '(' || c == ')' || c == '{' || c == '}' || c == '[' || c == ']';
        }

        /// <summary>
        /// 判断字符串中所有括号是否平衡（每种括号的开闭数量相等，总深度为 0）。
        /// 支持圆括号 ()、花括号 {}、方括号 [] 三种括号的深度追踪。
        /// 用于区分内层元组的闭括号和外层元组的闭括号。
        /// </summary>
        private static bool IsBracketBalanced(string s)
        {
            var depth = 0;
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '(' || c == '{' || c == '[') depth++;
                else if (c == ')' || c == '}' || c == ']') depth--;
            }

            return depth == 0;
        }

        /// <summary>
        /// 获取子参数的解析结果。通过解析树遍历将各子节点委托给对应的处理器进行解析。
        /// </summary>
        /// <param name="parameter">已通过 <see cref="IsValid"/> 验证的参数字符串</param>
        /// <returns>各子处理器的解析结果数组</returns>
        protected object[] GetParsedSubParameters([DisallowNull] string parameter)
        {
            var root = ParseInput(parameter.Trim());

            if (root.IsParseFailed || !root.IsTuple || !root.IsClosed)
            {
                return Array.Empty<object>();
            }

            if (_handlers.Count == 0)
            {
                return Array.Empty<object>();
            }

            var results = new object[root.Children.Count];
            for (var i = 0; i < root.Children.Count; i++)
            {
                results[i] = ParseChild(root.Children[i]);
            }

            return results;
        }

        /// <summary>
        /// 递归解析子节点。叶节点委托给处理器；嵌套元组重建完整文本后委托给其处理器。
        /// </summary>
        private static object ParseChild(TupleInputNode node)
        {
            if (node.IsParseFailed) throw new ArgumentException("ParseChild called on a failed node.", nameof(node));
            if (node.IsLeaf)
            {
                return node.Handler.Parse(node.LeafText);
            }

            // 嵌套元组：重建完整括号文本 → 委托给元组处理器的 Parse
            return node.Handler.Parse(node.Reconstruct());
        }

        /// <summary>
        /// 获取当前元组的开/闭括号字符对。
        /// </summary>
        internal (char open, char close) GetBracketChars()
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