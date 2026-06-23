using NUnit.Framework;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Helpers;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Soyo.SoyoRuntimeConsole.Tests.Editor
{
    /// <summary>
    /// <see cref="ParameterHandlerRegistry"/> 的单元测试。
    /// 覆盖内置类型获取、多工厂组合、动态类型构造、降级行为和 Freeze 功能。
    /// </summary>
    public class ParameterHandlerRegistryTests
    {
        private enum TestEnum
        {
            Red,
            Green,
            Blue
        }

        private ParameterHandlerRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _registry = new ParameterHandlerRegistry();
        }

        #region 内置类型

        [Test]
        public void Handler_StringType_ReturnsStringParameterHandler()
        {
            var handler = _registry.HandlerOf<string>("text");
            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_IntType_ReturnsIntegerParameterHandler()
        {
            var handler = _registry.HandlerOf<int>("count");
            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_FloatType_ReturnsFloatParameterHandler()
        {
            var handler = _registry.HandlerOf<float>("speed");
            Assert.That(handler, Is.InstanceOf<FloatParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_BoolType_ReturnsBooleanParameterHandler()
        {
            var handler = _registry.HandlerOf<bool>("flag");
            Assert.That(handler, Is.InstanceOf<BooleanParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Handler_Vector2Type_ReturnsVector2ParameterHandler()
        {
            var handler = _registry.HandlerOf<Vector2>("pos");
            Assert.That(handler, Is.InstanceOf<Vector2ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetDescription().Name, Is.EqualTo("pos"),
                "Vector2 handler should use the custom name passed via HandlerOf");
        }

        [Test]
        public void Handler_Vector2IntType_ReturnsVector2IntParameterHandler()
        {
            var handler = _registry.HandlerOf<Vector2Int>("pos");
            Assert.That(handler, Is.InstanceOf<Vector2IntParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetDescription().Name, Is.EqualTo("pos"),
                "Vector2Int handler should use the custom name passed via HandlerOf");
        }

        [Test]
        public void Handler_Vector3Type_ReturnsVector3ParameterHandler()
        {
            var handler = _registry.HandlerOf<Vector3>("pos");
            Assert.That(handler, Is.InstanceOf<Vector3ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetDescription().Name, Is.EqualTo("pos"),
                "Vector3 handler should use the custom name passed via HandlerOf");
        }

        [Test]
        public void Handler_Vector3IntType_ReturnsVector3IntParameterHandler()
        {
            var handler = _registry.HandlerOf<Vector3Int>("pos");
            Assert.That(handler, Is.InstanceOf<Vector3IntParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetDescription().Name, Is.EqualTo("pos"),
                "Vector3Int handler should use the custom name passed via HandlerOf");
        }

        [Test]
        public void Handler_Vector4Type_ReturnsVector4ParameterHandler()
        {
            var handler = _registry.HandlerOf<Vector4>("pos");
            Assert.That(handler, Is.InstanceOf<Vector4ParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
            Assert.That(handler.GetDescription().Name, Is.EqualTo("pos"),
                "Vector4 handler should use the custom name passed via HandlerOf");
        }

        #endregion

        #region 多工厂组合

        [Test]
        public void Register_MultipleFactoriesForSameType_ReturnsCompositeParameterHandler()
        {
            // 使用 System.Version（无内置工厂的类型）避免污染其他测试
            _registry.Register<System.Version>((type, name) =>
                new StringParameterHandler(name ?? "Version"));
            _registry.Register<System.Version>((type, name) =>
                new StringParameterHandler(name ?? "Version"));

            _registry.Freeze();
            var handler = _registry.HandlerOf<System.Version>("ver");

            // 有两个工厂，应返回 CompositeParameterHandler
            Assert.That(handler, Is.InstanceOf<CompositeParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Register_ConvenienceOverload_HandlerIsRegistered()
        {
            // 使用私有嵌套类型，确保没有其他代码意外注册同一类型
            var fixedHandler = new StringParameterHandler("test");
            _registry.Register<ConvenienceTestType>(fixedHandler);
            _registry.Freeze();

            var handler = _registry.HandlerOf<ConvenienceTestType>("test");

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
            var handler = _registry.HandlerOf<TestEnum>("color");
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
            var handler = _registry.HandlerOf(typeof(TestEnum), "mode");
            Assert.That(handler, Is.InstanceOf<EnumParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 动态类型 — 数组

        [Test]
        public void Handler_IntArrayType_DynamicallyCreatesArrayParameterHandler()
        {
            var handler = _registry.HandlerOf<int[]>("values");

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
            var handler = _registry.HandlerOf<float[]>("values");

            Assert.That(handler, Is.InstanceOf<ArrayParameterHandler<float>>());
            Assert.IsTrue(handler.IsInitialized);

            var result = handler.Parse("[1.5, 2.5]");
            Assert.That(result, Is.InstanceOf<float[]>());
            Assert.That((float[])result, Is.EqualTo(new[] { 1.5f, 2.5f }).Within(1e-6f));
        }

        [Test]
        public void Handler_StringArrayType_DynamicallyCreatesArrayParameterHandler()
        {
            var handler = _registry.HandlerOf<string[]>("names");

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
                "[ParameterHandlerRegistry] No handler registered for type 'System.DateTime'. " +
                "Falling back to StringParameterHandler.");

            var handler = _registry.HandlerOf<System.DateTime>("date");

            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        #endregion

        #region 边界情况

        [Test]
        public void Handler_NullName_UsesTypeNameAsDefault()
        {
            var handler = _registry.HandlerOf<int>(null);

            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void Freeze_CalledMultipleTimes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                _registry.Freeze();
                _registry.Freeze();
                _registry.Freeze();
            });
        }

        [Test]
        public void Register_AfterFreeze_IsIgnored()
        {
            _registry.Freeze();

            // 冻结后注册应被忽略（不抛异常，仅警告）
            LogAssert.Expect(LogType.Warning,
                "[ParameterHandlerRegistry] Cannot register factory for type 'System.Version' " +
                "after Freeze(). Ignoring.");

            _registry.Register<System.Version>((type, name) =>
                new StringParameterHandler(name ?? "Version"));

            // 降级行为应仍然返回 StringParameterHandler（未被注册）
            LogAssert.Expect(LogType.Warning,
                "[ParameterHandlerRegistry] No handler registered for type 'System.Version'. " +
                "Falling back to StringParameterHandler.");

            var handler = _registry.HandlerOf<System.Version>("ver");
            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
        }

        #endregion

        #region 动态处理器

        [Test]
        public void DynamicHandler_TypeMatches_ReturnsCustomHandler()
        {
            // 注册一个匹配 System.Version 的动态处理器
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new StringParameterHandler(name ?? "Version")
                    : null);

            var handler = _registry.HandlerOf<System.Version>("ver");

            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void DynamicHandler_TypeDoesNotMatch_FallsThrough()
        {
            // 注册一个只处理 int 的动态处理器（不处理 DateTime）
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(int)
                    ? new IntegerParameterHandler(name ?? "num")
                    : null);

            // DateTime 应降级
            LogAssert.Expect(LogType.Warning,
                "[ParameterHandlerRegistry] No handler registered for type 'System.DateTime'. " +
                "Falling back to StringParameterHandler.");

            var handler = _registry.HandlerOf<System.DateTime>("date");

            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
        }

        [Test]
        public void DynamicHandler_MultipleFactories_FirstNonNullWins()
        {
            // 注册两个动态处理器，都匹配 System.Version
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new IntegerParameterHandler(name ?? "ver")
                    : null);

            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new StringParameterHandler(name ?? "ver")
                    : null);

            // 第一个非 null 的应获胜 → IntegerParameterHandler
            var handler = _registry.HandlerOf<System.Version>("ver");

            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
        }

        [Test]
        public void DynamicHandler_GenericTypePattern_MatrixInt()
        {
            // 模拟 Matrix<T> 模式匹配：匹配任何封闭的 Matrix<T> 类型
            _registry.RegisterDynamicHandler((type, name) =>
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Matrix<>))
                {
                    var elementType = type.GetGenericArguments()[0];
                    if (elementType == typeof(int))
                    {
                        return new StringParameterHandler(name ?? "Matrix<int>");
                    }
                }
                return null;
            });

            // 构造 Matrix<int> 封闭泛型类型
            var matrixIntType = typeof(Matrix<>).MakeGenericType(typeof(int));

            var handler = _registry.HandlerOf(matrixIntType, "mat");

            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void DynamicHandler_AfterFreeze_IsIgnored()
        {
            _registry.Freeze();

            // 冻结后注册应被忽略
            LogAssert.Expect(LogType.Warning,
                "[ParameterHandlerRegistry] Cannot register dynamic handler after Freeze(). Ignoring.");

            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new StringParameterHandler(name ?? "Version")
                    : null);

            // 动态处理器未生效，Version 应降级
            LogAssert.Expect(LogType.Warning,
                "[ParameterHandlerRegistry] No handler registered for type 'System.Version'. " +
                "Falling back to StringParameterHandler.");

            var handler = _registry.HandlerOf<System.Version>("ver");
            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
        }

        [Test]
        public void DynamicHandler_EnumAndArrayStillWork()
        {
            // 注册一个广泛的动态处理器，不处理枚举/数组
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new StringParameterHandler(name ?? "Version")
                    : null);

            // 枚举仍然由内置逻辑处理，而非动态处理器
            var enumHandler = _registry.HandlerOf<TestEnum>("color");
            Assert.That(enumHandler, Is.InstanceOf<EnumParameterHandler>());

            // 数组仍然由内置逻辑处理
            var arrayHandler = _registry.HandlerOf<int[]>("values");
            Assert.That(arrayHandler, Is.InstanceOf<ArrayParameterHandler<int>>());
        }

        [Test]
        public void DynamicHandler_PrecedesFallback()
        {
            // 动态处理器处理 DateTime（没有内置注册）
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.DateTime)
                    ? new IntegerParameterHandler(name ?? "date")
                    : null);

            // 不应有降级警告（动态处理器捕获了它）
            var handler = _registry.HandlerOf<System.DateTime>("date");

            // 来自动态处理器，而非 StringParameterHandler 降级
            Assert.That(handler, Is.InstanceOf<IntegerParameterHandler>());
        }

        [Test]
        public void DynamicHandler_FrozenRegistry_ReadWorksAfterFreeze()
        {
            // 注册动态处理器
            _registry.RegisterDynamicHandler((type, name) =>
                type == typeof(System.Version)
                    ? new StringParameterHandler(name ?? "Version")
                    : null);

            _registry.Freeze();

            // 冻结后多次读取，验证无锁路径正常工作
            for (int i = 0; i < 10; i++)
            {
                var handler = _registry.HandlerOf<System.Version>("ver");
                Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
                Assert.IsTrue(handler.IsInitialized);
            }
        }

        #endregion

        #region [ConsoleParameterHandler] 扫描注册

        [Test]
        public void ScanType_ScansConsoleParameterHandler_HandlerIsRegistered()
        {
            // 显式扫描 ParameterHandlerTestFixture 中的 [ConsoleParameterHandler]
            ConsoleParameterHandlerScanner.ScanType(typeof(ParameterHandlerTestFixture), _registry);
            _registry.Freeze();

            var handler = _registry.HandlerOf<Point2D>("point");

            Assert.That(handler, Is.InstanceOf<TupleParameterHandler>());
            Assert.IsTrue(handler.IsInitialized);
        }

        [Test]
        public void ConsoleParameterHandler_Parse_ReturnsCorrectType()
        {
            ConsoleParameterHandlerScanner.ScanType(typeof(ParameterHandlerTestFixture), _registry);
            _registry.Freeze();

            var handler = _registry.HandlerOf<Point2D>("point");

            // Point2D 的 [ConsoleParameterHandler] 接受 (int x, int y)，返回 Point2D
            // 默认括号类型为花括号 {}
            Assert.IsTrue(handler.IsValid("{10, 20}"));

            var result = handler.Parse("{10, 20}");
            Assert.That(result, Is.InstanceOf<Point2D>());

            var point = (Point2D)result!;
            Assert.That(point.X, Is.EqualTo(10));
            Assert.That(point.Y, Is.EqualTo(20));
        }

        [Test]
        public void ConsoleParameterHandler_InvalidInput_ReturnsFalse()
        {
            ConsoleParameterHandlerScanner.ScanType(typeof(ParameterHandlerTestFixture), _registry);
            _registry.Freeze();

            var handler = _registry.HandlerOf<Point2D>("point");

            // 只有一个子参数时非法
            Assert.IsFalse(handler.IsValid("{10}"));
            // 三个子参数时非法
            Assert.IsFalse(handler.IsValid("{10, 20, 30}"));
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

    /// <summary>
    /// 用于测试动态处理器泛型模式匹配的模拟 Matrix&lt;T&gt; 类型。
    /// </summary>
    internal class Matrix<T>
    {
    }
}
