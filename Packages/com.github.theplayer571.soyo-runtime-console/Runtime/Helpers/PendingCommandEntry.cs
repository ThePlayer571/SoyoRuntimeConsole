using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Helpers
{
    /// <summary>
    /// 扫描阶段暂存的原始命令方法数据。
    /// 在扫描阶段收集，在构建阶段解析为 <see cref="ConsoleCommandDefinition"/>。
    /// </summary>
    public readonly struct PendingCommandEntry
    {
        /// <summary>
        /// 标记了 <see cref="Attributes.ConsoleCommandAttribute"/> 的静态方法。
        /// </summary>
        [DisallowNull]
        public readonly MethodInfo Method;

        /// <summary>
        /// 解析后的命令名（优先使用 <see cref="Attributes.ConsoleCommandAttribute.Name"/>，否则为方法名）。
        /// </summary>
        public readonly CommandName CommandName;

        /// <summary>
        /// 命令帮助文本（来自 <see cref="Attributes.CommandHelpTextAttribute"/>）。无则为 null。
        /// </summary>
        [AllowNull]
        public readonly string HelpText;

        /// <summary>
        /// 创建新的暂存命令条目。
        /// </summary>
        /// <param name="method">标记了命令属性的静态方法</param>
        /// <param name="commandName">解析后的命令名</param>
        /// <param name="helpText">命令帮助文本，无则为 null</param>
        public PendingCommandEntry(
            [DisallowNull] MethodInfo method,
            CommandName commandName,
            [AllowNull] string helpText)
        {
            Method = method;
            CommandName = commandName;
            HelpText = helpText;
        }
    }
}
