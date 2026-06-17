using System;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Tests
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
        public void BooleanParameterHandler_ValidatesAndParsesTrueFalseOnly()
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

            Assert.IsTrue(handler.TryParse("true", out var trueValue));
            Assert.That(trueValue, Is.EqualTo(true));

            Assert.IsTrue(handler.TryParse("false ", out var falseValue));
            Assert.That(falseValue, Is.EqualTo(false));
        }

        [Test]
        public void StringParameterHandler_SupportsQuotedAndUnquotedStrings()
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

            Assert.IsTrue(handler.TryParse("hello world", out var rawString));
            Assert.That(rawString, Is.EqualTo("hello world"));

            Assert.IsTrue(handler.TryParse("\"hello\\\"world\"", out var quotedString));
            Assert.That(quotedString, Is.EqualTo("hello\"world"));
        }

        [Test]
        public void EnumParameterHandler_UsesEnumNamesAndCaseSensitiveValidity()
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

            Assert.IsTrue(handler.TryParse("Beta", out var value));
            Assert.That(value, Is.EqualTo(SampleEnum.Beta));

            Assert.IsTrue(handler.TryParse("Gamma", out var gammaValue));
            Assert.That(gammaValue, Is.EqualTo(SampleEnum.Gamma));
        }

        [Test]
        public void Vector2ParameterHandler_ParsesBracketedFloatTuple()
        {
            var handler = new Vector2ParameterHandler("pos2");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("(1, 2)"));
            Assert.IsTrue(handler.IsValid("(1.5, -2.25) "));
            Assert.IsFalse(handler.IsValid("1, 2"));

            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));
            Assert.IsFalse(handler.ShouldAdvance("(1, 2)"));

            Assert.IsTrue(handler.TryParse("(1.5, -2.25)", out var value));
            Assert.That(value, Is.EqualTo(new Vector2(1.5f, -2.25f)));
        }

        [Test]
        public void Vector3ParameterHandler_ParsesBracketedFloatTuple()
        {
            var handler = new Vector3ParameterHandler("pos3");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("(1, 2, 3)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2, 3) "));
            Assert.IsTrue(handler.TryParse("(1.25, -2.5, 3.75)", out var value));
            Assert.That(value, Is.EqualTo(new Vector3(1.25f, -2.5f, 3.75f)));
        }

        [Test]
        public void Vector4ParameterHandler_ParsesBracketedFloatTuple()
        {
            var handler = new Vector4ParameterHandler("pos4");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("(1, 2, 3, 4)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2, 3, 4) "));
            Assert.IsTrue(handler.TryParse("(1, 2, 3, 4)", out var value));
            Assert.That(value, Is.EqualTo(new Vector4(1f, 2f, 3f, 4f)));
        }

        [Test]
        public void Vector2IntParameterHandler_ParsesBracketedIntegers()
        {
            var handler = new Vector2IntParameterHandler("ipos2");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("(1, 2)"));
            Assert.IsFalse(handler.IsValid("(1.0, 2)"));
            Assert.IsFalse(handler.IsValid("(1., 2)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));
            Assert.IsTrue(handler.TryParse("(1, 2)", out var value));
            Assert.That(value, Is.EqualTo(new Vector2Int(1, 2)));
        }

        [Test]
        public void Vector3IntParameterHandler_ParsesBracketedIntegers()
        {
            var handler = new Vector3IntParameterHandler("ipos3");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("(1, 2, 3)"));
            Assert.IsFalse(handler.IsValid("(1.0, 2, 3)"));
            Assert.IsFalse(handler.IsValid("(1., 2, 3)"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2, 3) "));
            Assert.IsTrue(handler.TryParse("(1, 2, 3)", out var value));
            Assert.That(value, Is.EqualTo(new Vector3Int(1, 2, 3)));
        }

        [Test]
        public void IntegerParameterHandler_StillParsesWithUtilityDrivenWhitespaceHandling()
        {
            var handler = new IntegerParameterHandler("count");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("12"));
            Assert.IsTrue(handler.IsValid("12 "));
            Assert.IsFalse(handler.IsValid("1.0"));
            Assert.IsFalse(handler.IsValid("1."));
            Assert.IsTrue(handler.ShouldAdvance("12 "));
            Assert.IsFalse(handler.ShouldAdvance("12"));
            Assert.IsTrue(handler.TryParse("12 ", out var value));
            Assert.That(value, Is.EqualTo(12));
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "0" }));
        }

        [Test]
        public void FloatParameterHandler_StillParsesWithUtilityDrivenWhitespaceHandling()
        {
            var handler = new FloatParameterHandler("ratio");

            Assert.IsTrue(handler.IsInitialized);
            Assert.IsTrue(handler.IsValid("1.5"));
            Assert.IsTrue(handler.IsValid("1.5 "));
            Assert.IsTrue(handler.ShouldAdvance("1.5 "));
            Assert.IsFalse(handler.ShouldAdvance("1.5"));
            Assert.IsTrue(handler.TryParse("1.5 ", out var value));
            Assert.That(value, Is.EqualTo(1.5f));
        }

        [Test]
        public void FixedStringParameterHandler_UsesFixedCandidateAndExactMatch()
        {
            var handler = new FixedStringParameterHandler("hello");

            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetCandidates(string.Empty), Is.EquivalentTo(new[] { "hello" }));
            Assert.That(handler.GetCandidates("he"), Is.EquivalentTo(new[] { "hello" }));
            Assert.IsTrue(handler.IsValid("hello"));
            Assert.IsTrue(handler.IsValid("hello "));
            Assert.IsFalse(handler.IsValid("hell"));
            Assert.IsTrue(handler.ShouldAdvance("hello "));
            Assert.IsFalse(handler.ShouldAdvance("hello"));
            Assert.IsTrue(handler.TryParse("hello", out var value));
            Assert.That(value, Is.EqualTo("hello"));
        }
    }
}


