using System;
using Soyo.SoyoRuntimeConsole.Attributes;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 为控制台命令提供帮助文本。
    /// 必须与 <see cref="ConsoleCommandAttribute"/> 标记在同一个方法上。
    /// 同一个命令名只能有一个帮助文本，重复定义时只会保留第一个并发出警告。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class CommandHelpTextAttribute : Attribute
    {
        /// <summary>
        /// 命令的帮助文本。
        /// </summary>
        public string HelpText { get; }

        /// <summary>
        /// 使用指定的帮助文本。
        /// </summary>
        /// <param name="helpText">帮助文本内容</param>
        public CommandHelpTextAttribute(string helpText)
        {
            HelpText = helpText;
        }
    }
}
