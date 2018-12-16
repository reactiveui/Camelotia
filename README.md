# Compiling

[![Build Status](https://worldbeater.visualstudio.com/Camelotia/_apis/build/status/Camelotia-CI)](https://worldbeater.visualstudio.com/Camelotia/_build/latest?definitionId=1) [![Pull Requests](https://img.shields.io/github/issues-pr/worldbeater/camelotia.svg)](https://github.com/worldbeater/Camelotia/pulls) [![Issues](https://img.shields.io/github/issues/worldbeater/camelotia.svg)](https://github.com/worldbeater/Camelotia/issues) ![License](https://img.shields.io/github/license/worldbeater/camelotia.svg) ![Size](https://img.shields.io/github/repo-size/worldbeater/camelotia.svg)

The app runs on Windows, Linux and MacOS. Make sure you have latest [.NET Core SDK](https://dot.net/) installed.

<img src="./Camelotia.png" width="500">

```sh
# Linux or MacOS shell
git clone https://github.com/worldbeater/Camelotia
cd Camelotia/Camelotia.Presentation.Avalonia
dotnet run
```

On Windows, run the `./run.bat` file.

### Adding Custom Providers

File system providers are located at `./Camelotia.Services/Providers/`. To add a custom file system provider, you need to create a separate class and implement the [IProvider](https://github.com/worldbeater/Camelotia/blob/master/Camelotia.Services/Interfaces/IProvider.cs) interface. It'll get integrated into the UI automagically.

### Technologies and Tools Used

- <a href="https://reactiveui.net/">ReactiveUI</a>
- <a href="http://github.com/avaloniaui">AvaloniaUI</a>
- <a href="https://github.com/nsubstitute/NSubstitute">NSubstitute</a>
- <a href="https://github.com/fluentassertions/fluentassertions">FluentAssertions</a>
- <a href="https://www.jetbrains.com/rider/">JetBrains Rider IDE</a>
