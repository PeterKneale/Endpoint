﻿using Endpoint.Cli.Services;
using Endpoint.Cli.ValueObjects;

namespace Endpoint.Cli.Builders
{
    public class BuilderBase<T>
        where T: class
    {
        protected readonly ICommandService _commandService;
        protected readonly ITemplateProcessor _templateProcessor;
        protected readonly ITemplateLocator _templateLocator;
        protected readonly IFileSystem _fileSystem;

        protected Token _directory = (Token)System.Environment.CurrentDirectory;
        protected Token _rootNamespace;
        protected Token _namespace;

        public BuilderBase(
            ICommandService commandService,
            ITemplateProcessor templateProcessor,
            ITemplateLocator templateLocator,
            IFileSystem fileSystem)
        {
            _commandService = commandService;
            _templateProcessor = templateProcessor;
            _templateLocator = templateLocator;
            _fileSystem = fileSystem;
        }

        public T SetDirectory(string directory)
        {
            _directory = (Token)directory;
            return this as T;
        }

        public T SetRootNamespace(string rootNamespace)
        {
            _rootNamespace = (Token)rootNamespace;
            return this as T;
        }

        public T SetNamespace(string entityName)
        {
            _namespace = (Token)entityName;
            return this as T;
        }
    }
}
