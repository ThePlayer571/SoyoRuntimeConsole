using Soyo.SoyoRuntimeConsole.Attributes;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingConsoleBuilder
{
    // 使用 [TargetConsoleKey] 标记类
    [TargetConsoleKey("UsingBuilderConsoleA")]
    public class ConsoleCommands_1
    {
        [ConsoleCommand]
        public static void run_command_a1()
        {
            Debug.Log("run_command_a1");
        }

        // 使用 [TargetConsoleKey] 标记方法，优先级高于类标记
        [TargetConsoleKey("UsingBuilderConsoleB")]
        [ConsoleCommand]
        public static void run_command_b1()
        {
            Debug.Log("run_command_b1");
        }
    }

    [TargetConsoleKey("UsingBuilderConsoleB")]
    public class ConsoleCommands_2
    {
        [ConsoleCommand]
        public static void run_command_b2()
        {
            Debug.Log("run_command_b2");
        }

        [TargetConsoleKey("UsingBuilderConsoleA")]
        [ConsoleCommand]
        public static void run_command_a2()
        {
            Debug.Log("run_command_a2");
        }
    }

    [TargetConsoleKey("UsingBuilderConsoleB")]
    public class ConsoleCommands_3
    {
        // 本示例用于 DynamicHandler，解析MyNumber及其子类
        [ConsoleCommand]
        public static void run_command_b_number(MyNumber number)
        {
            Debug.Log($"run_command_b: {number.X}");
        }

        [ConsoleCommand]
        public static void run_command_b_number_son(MyNumberSon number)
        {
            Debug.Log($"run_command_b: {number.X}");
        }
    }
}