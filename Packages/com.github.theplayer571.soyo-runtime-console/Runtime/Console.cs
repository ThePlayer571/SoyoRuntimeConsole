namespace Soyo.SoyoRuntimeConsole
{
    public sealed class Console : ConsoleBase
    {
        private Console(ConsoleConfig config) : base(config)
        {
        }

        public static IConsole Create(ConsoleConfig config)
        {
            return new Console(config);
        }
    }
}