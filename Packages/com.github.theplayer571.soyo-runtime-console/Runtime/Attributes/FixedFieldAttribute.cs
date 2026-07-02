using System;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 标记控制台命令方法的参数为固定字段参数。
    /// 固定字段参数要求输入精确匹配一个固定的字符串（不允许包含空白字符），
    /// 通常用于命令关键字或固定子命令，如子命令名 <c>"list"</c>、<c>"add"</c>、<c>"remove"</c>。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此参数始终返回 null——固定字段的值在命令结构层面就已确定，
    /// 不需要将解析结果传递给执行逻辑。
    /// </para>
    /// <para>
    /// 仅当方法同时标记了 <see cref="ConsoleCommandAttribute"/> 时生效。
    /// 参数类型必须为 <see cref="object"/>。
    /// </para>
    /// <para>
    /// 无参构造时使用 C# 参数的原始名称作为固定字段值。
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FixedFieldAttribute : Attribute
    {
        /// <summary>
        /// 固定字段的值。为 null 时使用 C# 参数的原始名称。
        /// </summary>
        public string FixedField { get; }

        /// <summary>
        /// 无参构造。固定字段值将使用 C# 参数的原始名称。
        /// </summary>
        public FixedFieldAttribute()
        {
            FixedField = null;
        }

        /// <summary>
        /// 使用指定的固定字段值。
        /// </summary>
        /// <param name="fixedField">固定字段的值，不能包含空白字符</param>
        public FixedFieldAttribute(string fixedField)
        {
            FixedField = fixedField;
        }
    }
}
