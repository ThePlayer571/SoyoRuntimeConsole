## AI不要读这部分！

```
目前的Sample太垃圾了
- 包括：
- 基础的greet string/ string int count / string cute bool
- 自定义类，嵌套元组，组合器

EnumPara支持不使用Enum，直接传入n个string
- 要提醒ai这个和fixedString不一样，因为fixedString做了额外的处理，你需要自己新写一个string（也就是不能重构FixedString让FixedString支持这个需求）

## 询问

找到文档更新工作流

## 文档

规范：(记得写进README.md)
- 只有向量类型使用()构造，其余的构造更推荐使用{}
- 列表参数使用[]语法，只有列表和具有列表语义的参数才使用[]