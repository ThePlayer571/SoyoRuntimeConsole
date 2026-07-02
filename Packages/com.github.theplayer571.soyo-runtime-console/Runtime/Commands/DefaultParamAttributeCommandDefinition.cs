using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Commands
{
    /// <summary>
    /// 带默认参数的命令定义。将标记了 <see cref="ConsoleCommandAttribute"/> 且有默认参数值的静态方法包装为
    /// <see cref="ConsoleCommandDefinition"/>。
    /// 与 <see cref="AttributeCommandDefinition"/> 不同，本类只包含用户需要提供的参数子集对应的
    /// <see cref="IParameterHandler"/>，在 <see cref="Execute"/> 时自动将默认参数值填充到方法调用中。
    /// </summary>
    /// <remarks>
    /// 例如，对于方法 <c>cmd(int a, int b = 1, int c = 2)</c>，会生成三个变体：
    /// <list type="bullet">
    /// <item>1 个 handler（a）→ 执行 <c>method(a, 1, 2)</c></item>
    /// <item>2 个 handler（a, b）→ 执行 <c>method(a, b, 2)</c></item>
    /// <item>3 个 handler（a, b, c）→ 由 <see cref="AttributeCommandDefinition"/> 处理</item>
    /// </list>
    /// </remarks>
    internal sealed class DefaultParamAttributeCommandDefinition : ConsoleCommandDefinition
    {
        private readonly MethodInfo _method;
        private readonly object[] _defaultParamValues;

        /// <summary>
        /// 构造带默认参数的命令定义。
        /// </summary>
        /// <param name="method">目标静态方法</param>
        /// <param name="commandName">命令名</param>
        /// <param name="subsetHandlers">
        /// 用户需要提供的参数对应的处理器子集（长度 = 用户参数个数，小于方法参数总数）
        /// </param>
        /// <param name="defaultParamValues">
        /// 剩余参数的默认值，顺序与方法参数顺序一致。
        /// 长度 = 方法参数总数 - subsetHandlers 长度。
        /// </param>
        public DefaultParamAttributeCommandDefinition(
            [DisallowNull] MethodInfo method,
            CommandName commandName,
            [AllowNull] IParameterHandler[] subsetHandlers,
            [DisallowNull] object[] defaultParamValues)
            : base(commandName, subsetHandlers)
        {
            _method = method;
            _defaultParamValues = defaultParamValues;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 将用户提供的解析参数与默认参数值拼接后，通过 <see cref="MethodInfo.Invoke(object, object[])"/>
        /// 调用目标静态方法。
        /// </remarks>
        public override void Execute(
            [AllowNull] IReadOnlyList<object> parameters,
            [AllowNull] IConsole console)
        {
            var userCount = parameters?.Count ?? 0;
            var totalCount = userCount + _defaultParamValues.Length;
            var args = new object[totalCount];

            // 复制用户提供的解析参数
            if (parameters != null)
            {
                for (int i = 0; i < userCount; i++)
                {
                    args[i] = parameters[i];
                }
            }

            // 复制默认参数值
            for (int i = 0; i < _defaultParamValues.Length; i++)
            {
                args[userCount + i] = _defaultParamValues[i];
            }

            _method.Invoke(null, args);
        }
    }
}
