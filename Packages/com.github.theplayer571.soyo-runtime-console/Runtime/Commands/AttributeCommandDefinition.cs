using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Commands
{
    /// <summary>
    /// 基于反射的命令定义，将标记了 <see cref="ConsoleCommandAttribute"/> 的静态方法包装为
    /// <see cref="ConsoleCommandDefinition"/>，在 <see cref="Execute"/> 时通过反射调用目标方法。
    /// </summary>
    internal sealed class AttributeCommandDefinition : ConsoleCommandDefinition
    {
        private readonly MethodInfo _method;

        /// <summary>
        /// 构造基于反射的命令定义。
        /// </summary>
        /// <param name="method">目标静态方法</param>
        /// <param name="commandName">命令名</param>
        /// <param name="parameterHandlers">参数处理器列表（与方法参数一一对应）</param>
        public AttributeCommandDefinition(
            [DisallowNull] MethodInfo method,
            CommandName commandName,
            [AllowNull] IParameterHandler[] parameterHandlers)
            : base(commandName, parameterHandlers)
        {
            _method = method;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 通过 <see cref="MethodInfo.Invoke(object, object[])"/> 调用目标静态方法。
        /// 参数已在调用前通过 <see cref="IParameterHandler.Parse"/> 解析为正确的类型。
        /// </remarks>
        public override void Execute(
            [AllowNull] IReadOnlyList<object> parameters,
            [AllowNull] IConsole console)
        {
            var args = parameters != null && parameters.Count > 0
                ? new object[parameters.Count]
                : null;

            if (args != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    args[i] = parameters[i];
                }
            }

            _method.Invoke(null, args);
        }
    }
}
