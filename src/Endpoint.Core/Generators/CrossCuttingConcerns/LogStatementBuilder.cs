﻿using Endpoint.Core.Builders.Common;
using Endpoint.Core.Enums;
using Endpoint.Core.Models;
using Endpoint.Core.ValueObjects;

namespace Endpoint.Core.Builders
{
    internal class LogStatementBuilder
    {

        private int _indent;
        private string _resource;
        private EndpointType _endpointType;
        private Settings _settings;

        public LogStatementBuilder(Settings settings, string resource, EndpointType? endpointType = EndpointType.Create, int indent = 0)
        {
            _indent = indent;
            _resource = resource ?? throw new System.ArgumentNullException(nameof(resource));
            _endpointType = endpointType ?? throw new System.ArgumentNullException(nameof(endpointType));
            _settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
        }

        public string[] BuildForCreateCommand()
            => new string[4]
            {
                "_logger.LogInformation(".Indent(_indent),
                "\"----- Sending command: {CommandName}: ({@Command})\",".Indent(_indent + 1),
                $"nameof(Create{((Token)_resource).PascalCase}Request),".Indent(_indent + 1),
                "request);".Indent(_indent + 1)
            };

        public string[] BuildForUpdateCommand()
            => new string[6]
            {
                "_logger.LogInformation(".Indent(_indent),
                "\"----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})\",".Indent(_indent + 1),
                $"nameof(Update{((Token)_resource).PascalCase}Request),".Indent(_indent + 1),
                $"nameof(request.{((Token)_resource).PascalCase}.{IdPropertyNameBuilder.Build(_settings,_resource)}),".Indent(_indent + 1),
                $"request.{((Token)_resource).PascalCase}.{IdPropertyNameBuilder.Build(_settings,_resource)},".Indent(_indent + 1),
                "request);".Indent(_indent + 1)
            };

        public string[] BuildForDeleteCommand()
            => new string[6]
            {
                "_logger.LogInformation(".Indent(_indent),
                "\"----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})\",".Indent(_indent + 1),
                $"nameof(Remove{((Token)_resource).PascalCase}Request),".Indent(_indent + 1),
                $"nameof(request.{IdPropertyNameBuilder.Build(_settings,_resource)}),".Indent(_indent + 1),
                $"request.{IdPropertyNameBuilder.Build(_settings,_resource)},".Indent(_indent + 1),
                "request);".Indent(_indent + 1)
            };

        public string[] Build()
        {
            switch (_endpointType)
            {
                case EndpointType.Create:
                    return BuildForCreateCommand();


                case EndpointType.Update:
                    return BuildForUpdateCommand();


                case EndpointType.Delete:
                    return BuildForDeleteCommand();
            }

            return System.Array.Empty<string>();
        }
    }
}
