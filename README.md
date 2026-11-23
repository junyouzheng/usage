# usage

Windows 托盘程序，用来查看 SuperGrok 周额度（Chat / Imagine / Build 共用额度池）。

登录信息读取自本机 `~/.grok/auth.json`（先运行 `grok login`）。程序不会把令牌写回该文件。

## 功能

- 不占用任务栏，只显示系统托盘图标
- 窗口固定在工作区右下角，贴边，宽 400，高度随内容升高，给走势图留出固定高度，不能拖动、不能改大小
- 单击托盘图标：一次打开，再点一次隐藏。托盘没有右键菜单
- 窗口底部从左到右是「刷新」「自启动」「退出」。点「退出」后要再点红色「确认」才会结束程序；点「取消」或把窗口缩回托盘则不退出。打开「自启动」后，登录时只驻留托盘，不弹出窗口。手动打开程序仍会显示窗口
- 启动时不拉取用量，直接显示上次刷新的结果。点「刷新」会立刻更新
- 托盘图标显示剩余百分比。尚未刷新时，图标为空闲状态
- 产品拆分下面是近 7 天走势。鼠标移上去时，显示该点的已用百分比和当时的时间

命令行一次性查询：

```bat
usage.exe --once
usage.exe --silent
```

`--silent` 只创建托盘图标，不打开窗口。开机自启动注册的就是这条命令。

## 本地编译

仓库只保留源码。需要本机安装 .NET 8 SDK，自行编译后运行。

```bat
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist
```
