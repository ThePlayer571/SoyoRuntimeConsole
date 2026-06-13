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


        Description GetDescription();

        /// <summary>
        /// 根据输入参数给出推荐，用于提示和补全
        /// </summary>
        /// <param name="parameter">输入的参数（可能不完整）</param>
        /// <returns></returns>
        [return: MaybeNull]
        IEnumerable<string> GetCandidates([DisallowNull] string parameter);

        /// <summary>
        /// 判断当前参数已经完整，该进行下一个参数的解析了（这个方法不用考虑参数是否合法，只需要指导分析器分析）
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        bool ShouldAdvance([DisallowNull] string parameter);

        bool IsValid([DisallowNull] string parameter);

        // 此函数每次被调用时，已确保IsValid
        bool TryParse([DisallowNull] string parameter, [MaybeNull] out object value);

        bool IsInitialized { get; }
    }
}