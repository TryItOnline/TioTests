# TioTests

!!! Work in progress !!!

An attempt to build a tool that runs test suits on [TryItOnline!](https://tryitonline.net) as well as a default "Hello, World!" test for every supported language. Runs everywhere, where .NET Core runs. Tested on Windows 10 and Fedora 24.

    1. Install [.NET Core](https://www.microsoft.com/net/core) you need version 1.1
    2. `git clone https://github.com/AndrewSav/TioTests.git`
    3. `cd TioTests`
    4. `dotnet restore`
    5. Edit `config.json` if required
    6. `dotnet run`

If you are running it's on Windows, it's recommended to start it from powershell, as Windows Console does not support ANSI sequences and you'll see weird output.
