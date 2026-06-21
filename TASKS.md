# TASKS.md

我对ConsoleViewModel的补全功能(AutoComplete)不满意，现在不能补全命令名，我希望添加这个功能。

1. [x] Suggestion.CandidateParameters 感觉不太好，可以改名为Candidates，反正都是用于补全的信息
2. [x] 假设有个叫hello的命令，输入"he"时，应该能补全为"hello"
3. [x] 补全对大小写敏感，例如输入"HE"时不应该补全为"hello"