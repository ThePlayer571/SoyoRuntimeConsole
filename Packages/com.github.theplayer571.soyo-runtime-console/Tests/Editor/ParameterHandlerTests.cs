using System;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public class ParameterHandlerTests
    {
        private enum SampleEnum
        {
            Alpha,
            Beta,
            Gamma
        }

        /// <summary>
        /// 用于测试 CompositeParameterHandler 的最小具体实现。
        /// Parse 直接返回 GetParsedSubParameters 的结果（object[]）。
        /// </summary>
        private class TestCompositeHandler : CompositeParameterHandler
        {
            public TestCompositeHandler(string name, string type, BracketType bracketType,
                params IParameterHandler[] handlers)
                : base(name, type, bracketType, handlers)
            {
            }

            public override object Parse(string parameter)
            {
                return GetParsedSubParameters(parameter);
            }
        }

        [Test]
        public void BooleanParameterHandler()
        {
            var handler = new BooleanParameterHandler("flag");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("true"));
            Assert.IsTrue(handler.IsValid("false"));
            Assert.IsFalse(handler.IsValid("True"));
            Assert.IsFalse(handler.IsValid("yes"));

            Assert.IsFalse(handler.ShouldAdvance("true"));
            Assert.IsTrue(handler.ShouldAdvance("true "));

            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "true", "false" }));

            Assert.IsTrue(handler.Parse("true") is true);

            Assert.IsTrue(handler.Parse("false ") is false);
        }

        [Test]
        public void StringParameterHandler()
        {
            var handler = new StringParameterHandler("text");

            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { @"""" }));

            Assert.IsTrue(handler.IsValid("hello"));
            Assert.IsTrue(handler.IsValid("hello world"));
            Assert.IsFalse(handler.IsValid("\"hello"));

            Assert.IsFalse(handler.ShouldAdvance("hello"));
            Assert.IsTrue(handler.ShouldAdvance("hello "));
            Assert.IsFalse(handler.ShouldAdvance("\"hello"));
            Assert.IsFalse(handler.ShouldAdvance("\"hello "));
            Assert.IsFalse(handler.ShouldAdvance("\"hello\""));
            Assert.IsFalse(handler.ShouldAdvance("\"hello\\\"world\""));
            Assert.IsTrue(handler.ShouldAdvance("\"hello\" "));
            Assert.IsTrue(handler.ShouldAdvance("\"hello\\\"world\" "));

            Assert.That(handler.Parse("hello world"), Is.EqualTo("hello world"));

            Assert.That(handler.Parse("\"hello\\\"world\""), Is.EqualTo("hello\"world"));
        }

        [Test]
        public void EnumParameterHandler()
        {
            var handler = new EnumParameterHandler("mode", typeof(SampleEnum));

            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "Alpha", "Beta", "Gamma" }));
            Assert.That(handler.GetCandidates("a"), Contains.Item("Alpha"));
            Assert.That(Enum.GetName(typeof(SampleEnum), SampleEnum.Alpha), Is.EqualTo("Alpha"));
            Assert.That(Enum.GetName(typeof(SampleEnum), SampleEnum.Gamma), Is.EqualTo("Gamma"));
            Assert.IsFalse(handler.IsValid("alpha"));
            Assert.IsTrue(handler.IsValid("Alpha"));
            Assert.IsTrue(handler.ShouldAdvance("Alpha "));
            Assert.IsFalse(handler.ShouldAdvance("Alpha"));

            Assert.That((SampleEnum)handler.Parse("Beta"), Is.EqualTo(SampleEnum.Beta));

            Assert.That((SampleEnum)handler.Parse("Gamma"), Is.EqualTo(SampleEnum.Gamma));
        }

        [Test]
        public void IntegerParameterHandler()
        {
            var handler = new IntegerParameterHandler("count");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("12"));
            Assert.IsTrue(handler.IsValid("12 "));
            Assert.IsFalse(handler.IsValid("1.0"));
            Assert.IsFalse(handler.IsValid("1."));
            Assert.IsTrue(handler.ShouldAdvance("12 "));
            Assert.IsFalse(handler.ShouldAdvance("12"));
            Assert.That((int)handler.Parse("12 "), Is.EqualTo(12));
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "0" }));
        }

        [Test]
        public void FloatParameterHandler()
        {
            var handler = new FloatParameterHandler("ratio");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("1.5"));
            Assert.IsTrue(handler.IsValid("1.5 "));
            Assert.IsTrue(handler.ShouldAdvance("1.5 "));
            Assert.IsFalse(handler.ShouldAdvance("1.5"));
            Assert.That((float)handler.Parse("1.5 "), Is.EqualTo(1.5f));
        }

        [Test]
        public void FixedStringParameterHandler()
        {
            var handler = new FixedFieldParameterHandler("hello");

            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "hello" }));
            Assert.That(handler.GetCandidates("he"), Is.EquivalentTo(new[] { "hello" }));
            Assert.IsTrue(handler.IsValid("hello"));
            Assert.IsTrue(handler.IsValid("hello "));
            Assert.IsFalse(handler.IsValid("hell"));
            Assert.IsTrue(handler.ShouldAdvance("hello "));
            Assert.IsFalse(handler.ShouldAdvance("hello"));
            Assert.That(handler.Parse("hello"), Is.EqualTo(null));
        }

        [Test]
        public void CompositeParameterHandler()
        {
            // 构造：Integer + Float + Boolean 子处理器，使用圆括号
            var handler = new TestCompositeHandler("vector", "Vector3", BracketType.Parentheses,
                new IntegerParameterHandler("x"),
                new FloatParameterHandler("y"),
                new BooleanParameterHandler("flag"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(12, 1.5, true)"));
            Assert.IsTrue(handler.IsValid("(12, 1.5, false)"));
            // 尾随空格（trim 后合法）
            Assert.IsTrue(handler.IsValid("(12, 1.5, true) "));
            // 数量不匹配
            Assert.IsFalse(handler.IsValid("(12, 1.5)"));
            Assert.IsFalse(handler.IsValid("(12, 1.5, true, 0)"));
            // 子参数不合法
            Assert.IsFalse(handler.IsValid("(12, abc, true)"));
            Assert.IsFalse(handler.IsValid("(12, 1.5, True)"));

            // ShouldAdvance：必须以闭括号 + 空格结尾才 advance
            Assert.IsFalse(handler.ShouldAdvance("(12, 1.5, true)"));
            Assert.IsTrue(handler.ShouldAdvance("(12, 1.5, true) "));
            Assert.IsFalse(handler.ShouldAdvance("(12, 1.5, true"));
            // 多个尾随空格也可以 advance
            Assert.IsTrue(handler.ShouldAdvance("(12, 1.5, true)  "));

            // Parse
            var result = (object[])handler.Parse("(12, 1.5, true) ");
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That((int)result[0], Is.EqualTo(12));
            Assert.That((float)result[1], Is.EqualTo(1.5f));
            Assert.That(result[2], Is.EqualTo(true));

            // Parse 尾随空格版本
            var result2 = (object[])handler.Parse("(0, 0.0, false)");
            Assert.That(result2.Length, Is.EqualTo(3));
            Assert.That((int)result2[0], Is.EqualTo(0));
            Assert.That((float)result2[1], Is.EqualTo(0.0f));
            Assert.That(result2[2], Is.EqualTo(false));

            // GetCandidates：候选项包含完整前缀（开括号 + 已完成子参数 + 逗号）
            // 因为自动补全会用候选项替换最后一个参数
            // 空输入 → 给出开括号提示
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "(" }));
            // 刚输入 "(" → 第一个参数候选项带前缀
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            // 输入 "(1," → 第二个参数候选项，前缀规范化后为 "(1, "
            Assert.That(handler.GetCandidates("(1,"), Contains.Item("(1, 0"));
            Assert.That(handler.GetCandidates("(1,"), Contains.Item("(1, 0.0"));
            // 输入 "(1, 0." → 第二个参数候选项，前缀为 "(1, "
            Assert.That(handler.GetCandidates("(1, 0."), Contains.Item("(1, 0.0"));
            // 输入 "(1, 0.5," → 第三个参数候选项，前缀为 "(1, 0.5,"
            Assert.That(handler.GetCandidates("(1, 0.5,"),
                Is.EquivalentTo(new[] { "(1, 0.5, true", "(1, 0.5, false" }));
            // 输入 "(1, 0.5, t" → 第三个参数候选项，正在输入 "t"
            Assert.That(handler.GetCandidates("(1, 0.5, t"), Contains.Item("(1, 0.5, true"));
            // 最后一个参数已输入完整 → 提示闭括号
            Assert.That(handler.GetCandidates("(1, 0.5, true"), Contains.Item("(1, 0.5, true)"));
            // 最后一个参数完整但带尾随空格 → 也提示闭括号
            Assert.That(handler.GetCandidates("(1, 0.5, true "), Contains.Item("(1, 0.5, true)"));
            // 最后一个参数未完整 → 不提示闭括号
            Assert.That(handler.GetCandidates("(1, 0.5, fals"),
                Is.EquivalentTo(new[] { "(1, 0.5, false" }));
            // 超出处理器数量 → 无候选项
            Assert.That(handler.GetCandidates("(1, 0.5, true,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3"));
        }

        [Test]
        public void CompositeParameterHandler_EmptyHandlers()
        {
            // 空子处理器集合（支持 {} 空括号语法）
            var handler = new TestCompositeHandler("empty", "Empty", BracketType.Braces);

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：空括号
            Assert.IsTrue(handler.IsValid("{}"));
            Assert.IsTrue(handler.IsValid("{} "));
            Assert.IsFalse(handler.IsValid("{stuff}"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("{}"));
            Assert.IsTrue(handler.ShouldAdvance("{} "));

            // GetCandidates：无子处理器时不产生候选项
            Assert.That(handler.GetCandidates(string.Empty), Is.Empty);

            // Parse
            var result = (object[])handler.Parse("{} ");
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void CompositeParameterHandler_BracketTypes()
        {
            // 花括号 {}
            var braceHandler = new TestCompositeHandler("a", "T", BracketType.Braces,
                new IntegerParameterHandler("x"));
            Assert.IsTrue(braceHandler.IsInitialized);
            Assert.IsTrue(braceHandler.IsValid("{42}"));
            Assert.IsTrue(braceHandler.IsValid("{42} "));
            Assert.IsFalse(braceHandler.IsValid("(42)"));
            Assert.IsTrue(braceHandler.ShouldAdvance("{42} "));
            Assert.IsFalse(braceHandler.ShouldAdvance("{42}"));
            var braceResult = (object[])braceHandler.Parse("{42} ");
            Assert.That((int)braceResult[0], Is.EqualTo(42));
            // 花括号 GetCandidates：前缀为 "{"
            Assert.That(braceHandler.GetCandidates("{"), Contains.Item("{0"));

            // 方括号 []
            var bracketHandler = new TestCompositeHandler("b", "T", BracketType.Brackets,
                new IntegerParameterHandler("x"));
            Assert.IsTrue(bracketHandler.IsInitialized);
            Assert.IsTrue(bracketHandler.IsValid("[42]"));
            Assert.IsTrue(bracketHandler.IsValid("[42] "));
            Assert.IsFalse(bracketHandler.IsValid("(42)"));
            Assert.IsTrue(bracketHandler.ShouldAdvance("[42] "));
            Assert.IsFalse(bracketHandler.ShouldAdvance("[42]"));
            var bracketResult = (object[])bracketHandler.Parse("[42] ");
            Assert.That((int)bracketResult[0], Is.EqualTo(42));
            // 方括号 GetCandidates：前缀为 "["
            Assert.That(bracketHandler.GetCandidates("["), Contains.Item("[0"));
        }
    }
}