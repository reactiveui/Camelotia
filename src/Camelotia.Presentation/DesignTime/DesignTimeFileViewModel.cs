using System;
using System.Globalization;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeFileViewModel : ReactiveObject, IFileViewModel
    {
        public string Name { get; } = "Awesome file.";

        public IProviderViewModel Provider { get; } = null;

        public string Modified { get; } = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        public bool IsFolder { get; } = false;

        public bool IsFile { get; } = true;

        public string Path { get; } = "/home/path/file";

        public string Size { get; } = "42 KB";
    }
}