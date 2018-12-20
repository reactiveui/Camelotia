using Camelotia.Services.Interfaces;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Camelotia.Presentation.Uwp.Services
{
    public sealed class UniversalFileManager : IFileManager
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
