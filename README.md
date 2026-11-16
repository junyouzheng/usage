# usage

Windows 托盘程序，用来查看 SuperGrok 周额度（Chat / Imagine / Build 共用额度池）。

登录信息读取自本机 `~/.grok/auth.json`（先运行 `grok login`）。程序不会把令牌写回该文件。

## 功能

- 不占用任务栏，只显示系统托盘图标
- 窗口固定在工作区右下角，贴边，宽 400，高度随内容，不能拖动、不能改大小
- 左键点托盘图标：一次打开，再点一次隐藏
- 托盘右键「退出」结束程序
- 启动时不拉取用量，直接显示上次刷新的结果。点「刷新」会立刻更新
- 托盘图标显示剩余百分比。尚未刷新时，图标为空闲状态

命令行一次性查询：

```bat
usage.exe --once
```

## 本地编译

仓库只保留源码。需要本机安装 .NET 8 SDK，自行编译后运行。

```bat
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
```
