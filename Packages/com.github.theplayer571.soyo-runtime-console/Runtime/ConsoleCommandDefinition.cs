using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole
{
    public abstract class ConsoleCommandDefinition
    {
        #region 对外接口

        public CommandName CommandName => _commandName;

        [NotNull] public IReadOnlyList<IParameterHandler> ParameterHandlers => _parameterHandlers;

        // 调用时已确保所有ParameterHandler通过IsValid检查并成功TryParse
        public abstract void Execute([AllowNull] IReadOnlyList<object> parameters, [AllowNull] IConsole console);

        #endregion

        protected ConsoleCommandDefinition(
            [DisallowNull] string name,
            params IParameterHandler[] parameterHandlers)
            : this(new CommandName(name), parameterHandlers)
        {
        }

        protected ConsoleCommandDefinition(
            [DisallowNull] string name,
            [AllowNull] IEnumerable<IParameterHandler> parameterHandlers)
            : this(new CommandName(name), parameterHandlers)
        {
        }

        protected ConsoleCommandDefinition(
            in CommandName name,
            params IParameterHandler[] parameterHandlers)
            : this(name, parameterHandlers.AsEnumerable())
        {
        }

        protected ConsoleCommandDefinition(
            in CommandName name,
            [AllowNull] IEnumerable<IParameterHandler> parameterHandlers)
        {
            _commandName = name;
            if (parameterHandlers != null)
            {
                _parameterHandlers = parameterHandlers.Where(h => h is { IsInitialized: true }).ToList();
            }
            else
            {
                _parameterHandlers = new List<IParameterHandler>();
            }
        }


        private readonly CommandName _commandName;
        private readonly IReadOnlyList<IParameterHandler> _parameterHandlers;
    }
}