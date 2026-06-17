using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class EnumParameterHandler : ParameterHandlerBase
    {
        private readonly Type _enumType;
        private readonly string[] _enumNames;

        public EnumParameterHandler([DisallowNull] string name, [AllowNull] Type enumType)
            : base(name, enumType == null ? "Enum" : enumType.Name)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                Debug.LogError($"EnumParameterHandler: invalid enum type '{enumType?.FullName ?? "null"}'.");
                _enumType = null;
                _enumNames = Array.Empty<string>();
                IsInitialized = false;
                return;
            }

            _enumType = enumType;
            _enumNames = Enum.GetNames(enumType);
            IsInitialized = true;
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (!IsInitialized)
            {
                yield break;
            }

            var query = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (string.IsNullOrEmpty(query))
            {
                foreach (var enumName in _enumNames)
                {
                    yield return enumName;
                }

                yield break;
            }

            foreach (var enumName in _enumNames)
            {
                if (enumName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                {
                    yield return enumName;
                }
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            return ParameterHandlerParsingUtility.HasTrailingDelimiter(parameter);
        }

        public override bool IsValid(string parameter)
        {
            if (!IsInitialized)
            {
                return false;
            }

            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            for (var i = 0; i < _enumNames.Length; i++)
            {
                if (_enumNames[i] == core)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool TryParse(string parameter, out object value)
        {
            if (IsValid(parameter))
            {
                value = Enum.Parse(_enumType, ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter), false);
                return true;
            }

            value = null;
            return false;
        }

        public override bool IsInitialized { get; }
    }
}