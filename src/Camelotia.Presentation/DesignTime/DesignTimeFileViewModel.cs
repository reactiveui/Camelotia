using System;
using System.Globalization;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.DesignTime
{
    public class DesignTimeFileViewModel : ReactiveObject, IFileViewModel
    {
        public DesignTimeFileViewModel() : this(null) { }

        public DesignTimeFileViewModel(DesignTimeCloudViewModel provider) => Provider = provider;

        public string Name { get; } = "Awesome file.";

        public ICloudViewModel Provider { get; }

        public string Modified { get; } = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        public bool IsFolder { get; } = false;

        public bool IsFile { get; } = true;

        public string Path { get; } = "/home/path/file";

        public string Size { get; } = "42 KB";
    }
}