## AI不要读这部分！

```

目前的Sample太垃圾了

claude怎么运行unity的测试

找到文档更新工作流

规范：(记得写进README.md)
- 只有向量类型使用()构造，其余的构造更推荐使用{}
- 列表参数使用[]语法，只有列表和具有列表语义的参数才使用[]

列表参数

Analyzer还是有问题：hello_vec3 (0, 0) 最后带空格，会分析不通过，但应该是通过的
- 这个本质上是特殊情况处理了

检查CompositeParameterHandler的嵌套测试

ViewModel回溯功能

命令名没有补全