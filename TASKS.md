# TASKS.md

目前Soyo.SoyoRuntimeConsole.ConsoleViewModel的功能不是很完善，你需要进行以下任务：

0. Suggestion缓存功能，上次的Suggestion缓存起来，检测如果InputText没有变化，就不再重新计算Suggestion，直接使用缓存的Suggestion
1. 加强AutoCompolete功能。更改签名为 bool AutoComplete()，Index通过其他方式提供
    - 目前缺少“让View层轻松写 使用选择的补全”的功能。你需要提供一个方法，能设置CandidateIndex，设置之后，AutoComplete会使用这个Index对应的CandidateCommand进行补全
    - 提供CandidateIndex的获取方法，提供api能让它安全地往前后移动（会回绕）。api命名你决定
    - 提供api GetAutoCompleteText 获取完整的补全后的整条命令的文本
    - 在函数注释提醒使用者：调用 AutoComplete 会导致InputText被修改，如果你自定义UI，记得更新
2. 加强历史记录功能。
    - 提供api string GetHistory(int offset) 获取发送命令的历史记录，offset应该为正数(1 代表 上一条)
    - GetHistory作为函数名语义并不是很明确，你需要想一个更好的名字
    - 目前的IReadOnlyList<string> GetHistory() 改成属性，换个名字
    - 可以指定历史记录的最大条数，超过这个条数的历史记录会被丢弃。默认为20条
    - 提供api “恢复历史记录(int offset)”，直接将记录写进InputText中
    - 在函数注释提醒使用者：调用 恢复历史记录 会导致InputText被修改，如果你自定义UI，记得更新
3. 提供一个可选的功能，是否记录每条LogEntry。默认不记录
   - 可以设置记录上限
4. 将ViewModel中的public方法改成virtual的，方便用户自定义
   - 至于要改哪些，你肯定清楚