using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;

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

    public IObservable<object> LoadState()
    {
        if (!File.Exists(stateFilePath))
        {
            return Observable.Throw<object>(new FileNotFoundException(stateFilePath));
        }

        var lines = File.ReadAllText(stateFilePath);
        var state = JsonConvert.DeserializeObject<object>(lines, _settings);
        return Observable.Return(state);
    }

    public IObservable<Unit> SaveState(object state)
    {
        var lines = JsonConvert.SerializeObject(state, Formatting.Indented, _settings);
        File.WriteAllText(stateFilePath, lines);
        return Observable.Return(Unit.Default);
    }
}
