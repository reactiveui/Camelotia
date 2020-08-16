using System;
using System.Collections.Generic;
using Akavache;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;

namespace Camelotia.Services
{
    public sealed class ProviderFactory : IProviderFactory
    {
        private readonly IAuthenticator _authenticator;
        private readonly IBlobCache _cache;

        public ProviderFactory(
            IAuthenticator authenticator,
            IBlobCache cache,
            IReadOnlyCollection<ProviderType> supported = null)
        {
            _cache = cache;
            _authenticator = authenticator;
            SupportedTypes = supported ?? new []
            {
                ProviderType.Local, 
                ProviderType.Ftp,
                ProviderType.Sftp,
                ProviderType.Yandex,
                ProviderType.GitHub,
                ProviderType.GoogleDrive,
                ProviderType.VkDocs
            };
        }
        
        public IReadOnlyCollection<ProviderType> SupportedTypes { get; }

        public IProvider CreateProvider(ProviderParameters parameters) => parameters.Type switch
        {
            ProviderType.Ftp => new FtpProvider(parameters),
            ProviderType.GitHub => new GitHubProvider(parameters),
            ProviderType.GoogleDrive => new GoogleDriveProvider(parameters, _cache),
            ProviderType.Local => new LocalProvider(parameters),
            ProviderType.Sftp => new SftpProvider(parameters),
            ProviderType.VkDocs => new VkDocsProvider(parameters),
            ProviderType.Yandex => new YandexDiskProvider(parameters, _authenticator),
            _ => throw new ArgumentOutOfRangeException(nameof(parameters))
        };
    }
}