using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class FixedFieldParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly string _fixedField;

        public FixedFieldParameterHandler([DisallowNull] string name)
            : base(name, null)
        {
            _fixedField = name;

            // 检查 null 或纯空白
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError("FixedFieldParameterHandler: fixedField cannot be null, empty, or whitespace.");
                IsInitialized = false;
                return;
            }

            // 检查是否包含任何空白字符
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsWhiteSpace(name[i]))
                {
                    Debug.LogError(
                        $"FixedStringParameterHandler: fixedString contains whitespace at index {i}. Input: \"{name}\"");
                    IsInitialized = false;
                    return;
                }
            }

            IsInitialized = true;
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            parameter = parameter.Trim();
            if (_fixedField.Contains(parameter))
            {
                yield return _fixedField;
            }
        }

        public override bool IsValid(string parameter)
        {
            return parameter.Trim() == _fixedField;
        }

        public override object Parse(string parameter)
        {
            return null;
        }

        public override bool IsInitialized { get; }
    }
}