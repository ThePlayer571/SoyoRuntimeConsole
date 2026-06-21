# TASKS.md

✅ 请你为ConsoleViewModel添加测试 — 已完成 2026/06/21

## 技术

ConsoleViewModel是对IConsole的封装，它的注释里写的很清楚。

因此ConsoleViewModel是依赖IConsole，目前已经存在对ConsoleBase的测试，ConsoleViewModel需要借用那个测试里实现的Console（因为创建Console是比较繁琐的事，没必要单独再写）

你需要搞嵌套类，相当于用类封装这几个测试，最外层的类是partial的，分三个文件，一个文件写Console和相关Command的定义，一个文件是原本对ConsoleBase的测试，另一个文件是对ConsoleViewModel的测试。

## 测试内容

由你决定，你可以看到ConsoleBase测试的内容，ViewModel的测试也大同小异。

## 实现

已将 `ConsoleBaseTests` 重构为 `partial class`，拆分为三个文件：

1. **`ConsoleTestFixtures.cs`** — 测试用 Console 和 Command 定义（HelloCommand, HelloWithNameCommand, HelloWorldCommand, TestConsole, TestConsoleWithHelp）
2. **`ConsoleBaseTests.cs`** — 原有的 16 个 ConsoleBase 测试（不变），添加了 `_viewModel` 字段和 `TearDown`
3. **`ConsoleViewModelTests.cs`** — 新增 24 个 ConsoleViewModel 测试，覆盖：InputText/SetInputText（4）、SendInput（5）、GetHistory（4）、AutoComplete（5）、GetSuggestion（4）、OnLogEntry（1）、Constructor（1）

请在 Unity Editor 中运行 EditMode 测试验证。