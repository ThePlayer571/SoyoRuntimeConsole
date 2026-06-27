using Soyo.SoyoRuntimeConsole.Helpers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingConsoleBuilder
{
    // 创建ConsoleProvider，给SimpleConsole使用
    [CreateAssetMenu(menuName = "Soyo Runtime Console/UsingBuilderConsoleAProvider", fileName = "UsingBuilderConsoleAProvider")]
    public class UsingBuilderConsoleAProvider : ConsoleProvider
    {
        public override IConsole CreateConsole()
        {
            var console = new ConsoleBuilder("UsingBuilderConsoleA") // 设置控制台Key
                .RegisterFromClass<ConsoleCommands_1>() // 扫描类
                .Build();

            // 只扫描了ConsoleCommands_1，因此ConsoleCommands_2里的命令不会出现
            return console;
        }
    }
}