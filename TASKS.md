# TASKS.md

## ✅ 已完成

~~添加一个ParameterHandler，叫做数组参数，按这种语法传入[0, 0, 0]，支持若干个参数（支持0个参数）。~~

**实现：** `ArrayParameterHandler` 继承自 `TupleParameterHandler`，通过 `IsVariableLength` 虚属性支持变长数组。
- 文件：`Runtime/ParameterHandlers/ArrayParameterHandler.cs`
- 测试：`Tests/Editor/ParameterHandlerArrayTests.cs`
- 修改：`TupleParameterHandler.cs` 增加 `IsVariableLength` 虚属性及 16 处兼容性修改