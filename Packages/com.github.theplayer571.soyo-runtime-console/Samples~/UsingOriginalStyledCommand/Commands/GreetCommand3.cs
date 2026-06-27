using System.Collections.Generic;
using System.Linq;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public class GreetCommand3 : ConsoleCommandDefinition
    {
        public GreetCommand3() : base("greet", new GreetToneHandler("tone"),
            new BooleanParameterHandler("withHandShake"),
            new ArrayParameterHandler("words", new StringParameterHandler("word")))
        {
        }

        public override void Execute(IReadOnlyList<object> parameters, IConsole console)
        {
            var greetTone = (ValueObjects.GreetTone)parameters![0];
            var withHandShake = (bool)parameters[1];
            var words = (object[])parameters[2]; // 这里没用ArrayParameterHandler<T>，解析为object
            var greetStyle = new ValueObjects.GreetStyle(greetTone, withHandShake);
            var greetConfig = new ValueObjects.GreetConfig(greetStyle, words.Select(w => (string)w).ToArray());
            Debug.Log(greetConfig.ToString());
        }
    }
}