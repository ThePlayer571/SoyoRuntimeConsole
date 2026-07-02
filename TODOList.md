## AI不要读这部分！

```




CommandLineAnalyzer 缓存分析数据
- ConsoleCommandDesc注释改成中文的
- 增强CommandLineAnalyzeResult，提供额外信息
- 缓存已经解析完毕的参数（ConsoleCommandDesc的结构需要修改，以后再看）










## 新需求

目前，通过 Attribute 标记函数定义ConsoleCommand的方式不支持 fixedField 参数类型（Soyo.SoyoRuntimeConsole.ParameterHandlers.FixedFieldParameterHandler）

我希望支持。

方式是：用户标记函数参数 [FixedField]，这个个参数必须是object。

## 细节

1. 在 FixedFieldAttribute 的注释标明：这个参数始终返回 null
2.FixedField 提供两个构造函数，FixedFieldAttribute(string fixedField) 和 无参。无参的话，使用函数参数名作为fixedField。









todo
用户能够参考Attribute决定handler（重构DynamicHandler机制）