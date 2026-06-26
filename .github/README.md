# Soyo Runtime Console

[![Unity 2022.3+](https://img.shields.io/badge/unity-2022.3%2B-blue.svg)](https://unity3d.com/get-unity/download)

这是一个运行时控制台插件，用于运行时调试、给玩家作为控制台使用。

特点：

- 开箱即用
    - 使用 Attribute 定义 ConsoleCommand 和参数解析器
    - 完整的参数提示和自动补全
    - 拖入 Prefab 即可使用
    - 支持同时存在多个Console
- 可扩展
    - 支持自定义控制台UI，你只需与可读性极佳的 ConsoleViewModel 交互
    - 支持自定义参数解析器，解析任意参数
- 性能优化
    - 使用 ConsoleBuilder 限定 Attribute 扫描范围
    - 命令解析器使用缓存提高解析效率

## 安装

1. 通过 Unity Package Manager 安装：
   `https://github.com/ThePlayer571/SoyoRuntimeConsole.git?path=Packages/com.github.theplayer571.soyo-runtime-console`
2. 手动修改 manifest.json，添加：
   `"com.github.theplayer571.soyo-runtime-console": "https://github.com/ThePlayer571/SoyoRuntimeConsole.git?path=Packages/com.github.theplayer571.soyo-runtime-console"`

## 快速开始

1. 从 `Soyo Runtime Console/Runtime/Prefabs` 中拖入 `SimpleConsole.prefab` 到场景中。（需要额外创建EventSystem）
2. 新建类，定义 ConsoleCommand：

```csharp

public class ConsoleCommands
{
    [ConsoleCommand] 
    public static void hello_world()
    {
        Debug.Log("Hello World!");
    }
}
```

3. 运行游戏，在控制台UI中输入 `hello_world`，按回车执行命令。
4. 如果你想自定义参数解析器：

```csharp

public class MyClass
{
    public int Number;
    public string Name;
}

public class ParameterHandlers
{
    [ConsoleParameterHandler]
    public static MyClass ParseMyClass(int number, string name)
    {
        return new MyClass { Number = number, Name = name };
    }
}

public class ConsoleCommands
{
    [ConsoleCommand] 
    public static void hello_world()
    {
        Debug.Log("Hello World!");
    }
    
    [ConsoleCommand] 
    public static void log_my_class(MyClass myClass)
    {
        Debug.Log($"MyClass: Number={myClass.Number}, Name={myClass.Name}");
    }
}
```

5. 运行游戏，在控制台UI中输入 `log_my_class {7, "Words"}`，按回车执行命令。

6. 查看QuickStart示例，了解更多用法。（用法在注释中作了解释）

## 进阶话题

### ConsoleKey

如果想同时存在多个Console，可以使用 *ConsoleKey* 来区分不同的控制台。

在 ConsoleViewModel 的构造函数中，可以传入一个 string 作为 ConsoleKey。

使用 [TargetConsoleKey] 特性标记的 ConsoleCommand 仅被注册到对应 key 的 Console 中。

也可以使用 [TargetConsoleKey] 标记类，效果等价于标记类中所有的 ConsoleCommand。

当类和方法都标记了 [TargetConsoleKey] 时，方法的 ConsoleKey 优先级更高。

### 规范

- 命令命名:
    - 使用蛇形命名法：console_command_name
    - 只得包含大小写字母、数字和下划线，不得以数字开头
- 使用 [ConsoleCommand] 定义命令时:
    - 方法为静态公有
    - 比起显式指定命令名，更推荐使用方法名作为命令名
- 参数命名:
    - 使用小写驼峰命名法：parameterName
    - 参数类型使用帕斯卡命名法：ParameterType
- 使用 [ConsoleParameterHandler] 定义参数解析器时:
    - 方法为静态公有
    - 方法名可以任意命名
- 使用 [TargetConsoleKey] 时:
    - 优先标记类，特殊需求时才标记方法
    - ConsoleKey 使用帕斯卡命名法：ConsoleKeyName

### 自定义UI

TODO

### 继承 ParameterHandler 定义参数解析器

使用 [ConsoleParameterHandler] 特性定义的参数解析器，本质是封装了一个 TupleParameterHandler。如果你希望获得更好的自定义性，可以继承
ParameterHandlerBase 来定义参数解析器。

你可以查看 `Runtime/ParameterHandlers` 源码获得参考。推荐的工作流是：当你想定义一个参数解析器时，先思考有没有类似功能的解析器已经存在，如果有，参考它的源码进行实现。

### 使用ConsoleBuilder

TODO

ConsoleBuilder 设计是为了提高性能和可扩展性。

使用 `new ConsoleViewModel()` 和 `Console.Create()` 创建 Console 时会遍历所有程序集，造成性能损耗。

可以使用 ConsoleBuilder 来限定扫描范围：

```csharp

public ConsoleViewModel CreateConsole()
{
    return new ConsoleBuilder()
    
}
