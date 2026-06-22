## AI不要读这部分！

```

参数handler可以指定括号类型

Vector系列可以命名

Color 和其他基本类型的handler

PreferredParameterHandler注册逻辑，以及

string隐式转换CommandName，检查修改已有的api并修改

## 询问

找到文档更新工作流

## 文档

规范：(记得写进README.md)
- 只有向量类型使用()构造，其余的构造更推荐使用{}
- 列表参数使用[]语法，只有列表和具有列表语义的参数才使用[]
- 类型使用帕斯卡，name使用小写驼峰


```

```










还存在一些不足
本项目的规范是：公有成员必须使用[DisallowNull] / [AllowNull] / [NotNull] / [MaybeNull] 来处理空引用，不使用可空引用。你的代码中存在一些没有使用这些特性的成员
    请你把这个规范写入CLAUDE.md，然后修改你刚刚写的代码





