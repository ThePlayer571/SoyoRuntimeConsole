using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole
{
    /// <summary>
    /// 控制台的配置结构，包含命令定义和命令帮助文本。
    /// 构造函数会对传入的数据进行校验，自动过滤 null 项和重复项。
    /// </summary>
    public readonly struct ConsoleConfig
    {
        public bool IsValid { get; }
        public ConsoleKey Key { get; }
        public IReadOnlyDictionary<CommandName, string> CommandHelpText { get; }
        public IReadOnlyList<ConsoleCommandDefinition> CommandDefinitions { get; }

        public ConsoleConfig(
            ConsoleKey key,
            [AllowNull] IEnumerable<ConsoleCommandDefinition> commands,
            [AllowNull] IEnumerable<ValueTuple<CommandName, string>> commandHelpText)
        {
            Key = key;
            
            var commandHelpTextDict = new Dictionary<CommandName, string>();
            var commandDefinitionsList = new List<ConsoleCommandDefinition>();

            if (commands != null)
            {
                foreach (var command in commands)
                {
                    if (command == null)
                    {
                        Debug.LogError("Cannot add null command.");
                        continue;
                    }

                    if (command.CommandName.IsNullName)
                    {
                        Debug.LogError($"Cannot add null name command '{command.GetType().FullName}'.");
                        continue;
                    }

                    if (commandDefinitionsList.Contains(command))
                    {
                        Debug.LogWarning($"Command '{command.CommandName}' is already added.");
                        continue;
                    }

                    commandDefinitionsList.Add(command);
                }
            }

            if (commandHelpText != null)
            {
                foreach (var (commandName, helpText) in commandHelpText)
                {
                    if (!commandHelpTextDict.TryAdd(commandName, helpText))
                    {
                        Debug.LogWarning($"Help text for command '{commandName}' is already added.");
                        continue;
                    }
                }
            }

            CommandHelpText = commandHelpTextDict;
            CommandDefinitions = commandDefinitionsList;
            IsValid = true;
        }
    }
}