using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet;

namespace Camelotia.Services.Providers
{
    public sealed class VkontakteFileSystemProvider : IProvider
    {
        private readonly ReplaySubject<bool> _isAuthorized;
        private readonly VkApi _api;
        
        public VkontakteFileSystemProvider()
        {
            _api = new VkApi();
            _isAuthorized = new ReplaySubject<bool>();
            _isAuthorized.OnNext(false);
        }

        public string Size => "Unknown";

        public string Name => nameof(VkontakteFileSystemProvider);

        public string Description => "Vkontakte documents provider";

        public IObservable<bool> IsAuthorized => _isAuthorized;

        public bool SupportsDirectAuth => true;

        public bool SupportsOAuth => false;

        public Task OAuth() => Task.CompletedTask;

        public async Task DirectAuth(string login, string password)
        {
            await _api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = 5560698,
                Login = login,
                Password = password,
                Settings = Settings.Documents
            });
            _isAuthorized.OnNext(_api.IsAuthorized);
        }

        public async Task<IEnumerable<FileModel>> Get(string path)
        {
            var documents = await _api.Docs.GetAsync();
            return documents.Select(document =>
            {
                var size = string.Empty;
                if (document.Size.HasValue)
                    size = BytesToString(document.Size.Value);
                return new FileModel(document.Title, string.Empty, false, size);
            });
        }

        public Task DownloadFile(string from, Stream to) => Task.CompletedTask;

        public Task UploadFile(string to, Stream from) => Task.CompletedTask;

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num) + suf[place];
        }
    }
}