using System;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

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
        /// 用于测试 TupleParameterHandler 的最小具体实现。
        /// Parse 直接返回 GetParsedSubParameters 的结果（object[]）。
        /// </summary>
        private class TestTupleHandler : TupleParameterHandler
        {
            public TestTupleHandler(string name, string type, BracketType bracketType,
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
            Assert.IsFalse(handler.IsValid(@"""hello"));

            Assert.IsFalse(handler.ShouldAdvance("hello"));
            Assert.IsTrue(handler.ShouldAdvance("hello "));
            Assert.IsFalse(handler.ShouldAdvance(@"""hello"));
            Assert.IsFalse(handler.ShouldAdvance(@"""hello "));
            Assert.IsFalse(handler.ShouldAdvance(@"""hello"""));
            Assert.IsFalse(handler.ShouldAdvance(@"""hello\""world"""));
            Assert.IsTrue(handler.ShouldAdvance(@"""hello"" "));
            Assert.IsTrue(handler.ShouldAdvance(@"""hello\""world"" "));

            Assert.That(handler.Parse("hello world"), Is.EqualTo("hello world"));

            Assert.That(handler.Parse(@"""hello\""world"""), Is.EqualTo(@"hello\""world"));
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
        public void TupleParameterHandler()
        {
            // 构造：Integer + Float + Boolean 子处理器，使用圆括号
            var handler = new TestTupleHandler("vector", "Vector3", BracketType.Parentheses,
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
            // 空输入 → 给出开括号提示 + 一键填充完整结果
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0, true)" }));
            // 刚输入 "(" → 第一个参数候选项带前缀
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            // 输入 "(1," → 第二个参数候选项，前缀规范化后为 "(1, "
            Assert.That(handler.GetCandidates("(1,"), Contains.Item("(1, 0"));
            Assert.That(handler.GetCandidates("(1,"), Contains.Item("(1, 0.0"));
            // 输入 "(1, 0." → 第二个参数候选项，前缀为 "(1, "
            Assert.That(handler.GetCandidates("(1, 0."), Contains.Item("(1, 0.0"));
            // 输入 "(1, 0.5," → 第三个参数候选项，前缀为 "(1, 0.5,"；末尾附带完整填充结果
            Assert.That(handler.GetCandidates("(1, 0.5,"),
                Is.EquivalentTo(new[] { "(1, 0.5, true", "(1, 0.5, false", "(1, 0.5, true)" }));
            // 输入 "(1, 0.5, t" → 第三个参数候选项，正在输入 "t"
            Assert.That(handler.GetCandidates("(1, 0.5, t"), Contains.Item("(1, 0.5, true"));
            // 最后一个参数已输入完整 → 通过完整填充结果附带闭括号
            Assert.That(handler.GetCandidates("(1, 0.5, true"), Contains.Item("(1, 0.5, true)"));
            // 最后一个参数完整但带尾随空格 → 通过完整填充结果附带闭括号
            Assert.That(handler.GetCandidates("(1, 0.5, true "), Contains.Item("(1, 0.5, true)"));
            // 最后一个参数未完整 → 不提示闭括号，但末尾附带完整填充结果（使用最佳匹配补全）
            Assert.That(handler.GetCandidates("(1, 0.5, fals"),
                Is.EquivalentTo(new[] { "(1, 0.5, false", "(1, 0.5, false)" }));
            // 输入已包含闭括号 → 用户已表明完结意图，仅返回输入自身
            Assert.That(handler.GetCandidates("(1, 0.5, true)"),
                Is.EquivalentTo(new[] { "(1, 0.5, true)" }));
            // 超出处理器数量 → 无候选项
            Assert.That(handler.GetCandidates("(1, 0.5, true,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3"));
        }

        [Test]
        public void TupleParameterHandler_EmptyHandlers()
        {
            // 空子处理器集合（支持 {} 空括号语法）
            var handler = new TestTupleHandler("empty", "Empty", BracketType.Braces);

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
        public void TupleParameterHandler_BracketTypes()
        {
            // 花括号 {}
            var braceHandler = new TestTupleHandler("a", "T", BracketType.Braces,
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
            var bracketHandler = new TestTupleHandler("b", "T", BracketType.Brackets,
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

        [Test]
        public void Vector2ParameterHandler()
        {
            var handler = new Vector2ParameterHandler();

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(1.5, 2.0)"));
            Assert.IsTrue(handler.IsValid("(1.5, 2.0) "));
            Assert.IsFalse(handler.IsValid("(1.5)"));
            Assert.IsFalse(handler.IsValid("(1.5, 2.0, 3.0)"));
            Assert.IsFalse(handler.IsValid("(abc, 2.0)"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("(1.5, 2.0)"));
            Assert.IsTrue(handler.ShouldAdvance("(1.5, 2.0) "));

            // Parse
            var result = (Vector2)handler.Parse("(1.5, 2.0)");
            Assert.That(result.x, Is.EqualTo(1.5f).Within(1e-6f));
            Assert.That(result.y, Is.EqualTo(2.0f).Within(1e-6f));

            var result2 = (Vector2)handler.Parse("(0.0, 0.0) ");
            Assert.That(result2.x, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(result2.y, Is.EqualTo(0.0f).Within(1e-6f));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0)" }));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0.0"));
            Assert.That(handler.GetCandidates("(1.5,"), Contains.Item("(1.5, 0"));
            Assert.That(handler.GetCandidates("(1.5,"), Contains.Item("(1.5, 0.0"));
            Assert.That(handler.GetCandidates("(1.5, 2.0"), Contains.Item("(1.5, 2.0)"));
            Assert.That(handler.GetCandidates("(1.5, 2.0,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector2"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector2"));
        }

        [Test]
        public void Vector3ParameterHandler()
        {
            var handler = new Vector3ParameterHandler();

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(1.0, 2.0, 3.0)"));
            Assert.IsTrue(handler.IsValid("(1.0, 2.0, 3.0) "));
            Assert.IsFalse(handler.IsValid("(1.0, 2.0)"));
            Assert.IsFalse(handler.IsValid("(1.0, 2.0, 3.0, 4.0)"));
            Assert.IsFalse(handler.IsValid("(abc, 2.0, 3.0)"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("(1.0, 2.0, 3.0)"));
            Assert.IsTrue(handler.ShouldAdvance("(1.0, 2.0, 3.0) "));

            // Parse
            var result = (Vector3)handler.Parse("(1.0, 2.0, 3.0)");
            Assert.That(result.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(result.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(result.z, Is.EqualTo(3.0f).Within(1e-6f));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0, 0)" }));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            Assert.That(handler.GetCandidates("(1.0, 2.0,"),
                Contains.Item("(1.0, 2.0, 0"));
            Assert.That(handler.GetCandidates("(1.0, 2.0, 3.0"), Contains.Item("(1.0, 2.0, 3.0)"));
            Assert.That(handler.GetCandidates("(1.0, 2.0, 3.0,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector3"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3"));
        }

        [Test]
        public void Vector4ParameterHandler()
        {
            var handler = new Vector4ParameterHandler();

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(1.0, 2.0, 3.0, 4.0)"));
            Assert.IsTrue(handler.IsValid("(1.0, 2.0, 3.0, 4.0) "));
            Assert.IsFalse(handler.IsValid("(1.0, 2.0, 3.0)"));
            Assert.IsFalse(handler.IsValid("(1.0, 2.0, 3.0, 4.0, 5.0)"));
            Assert.IsFalse(handler.IsValid("(abc, 2.0, 3.0, 4.0)"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("(1.0, 2.0, 3.0, 4.0)"));
            Assert.IsTrue(handler.ShouldAdvance("(1.0, 2.0, 3.0, 4.0) "));

            // Parse
            var result = (Vector4)handler.Parse("(1.0, 2.0, 3.0, 4.0)");
            Assert.That(result.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(result.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(result.z, Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That(result.w, Is.EqualTo(4.0f).Within(1e-6f));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0, 0, 0)" }));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            Assert.That(handler.GetCandidates("(1.0, 2.0, 3.0,"),
                Contains.Item("(1.0, 2.0, 3.0, 0"));
            Assert.That(handler.GetCandidates("(1.0, 2.0, 3.0, 4.0"),
                Contains.Item("(1.0, 2.0, 3.0, 4.0)"));
            Assert.That(handler.GetCandidates("(1.0, 2.0, 3.0, 4.0,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector4"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector4"));
        }

        [Test]
        public void Vector2IntParameterHandler()
        {
            var handler = new Vector2IntParameterHandler();

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(1, 2)"));
            Assert.IsTrue(handler.IsValid("(1, 2) "));
            Assert.IsFalse(handler.IsValid("(1)"));
            Assert.IsFalse(handler.IsValid("(1, 2, 3)"));
            Assert.IsFalse(handler.IsValid("(1.5, 2)"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("(1, 2)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));

            // Parse
            var result = (Vector2Int)handler.Parse("(1, 2)");
            Assert.That(result.x, Is.EqualTo(1));
            Assert.That(result.y, Is.EqualTo(2));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0)" }));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            Assert.That(handler.GetCandidates("(1,"), Contains.Item("(1, 0"));
            Assert.That(handler.GetCandidates("(1, 2"), Contains.Item("(1, 2)"));
            // 输入已包含闭括号 → 用户已表明完结意图，仅返回输入自身
            Assert.That(handler.GetCandidates("(1, 2)"),
                Is.EquivalentTo(new[] { "(1, 2)" }));
            Assert.That(handler.GetCandidates("(1, 2,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector2int"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector2Int"));
        }

        [Test]
        public void Vector3IntParameterHandler()
        {
            var handler = new Vector3IntParameterHandler();

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("(1, 2, 3)"));
            Assert.IsTrue(handler.IsValid("(1, 2, 3) "));
            Assert.IsFalse(handler.IsValid("(1, 2)"));
            Assert.IsFalse(handler.IsValid("(1, 2, 3, 4)"));
            Assert.IsFalse(handler.IsValid("(1.5, 2, 3)"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("(1, 2, 3)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2, 3) "));

            // Parse
            var result = (Vector3Int)handler.Parse("(1, 2, 3)");
            Assert.That(result.x, Is.EqualTo(1));
            Assert.That(result.y, Is.EqualTo(2));
            Assert.That(result.z, Is.EqualTo(3));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "(0, 0, 0)" }));
            Assert.That(handler.GetCandidates("("), Contains.Item("(0"));
            Assert.That(handler.GetCandidates("(1, 2,"),
                Contains.Item("(1, 2, 0"));
            Assert.That(handler.GetCandidates("(1, 2, 3"), Contains.Item("(1, 2, 3)"));
            Assert.That(handler.GetCandidates("(1, 2, 3,"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector3int"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3Int"));
        }

        [Test]
        public void CompositeParameterHandler_MultipleSimple()
        {
            // 组合 Integer 和 Float：同一个参数位置既可以输入整数也可以输入浮点数
            var handler = new CompositeParameterHandler("number", "Number",
                new IntegerParameterHandler("int"),
                new FloatParameterHandler("float"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("12"));
            Assert.IsTrue(handler.IsValid("12 "));
            Assert.IsTrue(handler.IsValid("1.5"));
            Assert.IsTrue(handler.IsValid("1.5 "));
            Assert.IsFalse(handler.IsValid("abc"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("12"));
            Assert.IsTrue(handler.ShouldAdvance("12 "));
            Assert.IsFalse(handler.ShouldAdvance("1.5"));
            Assert.IsTrue(handler.ShouldAdvance("1.5 "));
            Assert.IsFalse(handler.ShouldAdvance("abc ")); // 无效输入不 advance

            // Parse：整数走 IntegerHandler，浮点数走 FloatHandler
            Assert.That((int)handler.Parse("12 "), Is.EqualTo(12));
            Assert.That((float)handler.Parse("1.5 "), Is.EqualTo(1.5f));

            // GetCandidates：合并去重
            var candidates = handler.GetCandidates(string.Empty);
            Assert.That(candidates, Contains.Item("0"));
            Assert.That(candidates, Contains.Item("0.0"));

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("number"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Number"));
        }

        [Test]
        public void CompositeParameterHandler_MixedTupleAndSimple()
        {
            // 组合 Integer、Tuple(int,int)、Tuple(int,int,int)：模拟 Vec3Int 的多种输入格式
            var handler = new CompositeParameterHandler("vec", "Vector3Int",
                new IntegerParameterHandler("x"),
                new TestTupleHandler("vec2", "Vector2Int", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y")),
                new Vector3IntParameterHandler());

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：三种格式各自匹配
            Assert.IsTrue(handler.IsValid("5"));           // 简单 int
            Assert.IsTrue(handler.IsValid("5 "));
            Assert.IsTrue(handler.IsValid("(1, 2)"));      // 二元组
            Assert.IsTrue(handler.IsValid("(1, 2) "));
            Assert.IsTrue(handler.IsValid("(1, 2, 3)"));   // 三元组
            Assert.IsTrue(handler.IsValid("(1, 2, 3) "));
            Assert.IsFalse(handler.IsValid("(1, 2, 3, 4)")); // 四元组不匹配
            Assert.IsFalse(handler.IsValid("abc"));

            // ShouldAdvance
            Assert.IsTrue(handler.ShouldAdvance("5 "));
            Assert.IsFalse(handler.ShouldAdvance("5"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));
            Assert.IsFalse(handler.ShouldAdvance("(1, "));   // 正在输入元组，不 advance
            Assert.IsTrue(handler.ShouldAdvance("(1, 2, 3) "));
            Assert.IsFalse(handler.ShouldAdvance("(1, 2, 3)"));

            // Parse：各自委托到正确的子处理器
            Assert.That((int)handler.Parse("5 "), Is.EqualTo(5));
            var tupleResult = (object[])handler.Parse("(1, 2) ");
            Assert.That(tupleResult.Length, Is.EqualTo(2));
            Assert.That((int)tupleResult[0], Is.EqualTo(1));
            Assert.That((int)tupleResult[1], Is.EqualTo(2));
            var vec3Result = (Vector3Int)handler.Parse("(1, 2, 3) ");
            Assert.That(vec3Result.x, Is.EqualTo(1));
            Assert.That(vec3Result.y, Is.EqualTo(2));
            Assert.That(vec3Result.z, Is.EqualTo(3));

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vec"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3Int"));
        }

        [Test]
        public void CompositeParameterHandler_EmptyHandlers()
        {
            // 空子处理器集合：IsInitialized 应为 false
            var handler = new CompositeParameterHandler("empty", "Empty");
            Assert.IsFalse(handler.IsInitialized);

            // 通过 IEnumerable 构造空集合
            var handler2 = new CompositeParameterHandler("empty2", "Empty2",
                System.Array.Empty<IParameterHandler>());
            Assert.IsFalse(handler2.IsInitialized);
        }

        [Test]
        public void CompositeParameterHandler_ShouldAdvanceScenarios()
        {
            // 覆盖各种 ShouldAdvance 边界场景
            var handler = new CompositeParameterHandler("val", "Value",
                new IntegerParameterHandler("i"),
                new TestTupleHandler("tup", "Tuple", BracketType.Parentheses,
                    new IntegerParameterHandler("a"),
                    new IntegerParameterHandler("b")));

            // 1. 简单 int 完整 + 尾部空格 → advance
            Assert.IsTrue(handler.ShouldAdvance("42 "));

            // 2. 简单 int 无尾部空格 → 不 advance
            Assert.IsFalse(handler.ShouldAdvance("42"));

            // 3. 完整元组 + 尾部空格 → advance
            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));

            // 4. 不完整元组（正在输入） → 不 advance
            //    关键：IntegerHandler 会因尾部空格说 ShouldAdvance=true，
            //    但它的 IsValid 为 false，所以 CompositeParameterHandler 不应 advance
            Assert.IsFalse(handler.ShouldAdvance("(1, "));

            // 5. 无效输入 + 尾部空格 → 不 advance（无 handler 同时满足 IsValid + ShouldAdvance）
            Assert.IsFalse(handler.ShouldAdvance("abc "));

            // 6. 空或 null → 不 advance
            Assert.IsFalse(handler.ShouldAdvance(string.Empty));
            Assert.IsFalse(handler.ShouldAdvance(null));
        }

        [Test]
        public void CompositeParameterHandler_GetCandidates()
        {
            var handler = new CompositeParameterHandler("val", "Value",
                new IntegerParameterHandler("i"),
                new BooleanParameterHandler("b"));

            // 空输入：合并所有候选项
            var candidates = handler.GetCandidates(string.Empty);
            Assert.That(candidates, Contains.Item("0"));       // IntegerHandler
            Assert.That(candidates, Contains.Item("true"));    // BooleanHandler
            Assert.That(candidates, Contains.Item("false"));   // BooleanHandler

            // 部分输入匹配多个
            var candidatesForT = handler.GetCandidates("t");
            Assert.That(candidatesForT, Contains.Item("true"));

            // 输入只匹配一个
            var candidatesFor1 = handler.GetCandidates("1");
            // IntegerHandler: TryParse("1") → true, result=1 ≠ 0 → 无候选项
            // BooleanHandler: "1" 不以 "true"/"false" 开头 → 无候选项
            Assert.That(candidatesFor1, Is.Empty);
        }

        [Test]
        public void CompositeParameterHandler_ParseDelegation()
        {
            // 验证 Parse 委托给正确的子处理器
            var handler = new CompositeParameterHandler("val", "Value",
                new IntegerParameterHandler("i"),
                new FloatParameterHandler("f"),
                new BooleanParameterHandler("b"));

            // int
            Assert.That(handler.Parse("42"), Is.TypeOf<int>());
            Assert.That((int)handler.Parse("42"), Is.EqualTo(42));

            // float
            Assert.That(handler.Parse("3.14"), Is.TypeOf<float>());
            Assert.That((float)handler.Parse("3.14"), Is.EqualTo(3.14f).Within(1e-6f));

            // bool (int 也会对 "true" 返回 false，因为 int.TryParse("true") 失败)
            Assert.That(handler.Parse("true"), Is.TypeOf<bool>());
            Assert.That((bool)handler.Parse("true"), Is.True);

            // 匹配优先级：IntegerHandler 在 FloatHandler 前面，"1.5" 的 int 解析会失败，
            // 但 "1" 会被 int 先匹配
            // "1" 对 int 有效，对 float 也有效，但 int 先匹配
            Assert.That(handler.Parse("1"), Is.TypeOf<int>());
        }
    }
}