using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 全局首选的参数处理器注册中心。
    /// 每个类型唯一对应一个首选 <see cref="IParameterHandler"/>，
    /// 可通过 <see cref="Handler{T}(string)"/> 或 <see cref="Handler(Type, string)"/> 获取。
    /// </summary>
    /// <remarks>
    /// <para>内置注册了以下类型的处理器工厂：</para>
    /// <list type="bullet">
    /// <item><c>string</c> → <see cref="StringParameterHandler"/></item>
    /// <item><c>int</c> → <see cref="IntegerParameterHandler"/></item>
    /// <item><c>float</c> → <see cref="FloatParameterHandler"/></item>
    /// <item><c>bool</c> → <see cref="BooleanParameterHandler"/></item>
    /// <item><c>Vector2</c> → <see cref="Vector2ParameterHandler"/></item>
    /// <item><c>Vector2Int</c> → <see cref="Vector2IntParameterHandler"/></item>
    /// <item><c>Vector3</c> → <see cref="Vector3ParameterHandler"/></item>
    /// <item><c>Vector3Int</c> → <see cref="Vector3IntParameterHandler"/></item>
    /// <item><c>Vector4</c> → <see cref="Vector4ParameterHandler"/></item>
    /// </list>
    /// <para>枚举类型和数组类型在运行时动态构造，无需手动注册。</para>
    /// <para>
    /// 同一类型注册多个工厂时，会自动使用 <see cref="CompositeParameterHandler"/> 组合它们。
    /// 仅支持注册，不支持注销。
    /// </para>
    /// </remarks>
    public static class PreferredParameterHandler
    {
        /// <summary>
        /// 参数处理器工厂委托。接收目标类型和参数名称，返回对应的参数处理器实例。
        /// </summary>
        /// <param name="type">需要处理的类型</param>
        /// <param name="name">参数名称（用于提示），可为 null</param>
        /// <returns>对应类型的参数处理器实例</returns>
        public delegate IParameterHandler HandlerFactory([DisallowNull] Type type, [AllowNull] string name);

        /// <summary>
        /// 存储每个类型对应的处理器工厂列表。
        /// 单一工厂直接使用；多个工厂自动组合为 <see cref="CompositeParameterHandler"/>。
        /// </summary>
        private static readonly Dictionary<Type, List<HandlerFactory>> Factories = new();

        /// <summary>
        /// 懒加载标志。首次调用 <see cref="Handler(Type, string)"/> 时触发扫描。
        /// </summary>
        private static bool _initialized;

        /// <summary>
        /// 线程安全锁（Unity 主线程操作，但保持防御性）。
        /// </summary>
        private static readonly object Lock = new();

        #region 静态构造 — 注册内置处理器

        static PreferredParameterHandler()
        {
            RegisterBuiltinHandlers();
        }

        /// <summary>
        /// 注册所有内置的处理器工厂。
        /// </summary>
        private static void RegisterBuiltinHandlers()
        {
            // 基础类型
            Register<string>((_, name) => new StringParameterHandler(name ?? "String"));
            Register<int>((_, name) => new IntegerParameterHandler(name ?? "Integer"));
            Register<float>((_, name) => new FloatParameterHandler(name ?? "Float"));
            Register<bool>((_, name) => new BooleanParameterHandler(name ?? "Boolean"));

            // Unity 数学类型 — 无参构造，忽略 name
            Register<Vector2>((_, _) => new Vector2ParameterHandler());
            Register<Vector2Int>((_, _) => new Vector2IntParameterHandler());
            Register<Vector3>((_, _) => new Vector3ParameterHandler());
            Register<Vector3Int>((_, _) => new Vector3IntParameterHandler());
            Register<Vector4>((_, _) => new Vector4ParameterHandler());
        }

        #endregion

        #region 公开 API — 注册

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册处理器工厂。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂自动组合为 CompositeParameterHandler）。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="factory">处理器工厂委托</param>
        public static void Register<T>([DisallowNull] HandlerFactory factory)
        {
            Register(typeof(T), factory);
        }

        /// <summary>
        /// 为指定类型注册处理器工厂。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂自动组合为 CompositeParameterHandler）。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="factory">处理器工厂委托</param>
        public static void Register([DisallowNull] Type type, [DisallowNull] HandlerFactory factory)
        {
            lock (Lock)
            {
                if (!Factories.TryGetValue(type, out var list))
                {
                    list = new List<HandlerFactory>();
                    Factories[type] = list;
                }

                list.Add(factory);
            }
        }

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册一个固定的处理器实例（便捷方法）。
        /// 内部将处理器包装为始终返回该实例的工厂。
        /// 主要用于注册无状态的处理器实例。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="handler">处理器实例</param>
        public static void Register<T>([DisallowNull] IParameterHandler handler)
        {
            Register<T>((_, _) => handler);
        }

        #endregion

        #region 公开 API — 获取

        /// <summary>
        /// 获取泛型类型 <typeparamref name="T"/> 的首选参数处理器。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="name">参数名称（用于提示），为 null 时使用类型名</param>
        /// <returns>对应的参数处理器。若类型无注册且无法动态构造，则降级返回 StringParameterHandler。</returns>
        [return: NotNull]
        public static IParameterHandler Handler<T>([AllowNull] string name = null)
        {
            return Handler(typeof(T), name);
        }

        /// <summary>
        /// 获取指定类型的首选参数处理器。
        /// 查找顺序：
        /// 1. 已注册的工厂列表（若多个则组合为 CompositeParameterHandler）
        /// 2. 枚举类型 — 动态构造 EnumParameterHandler
        /// 3. 一维数组类型 — 动态构造 ArrayParameterHandler&lt;T&gt;
        /// 4. 降级 — 警告并返回 StringParameterHandler
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="name">参数名称（用于提示），为 null 时使用类型名</param>
        /// <returns>对应的参数处理器</returns>
        [return: NotNull]
        public static IParameterHandler Handler([DisallowNull] Type type, [AllowNull] string name = null)
        {
            // 懒加载：首次调用时扫描 [ConsoleParameterHandler]
            EnsureInitialized();

            // 解引用 ref / out 参数类型（如 System.Int32& → System.Int32）
            if (type.IsByRef)
            {
                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    type = elementType;
                }
            }

            var effectiveName = name ?? type.Name;

            // 1. 查找已注册的工厂
            List<HandlerFactory> factoryList;
            lock (Lock)
            {
                Factories.TryGetValue(type, out factoryList);
            }

            if (factoryList != null && factoryList.Count > 0)
            {
                return CreateHandlerFromFactories(type, effectiveName, factoryList);
            }

            // 2. 枚举类型 — 动态构造
            if (type.IsEnum)
            {
                return new EnumParameterHandler(effectiveName, type);
            }

            // 3. 一维数组类型 — 动态构造
            if (type.IsArray && type.GetArrayRank() == 1)
            {
                return CreateArrayHandler(type, effectiveName);
            }

            // 4. 降级为 StringParameterHandler
            Debug.LogWarning(
                $"[PreferredParameterHandler] No preferred handler registered for type '{type.FullName}'. " +
                $"Falling back to StringParameterHandler.");
            return new StringParameterHandler(effectiveName);
        }

        #endregion

        #region 懒加载

        /// <summary>
        /// 手动触发懒加载扫描。通常无需调用，首次获取处理器时会自动执行。
        /// 多次调用是安全的（仅首次生效）。
        /// </summary>
        public static void Initialize()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// 确保懒加载已执行。线程安全，仅执行一次。
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (Lock)
            {
                if (_initialized)
                {
                    return;
                }

                _initialized = true;

                ConsoleParameterHandlerScanner.ScanAllAssemblies();
            }
        }

        #endregion

        #region 内部辅助方法

        /// <summary>
        /// 从工厂列表创建处理器。
        /// 单一工厂直接调用；多个工厂组合为 CompositeParameterHandler。
        /// </summary>
        private static IParameterHandler CreateHandlerFromFactories(
            Type type, string name, List<HandlerFactory> factories)
        {
            if (factories.Count == 1)
            {
                return factories[0](type, name);
            }

            // 多个工厂 — 组合为 CompositeParameterHandler
            var handlers = new IParameterHandler[factories.Count];
            for (int i = 0; i < factories.Count; i++)
            {
                handlers[i] = factories[i](type, name);
            }

            return new CompositeParameterHandler(name, type.Name, handlers);
        }

        /// <summary>
        /// 动态构造数组参数处理器。
        /// </summary>
        private static IParameterHandler CreateArrayHandler(Type arrayType, string name)
        {
            var elementType = arrayType.GetElementType();
            if (elementType == null)
            {
                Debug.LogWarning(
                    $"[PreferredParameterHandler] Cannot determine element type for array type '{arrayType.FullName}'. " +
                    "Falling back to StringParameterHandler.");
                return new StringParameterHandler(name);
            }

            // 递归获取元素类型的处理器
            var elementHandler = Handler(elementType, name);

            // 构造 ArrayParameterHandler<T>
            var handlerType = typeof(ArrayParameterHandler<>).MakeGenericType(elementType);
            try
            {
                return (IParameterHandler)Activator.CreateInstance(handlerType, name, elementHandler);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[PreferredParameterHandler] Failed to create ArrayParameterHandler for type '{arrayType.FullName}': {ex.Message}");
                return new StringParameterHandler(name);
            }
        }

        #endregion
    }
}