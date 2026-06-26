using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ValueObjects
{
    /// <summary>
    /// 命令描述，完整描述一个用于执行的命令。
    /// </summary>
    public readonly struct ConsoleCommandDesc
    {
        /// <summary>
        /// 命令定义
        /// </summary>
        [NotNull] public ConsoleCommandDefinition Definition { get; }

        /// <summary>
        /// 命令参数。承诺：Parameters的数量小于或等于Definition.ParameterHandlers的数量。
        /// </summary>
        [NotNull]
        public IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// 有效参数数量。
        /// </summary>
        public int ValidParameterCount { get; }

        /// <summary>
        /// 命令可执行。承诺：当为true时，Parameters的数量与Definition.ParameterHandlers相同，且所有参数都通过了IsValid验证。
        /// </summary>
        public bool Executable { get; }

        public ConsoleCommandDesc(
            [DisallowNull] ConsoleCommandDefinition definition,
            [DisallowNull] IReadOnlyList<string> parameters,
            int validParameterCount,
            bool executable
        )
        {
            Definition = definition;
            Parameters = parameters;
            ValidParameterCount = validParameterCount;
            Executable = executable;
        }
    }
}