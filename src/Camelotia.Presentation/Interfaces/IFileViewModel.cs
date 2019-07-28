﻿using System.ComponentModel;

namespace Camelotia.Presentation.Interfaces
{
    public interface IFileViewModel : INotifyPropertyChanged
    {
        string Name { get; }

        IProviderViewModel Provider { get; }

        string Modified { get; }

        bool IsFolder { get; }

        bool IsFile { get; }

        string Path { get; }

        string Size { get; }
    }
}
