# TASKS.md

接下来是个很大的任务：我打算让这个插件支持Attribute快速定义，包括定义 ParameterHandler 和 ConsoleCommand

## 需求分析

目前每个ParameterHandler和ConsoleCommand都需要手动编写一个类来实现接口，这太繁琐了，我希望能够通过Attribute来快速定义这些Handler和Command

预期是：正常使用情况下，只使用Attribute定义，也能实现所有需求。如果追求性能或者更强大的自定义，也可以选择手动编写类来实现接口

## 设计方案

我将按照理解顺序给你陈述方案：

### 概述

使用概述：
1. 使用[ConsoleCommand]标记方法，定义一个ConsoleCommand（这个方法必须是BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly）
2. 使用[ConsoleParameterHandler]标记方法，定义一个ParameterHandler（这个方法对应了Parse的功能，必须是BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly）
3. 使用[TargetConsoleKey]标记类或方法，作用是提供Soyo.SoyoRuntimeConsole.ConsoleKey信息，传入一个ConsoleKey。这个是为了以后做准备

技术概述：
1. 需要新增：类型首选ParameterHandler的机制，用于解析[ConsoleCommand]标记的方法的参数类型的首选ParameterHandler
  - 我设想是，有一个全局的单例，提供一个IParameterHandler (type, string name)方法，可以往里面注册handler（IMPORTANT: 准确来说是注册handler的工厂）。（需要提供泛型支持）
  - 每个类型唯一对应一个首选ParameterHandler，如果注册了多个，会自动使用CompositeParameterHandler来组合它们。
  - 只提供注册，不提供注销。
  - 支持传入逻辑，提供参数信息（type + string name），返回ParameterHandler。这是为了解决例如数组解析的问题，没有这个机制每个数组类型都需要单独注册一个ParameterHandler。
  - 提供获取的方法，大概像.Handler<T>()这样，获取首选ParameterHandler。
  - 这个是懒加载的，会扫描所有程序集的[ConsoleParameterHandler]，优化方案见"性能设计 1."。也可以手动加载
  - 包含所有自带的ParameterHandler

### 细节

1. ConsoleCommandAttribute
  - 提供无参构造函数，参数构造函数（string name），（CommandName name）。无参构造函数将当前方法名作为name。
  - CommandHelpText使用单独的Attribute来设置，要求是当前方法上必须包含ConsoleCommandAttribute。同一个name只能有一个CommandHelpText，如果定义多次，请警告并且无视后面的。
  - 不支持默认参数，用户定义了默认参数时发出警告说不支持，应该使用重载代替默认参数。带默认参数的方法依然可以正常解析。
  - 每个参数可以标记[CommandParameter()]，提供无参构造函数，参数构造函数（string name），无参就使用参数名作为name。
  - 参数必须使用首选ParameterHandler解析。如果不存在首选ParameterHandler，发出警告并且无法解析这个参数。(降级为StringParameterHandler)
  - 参数的一些关键字都不做特殊支持，比如params，ref，但是尽量不发警告（如果可能出问题还是要发）
    - 这么设计的原因，是想支持[ConsoleCommand]标记的方法，也应该可以通过正常方式调用，方便用户调试和使用。
  - 不支持泛型方法，发出警告并且无法解析这个方法。

2. ConsoleParameterHandlerAttribute
  - 本质逻辑就是注册到首选ParameterHandler机制里。方法返回值的类型作为首选ParameterHandler的类型
  - 所有ConsoleParameterHandler都是TupleParameterHandler，本质就是参考所有参数的类型和名字，组合成一个TupleParameterHandler，然后方法本体就是Parse的逻辑。
  - 提供默认构造函数，参数构造函数（string type），无参构造函数将方法返回值的类型作为type。
  - 不支持泛型方法，发出警告并且无法解析这个方法。
  - 其余的需求与ConsoleCommandAttribute差不多，你自己理解。

### 性能设计

1. Soyo.SoyoRuntimeConsole.Console类提供一个公有静态工厂方法(ConsoleKey key)，考虑所有加载的程序集，扫描所有类，扫描所有标记了ConsoleCommand的方法（这个方法必须是BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly）
  - 扫描TargetConsoleKey，如果当前方法有就使用当前方法的，如果没有，就看当前类有没有。ConsoleKey不对应的Command排除掉，未定义ConsoleKey的，认为是全局Command，可要包含它
  - 这里需要性能优化，请按照这个方案 1. 排除系统程序集和unity程序集(你需要自己考虑有哪些，这个粗略的排除即可) 2. 排除未引用SoyoRuntimeConsole的程序集
  - 每次调用这个方法都是单独的一次扫描，也就是不会缓存扫描结果（因为完全不会有这个需求）
2. ConsoleBuilder
  - 这个类的设计目的，是提供一个性能高且易用性好的类
  - 功能包括：
    1. 设置ConsoleKey
    2. 注册单个命令，注册单条CommandHelpText
    3. 注册一整个ConsoleConfig（逻辑是将ConsoleConfig里面包含的信息全部添加进去，注意不是覆盖是添加）
       - 如果ConsoleKey重复输入了，忽略
    4. 注册一个类里面所有的，通过Attri注册的ConsoleCommand
    5. 注册一个程序集里面所有的，通过Attri注册的ConsoleCommand
    6. 遍历所有加载的程序集，注册所有ConsoleCommand
  - 刚刚提到的Soyo.SoyoRuntimeConsole.Console的构建方法，应该基于ConsoleBuilder实现。
  - Builder本质是生成一个ConsoleConfig，然后利用它创建Console。


## 测试相关

由于你无法运行测试，你直接写完，我再测试反馈你即可。

测试不要定义全局的[ConsoleCommand]，会污染其他地方。
[ConsoleParameterHandler]是全局的，但不会污染别处，因为别处不引用测试程序集，也就用不到构造出来那个类。

如果你想开个新的程序集来搞测试，你需要在计划末尾明确地指出，我手动创建。


## 工作目标

你需要把这个任务拆成若干个可验证成果的小任务，我计划分多个Context完成这个任务

也就是说，你需要修改TASK.md文件（我已做好备份，放心修改）

IMPORTANT: 你需要思考怎么分任务，以及用什么工作流，能最稳定可靠地完成这个任务