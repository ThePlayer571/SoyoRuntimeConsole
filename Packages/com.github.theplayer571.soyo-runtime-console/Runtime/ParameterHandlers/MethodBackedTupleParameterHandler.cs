using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 基于反射方法的元组参数处理器。
    /// 将标记了 <see cref="ConsoleParameterHandlerAttribute"/> 的静态方法包装为
    /// <see cref="TupleParameterHandler"/>，子参数由方法的参数类型决定，
    /// <see cref="Parse"/> 时调用方法体将子参数结果转换为返回值类型。
    /// </summary>
    internal sealed class MethodBackedTupleParameterHandler : TupleParameterHandler
    {
        private readonly MethodInfo _method;

        /// <summary>
        /// 构造基于反射方法的元组参数处理器。
        /// </summary>
        /// <param name="name">处理器名称（用于提示）</param>
        /// <param name="type">处理器类型名（用于提示）</param>
        /// <param name="bracketType">括号类型</param>
        /// <param name="method">目标静态方法，用于 Parse 时将子参数转换为返回值</param>
        /// <param name="subHandlers">子参数处理器列表</param>
        public MethodBackedTupleParameterHandler(
            string name,
            string type,
            BracketType bracketType,
            MethodInfo method,
            IParameterHandler[] subHandlers)
            : base(name, type, bracketType, subHandlers)
        {
            _method = method;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 先通过 <see cref="BracketParameterHandler.GetParsedSubParameters"/> 解析子参数，
        /// 再将解析结果传给目标方法，返回方法的返回值。
        /// </remarks>
        public override object Parse(string parameter)
        {
            var subResults = GetParsedSubParameters(parameter);
            var args = new object[subResults.Length];
            for (int i = 0; i < subResults.Length; i++)
            {
                args[i] = subResults[i];
            }

            return _method.Invoke(null, args);
        }
    }
}
