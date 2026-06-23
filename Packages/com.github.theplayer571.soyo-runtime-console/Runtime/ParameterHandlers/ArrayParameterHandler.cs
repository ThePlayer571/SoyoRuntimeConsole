using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 变长数组参数处理器（泛型版本）。继承自 <see cref="BracketParameterHandler"/>，支持可变长度的方括号包裹、
    /// 逗号分隔的数组输入，如 [0, 0, 0]、[1, 2, 3, 4, 5]。
    /// 所有位置的子参数使用同一个元素处理器解析。
    /// </summary>
    /// <typeparam name="T">数组元素类型。Parse 返回的 object 在 box 内为 T[]，可直接 cast 为 T[]。</typeparam>
    /// <example>
    /// <code>
    /// var handler = new ArrayParameterHandler&lt;int&gt;("values", new IntegerParameterHandler("value"));
    /// handler.IsValid("[1, 2, 3]"); // true
    /// handler.IsValid("[]"); // true（空数组）
    /// var result = (int[])handler.Parse("[1, 2, 3]"); // int[] { 1, 2, 3 }
    /// </code>
    /// </example>
    public class ArrayParameterHandler<T> : BracketParameterHandler
    {
        /// <summary>
        /// 使用指定的名称和元素处理器构造变长数组参数处理器。
        /// 括号类型固定为方括号 []，数组类型名根据元素处理器自动生成。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="elementHandler">用于解析每个数组元素的处理器</param>
        public ArrayParameterHandler(
            [AllowNull] string name,
            [DisallowNull] IParameterHandler elementHandler)
            : base(name, GenerateArrayType(elementHandler), BracketType.Brackets, elementHandler)
        {
        }

        // ==================== 变长模式：重写 5 个虚方法 ====================

        /// <inheritdoc />
        /// <remarks>变长模式下所有位置使用同一元素处理器。</remarks>
        protected override IParameterHandler GetHandlerForPosition(int index)
        {
            return Handlers[0];
        }

        /// <inheritdoc />
        /// <remarks>变长模式下不限制位置数量。</remarks>
        protected override bool CanCreateChildAt(int index)
        {
            return true;
        }

        /// <inheritdoc />
        /// <remarks>变长模式下任意数量均合法。</remarks>
        protected override bool IsElementCountValid(int totalPartCount)
        {
            return true;
        }

        /// <inheritdoc />
        /// <remarks>空输入时默认填充 1 个元素。</remarks>
        protected override int GetEmptyFillCount()
        {
            return 1;
        }

        /// <inheritdoc />
        /// <remarks>根据已有子节点数量动态确定填充数量（已完成 + 1 个 in-progress）。</remarks>
        protected override int GetFillElementCount(int childrenCount)
        {
            return childrenCount + 1;
        }

        // ==================== Parse ====================

        /// <inheritdoc />
        /// <remarks>
        /// 返回 T[]（装箱为 object）。调用者可直接 cast 为目标数组类型，无需逐元素转换。
        /// 例如：<c>(int[])handler.Parse("[1, 2, 3]")</c>。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return GetParsedSubParameters(parameter).Cast<T>().ToArray();
        }

        /// <summary>
        /// 根据元素处理器的类型生成数组类型名（如 "Integer[]"）。
        /// </summary>
        private static string GenerateArrayType(IParameterHandler handler)
        {
            var elementType = handler.GetDescription().Type;
            return elementType != null ? $"{elementType}[]" : null;
        }
    }

    /// <summary>
    /// 变长数组参数处理器（非泛型版本，等价于 <see cref="ArrayParameterHandler{T}"/> 中 T 为 <see cref="object"/>）。
    /// 保留以提供向后兼容：Parse 返回 object[]。
    /// 如需强类型数组（如 int[]、float[]），请使用 <see cref="ArrayParameterHandler{T}"/>。
    /// </summary>
    /// <example>
    /// <code>
    /// // 非泛型用法（向后兼容）
    /// var handler = new ArrayParameterHandler("values", new IntegerParameterHandler("value"));
    /// var result = (object[])handler.Parse("[1, 2, 3]"); // object[] { 1, 2, 3 }
    ///
    /// // 泛型用法（推荐）
    /// var handler2 = new ArrayParameterHandler&lt;int&gt;("values", new IntegerParameterHandler("value"));
    /// var result2 = (int[])handler2.Parse("[1, 2, 3]"); // int[] { 1, 2, 3 }
    /// </code>
    /// </example>
    public class ArrayParameterHandler : ArrayParameterHandler<object>
    {
        /// <summary>
        /// 使用指定的名称和元素处理器构造变长数组参数处理器。
        /// 括号类型固定为方括号 []，数组类型名根据元素处理器自动生成。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="elementHandler">用于解析每个数组元素的处理器</param>
        public ArrayParameterHandler(
            [AllowNull] string name,
            [DisallowNull] IParameterHandler elementHandler)
            : base(name, elementHandler)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 返回 <see cref="object"/>[]，每个元素为元素处理器解析后的结果。
        /// 调用者可根据需要转换为具体数组类型。
        /// 直接返回 <see cref="BracketParameterHandler.GetParsedSubParameters"/> 的结果，
        /// 跳过不必要的 Cast&lt;object&gt; 以保持与旧版本完全一致的行为。
        /// </remarks>
        public override object Parse(string parameter)
        {
            return GetParsedSubParameters(parameter);
        }
    }
}
