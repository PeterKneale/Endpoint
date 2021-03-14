﻿using Endpoint.Application.Enums;
using Endpoint.Application.ValueObjects;
using System;
using System.Text;

namespace Endpoint.Application.Builders
{
    public class MethodBodyBuilder
    {
        private StringBuilder _string;
        private int _indent;
        private int _tab = 4;
        private EndpointType? _endpointType;
        private string _resource;

        public MethodBodyBuilder(EndpointType? endpointType = null, int indent = 0, string resource = null)
        {
            _string = new StringBuilder();
            _indent = indent;
            _endpointType = endpointType;
            _resource = resource;
        }

        
        public string[] Build()
        {           
            
            if(_endpointType != null)
            {
                return _endpointType switch
                {
                    EndpointType.Get => BuildGetEndpointBody(_resource),
                    EndpointType.GetById => BuildGetByIdEndpointBody(_resource),
                    EndpointType.Create => BuildEndpointBody(),
                    EndpointType.Delete => BuildEndpointBody(),
                    EndpointType.Update => BuildEndpointBody(),
                    _ => throw new NotImplementedException()
                };
            }

            throw new System.NotImplementedException();
        }

        public string[] BuildEndpointBody()
            => new string[1]
            {
                "=> await _mediator.Send(request);".PadLeft((_indent + 1) * _tab)
            };

        public string[] BuildGetEndpointBody(string resource)
            => new string[1]
            {
                $"=> await _mediator.Send(new Get{((Token)resource).PascalCasePlural}.Request());".PadLeft((_indent + 1) * _tab)
            };

        public string[] BuildGetByIdEndpointBody(string resource)
            => new string[10]
            {
                "        {",
                "            var response = await _mediator.Send(request);",
                "",
                $"            if (response.{((Token)resource).PascalCase} == null)",
                "            {",
                $"                return new NotFoundObjectResult(request.{((Token)resource).PascalCase}Id)",
                "            }",
                "",
                "            return response",
                "        }"
            };
    }
}
