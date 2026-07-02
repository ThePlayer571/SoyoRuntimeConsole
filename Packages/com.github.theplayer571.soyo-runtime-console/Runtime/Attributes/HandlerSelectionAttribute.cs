using System;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 用于标记控制台命令方法参数的特性抽象基类。
    /// 其子类携带的数据可在 <see cref="Helpers.ParameterHandlerRegistry.HandlerOf"/> 解析参数处理器时，
    /// 通过动态处理器工厂（<see cref="Helpers.ParameterHandlerRegistry.DynamicHandlerFactory"/>）读取，
    /// 以影响 <see cref="IParameterHandler"/> 的选择。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 与 <see cref="FixedFieldAttribute"/> 和 <see cref="CommandParameterAttribute"/> 不同，
    /// 这两个特性由 <see cref="Helpers.ConsoleBuilder"/> 作为一等公民直接处理。
    /// <see cref="HandlerSelectionAttribute"/> 的子类提供的是用户可扩展的 handler 选择机制，
    /// 仅在用户显式添加了子类特性时才会被收集到 <c>attributes</c> 数组中。
    /// </para>
    /// <para>
    /// 多个 <see cref="HandlerSelectionAttribute"/> 子类可同时标记在同一参数上（<c>AllowMultiple = true</c>）。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 定义自定义 handler 选择特性
    /// public sealed class RangeAttribute : HandlerSelectionAttribute
    /// {
    ///     public float Min { get; }
    ///     public float Max { get; }
    ///     public RangeAttribute(float min, float max) { Min = min; Max = max; }
    /// }
    ///
    /// // 在命令参数上使用
    /// [ConsoleCommand]
    /// public static void SetVolume([Range(0, 1)] float volume) { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public abstract class HandlerSelectionAttribute : Attribute
    {
    }
}
