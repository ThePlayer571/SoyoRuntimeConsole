using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 泛型枚举参数处理器。通过泛型参数指定枚举类型，简化构造。
    /// </summary>
    /// <typeparam name="T">枚举类型，必须继承自 <see cref="Enum"/></typeparam>
    /// <example>
    /// <code>
    /// var handler = new EnumParameterHandler&lt;KeyCode&gt;("key");
    /// handler.IsValid("Space"); // true
    /// </code>
    /// </example>
    public class EnumParameterHandler<T> : EnumParameterHandler where T : Enum
    {
        /// <summary>
        /// 使用指定的参数名称构造泛型枚举参数处理器。
        /// 自动使用 <typeparamref name="T"/> 作为枚举类型。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public EnumParameterHandler([DisallowNull] string name) : base(name, typeof(T))
        {
        }
    }

    /// <summary>
    /// 枚举参数处理器。匹配指定枚举类型的成员名称（精确匹配），
    /// 如 <c>"Space"</c>、<c>"Return"</c>（对于 <c>KeyCode</c> 枚举）。
    /// 输入使用空格分隔（继承自 <see cref="SpaceSplitParameterHandlerBase"/>）。
    /// </summary>
    /// <remarks>
    /// 候选项规则：空输入时返回所有枚举成员名称；
    /// 非空时按子串匹配（不区分大小写）返回匹配的成员名称。
    /// 验证使用精确匹配（不区分大小写）；解析通过 <see cref="Enum.Parse(Type, string, bool)"/> 完成。
    /// </remarks>
    public class EnumParameterHandler : SpaceSplitParameterHandlerBase
    {
        private readonly Type _enumType;
        private readonly string[] _enumNames;

        /// <summary>
        /// 使用指定的参数名称和枚举类型构造枚举参数处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="enumType">枚举类型，必须通过 <see cref="Type.IsEnum"/> 检查</param>
        public EnumParameterHandler(
            [DisallowNull] string name, [DisallowNull] Type enumType)
            : base(name, enumType.Name)
        {
            if (!enumType.IsEnum)
            {
                Debug.LogError($"EnumParameterHandler: invalid enum type '{enumType.FullName}'.");
                _enumType = null;
                _enumNames = null;
                IsInitialized = false;
                return;
            }

            _enumType = enumType;
            _enumNames = enumType.GetEnumNames();
            IsInitialized = true;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 空输入时返回所有枚举成员名称；非空时返回包含当前输入子串（不区分大小写）的成员名称。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (!IsInitialized)
            {
                yield break;
            }

            parameter = parameter.Trim();
            if (string.IsNullOrEmpty(parameter))
            {
                foreach (var enumName in _enumNames)
                {
                    yield return enumName;
                }

                yield break;
            }

            foreach (var enumName in _enumNames)
            {
                if (enumName.Contains(parameter, StringComparison.OrdinalIgnoreCase))
                {
                    yield return enumName;
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// 精确匹配（不区分大小写）：输入必须与某个枚举成员名称完全相等。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            if (!IsInitialized)
            {
                return false;
            }

            parameter = parameter.Trim();
            return _enumNames.Any(t => t == parameter);
        }

        /// <inheritdoc />
        /// <remarks>
        /// 解析为枚举类型的已装箱值。通过 <see cref="Enum.Parse(Type, string, bool)"/> 完成，
        /// 不区分大小写。
        /// </remarks>
        public override object Parse(string parameter)
        {
            // IsValid相当于加速的Try检查
            if (IsValid(parameter))
            {
                return Enum.Parse(_enumType, parameter.Trim(), false);
            }

            return null;
        }

        /// <inheritdoc />
        public override bool IsInitialized { get; }
    }
}