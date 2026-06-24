## AI不要读这部分！

```

ViewModel
  - SendInput 支持Index。Index是存储在ViewModel里面的
  - AutoComplete，GetHistory提供api：GetAutoCompleteText, 恢复历史状态（int 向前移动多少次历史）。在函数注释提醒使用者：调用 AutoComplete / 恢复历史状态 会导致InputText被修改，如果你自定义UI，记得更新
  - public方法改成virtual的，方便用户自定义
  - 可选的，是否记录每条LogEntry

CommandLineAnalyzer 缓存分析数据

Builder示例

## 文档

规范：(记得写进README.md)
- 只有向量类型使用()构造，其余的构造更推荐使用{}
- 列表参数使用[]语法，只有列表和具有列表语义的参数才使用[]
- 类型使用帕斯卡，name使用小写驼峰