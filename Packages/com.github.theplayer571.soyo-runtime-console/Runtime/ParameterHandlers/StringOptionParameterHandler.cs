using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class StringOptionParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly string[] _options;

        public StringOptionParameterHandler(
            [DisallowNull] string name,
            [DisallowNull] IEnumerable<string> options)
            : base(name, "option")
        {
            var optionsList = options.ToList();

            if (optionsList.Count == 0)
            {
                Debug.LogError("StringOptionParameterHandler: options must contain at least one string.");
                IsInitialized = false;
                return;
            }

            for (var i = 0; i < optionsList.Count; i++)
            {
                var option = optionsList[i];
                if (string.IsNullOrWhiteSpace(option))
                {
                    Debug.LogError(
                        $"StringOptionParameterHandler: option at index {i} is null, empty, or whitespace.");
                    IsInitialized = false;
                    return;
                }

                for (var j = 0; j < option.Length; j++)
                {
                    if (char.IsWhiteSpace(option[j]))
                    {
                        Debug.LogError(
                            $"StringOptionParameterHandler: option at index {i} contains whitespace at char {j}. Input: \"{option}\"");
                        IsInitialized = false;
                        return;
                    }
                }
            }

            _options = optionsList.ToArray();
            IsInitialized = true;
        }

        public StringOptionParameterHandler(
            [DisallowNull] string name,
            [DisallowNull] params string[] options)
            : this(name, (IEnumerable<string>)options)
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (!IsInitialized)
            {
                yield break;
            }

            parameter = parameter.Trim();
            if (string.IsNullOrEmpty(parameter))
            {
                foreach (var option in _options)
                {
                    yield return option;
                }

                yield break;
            }

            foreach (var option in _options)
            {
                if (option.Contains(parameter, System.StringComparison.OrdinalIgnoreCase))
                {
                    yield return option;
                }
            }
        }

        public override bool IsValid(string parameter)
        {
            if (!IsInitialized)
            {
                return false;
            }

            parameter = parameter.Trim();
            return _options.Any(t => t == parameter);
        }

        public override object Parse(string parameter)
        {
            // IsValid相当于加速的Try检查
            if (IsValid(parameter))
            {
                return parameter.Trim();
            }

            return null;
        }

        public override bool IsInitialized { get; }
    }
}
