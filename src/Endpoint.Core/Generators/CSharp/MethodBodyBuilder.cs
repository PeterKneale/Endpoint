﻿using Endpoint.Core.Enums;
using Endpoint.Core;
using Endpoint.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Endpoint.Core.Models;
using Endpoint.Core.Builders.Common;

namespace Endpoint.Core.Builders
{
    public class MethodBodyBuilder
    {
        private StringBuilder _string;
        private int _indent;
        private int _tab = 4;
        private EndpointType? _endpointType;
        private string _resource;
        private Settings _settings;
        public MethodBodyBuilder(Settings  settings,EndpointType? endpointType = null, int indent = 0, string resource = null)
        {
            _string = new StringBuilder();
            _indent = indent;
            _endpointType = endpointType;
            _resource = resource;
            _settings = settings;
        }

        public string[] Build()
        {
            var logStatementBuilder = new LogStatementBuilder(_settings, _resource, _endpointType, _indent + 1);

            var logStatement = logStatementBuilder.Build();

            var body = new List<string>()
            {
                "{"
            };


            switch (_endpointType)
            {
                case EndpointType.GetById:
                    body.AddRange(BuildGetByIdEndpointBody(_resource));
                    break;

                case EndpointType.Get:
                    body.AddRange(BuildGetEndpointBody(_resource));
                    break;

                case EndpointType.Page:
                    body.AddRange(BuildPageEndpointBody(_resource));
                    break;

                case EndpointType.Delete:
                    body.AddRange(BuildDeleteEndpointBody(_resource));
                    break;

                default:
                    body.AddRange(logStatement);
                    body.Add("");
                    body.AddRange(BuildEndpointBody());
                    break;
            }

            body.Add("}");

            return body.ToArray();

        }

        public string[] BuildEndpointBody()
            => new string[1]
            {
                "return await _mediator.Send(request, cancellationToken);".Indent(_indent + 1)
            };

        public string[] BuildPageEndpointBody(string resource)
            => new string[3]
            {
                ($"var request = new Get{((Token)resource).PascalCasePlural}PageRequest" + " { Index = index, PageSize = pageSize };").Indent(_indent + 1),
                "",
                "return await _mediator.Send(request, cancellationToken);".Indent(_indent + 1)
            };

        public string[] BuildGetEndpointBody(string resource)
            => new string[1]
            {
                $"return await _mediator.Send(new Get{((Token)resource).PascalCasePlural}Request(), cancellationToken);".Indent(_indent + 1)
            };

        public string[] BuildGetByIdEndpointBody(string resource)
            => new string[10]
            {
                ("var request = new Get" + ((Token)resource).PascalCase + "ByIdRequest() { " + IdPropertyNameBuilder.Build(_settings,resource) + " = " + ((Token)IdPropertyNameBuilder.Build(_settings, resource)).CamelCase + " };").Indent(_indent + 1),
                "",
                "var response = await _mediator.Send(request, cancellationToken);".Indent(_indent + 1),
                "",
                $"if (response.{((Token)resource).PascalCase} == null)".Indent(_indent + 1),
                "{".Indent(_indent + 1),
                $"return new NotFoundObjectResult(request.{IdPropertyNameBuilder.Build(_settings,resource)});".Indent(_indent + 2),
                "}".Indent(_indent + 1),
                "",
                "return response;".Indent(_indent + 1),
            };

        public string[] BuildCreateEndpointBody(string resource)
            => new string[8]
            {
                "var response = await _mediator.Send(request, cancellationToken);".Indent(_indent + 1),
                "",
                $"if (response.{((Token)resource).PascalCase} == null)".Indent(_indent + 1),
                "{".Indent(_indent + 1),
                $"return new NotFoundObjectResult(request.{IdPropertyNameBuilder.Build(_settings,resource)});".Indent(_indent + 2),
                "}".Indent(_indent + 1),
                "",
                "return response;".Indent(_indent + 1),
            };

        public string[] BuildDeleteEndpointBody(string resource)
        {
            var result = new List<string>
            {
                ("var request = new Remove" + ((Token)resource).PascalCase + "Request() { " + IdPropertyNameBuilder.Build(_settings, resource) + " = " + ((Token)IdPropertyNameBuilder.Build(_settings, resource)).CamelCase + " };").Indent(_indent + 1)
            };

            result.Add("");

            result.AddRange(new LogStatementBuilder(_settings, resource, EndpointType.Delete, _indent + 1).Build());

            result.Add("");

            result.Add("return await _mediator.Send(request, cancellationToken);".Indent(_indent + 1));

            return result.ToArray();
        }

        public string[] BuildUpdateEndpointBody(string resource)
            => new string[8]
            {
                "var response = await _mediator.Send(request, cancellationToken);".Indent(_indent + 1),
                "",
                $"if (response.{((Token)resource).PascalCase} == null)".Indent(_indent + 1),
                "{".Indent(_indent + 1),
                $"return new NotFoundObjectResult(request.{IdPropertyNameBuilder.Build(_settings,resource)});".Indent(_indent + 2),
                "}".Indent(_indent + 1),
                "",
                "return response;".Indent(_indent + 1),
            };
    }
}
