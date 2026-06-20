using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// 复合参数处理器。组合多个 IParameterHandler 作为备选格式，依次尝试直到找到匹配的处理器。
    /// 用于支持"一个参数位置接受多种格式"的场景。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="TupleParameterHandler"/>（乘积类型：所有子参数必须全部匹配）不同，
    /// 本类实现的是"选择"语义（sum type）：只要任意一个子处理器匹配即可。
    ///
    /// 示例：一个参数既可以是单个整数，也可以是括号包裹的元组——
    /// <code>
    /// new CompositeParameterHandler("vec", "Vector3Int",
    ///     new IntegerParameterHandler("x"),
    ///     new Vector3IntParameterHandler())
    /// </code>
    /// 此时输入 "1" 会匹配 IntegerParameterHandler（解析为 int），
    /// 输入 "(1, 2, 3)" 会匹配 Vector3IntParameterHandler（解析为 Vector3Int）。
    /// </remarks>
    public class CompositeParameterHandler : ParameterHandlerBase
    {
        private readonly IReadOnlyList<IParameterHandler> _handlers;

        /// <summary>
        /// 使用指定的名称、类型和子处理器集合构造复合参数处理器。
        /// 自动过滤未初始化的子处理器。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        /// <param name="type">参数类型名（用于提示）</param>
        /// <param name="handlers">备选参数处理器集合。依次尝试，第一个匹配的生效。</param>
        public CompositeParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            [DisallowNull] IEnumerable<IParameterHandler> handlers)
            : base(name, type)
        {
            _handlers = handlers?
                .Where(h => h is { IsInitialized: true })
                .ToArray() ?? Array.Empty<IParameterHandler>();
        }

        /// <summary>
        /// 使用指定的名称、类型和子处理器构造复合参数处理器（params 便捷重载）。
        /// </summary>
        public CompositeParameterHandler(
            [AllowNull] string name,
            [AllowNull] string type,
            [DisallowNull] params IParameterHandler[] handlers)
            : this(name, type, (IEnumerable<IParameterHandler>)handlers)
        {
        }

        /// <summary>
        /// 备选子处理器列表（只读）。已过滤未初始化的处理器。
        /// </summary>
        public IReadOnlyList<IParameterHandler> Handlers => _handlers;

        /// <summary>
        /// 判断该实例是否成功初始化。当至少有一个子处理器已初始化时返回 true。
        /// </summary>
        public override bool IsInitialized => _handlers.Count > 0;

        /// <inheritdoc />
        /// <remarks>
        /// 依次遍历子处理器，返回第一个 <see cref="IParameterHandler.IsValid"/> 为 true 的结果。
        /// 若所有子处理器均不匹配，返回 false。
        /// </remarks>
        public override bool IsValid(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            for (var i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].IsValid(parameter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 委托给第一个 <see cref="IParameterHandler.IsValid"/> 为 true 的子处理器执行解析。
        /// 调用前应先通过 <see cref="IsValid"/> 确认存在匹配的处理器。
        /// </remarks>
        /// <exception cref="InvalidOperationException">当没有子处理器匹配该参数时抛出。</exception>
        public override object Parse(string parameter)
        {
            for (var i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].IsValid(parameter))
                {
                    return _handlers[i].Parse(parameter);
                }
            }

            throw new InvalidOperationException(
                $"No handler in CompositeParameterHandler matches parameter: '{parameter}'");
        }

        /// <inheritdoc />
        /// <remarks>
        /// 仅当存在某个子处理器同时满足 <see cref="IParameterHandler.IsValid"/> 和
        /// <see cref="IParameterHandler.ShouldAdvance"/> 时才返回 true。
        /// 这确保了不会在用户正输入元组（如 "(1, "）时因简单处理器（如 IntegerHandler）
        /// 检测到尾部空格而提前 advance。
        /// </remarks>
        public override bool ShouldAdvance(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            for (var i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].IsValid(parameter) && _handlers[i].ShouldAdvance(parameter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        /// 合并所有子处理器的候选项，使用 HashSet 去重。
        /// </remarks>
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                parameter = string.Empty;
            }

            var seen = new HashSet<string>();
            for (var i = 0; i < _handlers.Count; i++)
            {
                var candidates = _handlers[i].GetCandidates(parameter);
                if (candidates == null)
                {
                    continue;
                }

                foreach (var candidate in candidates)
                {
                    if (seen.Add(candidate))
                    {
                        yield return candidate;
                    }
                }
            }
        }
    }
}
