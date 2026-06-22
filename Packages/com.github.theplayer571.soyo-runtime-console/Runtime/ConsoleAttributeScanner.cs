using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 控制台命令属性扫描器。
    /// 扫描程序集中的 <see cref="ConsoleCommandAttribute"/> 标记的静态方法，
    /// 将其包装为 <see cref="ConsoleCommandDefinition"/> 并收集对应的帮助文本。
    /// </summary>
    /// <remarks>
    /// <para>扫描规则：</para>
    /// <list type="number">
    /// <item>仅扫描 <c>BindingFlags.Public | NonPublic | Static | DeclaredOnly</c> 的方法</item>
    /// <item>泛型方法 → 警告并跳过</item>
    /// <item>默认参数 → 警告但不跳过</item>
    /// <item>ref/out 参数 → 警告但不跳过</item>
    /// <item>参数通过 <see cref="PreferredParameterHandler"/> 获取处理器</item>
    /// <item>命令名优先使用 <see cref="ConsoleCommandAttribute.Name"/>，否则使用方法名</item>
    /// <item><see cref="TargetConsoleKeyAttribute"/> 方法级优先于类级，无标记为全局命令</item>
    /// <item><see cref="CommandHelpTextAttribute"/> 同名重复时保留第一个并警告</item>
    /// </list>
    /// </remarks>
    public static class ConsoleAttributeScanner
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 扫描单个类中所有标记了 <see cref="ConsoleCommandAttribute"/> 的静态方法。
        /// </summary>
        /// <param name="classType">要扫描的类类型</param>
        /// <param name="targetKey">目标 ConsoleKey 过滤。为 null 时包含所有命令。</param>
        /// <returns>
        /// 元组：(命令定义列表, 命令帮助文本字典)。
        /// 若 classType 为 null 则返回空集合。
        /// </returns>
        public static (List<ConsoleCommandDefinition> Commands, Dictionary<CommandName, string> HelpTexts)
            ScanClass([DisallowNull] Type classType, ConsoleKey? targetKey = null)
        {
            var commands = new List<ConsoleCommandDefinition>();
            var helpTexts = new Dictionary<CommandName, string>();

            var methods = classType.GetMethods(MethodFlags);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<ConsoleCommandAttribute>();
                if (attr == null)
                {
                    continue;
                }

                if (!TryProcessMethod(method, attr, targetKey, commands, helpTexts))
                {
                    continue;
                }
            }

            return (commands, helpTexts);
        }

        /// <summary>
        /// 扫描单个程序集中所有标记了 <see cref="ConsoleCommandAttribute"/> 的静态方法。
        /// </summary>
        /// <param name="assembly">要扫描的程序集</param>
        /// <param name="targetKey">目标 ConsoleKey 过滤。为 null 时包含所有命令。</param>
        /// <returns>元组：(命令定义列表, 命令帮助文本字典)</returns>
        public static (List<ConsoleCommandDefinition> Commands, Dictionary<CommandName, string> HelpTexts)
            ScanAssembly([DisallowNull] Assembly assembly, ConsoleKey? targetKey = null)
        {
            var commands = new List<ConsoleCommandDefinition>();
            var helpTexts = new Dictionary<CommandName, string>();

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 某些类型可能无法加载（如引用了缺失的程序集），跳过这些类型
                types = ex.Types.Where(t => t != null).ToArray();
                Debug.LogWarning(
                    $"[ConsoleAttributeScanner] Some types could not be loaded from assembly '{assembly.GetName().Name}': " +
                    $"{ex.LoaderExceptions?.Length ?? 0} loader exception(s).");
            }

            foreach (var type in types)
            {
                if (type == null)
                {
                    continue;
                }

                var (classCommands, classHelpTexts) = ScanClass(type, targetKey);
                MergeResults(commands, helpTexts, classCommands, classHelpTexts);
            }

            return (commands, helpTexts);
        }

        /// <summary>
        /// 扫描所有已加载的程序集中标记了 <see cref="ConsoleCommandAttribute"/> 的静态方法。
        /// 排除系统程序集、Unity 程序集以及未引用 SoyoRuntimeConsole 的程序集。
        /// </summary>
        /// <param name="targetKey">目标 ConsoleKey 过滤。为 null 时包含所有命令。</param>
        /// <returns>元组：(命令定义列表, 命令帮助文本字典)</returns>
        /// <remarks>
        /// 每次调用都是独立的全量扫描，不缓存结果。
        /// </remarks>
        public static (List<ConsoleCommandDefinition> Commands, Dictionary<CommandName, string> HelpTexts)
            ScanAllAssemblies(ConsoleKey? targetKey = null)
        {
            var commands = new List<ConsoleCommandDefinition>();
            var helpTexts = new Dictionary<CommandName, string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var runtimeAssemblyName = typeof(ConsoleAttributeScanner).Assembly.GetName().Name;

            foreach (var assembly in assemblies)
            {
                if (ShouldSkipAssembly(assembly, runtimeAssemblyName))
                {
                    continue;
                }

                var (asmCommands, asmHelpTexts) = ScanAssembly(assembly, targetKey);
                MergeResults(commands, helpTexts, asmCommands, asmHelpTexts);
            }

            return (commands, helpTexts);
        }

        #region 内部辅助方法

        /// <summary>
        /// 处理单个标记了 <see cref="ConsoleCommandAttribute"/> 的方法。
        /// </summary>
        /// <returns>true 表示成功处理并添加了命令；false 表示该方法被跳过。</returns>
        private static bool TryProcessMethod(
            MethodInfo method,
            ConsoleCommandAttribute attr,
            ConsoleKey? targetKey,
            List<ConsoleCommandDefinition> commands,
            Dictionary<CommandName, string> helpTexts)
        {
            // 1. TargetConsoleKey 过滤 — 不匹配则静默跳过，避免为不相关的命令产生噪音 Warning
            if (!PassesKeyFilter(method, targetKey))
            {
                return false;
            }

            // 2. 泛型方法检查 — 跳过
            if (method.IsGenericMethod)
            {
                Debug.LogWarning(
                    $"[ConsoleCommand] '{method.DeclaringType!.FullName}.{method.Name}' is a generic method, " +
                    "which is not supported. Skipping.");
                return false;
            }

            // 3. 默认参数检查 — 警告但不跳过
            var parameters = method.GetParameters();
            if (parameters.Any(p => p.HasDefaultValue))
            {
                Debug.LogWarning(
                    $"[ConsoleCommand] '{method.DeclaringType!.FullName}.{method.Name}' has default parameter(s). " +
                    "Default parameters are not supported — use overloads instead.");
            }

            // 4. ref / out 参数检查 — 警告
            foreach (var param in parameters)
            {
                if (param.ParameterType.IsByRef)
                {
                    Debug.LogWarning(
                        $"[ConsoleCommand] '{method.DeclaringType!.FullName}.{method.Name}' " +
                        $"has ref/out parameter '{param.Name}', which may cause unexpected behavior.");
                }
            }

            // 5. 解析命令名
            var commandName = attr.Name ?? new CommandName(method.Name);

            // 6. 构建参数处理器列表
            var parameterHandlers = new IParameterHandler[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var paramName = param.GetCustomAttribute<CommandParameterAttribute>()?.Name ?? param.Name;
                parameterHandlers[i] = PreferredParameterHandler.Handler(param.ParameterType, paramName);
            }

            // 7. 创建命令定义
            var commandDef = new AttributeCommandDefinition(method, commandName, parameterHandlers);
            commands.Add(commandDef);

            // 8. 收集帮助文本
            var helpAttr = method.GetCustomAttribute<CommandHelpTextAttribute>();
            if (helpAttr != null)
            {
                if (!helpTexts.TryAdd(commandName, helpAttr.HelpText))
                {
                    Debug.LogWarning(
                        $"[ConsoleCommand] Help text for command '{commandName.Name}' is already defined. " +
                        $"Ignoring duplicate from '{method.DeclaringType!.FullName}.{method.Name}'.");
                }
            }

            return true;
        }

        /// <summary>
        /// 检查方法是否通过 ConsoleKey 过滤。
        /// </summary>
        private static bool PassesKeyFilter(MethodInfo method, ConsoleKey? targetKey)
        {
            // 未传入 targetKey — 不过滤
            if (targetKey == null)
            {
                return true;
            }

            // 方法级 TargetConsoleKey 优先
            var keyAttr = method.GetCustomAttribute<TargetConsoleKeyAttribute>();
            // 类级 TargetConsoleKey
            keyAttr ??= method.DeclaringType?.GetCustomAttribute<TargetConsoleKeyAttribute>();

            if (keyAttr == null)
            {
                // 无 TargetConsoleKey — 全局命令，总是包含
                return true;
            }

            // 有 TargetConsoleKey — 仅当匹配时包含
            return keyAttr.Key == targetKey.Value;
        }

        /// <summary>
        /// 将增量扫描结果合并到总结果中。
        /// </summary>
        private static void MergeResults(
            List<ConsoleCommandDefinition> commands,
            Dictionary<CommandName, string> helpTexts,
            List<ConsoleCommandDefinition> newCommands,
            Dictionary<CommandName, string> newHelpTexts)
        {
            commands.AddRange(newCommands);

            foreach (var kv in newHelpTexts)
            {
                if (!helpTexts.TryAdd(kv.Key, kv.Value))
                {
                    Debug.LogWarning(
                        $"[ConsoleCommand] Help text for command '{kv.Key.Name}' is already defined. " +
                        "Ignoring duplicate.");
                }
            }
        }

        /// <summary>
        /// 判断是否应跳过该程序集。
        /// </summary>
        private static bool ShouldSkipAssembly(Assembly assembly, string runtimeAssemblyName)
        {
            // 跳过动态程序集
            if (assembly.IsDynamic)
            {
                return true;
            }

            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            // 始终包含运行时程序集自身
            if (name == runtimeAssemblyName)
            {
                return false;
            }

            // 排除系统程序集
            if (name.StartsWith("System.", StringComparison.Ordinal) ||
                name.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                name.StartsWith("Mono.", StringComparison.Ordinal) ||
                name == "mscorlib" ||
                name == "netstandard")
            {
                return true;
            }

            // 排除 Unity 程序集
            if (name.StartsWith("UnityEngine.", StringComparison.Ordinal) ||
                name.StartsWith("UnityEditor.", StringComparison.Ordinal) ||
                name.StartsWith("Unity.", StringComparison.Ordinal))
            {
                return true;
            }

            // 排除未引用 SoyoRuntimeConsole 的程序集
            if (!ReferencesSoyoRuntimeConsole(assembly, runtimeAssemblyName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查程序集是否引用了 SoyoRuntimeConsole。
        /// </summary>
        private static bool ReferencesSoyoRuntimeConsole(Assembly assembly, string runtimeAssemblyName)
        {
            if (string.IsNullOrEmpty(runtimeAssemblyName))
            {
                return false;
            }

            var referencedAssemblies = assembly.GetReferencedAssemblies();
            foreach (var refAsm in referencedAssemblies)
            {
                if (refAsm.Name == runtimeAssemblyName)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
