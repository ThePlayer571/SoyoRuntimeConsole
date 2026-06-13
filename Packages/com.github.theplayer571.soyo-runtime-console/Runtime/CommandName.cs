using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Soyo.SoyoRuntimeConsole
{
    public readonly struct CommandName : IEquatable<CommandName>
    {
        public CommandName([DisallowNull] string name)
        {
            _name = RemoveUnsupportableChar(name);
            if (!IsSupportable(_name))
            {
                _name = null;
            }
        }

        private readonly string _name;
        private const string NullName = "null";

        [NotNull] public string Name => _name ?? NullName;
        private static readonly Regex UnsupportableCharRegex = new("[^a-zA-Z0-9_]");

        [return: NotNull]
        public static string RemoveUnsupportableChar([DisallowNull] string commandName)
        {
            return UnsupportableCharRegex.Replace(commandName, string.Empty);
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