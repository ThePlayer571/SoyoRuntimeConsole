using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Samples.QuickStart.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.QuickStart
{
    // 显式指定目标Console（如果不指定，认为是应用于所有Console）
    [TargetConsoleKey("QuickStartConsole")]
    public static class ConsoleCommands
    {
        // 定义控制台命令
        [ConsoleCommand("greet")]
        public static void greet([CommandParameter("config")] GreetConfig config)
        {
            Debug.Log(config.ToString());
        }

        // 可以省略参数名称指定
        [ConsoleCommand("greet")]
        public static void greet(
            [CommandParameter] GreetStyle style, // 等价于 [CommandParameter("style")] GreetStyle style
            [CommandParameter] string[] words)
        {
            greet(new GreetConfig(style, words));
        }

        // 可以省略命令名称指定
        [ConsoleCommand] // 等价于 [ConsoleCommand("greet")]
        public static void greet(
            [CommandParameter("tone")] GreetTone tone,
            [CommandParameter("withHandShake")] bool withHandShake,
            [CommandParameter("words")] string[] words)
        {
            var style = new GreetStyle(tone, withHandShake);
            greet(new GreetConfig(style, words));
        }

        // 可以是private的
        [ConsoleCommand]
        private static void quick_greet()
        {
            Debug.Log("Hello");
        }
    }
}