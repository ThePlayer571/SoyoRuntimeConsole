# TASKS.md

我打算做一个复合的ParameterHandler，叫做`CompositeParameterHandler`，它可以组合多个ParameterHandler来处理复杂的参数输入。

设计初衷是我希望提供给用户灵活的类构造方式，同时我自己不需要去写很多重复的代码来处理不同类型的参数输入(比如Vector3, Color等)

## 用户体验

参数输入是像这样的 (1, 2, true) 或者 {1.1, "hahaha"}。本质上是调用了Integer/Float/String/BoolParameterHandler来处理每个参数

我可以指定括号类型，包括三种: (), {}, []，用枚举指定。

参数强制使用逗号分隔。

特别注意：请看IntegerParameterHandler，你会发现参数包括了尾随的空格，因此当你解析每个子参数时，认为IsValid && 逗号结尾是ShouldAdvance的。

## 设计

`CompositeParameterHandler` 继承自 `ParameterHandlerBase`，实现 `IParameterHandler` 接口。你可以参考stringParameterHandler的实现。

CompositeParameterHandler需要传入一个IEnumerable<IParameterHandler>来指定它包含哪些ParameterHandler类型，还需要传入一个枚举值来指定括号类型。

用户可能会继承这个类通过构造函数来指定包含的ParameterHandler类型，因此为了方便，你需要建一个params IParameterHandler[]的构造函数。

你可能需要阅读IParameterHandler的注释来理解它的设计理念和使用方式。