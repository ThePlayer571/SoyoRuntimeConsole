using Soyo.SoyoRuntimeConsole;
using Soyo.SoyoRuntimeConsole.Helpers;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Samples.UsingConsoleBuilder
{
    [CreateAssetMenu(menuName = "Soyo Runtime Console/UsingBuilderConsoleBProvider", fileName = "UsingBuilderConsoleBProvider")]
    public class UsingBuilderConsoleBProvider : ConsoleProvider
    {
        public override IConsole CreateConsole()
        {
            var console = new ConsoleBuilder("UsingBuilderConsoleB") // 设置控制台Key
                .RegisterFromAssembly(this.GetType().Assembly) // 扫描程序集
                .RegisterHelpText(new CommandName("run_command_b1"), "help text of run_command_b1") // 手动注册HelpText
                .RegisterDynamicHandler((type, name) => // 使用 DynamicHandler
                {
                    // 使用 [ConsoleParameterHandler] 定义，无法同时支持解析MyNumber的子类
                    // 使用 DynamicHandler 支持
                    if (typeof(MyNumber).IsAssignableFrom(type))
                    {
                        return new MyNumberParameterHandler(name);
                    }

                    // 不满足逻辑则返回null，会进行下一个DynamicHandler的判断
                    return null;
                }).Build();

            // 扫描了ConsoleCommands_1和ConsoleCommands_2，因此两个类里的命令都会出现
            return console;
        }
    }
}