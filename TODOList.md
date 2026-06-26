## AI不要读这部分！

```
CommandLineAnalyzer 缓存分析数据
- ConsoleCommandDesc注释改成中文的
- 增强CommandLineAnalyzeResult，提供额外信息
- 缓存已经解析完毕的参数（ConsoleCommandDesc的结构需要修改，以后再看）

## 文档

规范：(记得写进README.md)
- 只有向量类型使用()构造，其余的构造更推荐使用{}
- 列表参数使用[]语法，只有列表和具有列表语义的参数才使用[]
- 类型使用帕斯卡，name使用小写驼峰