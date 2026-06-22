using System;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 指定命令或命令类所属的控制台 Key，用于在 <c>Console.Create(ConsoleKey)</c> 时过滤命令。
    /// 可以标记在类或方法上。方法级的 Key 优先于类级。
    /// 未标记此特性的命令为全局命令，会被所有 ConsoleKey 包含。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class TargetConsoleKeyAttribute : Attribute
    {
        /// <summary>
        /// 目标控制台 Key。
        /// </summary>
        public ConsoleKey Key { get; }

        /// <summary>
        /// 使用字符串指定目标控制台 Key。
        /// </summary>
        /// <param name="key">控制台 Key 的字符串表示</param>
        public TargetConsoleKeyAttribute(string key)
        {
            Key = new ConsoleKey(key);
        }
    }
}
