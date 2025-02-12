using Endpoint.Core.Enums;
using Endpoint.Core;
using Endpoint.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using Endpoint.Core.Models;
using Endpoint.Core.Builders.Common;
using Endpoint.Core.Builders.Statements;

namespace Endpoint.Core.Builders
{
    public class MethodBuilder
    {
        private List<string> _contents;
        private int _indent;
        private AccessModifier _accessModifier;
        private List<string> _attributes;
        public List<string> _body;
        private bool _authorize;
        private EndpointType _endpointType;
        private string _resource;
        private bool _static;
        private string _name;
        private string _returnType;
        private List<string> _parameters;
        private bool _async;
        private bool _override;
        private Settings _settings;
        public MethodBuilder()
        {
            _accessModifier = AccessModifier.Public;
            _contents = new();
            _attributes = new();
            _body = new();
            _authorize = false;
            _static = false;
            _name = "";
            _returnType = "";
            _parameters = new();
            _async = false;
            _override = false;
        }

        public MethodBuilder WithSettings(Settings settings)
        {
            _settings = settings;
            return this;
        }

        public MethodBuilder WithOverride(bool @override = true)
        {
            _override = @override;
            return this;
        }

        public MethodBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            _accessModifier = accessModifier;
            return this;
        }

        public MethodBuilder WithBody(List<string> body)
        {
            _body = body;

            return this;
        }

        public MethodBuilder WithAsync(bool @async)
        {
            _async = async;

            return this;
        }

        public MethodBuilder WithParameter(string paremeter)
        {
            _parameters.Add(paremeter);

            return this;
        }
        public MethodBuilder WithReturnType(string returnType)
        {
            _returnType = returnType;
            return this;
        }

        public MethodBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public MethodBuilder WithAuthorize(bool authorize)
        {
            this._authorize = authorize;
            return this;
        }

        public MethodBuilder WithPropertyName(string propertName)
        {
            return this;
        }

        public MethodBuilder IsStatic(bool isStatic = true)
        {
            this._static = isStatic;
            return this;
        }

        public MethodBuilder WithEndpointType(EndpointType endpointType)
        {
            _endpointType = endpointType;
            return this;
        }

        public MethodBuilder WithResource(string resource)
        {
            _resource = resource;
            return this;
        }

        public MethodBuilder WithIndent(int indent)
        {
            _indent = indent;
            return this;
        }

        public MethodBuilder WithAttribute(string attribute)
        {
            _attributes.Add(attribute);

            return this;
        }
        public string[] Build()
        {
            if (_endpointType != default)
            {
                var requestType = _endpointType switch
                {
                    EndpointType.Create => $"Create{((Token)_resource).PascalCase}",
                    EndpointType.Delete => $"Remove{((Token)_resource).PascalCase}",
                    EndpointType.Get => $"Get{((Token)_resource).PascalCasePlural}",
                    EndpointType.GetById => $"Get{((Token)_resource).PascalCase}ById",
                    EndpointType.Update => $"Update{((Token)_resource).PascalCase}",
                    EndpointType.Page => $"Get{((Token)_resource).PascalCasePlural}Page",
                    _ => throw new System.NotImplementedException()
                };

                _contents = AttributeBuilder.EndpointAttributes(_settings, _endpointType, _resource, _authorize).ToList();

                var methodBuilder = new MethodSignatureBuilder()
                    .WithEndpointType(_endpointType)
                    .WithAsync(true)
                    .WithReturnType(TypeBuilder.WithActionResult($"{requestType}Response"));

                if (_endpointType == EndpointType.GetById || _endpointType == EndpointType.Delete)
                {
                    methodBuilder.WithParameter(new ParameterBuilder(IdDotNetTypeBuilder.Build(_settings, _resource), ((Token)$"{IdPropertyNameBuilder.Build(_settings,_resource)}").CamelCase).WithFrom(From.Route).Build());
                }

                if (_endpointType == EndpointType.Page)
                {
                    methodBuilder.WithParameter(new ParameterBuilder("int", "pageSize").WithFrom(From.Route).Build());

                    methodBuilder.WithParameter(new ParameterBuilder("int", "index").WithFrom(From.Route).Build());
                }

                if (_endpointType == EndpointType.Update)
                {
                    methodBuilder.WithParameter(new ParameterBuilder($"{requestType}Request", "request").WithFrom(From.Body).Build());
                }

                if (_endpointType == EndpointType.Create)
                {
                    methodBuilder.WithParameter(new ParameterBuilder($"{requestType}Request", "request").WithFrom(From.Body).Build());
                }

                methodBuilder.WithParameter(new ParameterBuilder("CancellationToken", "cancellationToken").Build());

                _contents.Add(methodBuilder.Build());

                _contents = _contents.Concat(new MethodBodyBuilder(_settings, _endpointType, _indent, _resource).Build()).ToList();

                return _contents.ToArray();
            }

            foreach (var attribute in _attributes)
            {
                _contents.Add(attribute);
            }

            var methodSignatureBuilder = new MethodSignatureBuilder()
                .WithName(_name)
                .WithAccessModifier(_accessModifier)
                .WithOverride(_override)
                .IsStatic(_static)
                .WithAsync(_async);

            foreach (var parameter in _parameters)
            {
                methodSignatureBuilder.WithParameter(parameter);
            }

            methodSignatureBuilder.WithReturnType(_returnType);

            _contents.Add(methodSignatureBuilder.Build());


            if (_body.Count > 0)
            {
                _contents.Add("{");
                foreach (var line in _body)
                {
                    _contents.Add(line.Indent(_indent + 1));
                }
                _contents.Add("}");
            }

            return _contents.ToArray();
        }
    }
}
