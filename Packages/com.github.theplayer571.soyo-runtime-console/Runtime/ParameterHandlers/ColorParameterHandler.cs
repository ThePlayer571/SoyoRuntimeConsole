using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    /// <summary>
    /// <see cref="Color"/> 的复合参数处理器。
    /// 支持六种输入格式：4 浮点数、3 浮点数、4 整数（字节范围）、3 整数（字节范围）、十六进制字符串、十六进制+alpha 元组。
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="ParameterHandlerBase"/>，内部使用 <see cref="CompositeParameterHandler"/> 委托处理。
    /// <para>整数和浮点数格式的自动区分策略：解析为浮点数后，若最大值超过 1.0，则视为字节范围（0-255），
    /// 所有值除以 255 映射到 0-1 范围；否则保持原值（0-1 范围）。</para>
    /// <para>示例输入：</para>
    /// <list type="bullet">
    /// <item><c>(0.5, 0.2, 0.3, 1.0)</c> — 4 个浮点数（0-1 范围）</item>
    /// <item><c>(0.5, 0.2, 0.3)</c> — 3 个浮点数（a 默认为 1.0）</item>
    /// <item><c>(128, 0, 0, 255)</c> — 4 个整数（字节范围，自动除以 255）</item>
    /// <item><c>(128, 0, 0)</c> — 3 个整数（字节范围，a 默认为 255 → 1.0）</item>
    /// <item><c>#FF0000</c> — 十六进制颜色字符串</item>
    /// <item><c>(#FF0000, 0.5)</c> — 十六进制颜色 + alpha 浮点数</item>
    /// </list>
    /// </remarks>
    public class ColorParameterHandler : ParameterHandlerBase
    {
        private readonly CompositeParameterHandler _composite;

        /// <summary>
        /// 构造 <see cref="Color"/> 参数处理器。
        /// 内部创建一个 <see cref="CompositeParameterHandler"/> 包含所有支持的格式。
        /// </summary>
        /// <param name="name">参数名称（用于提示）</param>
        public ColorParameterHandler([DisallowNull] string name)
            : base(name, "Color")
        {
            _composite = new CompositeParameterHandler(name, "Color",
                new ColorHexHandler(name + "_hex"),
                new ColorHexAlphaHandler(name + "_hexa"),
                new ColorFloat4Handler(name + "_f4"),
                new ColorFloat3Handler(name + "_f3"));
        }

        /// <inheritdoc />
        public override bool IsInitialized => _composite.IsInitialized;

        /// <inheritdoc />
        public override bool IsValid(string parameter)
        {
            return _composite.IsValid(parameter);
        }

        /// <inheritdoc />
        public override bool ShouldAdvance(string parameter)
        {
            return _composite.ShouldAdvance(parameter);
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetCandidates(string parameter)
        {
            return _composite.GetCandidates(parameter);
        }

        /// <inheritdoc />
        public override object Parse(string parameter)
        {
            return _composite.Parse(parameter);
        }

        /// <summary>
        /// 字节范围自动检测：若任意解析值 &gt; 1.0f，将所有值除以 255f 映射到 0-1 范围。
        /// 用于区分 <c>(0.5, 0.2, 0.3)</c>（0-1 浮点范围）和 <c>(128, 0, 0)</c>（0-255 字节范围）。
        /// </summary>
        /// <param name="values">解析后的浮点值数组</param>
        /// <returns>映射到 0-1 范围的颜色分量（r, g, b, a）</returns>
        private static (float r, float g, float b, float a) ApplyByteRangeDetection(float[] values)
        {
            var max = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }
            }

            if (max > 1.0f)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] /= 255f;
                }
            }

            return (
                values[0],
                values[1],
                values[2],
                values.Length >= 4 ? values[3] : 1.0f
            );
        }

        #region 内嵌处理器

        /// <summary>
        /// 十六进制颜色字符串处理器。支持 <c>#RRGGBB</c>、<c>#RRGGBBAA</c>、<c>RRGGBB</c>、<c>RRGGBBAA</c> 格式。
        /// </summary>
        private sealed class ColorHexHandler : SpaceSplitParameterHandlerBase
        {
            public ColorHexHandler([DisallowNull] string name)
                : base(name, "Color")
            {
            }

            /// <inheritdoc />
            public override bool IsInitialized => true;

            /// <inheritdoc />
            public override bool IsValid(string parameter)
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    return false;
                }

                var hex = parameter.Trim();

                // 若字符串可解析为 float，则不应视为颜色十六进制值（避免 "128" 被
                // 当作 #128 简写色值，而应交给浮点/整数格式的处理器去处理）。
                if (float.TryParse(hex, out _))
                {
                    return false;
                }

                // 自动补全 # 前缀
                if (hex.Length > 0 && hex[0] != '#')
                {
                    return ColorUtility.TryParseHtmlString("#" + hex, out _);
                }

                return ColorUtility.TryParseHtmlString(hex, out _);
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var hex = parameter.Trim();
                if (hex.Length > 0 && hex[0] != '#')
                {
                    hex = "#" + hex;
                }

                ColorUtility.TryParseHtmlString(hex, out var color);
                return color;
            }

            /// <inheritdoc />
            /// <remarks>
            /// 空输入时返回 <c>#000000</c> 作为完整提示（而非仅 <c>#</c>）。
            /// 非空输入必须以 <c>#</c> 开头才提供候选项，避免纯数字等
            /// 合法十六进制字符串被误识别为颜色。
            /// 部分输入时，将用户输入的十六进制字符补零到 6 位或 8 位，
            /// 始终返回带 <c>#</c> 前缀的完整颜色预览。
            /// 输入包含非法字符时不提供候选项。
            /// </remarks>
            public override IEnumerable<string> GetCandidates(string parameter)
            {
                var input = parameter ?? string.Empty;

                // 非空输入必须以 # 开头才生成候选项
                if (input.Length > 0 && !input.StartsWith("#"))
                {
                    yield break;
                }

                var hexPart = input.StartsWith("#") ? input.Substring(1) : input;

                // 验证所有字符为合法十六进制数字
                foreach (var c in hexPart)
                {
                    if (!IsHexDigit(c))
                    {
                        yield break;
                    }
                }

                // 确定目标补齐长度：≤6 补到 6，7-8 补到 8，>8 不补
                string padded;
                if (hexPart.Length <= 6)
                {
                    padded = hexPart.PadRight(6, '0');
                }
                else if (hexPart.Length <= 8)
                {
                    padded = hexPart.PadRight(8, '0');
                }
                else
                {
                    padded = hexPart;
                }

                yield return "#" + padded;
            }

            /// <summary>
            /// 判断字符是否为合法十六进制数字（<c>0-9</c>、<c>a-f</c>、<c>A-F</c>）。
            /// </summary>
            private static bool IsHexDigit(char c)
            {
                return (c >= '0' && c <= '9')
                       || (c >= 'a' && c <= 'f')
                       || (c >= 'A' && c <= 'F');
            }
        }

        /// <summary>
        /// 十六进制颜色 + alpha 元组格式：<c>(hex, alpha)</c>。
        /// </summary>
        private sealed class ColorHexAlphaHandler : TupleParameterHandler
        {
            public ColorHexAlphaHandler([DisallowNull] string name)
                : base(name, "Color", BracketType.Braces,
                    new ColorHexHandler("hex"),
                    new FloatParameterHandler("alpha"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                var color = (Color)parts[0];
                color.a = (float)parts[1];
                return color;
            }
        }

        /// <summary>
        /// 4 个浮点数格式：<c>(r, g, b, a)</c>。自动检测字节范围。
        /// </summary>
        private sealed class ColorFloat4Handler : TupleParameterHandler
        {
            public ColorFloat4Handler([DisallowNull] string name)
                : base(name, "Color", BracketType.Braces,
                    new FloatParameterHandler("r"),
                    new FloatParameterHandler("g"),
                    new FloatParameterHandler("b"),
                    new FloatParameterHandler("a"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                var values = new[] { (float)parts[0], (float)parts[1], (float)parts[2], (float)parts[3] };
                var (r, g, b, a) = ApplyByteRangeDetection(values);
                return new Color(r, g, b, a);
            }
        }

        /// <summary>
        /// 3 个浮点数格式：<c>(r, g, b)</c>，alpha 默认为 1.0。自动检测字节范围。
        /// </summary>
        private sealed class ColorFloat3Handler : TupleParameterHandler
        {
            public ColorFloat3Handler([DisallowNull] string name)
                : base(name, "Color", BracketType.Braces,
                    new FloatParameterHandler("r"),
                    new FloatParameterHandler("g"),
                    new FloatParameterHandler("b"))
            {
            }

            /// <inheritdoc />
            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                var values = new[] { (float)parts[0], (float)parts[1], (float)parts[2] };
                var (r, g, b, a) = ApplyByteRangeDetection(values);
                return new Color(r, g, b, a);
            }
        }

        #endregion
    }
}
