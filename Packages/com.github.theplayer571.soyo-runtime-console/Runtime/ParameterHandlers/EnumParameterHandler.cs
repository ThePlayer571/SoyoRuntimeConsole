using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class EnumParameterHandler<T> : EnumParameterHandler where T : Enum
    {
        public EnumParameterHandler([DisallowNull] string name) : base(name, typeof(T))
        {
        }
    }

    public class EnumParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly Type _enumType;
        private readonly string[] _enumNames;

        public EnumParameterHandler(
            [DisallowNull] string name, [DisallowNull] Type enumType)
            : base(name, enumType.Name)
        {
            if (!enumType.IsEnum)
            {
                Debug.LogError($"EnumParameterHandler: invalid enum type '{enumType.FullName}'.");
                _enumType = null;
                _enumNames = null;
                IsInitialized = false;
                return;
            }

            _enumType = enumType;
            _enumNames = enumType.GetEnumNames();
            IsInitialized = true;
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
                foreach (var enumName in _enumNames)
                {
                    yield return enumName;
                }

                yield break;
            }

            foreach (var enumName in _enumNames)
            {
                if (enumName.Contains(parameter, StringComparison.OrdinalIgnoreCase))
                {
                    yield return enumName;
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
            return _enumNames.Any(t => t == parameter);
        }

        public override object Parse(string parameter)
        {
            // IsValid相当于加速的Try检查
            if (IsValid(parameter))
            {
                return Enum.Parse(_enumType, parameter.Trim(), false);
            }

            return null;
        }

        public override bool IsInitialized { get; }
    }
}