## AI不要读这部分！

```




CommandLineAnalyzer 缓存分析数据
- ConsoleCommandDesc注释改成中文的
- 增强CommandLineAnalyzeResult，提供额外信息
- 缓存已经解析完毕的参数（ConsoleCommandDesc的结构需要修改，以后再看）










## 重构任务

现在进行一个重构任务：
（你不用考虑旧版兼容，现在是项目的未发布时期）

目前，ParameterHandlerRegistry对ParameterHandler的确认，只依赖两个参数：Type 和 string parameterName。

这远远不够。我还希望能通过 Attribute 来确认 ParameterHandler。

现在所有的HandlerOf重载，都需要添加一个额外的参数：Attribute[] attributes。（注明attributes是只读的）（attributes允许为空）
不会用Attribute作为最终的类型，应该使用一个抽象Attribute类，约定：读取时只读取这个类的子类。你需要给这个特性命名，它的语义是：给Parameter使用的、用于决定Parameterhandler选择的特性。
（之所以想出这个机制，是为了不与现有的ParamterAttribute矛盾，不然attributes几乎总是不为null了，会少很多优化空间）

### 架构设计

Soyo.SoyoRuntimeConsole.Helpers.ParameterHandlerRegistry.HandlerFactory 的签名不用改，
你给它的名称改了，现在它的语义是“不考虑attributes的handler工厂”。
以后传arributes为空或null的，就去调用这个类型的HandlerFactory。

DynamicHandlerFactory 改签名，加一个参数：Attribute[] attributes。

这样，Soyo.SoyoRuntimeConsole.Helpers.ParameterHandlerRegistry._factories 依旧可以存在。

你还需要把外部api改了，以适应现在的结构（主要是ConsoleBuilder的api，你要好好设计）


### 额外说明

1. 永久移除Soyo.SoyoRuntimeConsole.Helpers.ParameterHandlerRegistry.HandlerOf的ParameterInfo重载。