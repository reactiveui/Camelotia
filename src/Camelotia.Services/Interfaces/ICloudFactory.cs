using System.Collections.Generic;
using Camelotia.Services.Models;

namespace Camelotia.Services.Interfaces;

public interface ICloudFactory
{
    ICloud CreateCloud(CloudParameters parameters);

    IReadOnlyCollection<CloudType> SupportedClouds { get; }
}