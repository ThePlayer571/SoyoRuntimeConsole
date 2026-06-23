using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using ConsoleKey = Soyo.SoyoRuntimeConsole.ValueObjects.ConsoleKey;

namespace Soyo.SoyoRuntimeConsole.Helpers
{
    /// <summary>
    /// 控制台构建器。提供流畅的 Builder API 来构建 <see cref="ConsoleConfig"/> 和 <see cref="IConsole"/>。
    /// 支持手动注册命令和帮助文本，也支持从类/程序集中扫描 <see cref="ConsoleCommandAttribute"/> 自动注册。
    /// </summary>
    /// <example>
    /// <code>
    /// var console = new ConsoleBuilder()
    ///     .SetConsoleKey("MyKey")
    ///     .RegisterFromClass&lt;MyCommands&gt;()
    ///     .RegisterHelpText(new CommandName("hello"), "Says hello")
    ///     .Build();
    /// </code>
    /// </example>
    public class ConsoleBuilder
    {
        private ConsoleKey? _key;
        private readonly List<ConsoleCommandDefinition> _commands = new();
        private readonly Dictionary<CommandName, string> _helpTexts = new();

        #region 配置方法

        /// <summary>
        /// 设置控制台 Key。多次调用以后者为准。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder SetConsoleKey(ConsoleKey key)
        {
            _key = key;
            return this;
        }

        /// <summary>
        /// 使用字符串设置控制台 Key。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder SetConsoleKey([DisallowNull] string key)
        {
            _key = new ConsoleKey(key);
            return this;
        }

        #endregion

        #region 手动注册

        /// <summary>
        /// 手动注册一个命令定义。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterCommand([DisallowNull] ConsoleCommandDefinition command)
        {
            _commands.Add(command);
            return this;
        }

        /// <summary>
        /// 手动注册一条命令帮助文本。同名重复时警告并保留第一个。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterHelpText(CommandName name, [AllowNull] string helpText)
        {
            if (!_helpTexts.TryAdd(name, helpText))
            {
                Debug.LogWarning(
                    $"[ConsoleBuilder] Help text for command '{name.Name}' is already registered. " +
                    "Ignoring duplicate.");
            }

            return this;
        }

        /// <summary>
        /// 合并一个 <see cref="ConsoleConfig"/> 中的所有命令和帮助文本。
        /// ConsoleKey 仅在当前 Builder 尚未设置时采用 config 的 Key，已设置则忽略。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterConsoleConfig(ConsoleConfig config)
        {
            if (!config.IsValid)
            {
                return this;
            }

            // 合并命令
            if (config.CommandDefinitions != null)
            {
                foreach (var cmd in config.CommandDefinitions)
                {
                    RegisterCommand(cmd);
                }
            }

            // 合并帮助文本
            if (config.CommandHelpText != null)
            {
                foreach (var kv in config.CommandHelpText)
                {
                    RegisterHelpText(kv.Key, kv.Value);
                }
            }

            // Key 仅首次设置
            if (_key == null)
            {
                _key = config.Key;
            }

            return this;
        }

        #endregion

        #region 扫描注册

        /// <summary>
        /// 扫描泛型类 <typeparamref name="T"/> 中的 <see cref="ConsoleCommandAttribute"/> 并注册。
        /// TargetConsoleKey 过滤使用当前 Builder 设置的 ConsoleKey（若已设置）。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromClass<T>()
        {
            return RegisterFromClass(typeof(T));
        }

        /// <summary>
        /// 扫描指定类中的 <see cref="ConsoleCommandAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromClass([DisallowNull] Type type)
        {
            var (commands, helpTexts) = ConsoleAttributeScanner.ScanClass(type, _key);
            MergeScanResults(commands, helpTexts);
            return this;
        }

        /// <summary>
        /// 扫描指定程序集中的 <see cref="ConsoleCommandAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromAssembly([DisallowNull] Assembly assembly)
        {
            var (commands, helpTexts) = ConsoleAttributeScanner.ScanAssembly(assembly, _key);
            MergeScanResults(commands, helpTexts);
            return this;
        }

        /// <summary>
        /// 扫描所有已加载程序集中的 <see cref="ConsoleCommandAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromAllAssemblies()
        {
            var (commands, helpTexts) = ConsoleAttributeScanner.ScanAllAssemblies(_key);
            MergeScanResults(commands, helpTexts);
            return this;
        }

        #endregion

        #region 构建

        /// <summary>
        /// 构建 <see cref="ConsoleConfig"/>。
        /// </summary>
        public ConsoleConfig BuildConfig()
        {
            return new ConsoleConfig(
                _key ?? new ConsoleKey(string.Empty),
                _commands,
                _helpTexts.Select(kv => (kv.Key, kv.Value)));
        }

        /// <summary>
        /// 构建 <see cref="IConsole"/> 实例。
        /// </summary>
        [return: NotNull]
        public IConsole Build()
        {
            return Console.Create(BuildConfig());
        }

        #endregion

        #region 内部辅助

        /// <summary>
        /// 合并扫描结果到 Builder 内部集合。
        /// </summary>
        private void MergeScanResults(
            List<ConsoleCommandDefinition> commands,
            Dictionary<CommandName, string> helpTexts)
        {
            _commands.AddRange(commands);

            foreach (var kv in helpTexts)
            {
                RegisterHelpText(kv.Key, kv.Value);
            }
        }

        #endregion
    }
}
