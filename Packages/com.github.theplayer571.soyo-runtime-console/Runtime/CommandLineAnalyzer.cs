using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    // bad SuggestionAnalyzer的设计逼迫Console不能AddCommand，不然得把这个类拉进IConsole里，或者加个OnAddCommand回调

    // todo 缓存分析结果，不然maybe性能问题
    public class CommandLineAnalyzer
    {
        public CommandLineAnalyzeResult Analyze([DisallowNull] string commandLine)
        {
            // 查缓存
            if (commandLine == _lastAnalyzedCommandLine)
            {
                return _lastAnalyzeResult;
            }

            // 分析commandLine
            var isTypingCommandName = !commandLine.Contains(' ');

            if (isTypingCommandName)
            {
                var inputCommandName = commandLine;
                var escapedInputCommandName = Regex.Escape(inputCommandName);

                var candidateCommandDescs =
                    from commandDefinition in _console.Commands
                    // 匹配命令
                    where Regex.IsMatch(commandDefinition.CommandName.Name, escapedInputCommandName)
                    // 先按照是否以commandName开头进行排序，再按照首字母排序
                    orderby commandDefinition.CommandName.Name.StartsWith(inputCommandName) descending,
                        commandDefinition.CommandName.Name
                    select new ConsoleCommandDesc(commandDefinition, Array.Empty<string>(),
                        commandDefinition.ParameterHandlers.Count == 0 &&
                        commandDefinition.CommandName.Name == inputCommandName);

                return new CommandLineAnalyzeResult(candidateCommandDescs.ToList());
            }
            else
            {
                var indexOfFirstSpace = commandLine.IndexOf(' ');

                var commandName = new CommandName(commandLine[..indexOfFirstSpace]);

                var parametersInput = commandLine[(indexOfFirstSpace + 1)..];
                // 逐条命令分析
                var candidateCommandDescs =
                    from commandDefinition in _console.Commands
                    // 匹配同名命令
                    where commandDefinition.CommandName == commandName
                    select AnalyzeParameters(commandDefinition, parametersInput)
                    into commandDesc
                    where commandDesc != null
                    select commandDesc.Value;

                return new CommandLineAnalyzeResult(candidateCommandDescs.ToList());
            }
        }

        private ConsoleCommandDesc? AnalyzeParameters(
            [DisallowNull] ConsoleCommandDefinition commandDefinition,
            [DisallowNull] string parametersInput)
        {
            // 提前退出：无参
            if (commandDefinition.ParameterHandlers.Count == 0)
            {
                if (string.IsNullOrEmpty(parametersInput))
                {
                    return new ConsoleCommandDesc(commandDefinition, Array.Empty<string>(), true);
                }
                else
                {
                    return null;
                }
            }


            var leftInput = parametersInput;
            var parameters = new List<string>();

            // 尝试根据每个parameterHandler分析参数
            for (var index = 0; index < commandDefinition.ParameterHandlers.Count; index++)
            {
                var parameterHandler = commandDefinition.ParameterHandlers[index];
                // 无参数可分析：提前退出
                if (string.IsNullOrEmpty(leftInput))
                {
                    parameters.Add(string.Empty);
                    return new ConsoleCommandDesc(commandDefinition, parameters, false);
                }

                leftInput = CutLeftStringWhenShouldAdvanceOrHasNoInput(leftInput, parameterHandler,
                    out var slice, out var cutByAdvance);

                var parameterNotValid = !parameterHandler.IsValid(slice);
                var hasLeftInput = !string.IsNullOrEmpty(leftInput);

                // 处理不合法切片
                if (parameterNotValid)
                {
                    if (hasLeftInput)
                        // 不合法切片出现在input中间：不通过
                    {
                        return null;
                    }
                    else
                        // 不合法切片出现在input末尾：认为是未输入完全的参数，通过
                    {
                        parameters.Add(slice);
                        return new ConsoleCommandDesc(commandDefinition, parameters, false);
                    }
                }

                // 此时切片全部合法
                
                // 处理并非触发advance的切片
                if (!cutByAdvance)
                {
                    // !cutByAdvance => !hasLeftInput
                    // executable = 全部参数解析完了
                    bool executable = index == commandDefinition.ParameterHandlers.Count - 1;

                    parameters.Add(slice);
                    return new ConsoleCommandDesc(commandDefinition, parameters, executable);
                }

                // advance，分析下一个参数
                parameters.Add(slice);
            }

            // handler遍历完了，但是还有输入：分析不通过
            if (!string.IsNullOrEmpty(leftInput))
            {
                return null;
            }

            // handler遍历完了，参数也都IsValid：分析通过
            return new ConsoleCommandDesc(commandDefinition, parameters, true);

            // returns: 裁剪后的leftString
            [return: NotNull]
            static string CutLeftStringWhenShouldAdvanceOrHasNoInput(       
                [DisallowNull] string leftString,
                [DisallowNull] IParameterHandler handler,
                [NotNull] out string slice,     
                out bool cutByAdvance)
            {
                var includeIndex = 0;
                for (; includeIndex < leftString.Length; includeIndex++)
                {
                    slice = leftString[..(includeIndex + 1)];
                    if (handler.ShouldAdvance(slice))
                    {
                        cutByAdvance = true;
                        return leftString[(includeIndex + 1)..];
                    }
                }

                cutByAdvance = false;
                slice = leftString;
                return string.Empty;
            }
        }

        // 只读
        private readonly IConsole _console;

        // 变量
        private string _lastAnalyzedCommandLine = null;
        private CommandLineAnalyzeResult _lastAnalyzeResult = default;

        public CommandLineAnalyzer(IConsole console)
        {
            _console = console;
        }
    }
}