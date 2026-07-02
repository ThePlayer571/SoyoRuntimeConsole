using System;
using Soyo.SoyoRuntimeConsole.Attributes;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.DevSandbox
{
    /// <summary>
    /// 用于演示 EnumParameterHandler 的测试枚举。
    /// </summary>
    public enum TestEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    /// <summary>
    /// 每个命令对应一个内置参数类型，便于逐个观察解析行为。
    /// </summary>
    [TargetConsoleKey("DevSandboxConsole")]
    public static class ConsoleCommands
    {
        [CommandHelpText("test parameters")]
        [ConsoleCommand]
        public static void test(bool value)
        {
            Debug.Log($"bool: {value}");
        }

        [ConsoleCommand]
        public static void test(int value)
        {
            Debug.Log($"int: {value}");
        }

        [ConsoleCommand]
        public static void test(float value)
        {
            Debug.Log($"float: {value}");
        }


        [ConsoleCommand]
        public static void test(Guid value)
        {
            Debug.Log($"Guid: {value}");
        }

        [ConsoleCommand]
        public static void test(Vector2Int value)
        {
            Debug.Log($"Vector2Int: {value}");
        }

        [ConsoleCommand]
        public static void test(Vector2 value)
        {
            Debug.Log($"Vector2: {value}");
        }

        [ConsoleCommand]
        public static void test(Vector3Int value)
        {
            Debug.Log($"Vector3Int: {value}");
        }

        [ConsoleCommand]
        public static void test(Vector3 value)
        {
            Debug.Log($"Vector3: {value}");
        }

        [ConsoleCommand]
        public static void test(Vector4 value)
        {
            Debug.Log($"Vector4: {value}");
        }

        [ConsoleCommand]
        public static void test(Rect value)
        {
            Debug.Log($"Rect: {value}");
        }

        [ConsoleCommand]
        public static void test(RectInt value)
        {
            Debug.Log($"RectInt: {value}");
        }

        [ConsoleCommand]
        public static void test(Bounds value)
        {
            Debug.Log($"Bounds: {value}");
        }

        [ConsoleCommand]
        public static void test(BoundsInt value)
        {
            Debug.Log($"BoundsInt: {value}");
        }

        [ConsoleCommand]
        public static void test(Color value)
        {
            Debug.Log($"Color: {value}");
        }

        [ConsoleCommand]
        public static void test(TestEnum value)
        {
            Debug.Log($"TestEnum: {value}");
        }

        [ConsoleCommand]
        public static void test(int[] value)
        {
            Debug.Log($"int[]: [{string.Join(", ", value)}]");
        }

        [ConsoleCommand]
        public static void test([FixedField] object fixed_test)
        {
            Debug.Log($"fixed_test: {fixed_test}");
        }

        [ConsoleCommand]
        public static void test(string[] value)
        {
            Debug.Log($"string[]: [{string.Join(", ", value)}]");
        }

        [ConsoleCommand]
        public static void test(string value)
        {
            Debug.Log($"string: \"{value}\"");
        }

        [ConsoleCommand]
        public static void test(int a, int b)
        {
            Debug.Log($"int a: {a}, int b: {b}");
        }


        [ConsoleCommand]
        public static void other_test(int value)
        {
            Debug.Log($"other_test: {value}");
        }

        [ConsoleCommand]
        public static void test_default_param(int value = 42, string name = "default")
        {
            Debug.Log($"test_default_param: value={value}, name={name}");
        }
    }
}