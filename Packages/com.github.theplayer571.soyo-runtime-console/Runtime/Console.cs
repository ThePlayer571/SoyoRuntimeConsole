using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.Attributes;

namespace Soyo.SoyoRuntimeConsole
{
    public sealed class Console : ConsoleBase
    {
        private Console(ConsoleConfig config) : base(config)
        {
        }

        /// <summary>
        /// 使用指定配置创建控制台实例。
        /// </summary>
        [return: NotNull]
        public static IConsole Create(ConsoleConfig config)
        {
            return new Console(config);
        }

        /// <summary>
        /// 扫描所有已加载程序集中标记了 <see cref="ConsoleCommandAttribute"/> 的静态方法，
        /// 自动构建并返回控制台实例。仅包含与指定 key 匹配的命令和全局命令。
        /// </summary>
        /// <param name="key">目标 ConsoleKey，用于过滤 <see cref="TargetConsoleKeyAttribute"/></param>
        /// <returns>基于属性扫描构建的控制台实例</returns>
        [return: NotNull]
        public static IConsole Create(ConsoleKey key)
        {
            var builder = new ConsoleBuilder()
                .SetConsoleKey(key)
                .RegisterFromAllAssemblies();
            return builder.Build();
        }

        /// <summary>
        /// 使用字符串 key 的便捷重载。等效于 <c>Create(new ConsoleKey(key))</c>。
        /// </summary>
        [return: NotNull]
        public static IConsole Create([DisallowNull] string key)
        {
            return Create(new ConsoleKey(key));
        }
    }
}
