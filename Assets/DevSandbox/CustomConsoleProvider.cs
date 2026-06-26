using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.DevSandbox
{
    [CreateAssetMenu(menuName = "Soyo Runtime Console/DevSandboxConsoleProvider", fileName = "DevSandboxConsoleProvider",
        order = 0)]
    public class CustomConsoleProvider : ConsoleProvider
    {
        public override IConsole CreateConsole()
        {
            return Console.Create(new ConsoleKey("DevSandboxConsole"));
        }
    }
}