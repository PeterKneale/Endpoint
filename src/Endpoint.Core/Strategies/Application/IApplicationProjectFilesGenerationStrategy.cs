﻿using Endpoint.Core.Models;

namespace Endpoint.Core.Services
{
    public interface IApplicationProjectFilesGenerationStrategy
    {
        void Build(Settings settings);
        void BuildAdditionalResource(AggregateRootModel aggregateModel, Settings settings);
    }
}
