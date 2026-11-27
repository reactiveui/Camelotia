// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Camelotia.Presentation.AppState;
using ReactiveUI;
using ReactiveUI.Builder;
using ReactiveUI.Primitives.Concurrency;

namespace Camelotia.Tests.Infrastructure;

/// <summary>Builds ReactiveUI once for the test run and runs its schedulers inline, so commands finish before each assertion.</summary>
public static class TestSessionSetup
{
    /// <summary>Configures ReactiveUI with a typed suspension host and inline schedulers.</summary>
    [Before(TestSession)]
    public static void Configure()
    {
        _ = RxAppBuilder.CreateReactiveUIBuilder().WithSuspensionHost<MainState>().WithCoreServices().BuildApp();
        RxSchedulers.MainThreadScheduler = Sequencer.Immediate;
        RxSchedulers.TaskpoolScheduler = Sequencer.Immediate;
    }
}
