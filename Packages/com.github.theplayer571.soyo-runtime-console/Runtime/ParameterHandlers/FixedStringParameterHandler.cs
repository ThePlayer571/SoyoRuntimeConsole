using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class FixedStringParameterHandler : ParameterHandlerBase
    {
        private readonly string _fixedString;

        public FixedStringParameterHandler([DisallowNull] string name)
            : base(name, null)
        {
            _fixedString = name;

            // 检查 null 或纯空白
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError("FixedStringParameterHandler: fixedString cannot be null, empty, or whitespace.");
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
            yield return _fixedString;
        }

        public override bool ShouldAdvance(string parameter)
        {
            return parameter.EndsWith(' ');
        }

        public override bool IsValid(string parameter)
        {
            return parameter == _fixedString || parameter == $"{_fixedString} ";
        }

        public override bool TryParse(string parameter, out object value)
        {
            value = _fixedString;
            return true;
        }

        public override bool IsInitialized { get; }
    }
}