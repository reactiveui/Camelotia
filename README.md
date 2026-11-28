[![Build](https://github.com/reactiveui/Camelotia/actions/workflows/ci-build.yml/badge.svg)](https://github.com/reactiveui/Camelotia/actions/workflows/ci-build.yml) [![Code Coverage](https://codecov.io/gh/reactiveui/Camelotia/branch/main/graph/badge.svg)](https://codecov.io/gh/reactiveui/Camelotia)
[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://reactiveui.net/contribute) [![](https://img.shields.io/badge/chat-slack-blue.svg)](https://reactiveui.net/slack)

<br />
<a href="https://github.com/reactiveui/Camelotia">
  <img width="160" src="https://raw.githubusercontent.com/reactiveui/Camelotia/main/images/vertical.png" alt="Camelotia">
</a>

# Camelotia: A cloud storage file manager built with ReactiveUI

Camelotia is a file manager for cloud storage, built with [ReactiveUI](https://github.com/reactiveui/ReactiveUI) and modern .NET UI frameworks. It browses, uploads, downloads, renames and deletes files in Yandex Disk, Google Drive, VK Documents, GitHub, FTP, SFTP and the local file system.

The same view models drive an [Avalonia](https://github.com/AvaloniaUI/Avalonia) app for Windows, Linux and macOS, and a WPF app for Windows. Both target .NET 10 and .NET 11.

| Avalonia | WPF |
| -------- | --- |
| <img src="images/UiAvalonia.png" width="420"> | <img src="images/UiPresentation.png" width="420"> |

## Build and run

Install the [.NET 11 SDK](https://dot.net/). Run these commands from the `src` folder.

```sh
# Build the Avalonia app, the shared libraries and the tests.
dotnet build Camelotia.Avalonia.slnf

# Run the Avalonia app.
dotnet run --project Camelotia.Presentation.Avalonia --framework net11.0

# Run the tests. Add the flags after "--" to collect Cobertura coverage.
dotnet test --project Camelotia.Tests -- --coverage --coverage-output-format cobertura
```

The WPF app builds on Windows with `dotnet build Camelotia.Wpf.slnf`. Its views use plain XAML and reactive bindings, so you can copy a view and its view model into your own WPF app.

## Signing in to each provider

| Provider | How you sign in | What to set up |
| -------- | --------------- | -------------- |
| Local | No sign-in. | Nothing. |
| FTP and SFTP | Address, port, user name and password. | Nothing. The SFTP client accepts any host key. |
| GitHub | Your user name, with a [personal access token](https://github.com/settings/tokens) as the password. | Give the token read and write access to the repositories you want to browse. |
| Google Drive | In the browser. | The Google client ID and secret are in `GoogleDriveCloudOptions`. |
| Yandex Disk | In the browser, with PKCE. | Register `http://127.0.0.1:3000/` as the redirect URI of the Yandex OAuth app in `YandexDiskCloudOptions`. |
| VK Documents | In the browser, through VK ID with PKCE. | Register `http://127.0.0.1:3001/` as a trusted redirect URL of the VK ID app in `VkDocsCloudOptions`. VK must also give that app access to the documents API. |

The browser providers open your default browser, then wait on the loopback address for the redirect. The app's settings live in `CloudConfiguration`, which the app saves with the rest of its state.

## How it is built

- `Camelotia.Services` holds one `ICloud` provider for each service. The Yandex Disk and VK clients are [Refit](https://github.com/reactiveui/refit) interfaces with source-generated code and source-generated JSON, registered through `AddCloudServices()`.
- `Camelotia.Presentation` holds the view models. They are written with [ReactiveUI.SourceGenerators](https://github.com/reactiveui/ReactiveUI.SourceGenerators), [ReactiveUI.Validation](https://github.com/reactiveui/ReactiveUI.Validation) and [DynamicData](https://github.com/reactivemarbles/DynamicData).
- ReactiveUI's typed suspension saves the app state to [Akavache](https://github.com/reactiveui/Akavache) with System.Text.Json.
- The tests use [TUnit](https://github.com/thomhurst/TUnit) on Microsoft.Testing.Platform. They run the real Refit, Octokit and Google clients against stubbed HTTP handlers, so no test talks to a real service.

## Technologies

- [ReactiveUI](https://reactiveui.net/) and [ReactiveUI.Primitives](https://github.com/reactiveui/Primitives) for observable state, commands and view activation
- [ReactiveUI.SourceGenerators](https://github.com/reactiveui/ReactiveUI.SourceGenerators) for reactive properties
- [ReactiveUI.Validation](https://github.com/reactiveui/ReactiveUI.Validation) for form validation
- [DynamicData](https://github.com/reactivemarbles/DynamicData) for reactive collections
- [Akavache](https://github.com/reactiveui/Akavache) for the persistent key-value store
- [Refit](https://github.com/reactiveui/refit) for the Yandex Disk and VK REST clients
- [Avalonia](https://github.com/AvaloniaUI/Avalonia) with the Fluent theme for the cross-platform app
- [Material Design in XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) and [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) for the WPF app
- [Octokit](https://github.com/octokit/octokit.net) for GitHub
- [Google APIs Client Library](https://github.com/googleapis/google-api-dotnet-client) for Google Drive
- [FluentFTP](https://github.com/robinrodricks/FluentFTP) for FTP
- [SSH.NET](https://github.com/sshnet/SSH.NET) for SFTP
- [TUnit](https://github.com/thomhurst/TUnit) and Microsoft.Testing.Platform for tests and coverage

## Contribute

Camelotia is developed under an OSI-approved open source license, making it freely usable and distributable, even for commercial use. We welcome contributors of all experience levels.

- Answer questions on StackOverflow: https://stackoverflow.com/questions/tagged/reactiveui
- Share knowledge and mentor the next generation of developers
- Donations: https://reactiveui.net/donate and Corporate Sponsorships: https://reactiveui.net/sponsorship
- Ask your employer to support open-source: https://github.com/github/balanced-employee-ip-agreement
- Improve documentation and examples
- Contribute features and bugfixes via PRs

## Sponsors

[JetBrains](https://www.jetbrains.com/) gives ReactiveUI's maintainers licences for its tools through its
[open source support programme](https://www.jetbrains.com/community/opensource/).
[Anthropic](https://www.anthropic.com/) supports them with [Claude](https://claude.com/) through
[Claude for Open Source](https://claude.com/contact-sales/claude-for-oss).
[OpenAI](https://openai.com/) supports them with [Codex](https://openai.com/codex/) through
[Codex for Open Source](https://developers.openai.com/community/codex-for-oss).
[Snyk](https://snyk.io/) gives the maintainers access to security tools through its
[Secure Developer Program](https://snyk.io/open-source/).

[![JetBrains](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/jetbrains.svg)](https://www.jetbrains.com/)
[![Claude by Anthropic](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/claude.svg)](https://claude.com/)
[![OpenAI](https://raw.githubusercontent.com/reactiveui/website/main/docs/images/sponsors/openai.svg)](https://openai.com/codex/)
[![Sponsored by Snyk](https://raw.githubusercontent.com/reactiveui/website/e37a112f97ea2e9ea54b21bb7ed9e56df034d618/docs/images/sponsors/snyk.svg)](https://snyk.io/)

See [our sponsors](https://www.reactiveui.net/sponsors/) for more information.
JetBrains, Claude, Anthropic, OpenAI and Codex names and logos are trademarks of their respective owners.
Snyk and the Snyk logo are trademarks of Snyk Ltd.
