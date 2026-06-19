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


        public IParameterHandler.Description GetDescription()
        {
            return _description;
        }

        public abstract IEnumerable<string> GetCandidates(string parameter);
        public abstract bool ShouldAdvance(string parameter);
        public abstract bool IsValid(string parameter);
        public abstract object Parse(string parameter);
        public abstract bool IsInitialized { get; }
    }
}