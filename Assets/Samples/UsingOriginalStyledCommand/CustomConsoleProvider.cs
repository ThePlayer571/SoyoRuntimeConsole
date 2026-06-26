using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand
{
    [CreateAssetMenu(menuName = "Soyo Runtime Console/CustomConsoleProvider", fileName = "CustomConsoleProvider", order = 0)]
    public class CustomConsoleProvider : ConsoleProvider
    {
        public override IConsole CreateConsole()
        {
            return new CustomConsole();
        }
    }
}