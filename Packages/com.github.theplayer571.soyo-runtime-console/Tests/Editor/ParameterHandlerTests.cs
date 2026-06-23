using System;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;
using UnityEngine.TestTools;

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
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { @"""""" }));
            Assert.That(handler.GetCandidates("hello world"), Is.Empty);
            Assert.That(handler.GetCandidates(@"""hello worl"), Is.EquivalentTo(new[] { @"""hello worl""" }));


            Assert.IsTrue(handler.IsValid("hello"));
            Assert.IsTrue(handler.IsValid("hello world"));
            Assert.IsFalse(handler.IsValid(@"""hello"));
            Assert.IsTrue(handler.IsValid(@"""hello world"""));
            Assert.IsTrue(handler.IsValid(@"""hello world"" "));

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
        public void StringOptionParameterHandler()
        {
            var handler = new StringOptionParameterHandler("difficulty", "Easy", "Normal", "Hard");

            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "Easy", "Normal", "Hard" }));
            Assert.That(handler.GetCandidates("a"), Contains.Item("Hard"));
            Assert.That(handler.GetCandidates("e"), Contains.Item("Easy"));
            Assert.That(handler.GetCandidates("X"), Is.Empty);
            Assert.IsFalse(handler.IsValid("easy"));
            Assert.IsTrue(handler.IsValid("Easy"));
            Assert.IsTrue(handler.ShouldAdvance("Easy "));
            Assert.IsFalse(handler.ShouldAdvance("Easy"));

            Assert.That((string)handler.Parse("Normal"), Is.EqualTo("Normal"));
            Assert.That((string)handler.Parse("Hard"), Is.EqualTo("Hard"));
        }

        [Test]
        public void StringOptionParameterHandler_EmptyOptions_NotInitialized()
        {
            LogAssert.Expect(LogType.Error, "StringOptionParameterHandler: options must contain at least one string.");
            var handler = new StringOptionParameterHandler("empty");
            Assert.IsFalse(handler.IsInitialized);
        }

        [Test]
        public void StringOptionParameterHandler_WhitespaceOption_NotInitialized()
        {
            LogAssert.Expect(LogType.Error, "StringOptionParameterHandler: option at index 1 contains whitespace at char 3. Input: \"has space\"");
            var handler = new StringOptionParameterHandler("bad", "ok", "has space");
            Assert.IsFalse(handler.IsInitialized);
        }

        [Test]
        public void GuidParameterHandler()
        {
            var handler = new GuidParameterHandler("guid");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：标准 GUID 格式
            Assert.IsTrue(handler.IsValid("12345678-1234-1234-1234-123456789abc"));
            Assert.IsTrue(handler.IsValid("12345678-1234-1234-1234-123456789abc "));
            Assert.IsTrue(handler.IsValid("{12345678-1234-1234-1234-123456789abc}"));
            Assert.IsTrue(handler.IsValid("(12345678-1234-1234-1234-123456789abc)"));
            Assert.IsTrue(handler.IsValid("00000000-0000-0000-0000-000000000000"));
            // 无效输入
            Assert.IsFalse(handler.IsValid("not-a-guid"));
            Assert.IsFalse(handler.IsValid("hello"));
            Assert.IsFalse(handler.IsValid(string.Empty));
            Assert.IsFalse(handler.IsValid(null));

            // ShouldAdvance：空格结尾即 advance
            Assert.IsTrue(handler.ShouldAdvance("12345678-1234-1234-1234-123456789abc "));
            Assert.IsFalse(handler.ShouldAdvance("12345678-1234-1234-1234-123456789abc"));
            Assert.IsFalse(handler.ShouldAdvance(string.Empty));
            Assert.IsFalse(handler.ShouldAdvance(null));

            // Parse
            var expectedGuid = System.Guid.Parse("12345678-1234-1234-1234-123456789abc");
            var result = (System.Guid)handler.Parse("12345678-1234-1234-1234-123456789abc");
            Assert.That(result, Is.EqualTo(expectedGuid));

            // 带花括号的解析
            var resultBrace = (System.Guid)handler.Parse("{12345678-1234-1234-1234-123456789abc}");
            Assert.That(resultBrace, Is.EqualTo(expectedGuid));

            // GetCandidates：空输入返回零 GUID 提示
            Assert.That(handler.GetCandidates(string.Empty),
                Contains.Item("00000000-0000-0000-0000-000000000000"));
            // 非空输入无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("guid"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Guid"));
        }


    }
}