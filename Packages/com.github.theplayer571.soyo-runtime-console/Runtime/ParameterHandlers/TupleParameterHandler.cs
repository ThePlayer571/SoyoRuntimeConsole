using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 元组参数处理器。组合多个 <see cref="IParameterHandler"/> 来解析形如 (1, 2.5, true) 或 {1.1, "hahaha"} 的括号包裹、
    /// 逗号分隔的固定长度参数输入。
    /// </summary>
    /// <remarks>
    /// 本类继承自 <see cref="BracketParameterHandler"/>，使用其默认的固定数量模式：
    /// 所有子参数必须全部匹配，且数量固定。
    /// 如需可变长度的数组参数，请使用 <see cref="ArrayParameterHandler"/>；
    /// 如需支持多种参数格式的"选择"语义，请使用 <see cref="CompositeParameterHandler"/>。
    /// </remarks>
    public abstract class TupleParameterHandler : BracketParameterHandler // abstract: Parse 由 BracketParameterHandler 声明，此处不实现
    {
        /// <inheritdoc cref="BracketParameterHandler(string, string, BracketType, IEnumerable{IParameterHandler})" />
        protected TupleParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            BracketType bracketType,
            [DisallowNull] IEnumerable<IParameterHandler> handlers)
            : base(name, type, bracketType, handlers)
        {
        }

        /// <inheritdoc cref="BracketParameterHandler(string, string, BracketType, IParameterHandler[])" />
        protected TupleParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            BracketType bracketType,
            [DisallowNull] params IParameterHandler[] handlers)
            : base(name, type, bracketType, (IEnumerable<IParameterHandler>)handlers)
        {
        }
    }
}
