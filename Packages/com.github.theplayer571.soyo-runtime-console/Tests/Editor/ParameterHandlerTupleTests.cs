using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.ValueObjects;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public class ParameterHandlerTupleTests
    {
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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);
            Assert.That(handler.GetCandidates("abc"), Is.Empty);

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
            // 输入完全不成立 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

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
            // 输入不以花括号开头 → 无候选项
            Assert.That(braceHandler.GetCandidates("1"), Is.Empty);

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
            // 输入不以方括号开头 → 无候选项
            Assert.That(bracketHandler.GetCandidates("1"), Is.Empty);
        }

        [Test]
        public void Vector2ParameterHandler()
        {
            var handler = new Vector2ParameterHandler("vector2");

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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector2"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector2"));
        }

        [Test]
        public void Vector3ParameterHandler()
        {
            var handler = new Vector3ParameterHandler("vector3");

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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector3"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3"));
        }

        [Test]
        public void Vector4ParameterHandler()
        {
            var handler = new Vector4ParameterHandler("vector4");

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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector4"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector4"));
        }

        [Test]
        public void Vector2IntParameterHandler()
        {
            var handler = new Vector2IntParameterHandler("vector2int");

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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector2int"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector2Int"));
        }

        [Test]
        public void Vector3IntParameterHandler()
        {
            var handler = new Vector3IntParameterHandler("vector3int");

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
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("vector3int"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Vector3Int"));
        }
        [Test]
        public void RectParameterHandler()
        {
            var handler = new RectParameterHandler("rect");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid — 4-float 格式
            Assert.IsTrue(handler.IsValid("{1.0, 2.0, 3.0, 4.0}"));
            Assert.IsTrue(handler.IsValid("{1.0, 2.0, 3.0, 4.0} "));
            Assert.IsFalse(handler.IsValid("{1.0, 2.0, 3.0}"));
            Assert.IsFalse(handler.IsValid("{1.0, 2.0, 3.0, 4.0, 5.0}"));
            Assert.IsFalse(handler.IsValid("{abc, 2.0, 3.0, 4.0}"));
            // IsValid — 2-Vector2 格式
            Assert.IsTrue(handler.IsValid("{(1.0, 2.0), (3.0, 4.0)}"));
            Assert.IsTrue(handler.IsValid("{(1.0, 2.0), (3.0, 4.0)} "));
            Assert.IsFalse(handler.IsValid("{(1.0, 2.0)}"));
            Assert.IsFalse(handler.IsValid("{(1.0, 2.0), (3.0, 4.0), (5.0, 6.0)}"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("{1.0, 2.0, 3.0, 4.0}"));
            Assert.IsTrue(handler.ShouldAdvance("{1.0, 2.0, 3.0, 4.0} "));
            Assert.IsFalse(handler.ShouldAdvance("{(1.0, 2.0), (3.0, 4.0)}"));
            Assert.IsTrue(handler.ShouldAdvance("{(1.0, 2.0), (3.0, 4.0)} "));

            // Parse — 4-float 格式
            var resultFloat = (Rect)handler.Parse("{1.0, 2.0, 3.0, 4.0}");
            Assert.That(resultFloat.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(resultFloat.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(resultFloat.width, Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That(resultFloat.height, Is.EqualTo(4.0f).Within(1e-6f));

            // Parse — 2-Vector2 格式
            var resultVec = (Rect)handler.Parse("{(1.0, 2.0), (3.0, 4.0)}");
            Assert.That(resultVec.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(resultVec.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(resultVec.width, Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That(resultVec.height, Is.EqualTo(4.0f).Within(1e-6f));

            // GetCandidates：复合处理器会合并两种格式的候选项（含去重）
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "{", "{0, 0, 0, 0}", "{(0, 0), (0, 0)}" }));
            Assert.That(handler.GetCandidates("{"), Contains.Item("{0"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0,"),
                Contains.Item("{1.0, 2.0, 3.0, 0"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0, 4.0"),
                Contains.Item("{1.0, 2.0, 3.0, 4.0}"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0, 4.0,"), Is.Empty);
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("rect"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Rect"));
        }

        [Test]
        public void RectIntParameterHandler()
        {
            var handler = new RectIntParameterHandler("rectint");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid — 4-int 格式
            Assert.IsTrue(handler.IsValid("{1, 2, 3, 4}"));
            Assert.IsTrue(handler.IsValid("{1, 2, 3, 4} "));
            Assert.IsFalse(handler.IsValid("{1, 2, 3}"));
            Assert.IsFalse(handler.IsValid("{1, 2, 3, 4, 5}"));
            Assert.IsFalse(handler.IsValid("{1.5, 2, 3, 4}"));
            // IsValid — 2-Vector2Int 格式
            Assert.IsTrue(handler.IsValid("{(1, 2), (3, 4)}"));
            Assert.IsTrue(handler.IsValid("{(1, 2), (3, 4)} "));
            Assert.IsFalse(handler.IsValid("{(1, 2)}"));
            Assert.IsFalse(handler.IsValid("{(1, 2), (3, 4), (5, 6)}"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("{1, 2, 3, 4}"));
            Assert.IsTrue(handler.ShouldAdvance("{1, 2, 3, 4} "));
            Assert.IsFalse(handler.ShouldAdvance("{(1, 2), (3, 4)}"));
            Assert.IsTrue(handler.ShouldAdvance("{(1, 2), (3, 4)} "));

            // Parse — 4-int 格式
            var resultInt = (RectInt)handler.Parse("{1, 2, 3, 4}");
            Assert.That(resultInt.x, Is.EqualTo(1));
            Assert.That(resultInt.y, Is.EqualTo(2));
            Assert.That(resultInt.width, Is.EqualTo(3));
            Assert.That(resultInt.height, Is.EqualTo(4));

            // Parse — 2-Vector2Int 格式
            var resultVec = (RectInt)handler.Parse("{(1, 2), (3, 4)}");
            Assert.That(resultVec.x, Is.EqualTo(1));
            Assert.That(resultVec.y, Is.EqualTo(2));
            Assert.That(resultVec.width, Is.EqualTo(3));
            Assert.That(resultVec.height, Is.EqualTo(4));

            // GetCandidates：复合处理器会合并两种格式的候选项（含去重）
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "{", "{0, 0, 0, 0}", "{(0, 0), (0, 0)}" }));
            Assert.That(handler.GetCandidates("{"), Contains.Item("{0"));
            Assert.That(handler.GetCandidates("{1, 2, 3,"),
                Contains.Item("{1, 2, 3, 0"));
            Assert.That(handler.GetCandidates("{1, 2, 3, 4"),
                Contains.Item("{1, 2, 3, 4}"));
            Assert.That(handler.GetCandidates("{1, 2, 3, 4,"), Is.Empty);
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("rectint"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("RectInt"));
        }

        [Test]
        public void BoundsParameterHandler()
        {
            var handler = new BoundsParameterHandler("bounds");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid — 6-float 格式
            Assert.IsTrue(handler.IsValid("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0}"));
            Assert.IsTrue(handler.IsValid("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0} "));
            Assert.IsFalse(handler.IsValid("{1.0, 2.0, 3.0, 4.0, 5.0}"));
            Assert.IsFalse(handler.IsValid("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0}"));
            Assert.IsFalse(handler.IsValid("{abc, 2.0, 3.0, 4.0, 5.0, 6.0}"));
            // IsValid — 2-Vector3 格式
            Assert.IsTrue(handler.IsValid("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)}"));
            Assert.IsTrue(handler.IsValid("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)} "));
            Assert.IsFalse(handler.IsValid("{(1.0, 2.0, 3.0)}"));
            Assert.IsFalse(handler.IsValid("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0), (7.0, 8.0, 9.0)}"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0}"));
            Assert.IsTrue(handler.ShouldAdvance("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0} "));
            Assert.IsFalse(handler.ShouldAdvance("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)}"));
            Assert.IsTrue(handler.ShouldAdvance("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)} "));

            // Parse — 6-float 格式
            var resultFloat = (Bounds)handler.Parse("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0}");
            Assert.That(resultFloat.center.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(resultFloat.center.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(resultFloat.center.z, Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That(resultFloat.size.x, Is.EqualTo(4.0f).Within(1e-6f));
            Assert.That(resultFloat.size.y, Is.EqualTo(5.0f).Within(1e-6f));
            Assert.That(resultFloat.size.z, Is.EqualTo(6.0f).Within(1e-6f));

            // Parse — 2-Vector3 格式
            var resultVec = (Bounds)handler.Parse("{(1.0, 2.0, 3.0), (4.0, 5.0, 6.0)}");
            Assert.That(resultVec.center.x, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(resultVec.center.y, Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That(resultVec.center.z, Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That(resultVec.size.x, Is.EqualTo(4.0f).Within(1e-6f));
            Assert.That(resultVec.size.y, Is.EqualTo(5.0f).Within(1e-6f));
            Assert.That(resultVec.size.z, Is.EqualTo(6.0f).Within(1e-6f));

            // GetCandidates：复合处理器会合并两种格式的候选项（含去重）
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "{", "{0, 0, 0, 0, 0, 0}", "{(0, 0, 0), (0, 0, 0)}" }));
            Assert.That(handler.GetCandidates("{"), Contains.Item("{0"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0, 4.0, 5.0,"),
                Contains.Item("{1.0, 2.0, 3.0, 4.0, 5.0, 0"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0"),
                Contains.Item("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0}"));
            Assert.That(handler.GetCandidates("{1.0, 2.0, 3.0, 4.0, 5.0, 6.0,"), Is.Empty);
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("bounds"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Bounds"));
        }

        [Test]
        public void BoundsIntParameterHandler()
        {
            var handler = new BoundsIntParameterHandler("boundsint");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid — 6-int 格式
            Assert.IsTrue(handler.IsValid("{1, 2, 3, 4, 5, 6}"));
            Assert.IsTrue(handler.IsValid("{1, 2, 3, 4, 5, 6} "));
            Assert.IsFalse(handler.IsValid("{1, 2, 3, 4, 5}"));
            Assert.IsFalse(handler.IsValid("{1, 2, 3, 4, 5, 6, 7}"));
            Assert.IsFalse(handler.IsValid("{1.5, 2, 3, 4, 5, 6}"));
            // IsValid — 2-Vector3Int 格式
            Assert.IsTrue(handler.IsValid("{(1, 2, 3), (4, 5, 6)}"));
            Assert.IsTrue(handler.IsValid("{(1, 2, 3), (4, 5, 6)} "));
            Assert.IsFalse(handler.IsValid("{(1, 2, 3)}"));
            Assert.IsFalse(handler.IsValid("{(1, 2, 3), (4, 5, 6), (7, 8, 9)}"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("{1, 2, 3, 4, 5, 6}"));
            Assert.IsTrue(handler.ShouldAdvance("{1, 2, 3, 4, 5, 6} "));
            Assert.IsFalse(handler.ShouldAdvance("{(1, 2, 3), (4, 5, 6)}"));
            Assert.IsTrue(handler.ShouldAdvance("{(1, 2, 3), (4, 5, 6)} "));

            // Parse — 6-int 格式
            var resultInt = (BoundsInt)handler.Parse("{1, 2, 3, 4, 5, 6}");
            Assert.That(resultInt.x, Is.EqualTo(1));
            Assert.That(resultInt.y, Is.EqualTo(2));
            Assert.That(resultInt.z, Is.EqualTo(3));
            Assert.That(resultInt.size.x, Is.EqualTo(4));
            Assert.That(resultInt.size.y, Is.EqualTo(5));
            Assert.That(resultInt.size.z, Is.EqualTo(6));

            // Parse — 2-Vector3Int 格式
            var resultVec = (BoundsInt)handler.Parse("{(1, 2, 3), (4, 5, 6)}");
            Assert.That(resultVec.x, Is.EqualTo(1));
            Assert.That(resultVec.y, Is.EqualTo(2));
            Assert.That(resultVec.z, Is.EqualTo(3));
            Assert.That(resultVec.size.x, Is.EqualTo(4));
            Assert.That(resultVec.size.y, Is.EqualTo(5));
            Assert.That(resultVec.size.z, Is.EqualTo(6));

            // GetCandidates：复合处理器会合并两种格式的候选项（含去重）
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "{", "{0, 0, 0, 0, 0, 0}", "{(0, 0, 0), (0, 0, 0)}" }));
            Assert.That(handler.GetCandidates("{"), Contains.Item("{0"));
            Assert.That(handler.GetCandidates("{1, 2, 3, 4, 5,"),
                Contains.Item("{1, 2, 3, 4, 5, 0"));
            Assert.That(handler.GetCandidates("{1, 2, 3, 4, 5, 6"),
                Contains.Item("{1, 2, 3, 4, 5, 6}"));
            Assert.That(handler.GetCandidates("{1, 2, 3, 4, 5, 6,"), Is.Empty);
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("boundsint"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("BoundsInt"));
        }

        [Test]
        public void ColorParameterHandler()
        {
            var handler = new ColorParameterHandler("color");

            Assert.IsTrue(handler.IsInitialized);

            // IsValid — 十六进制格式
            Assert.IsTrue(handler.IsValid("#FF0000"));
            Assert.IsTrue(handler.IsValid("#FF0000 "));
            Assert.IsTrue(handler.IsValid("#FF0000FF"));
            Assert.IsTrue(handler.IsValid("FF0000"));
            Assert.IsFalse(handler.IsValid("#GGG"));
            Assert.IsFalse(handler.IsValid("not-a-color"));
            // IsValid — 十六进制 + alpha 格式
            Assert.IsTrue(handler.IsValid("{#FF0000, 0.5}"));
            Assert.IsTrue(handler.IsValid("{#00FF00, 1.0} "));
            Assert.IsFalse(handler.IsValid("{#FF0000}"));
            Assert.IsFalse(handler.IsValid("{#FF0000, 0.5, extra}"));
            // IsValid — 4-float 格式
            Assert.IsTrue(handler.IsValid("{0.5, 0.2, 0.3, 1.0}"));
            Assert.IsTrue(handler.IsValid("{0.5, 0.2, 0.3, 1.0} "));
            Assert.IsFalse(handler.IsValid("{0.5, 0.2}"));
            Assert.IsFalse(handler.IsValid("{0.5, 0.2, 0.3, 1.0, 0.0}"));
            Assert.IsFalse(handler.IsValid("{abc, 0.2, 0.3, 1.0}"));
            // IsValid — 3-float 格式
            Assert.IsTrue(handler.IsValid("{0.5, 0.2, 0.3}"));
            Assert.IsTrue(handler.IsValid("{0.0, 0.0, 0.0} "));
            Assert.IsFalse(handler.IsValid("{0.5, 0.2}"));
            Assert.IsFalse(handler.IsValid("{0.5, 0.2, 0.3, 1.0, 0.0}"));
            // IsValid — 字节范围（由浮点处理器接受，自动检测）
            Assert.IsTrue(handler.IsValid("{128, 0, 0, 255}"));
            Assert.IsTrue(handler.IsValid("{128, 0, 0}"));
            Assert.IsFalse(handler.IsValid("{128, 0}"));

            // ShouldAdvance
            Assert.IsTrue(handler.ShouldAdvance("#FF0000 "));
            Assert.IsFalse(handler.ShouldAdvance("#FF0000"));
            Assert.IsTrue(handler.ShouldAdvance("{0.5, 0.2, 0.3, 1.0} "));
            Assert.IsFalse(handler.ShouldAdvance("{0.5, 0.2, 0.3, 1.0}"));
            Assert.IsTrue(handler.ShouldAdvance("{0.5, 0.2, 0.3} "));
            Assert.IsFalse(handler.ShouldAdvance("{0.5, 0.2, 0.3}"));
            Assert.IsTrue(handler.ShouldAdvance("{#FF0000, 0.5} "));
            Assert.IsFalse(handler.ShouldAdvance("{#FF0000, 0.5}"));

            // Parse — 十六进制格式
            var hexResult = (Color)handler.Parse("#FF0000");
            Assert.That(hexResult.r, Is.EqualTo(1.0f).Within(0.01f));
            Assert.That(hexResult.g, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(hexResult.b, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(hexResult.a, Is.EqualTo(1.0f).Within(0.01f));

            // Parse — 十六进制 + alpha 格式
            var hexAlphaResult = (Color)handler.Parse("{#FF0000, 0.5}");
            Assert.That(hexAlphaResult.r, Is.EqualTo(1.0f).Within(0.01f));
            Assert.That(hexAlphaResult.g, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(hexAlphaResult.b, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(hexAlphaResult.a, Is.EqualTo(0.5f).Within(1e-6f));

            // Parse — 4-float 格式
            var float4Result = (Color)handler.Parse("{0.5, 0.2, 0.3, 0.8}");
            Assert.That(float4Result.r, Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(float4Result.g, Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(float4Result.b, Is.EqualTo(0.3f).Within(1e-6f));
            Assert.That(float4Result.a, Is.EqualTo(0.8f).Within(1e-6f));

            // Parse — 3-float 格式（a 默认为 1.0）
            var float3Result = (Color)handler.Parse("{0.5, 0.2, 0.3}");
            Assert.That(float3Result.r, Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(float3Result.g, Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(float3Result.b, Is.EqualTo(0.3f).Within(1e-6f));
            Assert.That(float3Result.a, Is.EqualTo(1.0f).Within(1e-6f));

            // Parse — 4-int 字节范围格式（128/255 ≈ 0.502）
            var intResult = (Color)handler.Parse("{128, 0, 0, 255}");
            Assert.That(intResult.r, Is.EqualTo(128f / 255f).Within(1e-6f));
            Assert.That(intResult.g, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(intResult.b, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(intResult.a, Is.EqualTo(1.0f).Within(1e-6f));

            // Parse — 3-int 字节范围格式（a 默认为 1.0）
            var int3Result = (Color)handler.Parse("{128, 0, 0}");
            Assert.That(int3Result.r, Is.EqualTo(128f / 255f).Within(1e-6f));
            Assert.That(int3Result.g, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(int3Result.b, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(int3Result.a, Is.EqualTo(1.0f).Within(1e-6f));

            // Parse — 边界情况：{1, 0, 0} 所有值 ≤ 1.0，不走字节范围缩放
            var boundaryResult = (Color)handler.Parse("{1, 0, 0}");
            Assert.That(boundaryResult.r, Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(boundaryResult.g, Is.EqualTo(0.0f).Within(1e-6f));
            Assert.That(boundaryResult.b, Is.EqualTo(0.0f).Within(1e-6f));

            // GetCandidates：空输入合并所有子处理器候选项（含去重）
            var candidates = handler.GetCandidates(string.Empty).ToList();
            // 十六进制候选项（#000000 代替之前的单独 #）
            Assert.That(candidates, Contains.Item("#000000"));
            // 元组候选项（花括号前缀，及各格式的一键填充结果）
            Assert.That(candidates, Contains.Item("{"));
            Assert.That(candidates, Contains.Item("{0, 0, 0, 0}"));
            Assert.That(candidates, Contains.Item("{0, 0, 0}"));
            // 十六进制 + alpha 格式的一键填充
            Assert.That(candidates, Contains.Item("{#000000, 0}"));

            // GetCandidates — 十六进制格式的渐进补全
            Assert.That(handler.GetCandidates("#"), Contains.Item("#000000"));
            Assert.That(handler.GetCandidates("#F"), Contains.Item("#F00000"));
            Assert.That(handler.GetCandidates("#FF"), Contains.Item("#FF0000"));
            Assert.That(handler.GetCandidates("#FF0000"), Contains.Item("#FF0000"));
            // 无 # 前缀不触发候选项（避免纯数字等被误识别为颜色）
            Assert.That(handler.GetCandidates("FF"), Is.Empty);
            Assert.That(handler.GetCandidates("327312"), Is.Empty);
            Assert.That(handler.GetCandidates("#FF0000F"), Contains.Item("#FF0000F0"));
            Assert.That(handler.GetCandidates("#FF0000FF"), Contains.Item("#FF0000FF"));
            // 无效字符不产生候选项
            Assert.That(handler.GetCandidates("#GGG"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("color"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Color"));
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
            // 无效输入 + 尾部空格：所有子处理器 ShouldAdvance 均为 true（以空格结尾），
            // 回退路径生效 —— 此时 advance 是正确的，避免卡死在当前参数位置。
            // 该参数会在后续的 IsValid 检查中被拒绝。
            Assert.IsTrue(handler.ShouldAdvance("abc "));

            // Parse：整数走 IntegerHandler，浮点数走 FloatHandler
            Assert.That((int)handler.Parse("12 "), Is.EqualTo(12));
            Assert.That((float)handler.Parse("1.5 "), Is.EqualTo(1.5f));

            // GetCandidates：合并去重
            var candidates = handler.GetCandidates(string.Empty).ToList();
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
                new Vector3IntParameterHandler("vector3int"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：三种格式各自匹配
            Assert.IsTrue(handler.IsValid("5")); // 简单 int
            Assert.IsTrue(handler.IsValid("5 "));
            Assert.IsTrue(handler.IsValid("(1, 2)")); // 二元组
            Assert.IsTrue(handler.IsValid("(1, 2) "));
            Assert.IsTrue(handler.IsValid("(1, 2, 3)")); // 三元组
            Assert.IsTrue(handler.IsValid("(1, 2, 3) "));
            Assert.IsFalse(handler.IsValid("(1, 2, 3, 4)")); // 四元组不匹配
            Assert.IsFalse(handler.IsValid("abc"));

            // ShouldAdvance
            Assert.IsTrue(handler.ShouldAdvance("5 "));
            Assert.IsFalse(handler.ShouldAdvance("5"));
            Assert.IsTrue(handler.ShouldAdvance("(1, 2) "));
            Assert.IsFalse(handler.ShouldAdvance("(1, ")); // 正在输入元组，不 advance
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
            //    关键：IntegerHandler 对 "(1, " 的 ShouldAdvance=true（空格结尾），
            //    但 TupleHandler 的 ShouldAdvance=false（开括号未闭合），
            //    "所有子处理器均 true"的回退条件不成立，正确阻止了 advance。
            Assert.IsFalse(handler.ShouldAdvance("(1, "));

            // 5. 无效输入 + 尾部空格 → advance（回退路径：IntegerHandler 和 TupleHandler
            //    的 ShouldAdvance 都返回 true，因为两者都不在结构化解析中途）。
            //    该参数会在后续的 IsValid 检查中被命令分析器拒绝。
            Assert.IsTrue(handler.ShouldAdvance("abc "));

            // 6. 空或 null → 不 advance
            Assert.IsFalse(handler.ShouldAdvance(string.Empty));
            Assert.IsFalse(handler.ShouldAdvance(null));

            // 7. 无效输入 + 无尾部空格 → 不 advance
            //    所有子处理器的 ShouldAdvance 都为 false（无空格结尾），不触发回退路径。
            Assert.IsFalse(handler.ShouldAdvance("abc"));

            // 8. 纯空格分隔类处理器组合 + 无效输入 + 尾部空格 → advance（回退路径）
            //    验证当 Composite 内部全是 SpaceSplitHandler（无 BracketHandler）时，
            //    所有子处理器 ShouldAdvance 一致为 true，回退路径生效。
            var spaceOnlyHandler = new CompositeParameterHandler("num", "Number",
                new IntegerParameterHandler("i"),
                new FloatParameterHandler("f"),
                new BooleanParameterHandler("b"));

            Assert.IsTrue(spaceOnlyHandler.ShouldAdvance("xyz ")); // 回退路径 advance
            Assert.IsFalse(spaceOnlyHandler.ShouldAdvance("xyz")); // 无尾部空格，不回退

            // 9. 混合 BracketHandler + SpaceSplitHandler，BracketHandler 处于结构化解析中途
            //    → 不 advance（回退路径被 BracketHandler 的 false 阻断）
            Assert.IsFalse(handler.ShouldAdvance("(42, ")); // TupleHandler 检测到未闭合括号
        }

        [Test]
        public void CompositeParameterHandler_GetCandidates()
        {
            var handler = new CompositeParameterHandler("val", "Value",
                new IntegerParameterHandler("i"),
                new BooleanParameterHandler("b"));

            // 空输入：合并所有候选项
            var candidates = handler.GetCandidates(string.Empty).ToList();
            Assert.That(candidates, Contains.Item("0")); // IntegerHandler
            Assert.That(candidates, Contains.Item("true")); // BooleanHandler
            Assert.That(candidates, Contains.Item("false")); // BooleanHandler

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

        [Test]
        public void TupleParameterHandler_NestedTuples_SameBracketType()
        {
            // 嵌套元组：外层圆括号包含一个内部圆括号元组 + 一个布尔值
            // 输入格式：((int, int), bool)
            var handler = new TestTupleHandler("nested", "Nested", BracketType.Parentheses,
                new TestTupleHandler("inner", "Inner", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y")),
                new BooleanParameterHandler("flag"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：嵌套括号正确解析
            Assert.IsTrue(handler.IsValid("((1, 2), true)"));
            Assert.IsTrue(handler.IsValid("((1, 2), true) "));
            Assert.IsTrue(handler.IsValid("((10, 20), false)"));
            // 尾随空格（trim 后合法）
            Assert.IsTrue(handler.IsValid("((1, 2), true) "));
            // 数量不匹配：内部元组的逗号不应被外层计数
            Assert.IsFalse(handler.IsValid("((1, 2))")); // 缺少 bool
            Assert.IsFalse(handler.IsValid("((1, 2), true, 0)")); // 多了一个
            // 内部元组数量不匹配
            Assert.IsFalse(handler.IsValid("((1, 2, 3), true)")); // 内部 3 个 vs 2 个
            Assert.IsFalse(handler.IsValid("((1), true)")); // 内部 1 个 vs 2 个
            // 子参数不合法
            Assert.IsFalse(handler.IsValid("((1, abc), true)"));
            Assert.IsFalse(handler.IsValid("((1, 2), True)")); // 大写 True

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("((1, 2), true)"));
            Assert.IsTrue(handler.ShouldAdvance("((1, 2), true) "));
            Assert.IsFalse(handler.ShouldAdvance("((1, 2), true"));
            Assert.IsTrue(handler.ShouldAdvance("((1, 2), true)  "));

            // Parse：验证嵌套 object[] 结构
            var result = (object[])handler.Parse("((1, 2), true) ");
            Assert.That(result.Length, Is.EqualTo(2));
            // result[0] 是内部元组的解析结果（object[]）
            Assert.That(result[0], Is.TypeOf<object[]>());
            var innerResult = (object[])result[0];
            Assert.That(innerResult.Length, Is.EqualTo(2));
            Assert.That((int)innerResult[0], Is.EqualTo(1));
            Assert.That((int)innerResult[1], Is.EqualTo(2));
            // result[1] 是布尔值
            Assert.That(result[1], Is.EqualTo(true));

            // GetCandidates：空输入
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "(", "((0, 0), true)" }));
            // 输入 "((" → 委托给内部 TupleHandler 的第一个 int 参数
            Assert.That(handler.GetCandidates("(("), Contains.Item("((0"));
            // 输入 "((1, " → 内部元组的第二个参数候选项（前缀为 "((1, "）
            var candidates = handler.GetCandidates("((1, ").ToList();
            Assert.That(candidates, Contains.Item("((1, 0"));
            Assert.That(candidates, Contains.Item("((1, 0)"));
            // 输入 "((1, 2), " → 第二个子参数（bool）的候选项（前缀为 "((1, 2), "）
            Assert.That(handler.GetCandidates("((1, 2), "), Contains.Item("((1, 2), true"));
            Assert.That(handler.GetCandidates("((1, 2), "), Contains.Item("((1, 2), false"));
            // 输入 "((1, 2), t" → 布尔值的部分补全
            Assert.That(handler.GetCandidates("((1, 2), t"), Contains.Item("((1, 2), true"));
            // 输入内层元组已闭合但外层未闭合（内层 ) 不应被误判为外层闭括号）
            // → 应同时返回输入本身和完整填充结果
            Assert.That(handler.GetCandidates("((1, 2)"),
                Contains.Item("((1, 2), true)"));
            Assert.That(handler.GetCandidates("((1, 2)"),
                Contains.Item("((1, 2)"));
            // 输入已包含闭括号 → 仅返回输入自身
            Assert.That(handler.GetCandidates("((1, 2), true)"),
                Is.EquivalentTo(new[] { "((1, 2), true)" }));
            // 超出处理器数量 → 无候选项
            Assert.That(handler.GetCandidates("((1, 2), true,"), Is.Empty);
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("nested"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Nested"));
        }

        [Test]
        public void TupleParameterHandler_NestedTuples_MixedBracketTypes()
        {
            // 嵌套元组：外层方括号包含圆括号元组 + 花括号元组
            // 输入格式：[(int, int), {bool}]
            var handler = new TestTupleHandler("mixed", "Mixed", BracketType.Brackets,
                new TestTupleHandler("inner1", "Inner1", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y")),
                new TestTupleHandler("inner2", "Inner2", BracketType.Braces,
                    new BooleanParameterHandler("flag")));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：混合括号类型嵌套
            Assert.IsTrue(handler.IsValid("[(1, 2), {true}]"));
            Assert.IsTrue(handler.IsValid("[(1, 2), {true}] "));
            Assert.IsTrue(handler.IsValid("[(10, 20), {false}]"));
            // 数量不匹配
            Assert.IsFalse(handler.IsValid("[(1, 2)]"));
            Assert.IsFalse(handler.IsValid("[(1, 2), {true}, 0]"));
            // 子参数不合法
            Assert.IsFalse(handler.IsValid("[(1, 2), {True}]"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[(1, 2), {true}]"));
            Assert.IsTrue(handler.ShouldAdvance("[(1, 2), {true}] "));

            // Parse：验证嵌套 object[] 结构
            var result = (object[])handler.Parse("[(1, 2), {true}] ");
            Assert.That(result.Length, Is.EqualTo(2));
            var inner1 = (object[])result[0];
            Assert.That((int)inner1[0], Is.EqualTo(1));
            Assert.That((int)inner1[1], Is.EqualTo(2));
            var inner2 = (object[])result[1];
            Assert.That((bool)inner2[0], Is.EqualTo(true));

            // GetCandidates：方括号前缀
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[(0, 0), {true}]" }));
            // 输入 "[" → 第一个内部元组候选项
            Assert.That(handler.GetCandidates("["), Contains.Item("[(0"));
            // 输入 "[(1, 2), " → 第二个内部元组候选项（花括号前缀）
            Assert.That(handler.GetCandidates("[(1, 2), "), Contains.Item("[(1, 2), {true"));
            Assert.That(handler.GetCandidates("[(1, 2), "), Contains.Item("[(1, 2), {false"));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("mixed"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Mixed"));
        }

        [Test]
        public void TupleParameterHandler_DeeplyNested()
        {
            // 三层子参数：两个内部元组 + 一个布尔值
            // 输入格式：((int, int), (float, float), bool)
            var handler = new TestTupleHandler("deep", "Deep", BracketType.Parentheses,
                new TestTupleHandler("a", "A", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y")),
                new TestTupleHandler("b", "B", BracketType.Parentheses,
                    new FloatParameterHandler("u"),
                    new FloatParameterHandler("v")),
                new BooleanParameterHandler("flag"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：三个子参数，其中两个是元组
            Assert.IsTrue(handler.IsValid("((1, 2), (3.0, 4.0), true)"));
            Assert.IsTrue(handler.IsValid("((1, 2), (3.0, 4.0), true) "));
            // 数量不匹配
            Assert.IsFalse(handler.IsValid("((1, 2), (3.0, 4.0))"));
            Assert.IsFalse(handler.IsValid("((1, 2), (3.0, 4.0), true, 0)"));

            // ShouldAdvance
            Assert.IsTrue(handler.ShouldAdvance("((1, 2), (3.0, 4.0), true) "));
            Assert.IsFalse(handler.ShouldAdvance("((1, 2), (3.0, 4.0), true)"));

            // Parse：验证三层嵌套
            var result = (object[])handler.Parse("((1, 2), (3.0, 4.0), true) ");
            Assert.That(result.Length, Is.EqualTo(3));
            var inner1 = (object[])result[0];
            Assert.That((int)inner1[0], Is.EqualTo(1));
            Assert.That((int)inner1[1], Is.EqualTo(2));
            var inner2 = (object[])result[1];
            Assert.That((float)inner2[0], Is.EqualTo(3.0f).Within(1e-6f));
            Assert.That((float)inner2[1], Is.EqualTo(4.0f).Within(1e-6f));
            Assert.That(result[2], Is.EqualTo(true));

            // GetCandidates：输入 "((1, 2), " → 第二个元组的候选项（前缀为 "((1, 2), "）
            Assert.That(handler.GetCandidates("((1, 2), "), Contains.Item("((1, 2), ("));
            // 输入 "((1, 2), (3.0, " → 第二个元组的第二个参数候选项
            Assert.That(handler.GetCandidates("((1, 2), (3.0, "), Contains.Item("((1, 2), (3.0, 0"));
            Assert.That(handler.GetCandidates("((1, 2), (3.0, "), Contains.Item("((1, 2), (3.0, 0.0"));
            // 输入 "((1, 2), (3.0, 4.0), " → 第三个参数（bool）候选项
            Assert.That(handler.GetCandidates("((1, 2), (3.0, 4.0), "), Contains.Item("((1, 2), (3.0, 4.0), true"));
            Assert.That(handler.GetCandidates("((1, 2), (3.0, 4.0), "), Contains.Item("((1, 2), (3.0, 4.0), false"));
            // 输入已包含闭括号 → 仅返回输入自身
            Assert.That(handler.GetCandidates("((1, 2), (3.0, 4.0), true)"),
                Is.EquivalentTo(new[] { "((1, 2), (3.0, 4.0), true)" }));
            // 输入完全不成立（不以开括号开头）→ 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("deep"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Deep"));
        }
    }
}