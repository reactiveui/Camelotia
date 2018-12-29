using Camelotia.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Camelotia.Presentation.Xamarin.Services
{
    public sealed class XamarinFileManager : IFileManager
    {
        public Task<(string Name, Stream Stream)> OpenRead()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenWrite(string name)
        {
            throw new NotImplementedException();
        }
    }
}
