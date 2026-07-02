using System;
using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.Helpers;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 标记一个静态方法为控制台命令。
    /// 被标记的方法必须是 <c>public</c> 或 <c>private</c> 的静态方法，
    /// 且必须声明在类自身（<c>BindingFlags.DeclaredOnly</c>）。
    /// </summary>
    /// <remarks>
    /// 无参构造时，命令名将使用方法名（由扫描器填充）。
    /// 方法的每个参数会通过 <see cref="ParameterHandlerRegistry"/> 解析。
    /// 不支持泛型方法。带默认值的参数会被自动展开为多个命令变体。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ConsoleCommandAttribute : Attribute
    {
        /// <summary>
        /// 命令名。无参构造时为 null，扫描时使用方法名填充。
        /// </summary>
        public CommandName? Name { get; }

        /// <summary>
        /// 无参构造。命令名将使用方法名（扫描时填充）。
        /// </summary>
        public ConsoleCommandAttribute()
        {
            Name = null;
        }

        /// <summary>
        /// 使用字符串指定命令名。
        /// </summary>
        /// <param name="name">命令名字符串，会经过 <see cref="CommandName"/> 的规范化处理</param>
        public ConsoleCommandAttribute([DisallowNull] string name)
        {
            Name = new CommandName(name);
            if (Name.Value.IsNullName)
            {
                Name = null;
            }
        }

        /// <summary>
        /// 使用 <see cref="CommandName"/> 直接指定命令名。
        /// </summary>
        /// <param name="name">已构造好的命令名</param>
        public ConsoleCommandAttribute(CommandName name)
        {
            Name = name;
        }
    }
}