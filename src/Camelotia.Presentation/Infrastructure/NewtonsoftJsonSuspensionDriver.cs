using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;

namespace Camelotia.Presentation.Infrastructure
{
    public sealed class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
    {
        private readonly string _stateFilePath;
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public NewtonsoftJsonSuspensionDriver(string stateFilePath) => _stateFilePath = stateFilePath;

        public IObservable<Unit> InvalidateState()
        {
            if (File.Exists(_stateFilePath)) 
                File.Delete(_stateFilePath);
            return Observable.Return(Unit.Default);
        }

        public IObservable<object> LoadState()
        {            
            if (!File.Exists(_stateFilePath))
            {
                return Observable.Throw<object>(new FileNotFoundException(_stateFilePath));
            }
            var lines = File.ReadAllText(_stateFilePath);
            var state = JsonConvert.DeserializeObject<object>(lines, _settings);
            return Observable.Return(state);
        }

        public IObservable<Unit> SaveState(object state)
        {
            var lines = JsonConvert.SerializeObject(state, Formatting.Indented, _settings);
            File.WriteAllText(_stateFilePath, lines);
            return Observable.Return(Unit.Default);
        }
    }
}