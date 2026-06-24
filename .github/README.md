# Soyo Runtime Console

[![Unity 2022.3+](https://img.shields.io/badge/unity-2022.3%2B-blue.svg)](https://unity3d.com/get-unity/download)

这是一个运行时控制台插件，用于运行时调试、作为玩家控制台使用。

特点：

- 开箱即用
    - 使用 Attribute 定义 ConsoleCommand 和参数解析器
    - 完整的参数提示和自动补全
    - 拖入 Prefab 即可使用
    - 支持同时存在多个Console
- 可扩展
    - 自定义控制台UI，你只需与可读性极佳的 ConsoleViewModel 交互
    - 自定义参数解析器，解析任意参数
- 性能优化
    - 使用 ConsoleBuilder 限定 Attribute 扫描范围
    - 命令解析器使用缓存提高解析效率

## 安装

1. 通过 Unity Package Manager 安装：
   `https://github.com/ThePlayer571/SoyoRuntimeConsole.git?path=Packages/com.github.theplayer571.soyo-runtime-console`
2. 手动修改 manifest.json，添加：
   `"com.github.theplayer571.soyo-runtime-console": "https://github.com/ThePlayer571/SoyoRuntimeConsole.git?path=Packages/com.github.theplayer571.soyo-runtime-console"`

## 快速开始

1. 从 `com.github.theplayer571.soyo-runtime-console/Prefabs` 中拖入 `SoyoRuntimeConsole.prefab` 到场景中。
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

## 进阶话题

### ConsoleKey

### 规范

### 自定义UI

### 通过继承类定义 ConsoleCommand 和 ParameterHandler

### 使用ConsoleBuilder

