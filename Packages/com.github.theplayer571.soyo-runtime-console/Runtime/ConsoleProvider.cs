using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// ScriptableObject 形式的控制台提供器。通过继承此类并实现 <see cref="CreateConsole"/>，
    /// 可将自定义 <see cref="IConsole"/> 实例注入到 <see cref="View.EditorOnlyConsoleView"/> 中。
    /// </summary>
    /// <remarks>
    /// 当 <see cref="View.EditorOnlyConsoleView"/> 设置了 <see cref="ConsoleProvider"/> 后，
    /// 将优先通过其 <see cref="CreateConsole"/> 方法创建控制台，而非使用 <c>consoleKey</c>。
    /// </remarks>
    [CreateAssetMenu(
        menuName = "Soyo Runtime Console/Console Provider",
        fileName = "NewConsoleProvider")]
    public abstract class ConsoleProvider : ScriptableObject
    {
        /// <summary>
        /// 创建控制台实例。
        /// </summary>
        /// <returns>控制台实例。不应返回 null。</returns>
        [return: NotNull]
        public abstract IConsole CreateConsole();
    }
}
