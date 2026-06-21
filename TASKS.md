# TASKS.md

✅ 我打算新增一个参数ParameterHandler，是从n个字符串中选择一个填入。

这个和EnumPara非常像，可以说它是EnumPara的超集，使用场景是无需定义Enum就可以直接使用。

你需要参考EnumParameterHandler的实现，写这个新的ParameterHandler（名字还没想好，你来取）

构造函数是和EnumParameterHandler类似，不过传入一个IEnumerable<string>，而不是一个Enum类型。必须有至少一个字符串。

提供 params string[] 的重载构造函数，方便直接传入字符串列表。

→ 已完成：`StringOptionParameterHandler`（`Runtime/ParameterHandlers/StringOptionParameterHandler.cs`）+ 测试