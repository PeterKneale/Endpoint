using CommandLine;
using Endpoint.Core.Builders;
using Endpoint.Core.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;


namespace Endpoint.Application.Commands
{
    internal class Query
    {
        [Verb("query")]
        internal class Request : IRequest<Unit>
        {

            [Value(0)]
            public string Name { get; set; }

            [Value(1)]
            public string Entity { get; set; }

            [Option('d')]
            public string Directory { get; set; } = System.Environment.CurrentDirectory;
        }

        internal class Handler : IRequestHandler<Request, Unit>
        {
            private readonly ISettingsProvider _settingsProvder;

            public Handler(ISettingsProvider settingsProvider)
                => _settingsProvder = settingsProvider;

            public Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var settings = _settingsProvder.Get(request.Directory);

                /*                Create<QueryBuilder>((a, b, c, d) => new(a, b, c, d))
                                    .SetDirectory(request.Directory)
                                    .SetApplicationDirectory(settings.ApplicationDirectory)
                                    .SetRootNamespace(settings.RootNamespace)
                                    .SetApplicationNamespace(settings.ApplicationNamespace)
                                    .SetDomainNamespace(settings.DomainNamespace)
                                    .WithEntity(request.Entity)
                                    .WithName(request.Name)
                                    .WithDbContext(settings.DbContextName)
                                    .Build();*/

                return Task.FromResult(new Unit());
            }
        }
    }
}
