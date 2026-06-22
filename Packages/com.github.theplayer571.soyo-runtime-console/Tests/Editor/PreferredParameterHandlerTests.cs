using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="PreferredParameterHandler"/> 的单元测试。
    /// 覆盖内置类型获取、多工厂组合、动态类型构造和降级行为。
    /// </summary>
    public class PreferredParameterHandlerTests
    {
        private enum TestEnum
        {
            Red,
            Green,
            Blue
        }

        #region 内置类型

        [Test]
        public void Handler_StringType_ReturnsStringParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<string>("text");
            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_IntType_ReturnsIntegerParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<int>("count");
            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_FloatType_ReturnsFloatParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<float>("speed");
            Assert.That(handler, Is.InstanceOf<FloatParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_BoolType_ReturnsBooleanParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<bool>("flag");
            Assert.That(handler, Is.InstanceOf<BooleanParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector2Type_ReturnsVector2ParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<Vector2>("pos");
            Assert.That(handler, Is.InstanceOf<Vector2ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector2IntType_ReturnsVector2IntParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<Vector2Int>("pos");
            Assert.That(handler, Is.InstanceOf<Vector2IntParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector3Type_ReturnsVector3ParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<Vector3>("pos");
            Assert.That(handler, Is.InstanceOf<Vector3ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector3IntType_ReturnsVector3IntParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<Vector3Int>("pos");
            Assert.That(handler, Is.InstanceOf<Vector3IntParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector4Type_ReturnsVector4ParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<Vector4>("pos");
            Assert.That(handler, Is.InstanceOf<Vector4ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 多工厂组合

        [Test]
        public void Register_MultipleFactoriesForSameType_ReturnsCompositeParameterHandler()
        {
            // 使用 System.Version（无内置工厂的类型）避免污染其他测试
            PreferredParameterHandler.Register<System.Version>((type, name) =>
                new StringParameterHandler(name ?? "Version"));
            PreferredParameterHandler.Register<System.Version>((type, name) =>
                new StringParameterHandler(name ?? "Version"));

            var handler = PreferredParameterHandler.Handler<System.Version>("ver");

            // 有两个工厂，应返回 CompositeParameterHandler
            Assert.That(handler, Is.InstanceOf<CompositeParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Register_ConvenienceOverload_HandlerIsRegistered()
        {
            // 使用私有嵌套类型，确保没有其他代码意外注册同一类型
            var fixedHandler = new StringParameterHandler("test");
            PreferredParameterHandler.Register<ConvenienceTestType>(fixedHandler);

            var handler = PreferredParameterHandler.Handler<ConvenienceTestType>("test");

            // 只有一个工厂，返回工厂结果（即 fixedHandler 本身）
            Assert.That(handler, Is.SameAs(fixedHandler));
            Assert.IsTrue(handler.IsInitialized);
        }

        /// <summary>
        /// 仅供 <see cref="Register_ConvenienceOverload_HandlerIsRegistered"/> 使用的隔离类型。
        /// </summary>
        private class ConvenienceTestType
        {
        }

        #endregion

        #region 动态类型 — 枚举

        [Test]
        public void Handler_EnumType_DynamicallyCreatesEnumParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<TestEnum>("color");
            Assert.That(handler, Is.InstanceOf<EnumParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);

            // 验证枚举解析功能
            Assert.IsTrue(handler.IsValid("Red"));
            Assert.IsTrue(handler.IsValid("Green"));
            Assert.IsTrue(handler.IsValid("Blue"));
            Assert.IsFalse(handler.IsValid("Yellow"));

            Assert.That(handler.Parse("Red"), Is.EqualTo(TestEnum.Red));
        }

        [Test]
        public void Handler_EnumTypeViaTypeParameter_DynamicallyCreatesHandler()
        {
            var handler = PreferredParameterHandler.Handler(typeof(TestEnum), "mode");
            Assert.That(handler, Is.InstanceOf<EnumParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 动态类型 — 数组

        [Test]
        public void Handler_IntArrayType_DynamicallyCreatesArrayParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<int[]>("values");

            Assert.That(handler, Is.InstanceOf<ArrayParameterHandler<int>>());
            Assert.IsTrue(handler.IsInitialized);

            // 验证数组解析功能
            Assert.IsTrue(handler.IsValid("[1, 2, 3]"));
            Assert.IsTrue(handler.IsValid("[]"));

            var result = handler.Parse("[1, 2, 3]");
            Assert.That(result, Is.InstanceOf<int[]>());
            Assert.That((int[])result, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Handler_FloatArrayType_DynamicallyCreatesArrayParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<float[]>("values");

            Assert.That(handler, Is.InstanceOf<ArrayParameterHandler<float>>());
            Assert.IsTrue(handler.IsInitialized);

            var result = handler.Parse("[1.5, 2.5]");
            Assert.That(result, Is.InstanceOf<float[]>());
            Assert.That((float[])result, Is.EqualTo(new[] { 1.5f, 2.5f }).Within(1e-6f));
        }

        [Test]
        public void Handler_StringArrayType_DynamicallyCreatesArrayParameterHandler()
        {
            var handler = PreferredParameterHandler.Handler<string[]>("names");

            Assert.That(handler, Is.InstanceOf<ArrayParameterHandler<string>>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 降级行为

        [Test]
        public void Handler_UnregisteredType_FallsBackToStringParameterHandler()
        {
            // System.DateTime 没有注册的处理器
            LogAssert.Expect(LogType.Warning,
                "[PreferredParameterHandler] No preferred handler registered for type 'System.DateTime'. " +
                "Falling back to StringParameterHandler.");

            var handler = PreferredParameterHandler.Handler<System.DateTime>("date");

            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 边界情况

        [Test]
        public void Handler_NullName_UsesTypeNameAsDefault()
        {
            var handler = PreferredParameterHandler.Handler<int>(null);

            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Initialize_CalledMultipleTimes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                PreferredParameterHandler.Initialize();
                PreferredParameterHandler.Initialize();
                PreferredParameterHandler.Initialize();
            });
        }

        #endregion

        #region [ConsoleParameterHandler] 扫描注册

        [Test]
        public void Initialize_ScansConsoleParameterHandler_HandlerIsRegistered()
        {
            // 触发懒加载扫描（如果尚未初始化）
            PreferredParameterHandler.Initialize();

            var handler = PreferredParameterHandler.Handler<Point2D>("point");

            Assert.That(handler, Is.InstanceOf<TupleParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void ConsoleParameterHandler_Parse_ReturnsCorrectType()
        {
            PreferredParameterHandler.Initialize();

            var handler = PreferredParameterHandler.Handler<Point2D>("point");

            // Point2D 的 [ConsoleParameterHandler] 接受 (int x, int y)，返回 Point2D
            Assert.IsTrue(handler.IsValid("(10, 20)"));

            var result = handler.Parse("(10, 20)");
            Assert.That(result, Is.InstanceOf<Point2D>());

            var point = (Point2D)result!;
            Assert.That(point.X, Is.EqualTo(10));
            Assert.That(point.Y, Is.EqualTo(20));
        }

        [Test]
        public void ConsoleParameterHandler_InvalidInput_ReturnsFalse()
        {
            PreferredParameterHandler.Initialize();

            var handler = PreferredParameterHandler.Handler<Point2D>("point");

            // 只有一个子参数时非法
            Assert.IsFalse(handler.IsValid("(10)"));
            // 三个子参数时非法
            Assert.IsFalse(handler.IsValid("(10, 20, 30)"));
        }

        #endregion
    }

    /// <summary>
    /// 用于测试 [ConsoleParameterHandler] 的简单返回类型。
    /// </summary>
    public struct Point2D
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// 用于测试 [ConsoleParameterHandler] 扫描的 Fixture。
    /// </summary>
    internal static class ParameterHandlerTestFixture
    {
        [ConsoleParameterHandler]
        private static Point2D MakePoint2D(int x, int y)
        {
            return new Point2D { X = x, Y = y };
        }
    }
}
