using System;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 标记控制台命令方法的参数，用于自定义参数名称。
    /// 仅当方法同时标记了 <see cref="ConsoleCommandAttribute"/> 时生效。
    /// </summary>
    /// <remarks>
    /// 无参构造时使用 C# 参数的原始名称。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// 自定义的参数名称。为 null 时使用 C# 参数的原始名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 无参构造。参数名将使用 C# 参数的原始名称。
        /// </summary>
        public CommandParameterAttribute()
        {
            Name = null;
        }

        /// <summary>
        /// 使用指定的名称。
        /// </summary>
        /// <param name="name">自定义的参数名称</param>
        public CommandParameterAttribute(string name)
        {
            Name = name;
        }
    }
}
