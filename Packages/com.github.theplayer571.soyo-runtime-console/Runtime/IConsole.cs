using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 控制台的基础接口，定义了输入管理、命令执行以及命令相关数据的只读访问。
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// 设置InputText。InputText与自动补全、命令解析、命令执行等功能相关。
        /// </summary>
        /// <param name="text"></param>
        void SetInputText(string text);

        /// <summary>
        /// 执行当前InputText对应的命令。
        /// </summary>
        /// <param name="chosenCommandIndex">InputText可能解析为多个可选的命令，该参数用于指定命令。对应了CommandLineAnalyzer解析结果的第chosenCommandIndex个命令</param>
        /// <returns></returns>
        bool SendInput(int chosenCommandIndex = 0);

        /// <summary>
        /// 获取InputText。
        /// </summary>
        string InputText { get; }

        /// <summary>
        /// 当前控制台配置的命令列表。
        /// </summary>
        [NotNull]
        IReadOnlyList<ConsoleCommandDefinition> Commands { get; }

        /// <summary>
        /// 当前控制台配置的命令提示文本。
        /// </summary>
        [NotNull]
        IReadOnlyDictionary<CommandName, string> CommandHelpText { get; }

        /// <summary>
        /// 当前控制台绑定的命令行解析器。
        /// </summary>
        [NotNull]
        CommandLineAnalyzer CommandLineAnalyzer { get; }
    }
}