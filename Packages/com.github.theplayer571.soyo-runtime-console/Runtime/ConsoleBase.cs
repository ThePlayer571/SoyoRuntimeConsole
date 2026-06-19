using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    public abstract class ConsoleBase : IConsole
    {
        #region 接口实现

        public virtual void SetInputText(string text)
        {
            _inputText = text;
        }

        public virtual string InputText => _inputText;

        public IReadOnlyList<ConsoleCommandDefinition> Commands => _commands;

        public IReadOnlyDictionary<CommandName, string> CommandHelpText => _commandHelpText;

        public CommandLineAnalyzer CommandLineAnalyzer => _commandLineAnalyzer;

        public virtual bool SendInput(int chosenCommandIndex = 0)
        {
            // 分析Input
            var result = CommandLineAnalyzer.Analyze(InputText);

            if (result.CandidateCommandDescs == null || result.CandidateCommandDescs.Count == 0)
            {
                // 提前退出：根本没命令
                return false;
            }

            // 找到目标命令
            ConsoleCommandDesc targetCommand = result.CandidateCommandDescs.ElementAtOrDefault(chosenCommandIndex);
            if (!targetCommand.Executable)
            {
                targetCommand = result.CandidateCommandDescs.FirstOrDefault(desc => desc.Executable);
            }

            if (!targetCommand.Executable)
            {
                return false;
            }

            // 执行目标命令
            var parameters = new List<object>(targetCommand.Parameters.Count);

            for (int i = 0; i < targetCommand.Parameters.Count; ++i)
            {
                var parameterString = targetCommand.Parameters[i];
                var parameterHandler = targetCommand.Definition.ParameterHandlers.ElementAtOrDefault(i);

                if (parameterHandler == null)
                {
                    parameters.Add(null);
                    continue;
                }

                // targetCommand.Executable暗示了parameterHandler.IsValid，因此可以直接Parse
                parameters.Add(parameterHandler.Parse(parameterString));
            }

            targetCommand.Definition.Execute(parameters, this);
            return true;
        }

        #endregion

        protected ConsoleBase([DisallowNull] IEnumerable<ConsoleCommandDefinition> commandDefinitions)
        {
            foreach (var commandDefinition in commandDefinitions)
            {
                if (commandDefinition == null)
                {
                    Debug.LogError("Cannot add null command.");
                    continue;
                }

                AddCommand(commandDefinition);
            }

            _commandLineAnalyzer = new CommandLineAnalyzer(this);
        }

        // 变量
        private string _inputText;
        private readonly List<ConsoleCommandDefinition> _commands = new();
        private readonly CommandLineAnalyzer _commandLineAnalyzer;
        private readonly Dictionary<CommandName, string> _commandHelpText = new();

        private void AddCommand([DisallowNull] ConsoleCommandDefinition commandDefinition)
        {
            if (_commands.Contains(commandDefinition))
            {
                Debug.LogWarning($"Command '{commandDefinition.CommandName}' is already added.");
                return;
            }

            _commands.Add(commandDefinition);
        }
    }
}