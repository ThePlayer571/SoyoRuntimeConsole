using System;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole
{
    public readonly struct ConsoleKey : IEquatable<ConsoleKey>
    {
        [MaybeNull] public string Key { get; }

        public ConsoleKey([DisallowNull] string key)
        {
            Key = key;
        }

        public bool Equals(ConsoleKey other)
        {
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return obj is ConsoleKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }

        public static bool operator ==(ConsoleKey left, ConsoleKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsoleKey left, ConsoleKey right)
        {
            return !left.Equals(right);
        }
    }
}