using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ValueObjects
{
    public readonly struct CommandName : IEquatable<CommandName>
    {
        public CommandName([DisallowNull] string name)
        {
            var (sanitizedName, changed) = RemoveUnsupportableChar(name);

            if (changed)
            {
                Debug.LogWarning($"Command name '{name}' contains unsupported characters and was sanitized to '{sanitizedName}'.");
            }

            if (!IsSupportable(sanitizedName))
            {
                _name = null;
                return;
            }

            _name = sanitizedName;
        }

        private readonly string _name;
        private const string NullName = "null";

        [NotNull] public string Name => _name ?? NullName;
        public bool IsNullName => _name == null;
        
        
        private static readonly Regex UnsupportableCharRegex = new("[^a-zA-Z0-9_]");

        public static (string Result, bool Changed) RemoveUnsupportableChar([DisallowNull] string commandName)
        {
            if (UnsupportableCharRegex.IsMatch(commandName))
            {
                return (UnsupportableCharRegex.Replace(commandName, string.Empty), true);
            }
            return (commandName, false);
        }

        public static bool IsSupportable([DisallowNull] string commandName)
        {
            return !string.IsNullOrEmpty(commandName) && !UnsupportableCharRegex.IsMatch(commandName);
        }

        public bool Equals(CommandName other)
        {
            return _name == other._name;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public static bool operator ==(CommandName left, CommandName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CommandName left, CommandName right)
        {
            return !left.Equals(right);
        }
    }
}