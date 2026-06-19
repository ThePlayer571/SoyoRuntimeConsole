using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public static class ParameterHandlerParsingUtility
    {
        /// <summary>
        /// 将由空格分割的参数标准化。
        /// 适用对象：参数，由且仅由空格作为前进标志。
        /// 效果：移除前缀空格；如果有多个后缀空格，移除至仅剩一个。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string NormalizeSpaceSplitParameter([DisallowNull] string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return parameter;
            }

            parameter = parameter.TrimStart();

            if (parameter.EndsWith(' '))
            {
                parameter = parameter.TrimEnd() + ' ';
            }

            return parameter;
        }
    }
}