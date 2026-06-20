# TASKS.md

现在的CompositeParameterHandler存在问题：参数只有一种组织形式。
比如我定义了一个MyVec3ParameterHandler，参数是(int,int,int)，如果我想让(int, int)也能识别返回MyVec3，那我必须再写一个CompositeParameterHandler来处理(int, int)的情况。
这样会使命令的数量增加，比如一个fill MyVec3 MyVec3需要写4个命令来支持不同的参数组合。

为了解决这个问题，我打算引入一个新的Handler，传入若干个CompositeParameterHandler组合形成，具体功能需要你设计

## 你的任务

1. 设计新的Handler的名称以及交互思路
2. 思考：目前的CompositeParameterHandler的命名存在误导性，我刚刚提到的的“新的Handler”才应该这个名字才对。因此需要对现有的CompositeParameterHandler进行重命名，你觉得应该叫什么名字比较合适？为什么？
3. 先将旧的代码重构，然后再实现新的Handler

