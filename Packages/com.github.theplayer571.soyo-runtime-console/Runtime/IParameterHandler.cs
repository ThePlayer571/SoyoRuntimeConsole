using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole
{
    public interface IParameterHandler
    {
        /// <summary>
        /// 参数的描述信息，用于提示。
        /// 注意：Name和Type是MaybeNull的，意味着它们是否为null暗示了某些信息
        /// </summary>
        public readonly struct Description
        {
            [MaybeNull] public string Name { get; }
            [MaybeNull] public string Type { get; }

            public Description([AllowNull] string name, [AllowNull] string type)
            {
                Name = name;
                Type = type;
            }
        }

        /// <summary>
        /// 获取参数的描述。
        /// </summary>
        /// <returns></returns>
        Description GetDescription();

        /// <summary>
        /// 根据输入参数给出推荐，用于提示和补全。
        /// </summary>
        /// <param name="parameter">输入参数（可能不完整）</param>
        /// <returns></returns>
        [return: MaybeNull]
        IEnumerable<string> GetCandidates([DisallowNull] string parameter);

        /// <summary>
        /// 判断当前参数完整性。分析器会逐字符往后分析，直到该方法返回true。
        /// </summary>
        /// <remarks>这个方法不用考虑参数是否合法。比如IntParameterHandler对输入"abs "返回true，对"12"返回false。</remarks>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool ShouldAdvance([DisallowNull] string parameter);

        /// <summary>
        /// 判断参数是否合法，合法的参数才能参与解析。
        /// </summary>
        /// <remarks>本插件的CommandLine不存在"参数分界符"这个概念，这意味着CommandLine中的空格会视作参数的一部分。</remarks>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool IsValid([DisallowNull] string parameter);

        /// <summary>
        /// 解析参数。
        /// 承诺：每次调用这个方法时，parameter一定通过IsValid检查。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [return: MaybeNull]
        object Parse([DisallowNull] string parameter);

        /// <summary>
        /// 判断该实例是否成功初始化，标志着是否可以正常使用。
        /// </summary>
        /// <remarks>在初始化过程中：如果出现错误，请不要抛出异常，而是将IsInitialized置为false。</remarks>
        bool IsInitialized { get; }
    }
}