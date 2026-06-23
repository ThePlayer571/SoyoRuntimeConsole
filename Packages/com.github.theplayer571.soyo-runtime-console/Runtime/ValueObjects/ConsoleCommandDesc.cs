using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ValueObjects
{
    public readonly struct ConsoleCommandDesc
    {
        [NotNull] public ConsoleCommandDefinition Definition { get; }
        
        // 承诺：Parameters一定小于或等于IParameterHandler数量
        [NotNull] public IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// Guarantee: when this value is true, Parameters has the same count as Definition.ParameterHandlers,
        /// and all parameters have passed IsValid validation.
        /// </summary>
        public bool Executable { get; }

        public ConsoleCommandDesc(
            [DisallowNull] ConsoleCommandDefinition definition,
            [DisallowNull] IReadOnlyList<string> parameters,
            bool executable
        )
        {
            Definition = definition;
            Parameters = parameters;
            Executable = executable;
        }
    }
}
