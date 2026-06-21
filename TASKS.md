# TASKS.md

~~请你完成对Console的测试编写：~~

~~测试对象：（这两个类设计上是耦合的，所以一起测试）~~
~~- ConsoleBase~~
~~- CommandLineAnalyzer~~

~~测试内容：~~
~~- 你创建一些命令(3个简单命令即可，对复杂命令解析能力的测试在别处已经写了)，这三个命令分别是：~~
~~  - hello - 在控制台输入"Hello!"~~
~~  - hello <string: name> - 在控制台向指定人名问好~~
~~  - hello_world - 在控制台输出Hello World~~
~~  - 创建命令的方式可以参考注释或Sample~~
~~- 测试命令的正确解析和执行~~
~~  - 需知~~
~~    - 你不用关注CommandLineAnalyzeResult的信息，对ConsoleBase测试就好了（也就是不用写CommandLineAnalyzer的测试，但是你需要标注这个测试是对CommandLineAnalyzer有测试的）~~
~~    - 每个方法都要写测试~~
~~    - 你继承ConsoleBase创建一个测试类，从而展开测试（创建方法也可以参考Sample）~~
~~  - 测试内容~~
~~    - IConsole接口的每个方法都要测试~~

✅ 已完成。测试文件：`Packages/com.github.theplayer571.soyo-runtime-console/Tests/Editor/ConsoleBaseTests.cs`（18 个测试）