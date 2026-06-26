using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Commands;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;
using ConsoleKey = Soyo.SoyoRuntimeConsole.ValueObjects.ConsoleKey;

namespace Soyo.SoyoRuntimeConsole.Helpers
{
    /// <summary>
    /// 控制台构建器。提供流畅的 Builder API 来构建 <see cref="ConsoleConfig"/> 和 <see cref="IConsole"/>。
    /// 支持手动注册命令和帮助文本，也支持从类/程序集中扫描 <see cref="ConsoleCommandAttribute"/> 和
    /// <see cref="ConsoleParameterHandlerAttribute"/> 自动注册。
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
        private readonly List<PendingCommandEntry> _pendingCommands = new();
        private readonly Dictionary<CommandName, string> _helpTexts = new();
        private readonly ParameterHandlerRegistry _registry = new();
        private bool _built;

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
        /// 扫描泛型类 <typeparamref name="T"/> 中的 <see cref="ConsoleCommandAttribute"/> 和
        /// <see cref="ConsoleParameterHandlerAttribute"/> 并注册。
        /// TargetConsoleKey 过滤使用当前 Builder 设置的 ConsoleKey（若已设置）。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromClass<T>()
        {
            return RegisterFromClass(typeof(T));
        }

        /// <summary>
        /// 扫描指定类中的 <see cref="ConsoleCommandAttribute"/> 和
        /// <see cref="ConsoleParameterHandlerAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromClass([DisallowNull] Type type)
        {
            // 1. 扫描 [ConsoleParameterHandler] 并注册到 registry
            ConsoleParameterHandlerScanner.ScanType(type, _registry);

            // 2. 扫描 [ConsoleCommand] 并暂存为 pending entries
            var (pending, helpTexts) = ConsoleAttributeScanner.ScanClass(type, _key);
            _pendingCommands.AddRange(pending);
            MergeHelpTexts(helpTexts);
            return this;
        }

        /// <summary>
        /// 扫描指定程序集中的 <see cref="ConsoleCommandAttribute"/> 和
        /// <see cref="ConsoleParameterHandlerAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromAssembly([DisallowNull] Assembly assembly)
        {
            // 1. 扫描 [ConsoleParameterHandler] 并注册到 registry
            ConsoleParameterHandlerScanner.ScanAssembly(assembly, _registry);

            // 2. 扫描 [ConsoleCommand] 并暂存为 pending entries
            var (pending, helpTexts) = ConsoleAttributeScanner.ScanAssembly(assembly, _key);
            _pendingCommands.AddRange(pending);
            MergeHelpTexts(helpTexts);
            return this;
        }

        /// <summary>
        /// 扫描所有已加载程序集中的 <see cref="ConsoleCommandAttribute"/> 和
        /// <see cref="ConsoleParameterHandlerAttribute"/> 并注册。
        /// </summary>
        [return: NotNull]
        public ConsoleBuilder RegisterFromAllAssemblies()
        {
            // 1. 扫描 [ConsoleParameterHandler] 并注册到 registry
            ConsoleParameterHandlerScanner.ScanAllAssemblies(_registry);

            // 2. 扫描 [ConsoleCommand] 并暂存为 pending entries
            var (pending, helpTexts) = ConsoleAttributeScanner.ScanAllAssemblies(_key);
            _pendingCommands.AddRange(pending);
            MergeHelpTexts(helpTexts);
            return this;
        }

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册类型绑定的参数处理器工厂。
        /// 效果与使用 <see cref="ConsoleParameterHandlerAttribute"/> 特性相同。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂在构建时自动组合为 CompositeParameterHandler）。
        /// 仅在 <see cref="Build"/> 或 <see cref="BuildConfig"/> 之前有效（<see cref="ParameterHandlerRegistry.Freeze"/> 之后不可再注册）。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="factory">处理器工厂委托，接收目标类型和参数名称，返回对应的 <see cref="IParameterHandler"/> 实例</param>
        /// <returns>当前的 ConsoleBuilder 实例（用于链式调用）</returns>
        /// <example>
        /// <code>
        /// var console = new ConsoleBuilder()
        ///     .Register&lt;MyType&gt;((type, name) => new MyTypeParameterHandler(name))
        ///     .RegisterFromClass&lt;MyCommands&gt;()
        ///     .Build();
        /// </code>
        /// </example>
        [return: NotNull]
        public ConsoleBuilder Register<T>(
            [DisallowNull] ParameterHandlerRegistry.HandlerFactory factory)
        {
            _registry.Register<T>(factory);
            return this;
        }

        /// <summary>
        /// 为指定类型注册类型绑定的参数处理器工厂。
        /// 效果与使用 <see cref="ConsoleParameterHandlerAttribute"/> 特性相同。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂在构建时自动组合为 CompositeParameterHandler）。
        /// 仅在 <see cref="Build"/> 或 <see cref="BuildConfig"/> 之前有效（<see cref="ParameterHandlerRegistry.Freeze"/> 之后不可再注册）。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="factory">处理器工厂委托，接收目标类型和参数名称，返回对应的 <see cref="IParameterHandler"/> 实例</param>
        /// <returns>当前的 ConsoleBuilder 实例（用于链式调用）</returns>
        [return: NotNull]
        public ConsoleBuilder Register(
            [DisallowNull] Type type,
            [DisallowNull] ParameterHandlerRegistry.HandlerFactory factory)
        {
            _registry.Register(type, factory);
            return this;
        }

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册一个固定的参数处理器实例（便捷方法）。
        /// 内部将处理器包装为始终返回该实例的工厂。主要用于注册无状态的处理器实例。
        /// 效果与使用 <see cref="ConsoleParameterHandlerAttribute"/> 特性相同。
        /// 仅在 <see cref="Build"/> 或 <see cref="BuildConfig"/> 之前有效（<see cref="ParameterHandlerRegistry.Freeze"/> 之后不可再注册）。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="handler">处理器实例</param>
        /// <returns>当前的 ConsoleBuilder 实例（用于链式调用）</returns>
        [return: NotNull]
        public ConsoleBuilder Register<T>([DisallowNull] IParameterHandler handler)
        {
            _registry.Register<T>(handler);
            return this;
        }

        /// <summary>
        /// 注册一个动态处理器工厂，用于根据类型特征（如泛型构造、类型模式等）动态匹配参数处理逻辑。
        /// 动态处理器在 <see cref="ParameterHandlerRegistry.HandlerOf(Type, string)"/> 解析链中，
        /// 在枚举和数组动态构造之后、StringParameterHandler 降级之前被检查。
        /// 工厂应返回 null 表示"不处理此类型"，返回非 null 值表示"处理此类型并使用返回的处理器"。
        /// 多个动态处理器按注册顺序检查，首个返回非 null 的获胜。
        /// 仅在 <see cref="Build"/> 或 <see cref="BuildConfig"/> 之前有效（<see cref="ParameterHandlerRegistry.Freeze"/> 之后不可再注册）。
        /// </summary>
        /// <param name="factory">动态处理器工厂委托</param>
        /// <returns>当前的 ConsoleBuilder 实例（用于链式调用）</returns>
        [return: NotNull]
        public ConsoleBuilder RegisterDynamicHandler(
            [DisallowNull] ParameterHandlerRegistry.DynamicHandlerFactory factory)
        {
            _registry.RegisterDynamicHandler(factory);
            return this;
        }

        #endregion

        #region 构建

        /// <summary>
        /// 构建 <see cref="ConsoleConfig"/>。
        /// 首次调用时执行构建阶段：冻结参数处理器注册表，解析所有暂存的命令条目。
        /// 多次调用返回相同结果。
        /// </summary>
        public ConsoleConfig BuildConfig()
        {
            EnsureBuilt();
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
        /// 执行构建阶段（幂等）。冻结参数处理器注册表，解析暂存的命令条目。
        /// </summary>
        private void EnsureBuilt()
        {
            if (_built)
            {
                return;
            }

            _built = true;

            // 1. 冻结注册表 — 之后不可再注册新处理器
            _registry.Freeze();

            // 2. 解析暂存的命令条目
            foreach (var pending in _pendingCommands)
            {
                var paramInfos = pending.Method.GetParameters();
                var handlers = new IParameterHandler[paramInfos.Length];

                for (int i = 0; i < paramInfos.Length; i++)
                {
                    var param = paramInfos[i];
                    var paramName = param.GetCustomAttribute<CommandParameterAttribute>()?.Name ?? param.Name;
                    handlers[i] = _registry.HandlerOf(param.ParameterType, paramName);
                }

                // 检查是否有未能初始化的处理器，若存在则跳过整个命令
                var hasUninitializedHandler = false;
                for (int i = 0; i < handlers.Length; i++)
                {
                    var handler = handlers[i];
                    if (handler is { IsInitialized: false })
                    {
                        hasUninitializedHandler = true;
                        var param = paramInfos[i];
                        Debug.LogWarning(
                            $"[ConsoleBuilder] Command '{pending.CommandName.Name}' parameter " +
                            $"'{param.Name}' (type: {param.ParameterType.Name}): the resolved " +
                            $"IParameterHandler ({handler.GetType().Name}) has IsInitialized = false. " +
                            $"The command will be skipped and will not be available in the console. " +
                            $"Ensure the handler sets IsInitialized to true after successful construction.");
                    }
                }

                if (hasUninitializedHandler)
                {
                    continue;
                }

                var command = new AttributeCommandDefinition(pending.Method, pending.CommandName, handlers);
                _commands.Add(command);
            }

            _pendingCommands.Clear();
        }

        /// <summary>
        /// 合并帮助文本到 Builder 内部集合。
        /// </summary>
        private void MergeHelpTexts(Dictionary<CommandName, string> helpTexts)
        {
            foreach (var kv in helpTexts)
            {
                RegisterHelpText(kv.Key, kv.Value);
            }
        }

        #endregion
    }
}
