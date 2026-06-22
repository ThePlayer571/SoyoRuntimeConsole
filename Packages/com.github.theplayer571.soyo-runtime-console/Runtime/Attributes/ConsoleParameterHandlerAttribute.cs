using System;

namespace Soyo.SoyoRuntimeConsole.Attributes
{
    /// <summary>
    /// 标记一个静态方法为参数处理器工厂。
    /// 方法的返回类型即为该 Handler 处理的类型。
    /// 方法参数定义了元组的子参数列表和 Parse 逻辑。
    /// </summary>
    /// <remarks>
    /// 被标记的方法必须是静态方法，不支持泛型方法。
    /// 方法的参数列表定义了该 Handler 作为 <see cref="ParameterHandlers.TupleParameterHandler"/> 时的子参数结构，
    /// 方法体则作为 Parse 的实现（接收已解析的子参数，返回处理后的对象）。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ConsoleParameterHandlerAttribute : Attribute
    {
        /// <summary>
        /// Handler 处理的类型全名。为 null 时使用方法的返回类型。
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// 无参构造。Handler 处理的类型将使用方法的返回类型。
        /// </summary>
        public ConsoleParameterHandlerAttribute()
        {
            TypeName = null;
        }

        /// <summary>
        /// 使用指定的类型全名。
        /// </summary>
        /// <param name="typeName">Handler 处理的目标类型的全限定名</param>
        public ConsoleParameterHandlerAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}
