using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 空格分隔参数处理器基类。为使用空格作为参数结束标志的处理器提供统一的
    /// <see cref="ShouldAdvance"/> 实现。
    /// </summary>
    /// <remarks>
    /// 继承此类的处理器（如 <see cref="IntegerParameterHandler"/>、<see cref="FloatParameterHandler"/>、
    /// <see cref="BooleanParameterHandler"/>、<see cref="EnumParameterHandler"/>、
    /// <see cref="FixedFieldParameterHandler"/>、<see cref="StringOptionParameterHandler"/>）
    /// 共享相同的 advance 逻辑：当输入以规范化后的空格结尾时，认为当前参数已完整。
    ///
    /// 规范化通过 <see cref="ParameterHandlerParsingUtility.NormalizeSpaceSplitParameter"/> 完成，
    /// 该工具方法将连续空白字符压缩为单个空格。
    /// </remarks>
    public abstract class SpaceSplitParameterHandlerBase : ParameterHandlerBase
    {
        /// <inheritdoc cref="ParameterHandlerBase(string, string)" />
        protected SpaceSplitParameterHandlerBase([CanBeNull] string name, [CanBeNull] string type) : base(name, type)
        {
        }

        /// <inheritdoc cref="ParameterHandlerBase(in IParameterHandler.Description)" />
        protected SpaceSplitParameterHandlerBase(in IParameterHandler.Description description) : base(in description)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// 当输入非空且以规范化后的空格结尾时返回 true。
        /// </remarks>
        public override bool ShouldAdvance(string parameter)
        {
            return !string.IsNullOrEmpty(parameter) && Normalize(parameter).EndsWith(' ');
        }

        /// <summary>
        /// 规范化输入：将连续空白字符压缩为单个空格。
        /// </summary>
        protected string Normalize([DisallowNull] string parameter) =>
            ParameterHandlerParsingUtility.NormalizeSpaceSplitParameter(parameter);
    }
}