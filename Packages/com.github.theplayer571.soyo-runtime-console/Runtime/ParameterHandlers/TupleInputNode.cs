using System;
using System.Collections.Generic;
using System.Text;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 元组参数输入的递归解析树节点。
    /// 将输入字符串一次性解析为嵌套结构，每个节点携带对应的 <see cref="IParameterHandler"/>，
    /// 以便解析时正确理解内容边界（例如字符串内的逗号不应被当作元组分隔符）。
    ///
    /// 节点有四种形态：
    /// <list type="bullet">
    /// <item><see cref="IsEmpty"/>: 输入为空</item>
    /// <item><see cref="IsParseFailed"/>: 解析不成立 — 输入非空但不匹配处理器的括号格式</item>
    /// <item><see cref="IsLeaf"/>: 叶节点 — 由非 BracketParameterHandler 的处理器负责，文本原样保留</item>
    /// <item><see cref="IsTuple"/>: 元组节点 — 由 BracketParameterHandler 负责，包含已完成的子节点和正在输入的文本</item>
    /// </list>
    /// </summary>
    internal class TupleInputNode
    {
        // ==================== Handler ====================

        /// <summary>此节点对应的参数处理器。</summary>
        public IParameterHandler Handler { get; }

        // ==================== 元组节点字段 ====================

        /// <summary>开括号字符（仅元组节点有效）。</summary>
        public char OpenChar { get; }

        /// <summary>闭括号字符（仅元组节点有效）。</summary>
        public char CloseChar { get; }

        /// <summary>闭括号是否已输入。</summary>
        public bool IsClosed { get; }

        /// <summary>
        /// 已完成的子节点列表（最后一个顶层逗号之前的部分，每个已递归解析）。
        /// </summary>
        public IReadOnlyList<TupleInputNode> Children { get; }

        /// <summary>最后一个顶层逗号之后的文本（未解析的原始文本）。</summary>
        public string InProgressText { get; }

        /// <summary>InProgressText 的递归解析结果（可能为叶节点或嵌套元组节点）。</summary>
        public TupleInputNode InProgressChild { get; }

        // ==================== 叶节点字段 ====================

        /// <summary>叶节点的原始文本值（如 "1", "\"hello\""）。</summary>
        public string LeafText { get; }

        // ==================== 便捷属性 ====================

        /// <summary>输入是否为空（null / 空字符串 / 纯空白）。</summary>
        public bool IsEmpty { get; }

        /// <summary>解析是否不成立（输入非空但首字符不匹配处理器的开括号）。</summary>
        public bool IsParseFailed { get; }

        /// <summary>是否为元组节点。</summary>
        public bool IsTuple => !IsEmpty && !IsParseFailed && OpenChar != '\0';

        /// <summary>是否为叶节点。</summary>
        public bool IsLeaf => !IsEmpty && !IsParseFailed && OpenChar == '\0';

        /// <summary>已完成子节点的数量。</summary>
        public int CompletedCount => Children?.Count ?? 0;

        /// <summary>
        /// 逗号分隔部分的原始总数（= 顶层逗号数 + 1）。
        /// 仅元组节点有效，用于检测输入部分数量是否超出处理器数量。
        /// </summary>
        public int TotalPartCount { get; }

        // ==================== 构造函数 ====================

        private TupleInputNode()
        {
            IsEmpty = true;
            Children = Array.Empty<TupleInputNode>();
            InProgressText = string.Empty;
            LeafText = string.Empty;
        }

        private TupleInputNode(IParameterHandler handler, string leafText)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            LeafText = leafText ?? string.Empty;
            Children = Array.Empty<TupleInputNode>();
            InProgressText = string.Empty;
        }

        private TupleInputNode(string rawText)
        {
            IsParseFailed = true;
            LeafText = rawText ?? string.Empty;
            Children = Array.Empty<TupleInputNode>();
            InProgressText = string.Empty;
        }

        private TupleInputNode(
            IParameterHandler handler,
            char openChar,
            char closeChar,
            bool isClosed,
            IReadOnlyList<TupleInputNode> children,
            string inProgressText,
            TupleInputNode inProgressChild,
            int totalPartCount)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            OpenChar = openChar;
            CloseChar = closeChar;
            IsClosed = isClosed;
            Children = children ?? Array.Empty<TupleInputNode>();
            InProgressText = inProgressText ?? string.Empty;
            InProgressChild = inProgressChild;
            LeafText = string.Empty;
            TotalPartCount = totalPartCount;
        }

        // ==================== 静态工厂 ====================

        /// <summary>空输入单例。</summary>
        public static readonly TupleInputNode Empty = new TupleInputNode();

        /// <summary>创建解析不成立节点（输入非空但不匹配括号格式）。</summary>
        /// <param name="rawText">原始输入文本，用于 <see cref="Reconstruct"/> 还原</param>
        public static TupleInputNode Failed(string rawText)
        {
            return new TupleInputNode(rawText);
        }

        /// <summary>创建叶节点。</summary>
        public static TupleInputNode Leaf(IParameterHandler handler, string text)
        {
            return new TupleInputNode(handler, text);
        }

        /// <summary>创建元组节点。</summary>
        public static TupleInputNode Tuple(
            IParameterHandler handler,
            char openChar,
            char closeChar,
            bool isClosed,
            IReadOnlyList<TupleInputNode> children,
            string inProgressText,
            TupleInputNode inProgressChild,
            int totalPartCount)
        {
            return new TupleInputNode(handler, openChar, closeChar, isClosed,
                children, inProgressText, inProgressChild, totalPartCount);
        }

        // ==================== 文本重建 ====================

        /// <summary>
        /// 将此节点重建为原始文本表示。
        /// 用于构建 GetCandidates 的前缀字符串。
        /// </summary>
        public string Reconstruct()
        {
            if (IsEmpty) return string.Empty;
            if (IsParseFailed) return LeafText ?? string.Empty;
            if (IsLeaf) return LeafText ?? string.Empty;

            var sb = new StringBuilder();
            sb.Append(OpenChar);

            for (var i = 0; i < Children.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Children[i].Reconstruct());
            }

            if (!string.IsNullOrEmpty(InProgressText))
            {
                if (Children.Count > 0) sb.Append(", ");
                sb.Append(InProgressText);
            }

            if (IsClosed) sb.Append(CloseChar);

            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsEmpty) return "<Empty>";
            if (IsParseFailed) return $"<Failed: '{LeafText}'>";
            if (IsLeaf) return $"<Leaf: '{LeafText}'>";
            return $"<Tuple: {OpenChar}...{(IsClosed ? CloseChar.ToString() : "")} children={Children.Count}>";
        }
    }
}
