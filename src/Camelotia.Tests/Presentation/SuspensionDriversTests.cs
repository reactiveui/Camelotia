using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using DynamicData;
using FluentAssertions;
using ReactiveUI;
using Xunit;

namespace Camelotia.Tests.Presentation
{
    public class SuspensionDriversTests
    {
        [Fact]
        public Task NewtonsoftJsonSuspensionDriverShouldSaveAndLoadState() =>
            SuspensionDriverShouldSaveAndLoadState(
                new NewtonsoftJsonSuspensionDriver(Path.GetTempFileName()));
        
        [Fact]
        public Task AkavacheSuspensionDriverShouldSaveAndLoadState() =>
            SuspensionDriverShouldSaveAndLoadState(
                new AkavacheSuspensionDriver<MainState>());
        
        private static async Task SuspensionDriverShouldSaveAndLoadState(ISuspensionDriver driver)
        {
            var state = new MainState();
            state.Providers.AddOrUpdate(new ProviderState());
            state.Providers.AddOrUpdate(new ProviderState
            {
                AuthState = new AuthState
                {
                    DirectAuthState = new DirectAuthState
                    {
                        Username = "Joseph Joestar",
                        Password = "Dio"
                    }
                }
            });

            await driver.SaveState(state);
            var loaded = await driver.LoadState();
            loaded.Should().BeOfType<MainState>();

            var retyped = (MainState) loaded;
            retyped.Providers.Count.Should().Be(2);
            retyped.ProviderStates.Should().NotBeEmpty();
            retyped.ProviderStates.Should().Contain(provider =>
                provider.AuthState.DirectAuthState.Username == "Joseph Joestar" &&
                provider.AuthState.DirectAuthState.Password == "Dio");

            await driver.InvalidateState();
            await Assert.ThrowsAnyAsync<Exception>(async () => await driver.LoadState());
            await driver.SaveState(new MainState());
            await driver.LoadState();
        }
        
    }
}