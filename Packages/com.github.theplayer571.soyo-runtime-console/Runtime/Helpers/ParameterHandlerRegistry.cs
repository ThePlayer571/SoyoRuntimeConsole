using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Helpers
{
    /// <summary>
    /// 参数处理器注册中心。由 <see cref="ConsoleBuilder"/> 持有，管理类型到首选 <see cref="IParameterHandler"/> 的映射。
    /// 支持可变注册阶段（<see cref="Freeze"/> 之前）和冻结只读阶段（<see cref="Freeze"/> 之后）。
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
    /// <item><c>Rect</c> → <see cref="RectParameterHandler"/></item>
    /// <item><c>RectInt</c> → <see cref="RectIntParameterHandler"/></item>
    /// <item><c>Bounds</c> → <see cref="BoundsParameterHandler"/></item>
    /// <item><c>BoundsInt</c> → <see cref="BoundsIntParameterHandler"/></item>
    /// <item><c>Color</c> → <see cref="ColorParameterHandler"/></item>
    /// <item><c>Guid</c> → <see cref="GuidParameterHandler"/></item>
    /// </list>
    /// <para>枚举类型和数组类型在运行时动态构造，无需手动注册。</para>
    /// <para>
    /// 同一类型注册多个工厂时，<see cref="Freeze"/> 时会自动使用 <see cref="CompositeParameterHandler"/> 组合它们。
    /// 仅支持注册，不支持注销。
    /// </para>
    /// </remarks>
    public class ParameterHandlerRegistry
    {
        /// <summary>
        /// 参数处理器工厂委托。接收目标类型和参数名称，返回对应的参数处理器实例。
        /// </summary>
        /// <param name="type">需要处理的类型</param>
        /// <param name="name">参数名称（用于提示），可为 null</param>
        /// <returns>对应类型的参数处理器实例</returns>
        public delegate IParameterHandler HandlerFactory([DisallowNull] Type type, [DisallowNull] string name);

        /// <summary>
        /// 动态处理器工厂委托。接收目标类型和参数名称，若该委托能处理此类型则返回对应的 <see cref="IParameterHandler"/> 实例，
        /// 否则返回 null 表示"不处理此类型"。
        /// </summary>
        /// <param name="type">需要处理的类型</param>
        /// <param name="name">参数名称（用于提示），可为 null</param>
        /// <returns>若能处理则返回处理器实例，否则返回 null</returns>
        [return: MaybeNull]
        public delegate IParameterHandler DynamicHandlerFactory([DisallowNull] Type type, [DisallowNull] string name);

        /// <summary>
        /// 可变阶段：存储每个类型对应的处理器工厂列表。
        /// 单一工厂直接使用；多个工厂在 <see cref="Freeze"/> 时自动组合为 <see cref="CompositeParameterHandler"/>。
        /// </summary>
        private Dictionary<Type, List<HandlerFactory>> _factories = new();

        /// <summary>
        /// 冻结阶段：编译后的单工厂字典。为 null 表示尚未冻结。
        /// </summary>
        private Dictionary<Type, HandlerFactory> _frozenFactories;

        /// <summary>
        /// 可变阶段：动态处理器工厂列表。按注册顺序存储，<see cref="HandlerOf"/> 时按序检查，首个非 null 结果获胜。
        /// </summary>
        private List<DynamicHandlerFactory> _dynamicFactories = new();

        /// <summary>
        /// 冻结阶段：动态处理器工厂的快照数组。为 null 表示尚未冻结。
        /// </summary>
        private DynamicHandlerFactory[] _frozenDynamicFactories;

        /// <summary>
        /// 是否已冻结。冻结后不可再注册新工厂。
        /// </summary>
        private bool _isFrozen;

        /// <summary>
        /// 线程安全锁（Unity 主线程操作，但保持防御性）。
        /// </summary>
        private readonly object _lock = new();

        #region 构造 — 注册内置处理器

        /// <summary>
        /// 创建新的参数处理器注册中心，自动注册所有内置处理器工厂。
        /// </summary>
        public ParameterHandlerRegistry()
        {
            RegisterBuiltinHandlers();
        }

        /// <summary>
        /// 注册所有内置的处理器工厂。
        /// </summary>
        private void RegisterBuiltinHandlers()
        {
            // 基础类型
            Register<string>((_, name) => new StringParameterHandler(name ?? "string"));
            Register<int>((_, name) => new IntegerParameterHandler(name ?? "integer"));
            Register<float>((_, name) => new FloatParameterHandler(name ?? "float"));
            Register<bool>((_, name) => new BooleanParameterHandler(name ?? "boolean"));

            // Unity 数学类型 — type 固定，name 可自定义
            Register<Vector2>((_, name) => new Vector2ParameterHandler(name ?? "vector2"));
            Register<Vector2Int>((_, name) => new Vector2IntParameterHandler(name ?? "vector2int"));
            Register<Vector3>((_, name) => new Vector3ParameterHandler(name ?? "vector3"));
            Register<Vector3Int>((_, name) => new Vector3IntParameterHandler(name ?? "vector3int"));
            Register<Vector4>((_, name) => new Vector4ParameterHandler(name ?? "vector4"));

            // Unity 几何/颜色类型
            Register<Rect>((_, name) => new RectParameterHandler(name ?? "rect"));
            Register<RectInt>((_, name) => new RectIntParameterHandler(name ?? "rect_int"));
            Register<Bounds>((_, name) => new BoundsParameterHandler(name ?? "bounds"));
            Register<BoundsInt>((_, name) => new BoundsIntParameterHandler(name ?? "bounds_int"));
            Register<Color>((_, name) => new ColorParameterHandler(name ?? "color"));

            // System 类型
            Register<Guid>((_, name) => new GuidParameterHandler(name ?? "guid"));
        }

        #endregion

        #region 公开 API — 注册

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册处理器工厂。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂在 <see cref="Freeze"/> 时自动组合为 CompositeParameterHandler）。
        /// 仅在 <see cref="Freeze"/> 之前有效。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="factory">处理器工厂委托</param>
        public void Register<T>([DisallowNull] HandlerFactory factory)
        {
            Register(typeof(T), factory);
        }

        /// <summary>
        /// 为指定类型注册处理器工厂。
        /// 若该类型已注册工厂，新工厂将被追加（多个工厂在 <see cref="Freeze"/> 时自动组合为 CompositeParameterHandler）。
        /// 仅在 <see cref="Freeze"/> 之前有效。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="factory">处理器工厂委托</param>
        public void Register([DisallowNull] Type type, [DisallowNull] HandlerFactory factory)
        {
            if (_isFrozen)
            {
                Debug.LogWarning(
                    $"[ParameterHandlerRegistry] Cannot register factory for type '{type.FullName}' " +
                    "after Freeze(). Ignoring.");
                return;
            }

            lock (_lock)
            {
                if (_isFrozen)
                {
                    return;
                }

                if (!_factories.TryGetValue(type, out var list))
                {
                    list = new List<HandlerFactory>();
                    _factories[type] = list;
                }

                list.Add(factory);
            }
        }

        /// <summary>
        /// 为泛型类型 <typeparamref name="T"/> 注册一个固定的处理器实例（便捷方法）。
        /// 内部将处理器包装为始终返回该实例的工厂。
        /// 主要用于注册无状态的处理器实例。
        /// 仅在 <see cref="Freeze"/> 之前有效。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="handler">处理器实例</param>
        public void Register<T>([DisallowNull] IParameterHandler handler)
        {
            Register<T>((_, _) => handler);
        }

        /// <summary>
        /// 注册一个动态处理器工厂。动态处理器在 <see cref="HandlerOf(Type, string)"/> 的解析链中，
        /// 在枚举和数组动态构造之后、StringParameterHandler 降级之前被检查。
        /// 工厂应返回 null 表示"不处理此类型"，返回非 null 值表示"处理此类型并使用返回的处理器"。
        /// 多个动态处理器按注册顺序检查，首个返回非 null 的获胜。
        /// 仅在 <see cref="Freeze"/> 之前有效。
        /// </summary>
        /// <param name="factory">动态处理器工厂委托</param>
        public void RegisterDynamicHandler([DisallowNull] DynamicHandlerFactory factory)
        {
            if (_isFrozen)
            {
                Debug.LogWarning(
                    $"[ParameterHandlerRegistry] Cannot register dynamic handler after Freeze(). Ignoring.");
                return;
            }

            lock (_lock)
            {
                if (_isFrozen)
                {
                    return;
                }

                _dynamicFactories.Add(factory);
            }
        }

        #endregion

        #region 公开 API — 冻结

        /// <summary>
        /// 冻结注册表。编译所有已注册的工厂列表为单一工厂字典。
        /// 冻结后不可再注册新工厂，<see cref="HandlerOf"/> 调用变为无锁只读。
        /// 多次调用是安全的（仅首次生效）。
        /// </summary>
        public void Freeze()
        {
            if (_isFrozen)
            {
                return;
            }

            lock (_lock)
            {
                if (_isFrozen)
                {
                    return;
                }

                var frozen = new Dictionary<Type, HandlerFactory>(_factories.Count);
                foreach (var kv in _factories)
                {
                    if (kv.Value.Count == 1)
                    {
                        // 单一工厂 — 直接使用
                        frozen[kv.Key] = kv.Value[0];
                    }
                    else
                    {
                        // 多工厂 — 预包装 CompositeParameterHandler 工厂
                        var factories = kv.Value.ToArray(); // 快照，防止闭包被外部修改
                        frozen[kv.Key] = (type, name) =>
                            new CompositeParameterHandler(
                                name ?? type.Name,
                                type.Name,
                                factories.Select(f => f(type, name)));
                    }
                }

                _frozenFactories = frozen;

                // 快照动态处理器工厂
                _frozenDynamicFactories = _dynamicFactories.Count > 0
                    ? _dynamicFactories.ToArray()
                    : System.Array.Empty<DynamicHandlerFactory>();

                _isFrozen = true;
                _factories.Clear();          // 释放可变字典引用，帮助 GC
                _dynamicFactories.Clear();   // 释放可变动态处理器列表引用
            }
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
        public IParameterHandler HandlerOf<T>([AllowNull] string name = null)
        {
            return HandlerOf(typeof(T), name);
        }

        /// <summary>
        /// 获取指定类型的首选参数处理器。
        /// 查找顺序：
        /// 1. 已注册的工厂（若已冻结则查编译字典；否则查可变字典并即时组合）
        /// 2. 枚举类型 — 动态构造 EnumParameterHandler
        /// 3. 一维数组类型 — 动态构造 ArrayParameterHandler&lt;T&gt;
        /// 4. 动态处理器工厂 — 按注册顺序检查，首个返回非 null 的获胜
        /// 5. 降级 — 警告并返回 StringParameterHandler
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="name">参数名称（用于提示），为 null 时使用类型名</param>
        /// <returns>对应的参数处理器</returns>
        [return: NotNull]
        public IParameterHandler HandlerOf([DisallowNull] Type type, [AllowNull] string name = null)
        {
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
            HandlerFactory factory = null;

            if (_isFrozen && _frozenFactories != null)
            {
                // 冻结阶段：直接从编译字典获取
                _frozenFactories.TryGetValue(type, out factory);
            }
            else
            {
                // 可变阶段：从可变字典获取并即时组合
                List<HandlerFactory> factoryList;
                lock (_lock)
                {
                    _factories.TryGetValue(type, out factoryList);
                }

                if (factoryList != null && factoryList.Count > 0)
                {
                    return CreateHandlerFromFactories(type, effectiveName, factoryList);
                }
            }

            if (factory != null)
            {
                return factory(type, effectiveName);
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

            // 4. 动态处理器工厂 — 按注册顺序检查
            var dynamicResult = TryDynamicHandlers(type, effectiveName);
            if (dynamicResult != null)
            {
                return dynamicResult;
            }

            // 5. 降级为 StringParameterHandler
            Debug.LogWarning(
                $"[ParameterHandlerRegistry] No handler registered for type '{type.FullName}'. " +
                "Falling back to StringParameterHandler.");
            return new StringParameterHandler(effectiveName);
        }

        #endregion

        #region 内部辅助方法

        /// <summary>
        /// 从工厂列表创建处理器（仅可变阶段使用）。
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
        /// 遍历所有注册的动态处理器工厂，返回第一个非 null 的结果。
        /// 冻结阶段使用 <see cref="_frozenDynamicFactories"/> 快照数组（无锁）；
        /// 可变阶段在锁内快照 <see cref="_dynamicFactories"/> 列表后遍历。
        /// </summary>
        /// <returns>若能处理则返回处理器实例，否则返回 null</returns>
        [return: MaybeNull]
        private IParameterHandler TryDynamicHandlers(Type type, string name)
        {
            if (_isFrozen)
            {
                // 冻结阶段：从只读数组快照中读取（无锁）
                if (_frozenDynamicFactories != null)
                {
                    foreach (var factory in _frozenDynamicFactories)
                    {
                        var result = factory(type, name);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            else
            {
                // 可变阶段：在锁内快照后遍历，避免迭代期间被 Freeze 修改
                DynamicHandlerFactory[] snapshot;
                lock (_lock)
                {
                    snapshot = _dynamicFactories.ToArray();
                }

                foreach (var factory in snapshot)
                {
                    var result = factory(type, name);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 动态构造数组参数处理器。
        /// </summary>
        private IParameterHandler CreateArrayHandler(Type arrayType, string name)
        {
            var elementType = arrayType.GetElementType();
            if (elementType == null)
            {
                Debug.LogWarning(
                    $"[ParameterHandlerRegistry] Cannot determine element type for array type '{arrayType.FullName}'. " +
                    "Falling back to StringParameterHandler.");
                return new StringParameterHandler(name);
            }

            // 递归获取元素类型的处理器
            var elementHandler = HandlerOf(elementType, name);

            // 构造 ArrayParameterHandler<T>
            var handlerType = typeof(ArrayParameterHandler<>).MakeGenericType(elementType);
            try
            {
                return (IParameterHandler)Activator.CreateInstance(handlerType, name, elementHandler);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[ParameterHandlerRegistry] Failed to create ArrayParameterHandler for type '{arrayType.FullName}': {ex.Message}");
                return new StringParameterHandler(name);
            }
        }

        #endregion
    }
}
