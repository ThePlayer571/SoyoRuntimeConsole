# TASKS.md

我打算进行一个重构，涉及文件：
- ConsoleBuilder
- ConsoleAttributeScanner
- ConsoleParameterHandlerScanner
- PreferredParameterHandler

## 痛点

这是目前的痛点：
1. PreferredParameterHandler性能问题
  - PreferredParameterHandler设计上是全局的，因此初次使用会强制扫描所有程序集，导致性能问题
  - 目前的设计让ConsoleBuilder没办法对它进行优化


## 方案

1. 将PreferredParameterHandler改为非全局的，改成一个可实例化的类（不是单例），ConsoleBuilder在构建时创建一个实例。
  - 允许ConsoleBuilder按程序集/类型扫描参数。（api模仿ConsoleCommand的注册即可）
    - 注意，这时候的扫描，指定扫描一个类时，会包括类的ConsoleCommand+ParameterHandler，也就是不单独分出一个api来
    - 注意，你每次扫描，无法立即创建ConsoleCommand，因为ParameterHandler还没有扫描完。
  - 此外，我在PreferredParameterHandler里留了一些todo注释，也是你需要做的
2. 扫描出的结果不再缓存到全局。（逻辑是：既然使用Builder了，扫描一个程序集/类，不可能缺这点性能）