using CommandLine;
using Endpoint.Core.Services;
using Endpoint.Core.ValueObjects;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Application.Commands
{
    internal class ApiTest
    {
        [Verb("api-test")]
        internal class Request : IRequest<Unit>
        {
            [Value(0)]
            public string EntityName { get; set; }

            [Option('d', Required = false)]
            public string Directory { get; private set; } = System.Environment.CurrentDirectory;
        }

        internal class Handler : IRequestHandler<Request, Unit>
        {
            private readonly ITemplateLocator _templateLocator;
            private readonly ITemplateProcessor _templateProcessor;
            private readonly IFileSystem _fileSystem;
            public Handler(ITemplateLocator templateLocator, ITemplateProcessor templateProcessor, IFileSystem fileSystem)
            {
                _templateLocator = templateLocator;
                _templateProcessor = templateProcessor;
                _fileSystem = fileSystem;
            }
            public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var template = _templateLocator.Get(nameof(ApiTest));

                var tokens = new TokensBuilder()
                    .With(nameof(request.EntityName), (Token)request.EntityName)
                    .Build();

                var contents = _templateProcessor.Process(template, tokens);

                _fileSystem.WriteAllLines($@"{request.Directory}/{((Token)request.EntityName).PascalCase}ControllerTests.cs", contents);
                return Task.FromResult(new Unit());
            }
        }
    }
}
