## AI不要读这部分！

```




CommandLineAnalyzer 缓存分析数据
- ConsoleCommandDesc注释改成中文的
- 增强CommandLineAnalyzeResult，提供额外信息
- 缓存已经解析完毕的参数（ConsoleCommandDesc的结构需要修改，以后再看）



## 新需求

现在使用 [ConsoleCommand] 定义的命令不允许使用默认参数，我希望它可以支持。

默认参数的逻辑非常简单，比如 some_command(int a, int b = 1, int c = 2)，你就自动生成三个命令：

- some_command(int a) // 认为 b = 1, c = 2
- some_command(int a, int b) // 认为 c = 2
- some_command(int a, int b, int c)

本质就是多生成几个的事