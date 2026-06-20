using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public abstract class ParameterHandlerBase : IParameterHandler
    {
        protected ParameterHandlerBase([AllowNull] string name, [AllowNull] string type) :
            this(new IParameterHandler.Description(name, type))
        {
        }

        protected ParameterHandlerBase(in IParameterHandler.Description description)
        {
            _description = description;
        }

        private readonly IParameterHandler.Description _description;


        /// <inheritdoc />
        public IParameterHandler.Description GetDescription()
        {
            return _description;
        }

        /// <inheritdoc />
        public abstract IEnumerable<string> GetCandidates(string parameter);

        /// <inheritdoc />
        public abstract bool ShouldAdvance(string parameter);

        /// <inheritdoc />
        public abstract bool IsValid(string parameter);

        /// <inheritdoc />
        public abstract object Parse(string parameter);

        /// <inheritdoc />
        public abstract bool IsInitialized { get; }
    }
}