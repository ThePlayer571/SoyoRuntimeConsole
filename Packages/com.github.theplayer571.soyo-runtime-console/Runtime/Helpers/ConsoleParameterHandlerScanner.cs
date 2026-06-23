using System;
using System.Linq;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Helpers
{
    /// <summary>
    /// 参数处理器属性扫描器。
    /// 扫描程序集中标记了 <see cref="ConsoleParameterHandlerAttribute"/> 的静态方法，
    /// 将其注册到 <see cref="PreferredParameterHandler"/>。
    /// </summary>
    internal static class ConsoleParameterHandlerScanner
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 扫描所有已加载的程序集，将标记了 <see cref="ConsoleParameterHandlerAttribute"/> 的方法
        /// 注册为 <see cref="PreferredParameterHandler"/> 的处理器工厂。
        /// </summary>
        public static void ScanAllAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var runtimeAssemblyName = typeof(ConsoleParameterHandlerScanner).Assembly.GetName().Name;

            foreach (var assembly in assemblies)
            {
                if (ShouldSkipAssembly(assembly, runtimeAssemblyName))
                {
                    continue;
                }

                ScanAssembly(assembly);
            }
        }

        #region 内部实现

        /// <summary>
        /// 扫描单个程序集。
        /// </summary>
        private static void ScanAssembly(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type == null)
                {
                    continue;
                }

                var methods = type.GetMethods(MethodFlags);
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<ConsoleParameterHandlerAttribute>();
                    if (attr == null)
                    {
                        continue;
                    }

                    TryRegisterMethod(method, attr);
                }
            }
        }

        /// <summary>
        /// 尝试将单个方法注册为处理器工厂。
        /// </summary>
        private static void TryRegisterMethod(
            MethodInfo method,
            ConsoleParameterHandlerAttribute attr)
        {
            // 泛型方法检查 — 跳过
            if (method.IsGenericMethod)
            {
                Debug.LogWarning(
                    $"[ConsoleParameterHandler] '{method.DeclaringType!.FullName}.{method.Name}' " +
                    "is a generic method, which is not supported. Skipping.");
                return;
            }

            // 解析目标类型
            Type targetType;
            if (!string.IsNullOrEmpty(attr.TypeName))
            {
                // Type.GetType 仅在调用程序集和 mscorlib 中搜索，对于定义在其他程序集中的类型会返回 null。
                // 作为兜底，当返回类型与指定的类型名匹配时，使用返回类型。
                targetType = Type.GetType(attr.TypeName);
                if (targetType == null)
                {
                    // 兜底：检查方法的返回类型是否与指定名称匹配
                    var returnType = method.ReturnType;
                    if (returnType != typeof(void) &&
                        (returnType.Name == attr.TypeName || returnType.FullName == attr.TypeName))
                    {
                        targetType = returnType;
                    }
                }

                if (targetType == null)
                {
                    Debug.LogWarning(
                        $"[ConsoleParameterHandler] Cannot resolve type '{attr.TypeName}' " +
                        $"specified in '{method.DeclaringType!.FullName}.{method.Name}'. Skipping.");
                    return;
                }
            }
            else
            {
                targetType = method.ReturnType;
                if (targetType == typeof(void))
                {
                    Debug.LogWarning(
                        $"[ConsoleParameterHandler] '{method.DeclaringType!.FullName}.{method.Name}' " +
                        "returns void. A parameter handler must return a value. Skipping.");
                    return;
                }
            }

            // 注册工厂
            PreferredParameterHandler.Register(targetType, (requestedType, name) =>
                CreateHandler(method, targetType, name));
        }

        /// <summary>
        /// 根据方法的参数类型创建 <see cref="MethodBackedTupleParameterHandler"/>。
        /// </summary>
        private static IParameterHandler CreateHandler(
            MethodInfo method,
            Type targetType,
            string name)
        {
            var methodParams = method.GetParameters();
            var subHandlers = new IParameterHandler[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                subHandlers[i] = PreferredParameterHandler.HandlerOf(param.ParameterType, param.Name);
            }

            var effectiveName = name ?? targetType.Name;
            return new MethodBackedTupleParameterHandler(
                effectiveName, targetType.Name, method, subHandlers);
        }

        /// <summary>
        /// 判断是否应跳过该程序集（与 <see cref="ConsoleAttributeScanner"/> 相同逻辑）。
        /// </summary>
        private static bool ShouldSkipAssembly(Assembly assembly, string runtimeAssemblyName)
        {
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
