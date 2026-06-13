using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Soyo.SoyoRuntimeConsole
{
    public class GlobalConsole : ConsoleBase
    {
        public GlobalConsole() : base(Array.Empty<ConsoleCommandDefinition>())
        {
        }
    }
}