
using Endpoint.Core.Services;
using Endpoint.Core.ValueObjects;
using System.Collections.Generic;
using Endpoint.Core;
using Endpoint.Core.Models;
using Endpoint.Core.Builders.Common;
using Endpoint.Core.Builders.Statements;

namespace Endpoint.Core.Builders
{
    public class RemoveBuilder
    {
        private readonly List<string> _content;
        private readonly IContext _context;
        private readonly IFileSystem _fileSystem;
        private readonly Settings _settings;
        private string _entity;
        private string _dbContext;
        private string _directory;
        private string _namespace;
        private string _domainNamespace;
        private string _applicationNamespace;

        public RemoveBuilder(Settings settings, IContext context, IFileSystem fileSystem)
        {
            _content = new();
            _context = context;
            _fileSystem = fileSystem;
            _settings = settings;
        }

        public RemoveBuilder WithDomainNamespace(string domainNamespace)
        {
            _domainNamespace = domainNamespace;
            return this;
        }

        public RemoveBuilder WithApplicationNamespace(string applicationNamespace)
        {
            _applicationNamespace = applicationNamespace;
            return this;
        }

        public RemoveBuilder WithDirectory(string directory)
        {
            _directory = directory;
            return this;
        }

        public RemoveBuilder WithEntity(string entity)
        {
            _entity = entity;
            return this;
        }

        public RemoveBuilder WithDbContext(string dbContext)
        {
            _dbContext = dbContext;
            return this;
        }


        public RemoveBuilder WithNamespace(string @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        public void Build()
        {
            var request = new ClassBuilder($"Remove{((Token)_entity).PascalCase}Request", _context, _fileSystem)
                .WithInterface(new TypeBuilder().WithGenericType("IRequest", $"Remove{((Token)_entity).PascalCase}Response").Build())
                .WithProperty(new PropertyBuilder().WithType(IdDotNetTypeBuilder.Build(_settings, _entity)).WithName(IdPropertyNameBuilder.Build(_settings,_entity)).WithAccessors(new AccessorsBuilder().Build()).Build())
                .Class;

            var response = new ClassBuilder($"Remove{((Token)_entity).PascalCase}Response", _context, _fileSystem)
                .WithBase("ResponseBase")
                .WithProperty(new PropertyBuilder().WithType($"{((Token)_entity).PascalCase}Dto").WithName($"{((Token)_entity).PascalCase}").WithAccessors(new AccessorsBuilder().Build()).Build())
                .Class;

            var handlerBodyLine1 = _settings.IdDotNetType == IdDotNetType.Int
                ? $"var {((Token)_entity).CamelCase} = await _context.{((Token)_entity).PascalCasePlural}.FindAsync(request.{IdPropertyNameBuilder.Build(_settings, _entity)});"
                : $"var {((Token)_entity).CamelCase} = await _context.{((Token)_entity).PascalCasePlural}.FindAsync(new {((Token)_entity).PascalCase}Id(request.{IdPropertyNameBuilder.Build(_settings, _entity)}));";


            var handler = new ClassBuilder($"Remove{((Token)_entity).PascalCase}Handler", _context, _fileSystem)
                .WithBase(new TypeBuilder().WithGenericType("IRequestHandler", $"Remove{((Token)_entity).PascalCase}Request", $"Remove{((Token)_entity).PascalCase}Response").Build())
                .WithDependency($"I{((Token)_dbContext).PascalCase}", "context")
                .WithDependency($"ILogger<Remove{((Token)_entity).PascalCase}Handler>", "logger")
                .WithMethod(new MethodBuilder().WithName("Handle").WithAsync(true)
                .WithReturnType(new TypeBuilder().WithGenericType("Task", $"Remove{((Token)_entity).PascalCase}Response").Build())
                .WithParameter(new ParameterBuilder($"Remove{((Token)_entity).PascalCase}Request", "request").Build())
                .WithParameter(new ParameterBuilder("CancellationToken", "cancellationToken").Build())
                .WithBody(new List<string>() {
                handlerBodyLine1,
                "",
                $"_context.{((Token)_entity).PascalCasePlural}.Remove({((Token)_entity).CamelCase});",
                "",
                "await _context.SaveChangesAsync(cancellationToken);",
                "",
                "return new ()",
                "{",
                $"{((Token)_entity).PascalCase} = {((Token)_entity).CamelCase}.ToDto()".Indent(1),
                "};"
                }).Build())
                .Class;

            new NamespaceBuilder($"Remove{((Token)_entity).PascalCase}", _context, _fileSystem)
                .WithDirectory(_directory)
                .WithNamespace(_namespace)
                .WithUsing("FluentValidation")
                .WithUsing("Microsoft.Extensions.Logging")
                .WithUsing("MediatR")
                .WithUsing("System.Threading")
                .WithUsing("System.Threading.Tasks")
                .WithUsing("Microsoft.EntityFrameworkCore")
                .WithUsing("System")
                .WithUsing("System.Linq")
                .WithUsing(_domainNamespace)
                .WithUsing($"{_applicationNamespace}.Interfaces")
                .WithClass(request)
                .WithClass(response)
                .WithClass(handler)
                .Build();
        }
    }
}
