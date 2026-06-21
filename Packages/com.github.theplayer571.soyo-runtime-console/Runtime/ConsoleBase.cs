using System.Collections.Generic;
using System.Linq;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 控制台的抽象基类，实现了 IConsole 接口的核心逻辑。如果你想自定义Console，建议继承这个类。
    /// </summary>
    public abstract class ConsoleBase : IConsole
    {
        #region 接口实现

        /// <inheritdoc/>
        public virtual void SetInputText(string text)
        {
            _inputText = text;
        }

        /// <inheritdoc/>
        public virtual string InputText => _inputText;

        /// <inheritdoc/>
        public virtual IReadOnlyList<ConsoleCommandDefinition> Commands => _commands;

        /// <inheritdoc/>
        public virtual IReadOnlyDictionary<CommandName, string> CommandHelpText => _commandHelpText;

        /// <inheritdoc/>
        public virtual CommandLineAnalyzer CommandLineAnalyzer => _commandLineAnalyzer;

        /// <inheritdoc/>
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

        /// <summary>
        /// 使用指定配置初始化控制台。当配置有效时，直接持有配置中的命令列表和帮助文本的引用（而非拷贝），
        /// 以避免不必要的内存分配；配置无效时则初始化为空集合。
        /// </summary>
        protected ConsoleBase(ConsoleConfig consoleConfig)
        {
            if (consoleConfig.IsValid)
            {
                // 直接换引用而非拷贝：避免额外的内存分配，调用方不应在构造后修改配置中的集合
                _commands = consoleConfig.CommandDefinitions;
                _commandHelpText = consoleConfig.CommandHelpText;
            }
            else
            {
                _commands = new List<ConsoleCommandDefinition>();
                _commandHelpText = new Dictionary<CommandName, string>();
            }

            _commandLineAnalyzer = new CommandLineAnalyzer(this);
        }

        // 变量
        private string _inputText = string.Empty;
        private readonly IReadOnlyList<ConsoleCommandDefinition> _commands;

        private readonly CommandLineAnalyzer _commandLineAnalyzer;

        private readonly IReadOnlyDictionary<CommandName, string> _commandHelpText;
    }
}