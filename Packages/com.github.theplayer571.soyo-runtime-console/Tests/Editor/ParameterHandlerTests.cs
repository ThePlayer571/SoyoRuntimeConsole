using System;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Tests.Tests.Editor
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
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "\"\"" }));

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
            Assert.That(handler.Parse("hello"), Is.EqualTo("hello"));
        }
    }
}