using System.Linq;
using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    public class ParameterHandlerArrayTests
    {
        [Test]
        public void ArrayParameterHandler_Basic()
        {
            // Integer 元素，方括号包裹
            var handler = new ArrayParameterHandler("values", new IntegerParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("[1, 2, 3]"));
            Assert.IsTrue(handler.IsValid("[1, 2, 3] "));
            Assert.IsTrue(handler.IsValid("[42]"));
            Assert.IsTrue(handler.IsValid("[]"));
            // 数量不限制 → 任意数量均合法
            Assert.IsTrue(handler.IsValid("[1, 2, 3, 4, 5]"));
            // 子参数不合法
            Assert.IsFalse(handler.IsValid("[1, abc, 3]"));
            Assert.IsFalse(handler.IsValid("[1.5]")); // int 不接受浮点

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[1, 2, 3]"));
            Assert.IsTrue(handler.ShouldAdvance("[1, 2, 3] "));
            Assert.IsFalse(handler.ShouldAdvance("[1, 2, 3"));
            Assert.IsTrue(handler.ShouldAdvance("[1, 2, 3]  "));

            // Parse
            var result = (object[])handler.Parse("[1, 2, 3] ");
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That((int)result[0], Is.EqualTo(1));
            Assert.That((int)result[1], Is.EqualTo(2));
            Assert.That((int)result[2], Is.EqualTo(3));

            // Parse 尾随空格版本
            var result2 = (object[])handler.Parse("[0, 42, 100] ");
            Assert.That(result2.Length, Is.EqualTo(3));
            Assert.That((int)result2[0], Is.EqualTo(0));
            Assert.That((int)result2[1], Is.EqualTo(42));
            Assert.That((int)result2[2], Is.EqualTo(100));

            // GetCandidates：空输入 → 给出开括号提示 + 一键填充完整结果
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[0]" }));
            // 输入 "[" → 第一个参数候选项带前缀
            Assert.That(handler.GetCandidates("["), Contains.Item("[0"));
            // 输入 "[1," → 第二个参数候选项，前缀规范化后为 "[1, "
            Assert.That(handler.GetCandidates("[1,"), Contains.Item("[1, 0"));
            // 输入 "[1, 2," → 第三个参数候选项，前缀为 "[1, 2, "；末尾附带完整填充结果
            Assert.That(handler.GetCandidates("[1, 2,"),
                Is.EquivalentTo(new[] { "[1, 2, 0", "[1, 2, 0]" }));
            // 输入 "[1, 2, 3" → 通过完整填充结果附带闭括号
            Assert.That(handler.GetCandidates("[1, 2, 3"), Contains.Item("[1, 2, 3]"));
            // 输入已包含闭括号 → 用户已表明完结意图，仅返回输入自身
            Assert.That(handler.GetCandidates("[1, 2, 3]"),
                Is.EquivalentTo(new[] { "[1, 2, 3]" }));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);
            Assert.That(handler.GetCandidates("abc"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("values"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Integer[]"));
        }

        [Test]
        public void ArrayParameterHandler_EmptyArray()
        {
            var handler = new ArrayParameterHandler("empty", new IntegerParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid：空方括号
            Assert.IsTrue(handler.IsValid("[]"));
            Assert.IsTrue(handler.IsValid("[] "));
            Assert.IsFalse(handler.IsValid("[ ] ")); // 空格 → 内部不为空，但空格对 int 无效
            Assert.IsFalse(handler.IsValid("[")); // 无闭括号

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[]"));
            Assert.IsTrue(handler.ShouldAdvance("[] "));

            // Parse
            var result = (object[])handler.Parse("[] ");
            Assert.That(result.Length, Is.EqualTo(0));

            // GetCandidates：空输入
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[0]" }));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("empty"));
        }

        [Test]
        public void ArrayParameterHandler_SingleElement()
        {
            var handler = new ArrayParameterHandler("single", new IntegerParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("[42]"));
            Assert.IsTrue(handler.IsValid("[42] "));
            Assert.IsTrue(handler.IsValid("[0]"));
            Assert.IsFalse(handler.IsValid("[abc]"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[42]"));
            Assert.IsTrue(handler.ShouldAdvance("[42] "));

            // Parse
            var result = (object[])handler.Parse("[42] ");
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((int)result[0], Is.EqualTo(42));

            // GetCandidates
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[0]" }));
            Assert.That(handler.GetCandidates("["), Contains.Item("[0"));
            Assert.That(handler.GetCandidates("[4"), Contains.Item("[4]"));
            // 输入已包含闭括号 → 仅返回输入自身
            Assert.That(handler.GetCandidates("[42]"),
                Is.EquivalentTo(new[] { "[42]" }));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);
        }

        [Test]
        public void ArrayParameterHandler_VariableLength()
        {
            // 验证变长特性：任意数量元素均合法
            var handler = new ArrayParameterHandler("var", new IntegerParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // 0 个元素
            Assert.IsTrue(handler.IsValid("[]"));
            // 1 个元素
            Assert.IsTrue(handler.IsValid("[1]"));
            // 3 个元素
            Assert.IsTrue(handler.IsValid("[1, 2, 3]"));
            // 5 个元素
            Assert.IsTrue(handler.IsValid("[1, 2, 3, 4, 5]"));
            // 10 个元素
            Assert.IsTrue(handler.IsValid("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10]"));

            // Parse 变长
            var result5 = (object[])handler.Parse("[1, 2, 3, 4, 5] ");
            Assert.That(result5.Length, Is.EqualTo(5));
            for (var i = 0; i < 5; i++)
            {
                Assert.That((int)result5[i], Is.EqualTo(i + 1));
            }

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[1, 2, 3, 4, 5]"));
            Assert.IsTrue(handler.ShouldAdvance("[1, 2, 3, 4, 5] "));

            // GetCandidates：可继续添加更多元素（无上限）
            Assert.That(handler.GetCandidates("[1, 2, 3,"), Contains.Item("[1, 2, 3, 0"));
            Assert.That(handler.GetCandidates("[1, 2, 3,"), Contains.Item("[1, 2, 3, 0]"));
        }

        [Test]
        public void ArrayParameterHandler_FloatElements()
        {
            var handler = new ArrayParameterHandler("floats", new FloatParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("[1.5, 2.0, 3.14]"));
            Assert.IsTrue(handler.IsValid("[1.5]"));
            Assert.IsTrue(handler.IsValid("[]"));
            Assert.IsFalse(handler.IsValid("[1.5, abc]")); // abc 不是 float

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[1.5, 2.0]"));
            Assert.IsTrue(handler.ShouldAdvance("[1.5, 2.0] "));

            // Parse
            var result = (object[])handler.Parse("[1.5, 2.0, 3.14] ");
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That((float)result[0], Is.EqualTo(1.5f).Within(1e-6f));
            Assert.That((float)result[1], Is.EqualTo(2.0f).Within(1e-6f));
            Assert.That((float)result[2], Is.EqualTo(3.14f).Within(1e-6f));

            // GetCandidates：空输入
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[0]" }));
            Assert.That(handler.GetCandidates("["), Contains.Item("[0"));
            Assert.That(handler.GetCandidates("["), Contains.Item("[0.0"));
            Assert.That(handler.GetCandidates("[1.5,"), Contains.Item("[1.5, 0"));
            Assert.That(handler.GetCandidates("[1.5,"), Contains.Item("[1.5, 0.0"));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("floats"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Float[]"));
        }

        [Test]
        public void ArrayParameterHandler_BoolElements()
        {
            var handler = new ArrayParameterHandler("flags", new BooleanParameterHandler("flag"));

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("[true, false, true]"));
            Assert.IsTrue(handler.IsValid("[true]"));
            Assert.IsTrue(handler.IsValid("[]"));
            Assert.IsFalse(handler.IsValid("[true, True]")); // 大写 True 不合法

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[true, false]"));
            Assert.IsTrue(handler.ShouldAdvance("[true, false] "));

            // Parse
            var result = (object[])handler.Parse("[true, false, true] ");
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(true));
            Assert.That(result[1], Is.EqualTo(false));
            Assert.That(result[2], Is.EqualTo(true));

            // GetCandidates：空输入 → 给出开括号提示 + 一键填充完整结果
            Assert.That(handler.GetCandidates(string.Empty),
                Is.EquivalentTo(new[] { "[", "[true]" }));
            // 输入 "[" → 布尔候选项带前缀
            Assert.That(handler.GetCandidates("["), Contains.Item("[true"));
            Assert.That(handler.GetCandidates("["), Contains.Item("[false"));
            // 输入 "[true," → 第二个参数候选项
            Assert.That(handler.GetCandidates("[true,"), Contains.Item("[true, true"));
            Assert.That(handler.GetCandidates("[true,"), Contains.Item("[true, false"));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("flags"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Boolean[]"));
        }

        [Test]
        public void ArrayParameterHandler_NestedArrays()
        {
            // 数组的数组：[[1, 2], [3, 4]]
            var innerHandler = new ArrayParameterHandler("inner", new IntegerParameterHandler("value"));
            var handler = new ArrayParameterHandler("nested", innerHandler);

            Assert.IsTrue(handler.IsInitialized);

            // IsValid
            Assert.IsTrue(handler.IsValid("[[1, 2], [3, 4]]"));
            Assert.IsTrue(handler.IsValid("[[1, 2], [3, 4]] "));
            Assert.IsTrue(handler.IsValid("[[1]]"));
            Assert.IsTrue(handler.IsValid("[]")); // 外层空数组
            // 内层数组不合法
            Assert.IsFalse(handler.IsValid("[[1, abc], [3, 4]]"));
            // 括号类型不匹配（内层用方括号正确，但外层不能混用圆括号）
            Assert.IsFalse(handler.IsValid("([1, 2], [3, 4])"));

            // ShouldAdvance
            Assert.IsFalse(handler.ShouldAdvance("[[1, 2], [3, 4]]"));
            Assert.IsTrue(handler.ShouldAdvance("[[1, 2], [3, 4]] "));

            // Parse
            var result = (object[])handler.Parse("[[1, 2], [3, 4]] ");
            Assert.That(result.Length, Is.EqualTo(2));
            var inner1 = (object[])result[0];
            Assert.That(inner1.Length, Is.EqualTo(2));
            Assert.That((int)inner1[0], Is.EqualTo(1));
            Assert.That((int)inner1[1], Is.EqualTo(2));
            var inner2 = (object[])result[1];
            Assert.That(inner2.Length, Is.EqualTo(2));
            Assert.That((int)inner2[0], Is.EqualTo(3));
            Assert.That((int)inner2[1], Is.EqualTo(4));

            // GetCandidates：空输入
            var candidates = handler.GetCandidates(string.Empty).ToList();
            Assert.That(candidates, Contains.Item("["));
            // 完整填充：内层数组默认值包装在外层数组中
            Assert.That(candidates, Has.Some.EqualTo("[[0]]"));
            // 输入 "[" → 内层数组候选项带前缀
            Assert.That(handler.GetCandidates("["), Contains.Item("[["));
            // 输入 "[[1, 2], " → 第二个内层数组候选项
            Assert.That(handler.GetCandidates("[[1, 2], "), Contains.Item("[[1, 2], ["));
            // 输入 "[[1, 2], [3, 4" → 附带闭括号
            Assert.That(handler.GetCandidates("[[1, 2], [3, 4"),
                Contains.Item("[[1, 2], [3, 4]]"));
            // 输入不以方括号开头 → 无候选项
            Assert.That(handler.GetCandidates("1"), Is.Empty);

            // GetDescription
            Assert.That(handler.GetDescription().Name, Is.EqualTo("nested"));
            Assert.That(handler.GetDescription().Type, Is.EqualTo("Integer[][]"));
        }

        [Test]
        public void ArrayParameterHandler_InvalidInputs()
        {
            var handler = new ArrayParameterHandler("values", new IntegerParameterHandler("value"));

            Assert.IsTrue(handler.IsInitialized);

            // 无括号
            Assert.IsFalse(handler.IsValid("1, 2, 3"));
            Assert.IsFalse(handler.IsValid("abc"));

            // 括号不匹配
            Assert.IsFalse(handler.IsValid("[1, 2, 3")); // 无闭括号
            Assert.IsFalse(handler.IsValid("1, 2, 3]")); // 无开括号
            Assert.IsFalse(handler.IsValid("(1, 2, 3)")); // 括号类型错误
            Assert.IsFalse(handler.IsValid("{1, 2, 3}")); // 括号类型错误

            // 尾随逗号（空元素不合法）
            Assert.IsFalse(handler.IsValid("[1, 2,]"));

            // 前导逗号
            Assert.IsFalse(handler.IsValid("[, 1, 2]"));

            // 空字符串
            Assert.IsFalse(handler.IsValid(""));
            Assert.IsFalse(handler.IsValid(" "));

            // null（ShouldAdvance 返回 false）
            Assert.IsFalse(handler.ShouldAdvance(null));
            Assert.IsFalse(handler.ShouldAdvance(string.Empty));
        }

        [Test]
        public void ArrayParameterHandler_GetDescription()
        {
            // 验证 Name 和 Type 自动生成正确
            var intArray = new ArrayParameterHandler("ints", new IntegerParameterHandler("x"));
            Assert.That(intArray.GetDescription().Name, Is.EqualTo("ints"));
            Assert.That(intArray.GetDescription().Type, Is.EqualTo("Integer[]"));

            var floatArray = new ArrayParameterHandler("fs", new FloatParameterHandler("x"));
            Assert.That(floatArray.GetDescription().Name, Is.EqualTo("fs"));
            Assert.That(floatArray.GetDescription().Type, Is.EqualTo("Float[]"));

            var boolArray = new ArrayParameterHandler("bs", new BooleanParameterHandler("x"));
            Assert.That(boolArray.GetDescription().Name, Is.EqualTo("bs"));
            Assert.That(boolArray.GetDescription().Type, Is.EqualTo("Boolean[]"));

            // Enum 类型
            var enumArray = new ArrayParameterHandler("es",
                new EnumParameterHandler<UnityEngine.KeyCode>("key"));
            Assert.That(enumArray.GetDescription().Name, Is.EqualTo("es"));
            Assert.That(enumArray.GetDescription().Type, Is.EqualTo("KeyCode[]"));

            // name 为 null
            var nullNameArray = new ArrayParameterHandler(null, new IntegerParameterHandler("x"));
            Assert.That(nullNameArray.GetDescription().Name, Is.Null);
            Assert.That(nullNameArray.GetDescription().Type, Is.EqualTo("Integer[]"));
        }

        [Test]
        public void ArrayParameterHandler_ShouldAdvanceScenarios()
        {
            var handler = new ArrayParameterHandler("values", new IntegerParameterHandler("value"));

            // 1. 完整数组 + 尾部空格 → advance
            Assert.IsTrue(handler.ShouldAdvance("[42] "));

            // 2. 完整数组无尾部空格 → 不 advance
            Assert.IsFalse(handler.ShouldAdvance("[42]"));

            // 3. 不完整数组（正在输入）→ 不 advance
            Assert.IsFalse(handler.ShouldAdvance("[1, "));

            // 4. 空数组 + 尾部空格 → advance
            Assert.IsTrue(handler.ShouldAdvance("[] "));

            // 5. 空数组无尾部空格 → 不 advance
            Assert.IsFalse(handler.ShouldAdvance("[]"));

            // 6. 不以开括号开头 → 按空格分隔 advance
            Assert.IsFalse(handler.ShouldAdvance("42"));
            Assert.IsTrue(handler.ShouldAdvance("42 "));

            // 7. 空或 null → 不 advance
            Assert.IsFalse(handler.ShouldAdvance(string.Empty));
            Assert.IsFalse(handler.ShouldAdvance(null));
        }
    }
}
