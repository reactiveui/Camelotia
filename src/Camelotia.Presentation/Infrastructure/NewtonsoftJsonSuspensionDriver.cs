using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Infrastructure;

public sealed class NewtonsoftJsonSuspensionDriver(string stateFilePath) : ISuspensionDriver
{
    private readonly JsonSerializerSettings _settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        NullValueHandling = NullValueHandling.Ignore,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    public IObservable<Unit> InvalidateState()
    {
        if (File.Exists(stateFilePath))
            File.Delete(stateFilePath);
        return Observable.Return(Unit.Default);
    }

    public IObservable<object?> LoadState()
    {
        if (!File.Exists(stateFilePath))
        {
            return Observable.Throw<object?>(new FileNotFoundException(stateFilePath));
        }

        var lines = File.ReadAllText(stateFilePath);
        var state = JsonConvert.DeserializeObject<object>(lines, _settings);
        return Observable.Return<object?>(state);
    }

    public IObservable<T?> LoadState<T>(JsonTypeInfo<T> typeInfo)
    {
        if (!File.Exists(stateFilePath))
        {
            return Observable.Throw<T?>(new FileNotFoundException(stateFilePath));
        }

        return Observable.Return(System.Text.Json.JsonSerializer.Deserialize(File.ReadAllText(stateFilePath), typeInfo));
    }

    public IObservable<Unit> SaveState<T>(T state)
    {
        var lines = JsonConvert.SerializeObject(state, Formatting.Indented, _settings);
        File.WriteAllText(stateFilePath, lines);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> SaveState<T>(T state, JsonTypeInfo<T> typeInfo)
    {
        File.WriteAllText(stateFilePath, System.Text.Json.JsonSerializer.Serialize(state, typeInfo));
        return Observable.Return(Unit.Default);
    }
}
