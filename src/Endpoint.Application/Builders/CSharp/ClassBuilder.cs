using Endpoint.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Endpoint.Application.Extensions;
using static Endpoint.Application.Enums.AccessModifier;
using Endpoint.Application.Builders.CSharp;
using System;

namespace Endpoint.Application.Builders
{
    public class ClassBuilder
    {
        private readonly IContext _context;
        private readonly IFileSystem _fileSystem;
        private List<string> _content;
        private string _name;
        private string _namespace;
        private string[] _constructor;
        private string _directory;
        private List<string> _usings;
        private List<KeyValuePair<string,string>> _dependencies;
        private List<string> _attributes;
        private List<string[]> _methods;
        private int _indent = 0;
        private List<string> _properties;
        private string _baseClass;
        private List<KeyValuePair<string, string>> _baseDependencies;
        private List<string> _interfaces;
        private string _type;
        private bool _static;
        
        private void Indent() { _indent++; }
        private void Unindent() { _indent--; }

        public ClassBuilder(string name, IContext context, IFileSystem fileSystem, string type = "class")
        {
            _name = name;
            _context = context;
            _fileSystem = fileSystem;

            _content = new List<string>();
            _usings = new List<string>();
            _dependencies = new List<KeyValuePair<string, string>>();            
            _attributes = new List<string>();
            _methods = new List<string[]>();
            _properties = new List<string>();
            _baseDependencies = new List<KeyValuePair<string, string>>();
            _interfaces = new List<string>();
            _type = type;
            _static = false;

        }
        public ClassBuilder IsStatic(bool @static = true)
        {
            _static = @static;
            return this;
        }

        public ClassBuilder WithDirectory(string directory)
        {
            _directory = directory;
            return this;
        }

        public ClassBuilder WithBase(string baseClass)
        {
            _baseClass = baseClass;
            return this;
        }

        public ClassBuilder WithProperty(string property)
        {
            _properties.Add(property);

            return this;
        }
        public ClassBuilder WithUsing(string @using)
        {
            _usings.Add(@using);
            return this;
        }

        public ClassBuilder WithNamespace(string @namespace)
        {
            _namespace = @namespace;
            return this;
        }

        public ClassBuilder WithBaseDependency(string type, string name)
        {
            _baseDependencies.Add(new(type, name));
            return this;
        }

        public ClassBuilder WithInterface(string type)
        {
            _interfaces.Add(type);

            return this;
        }

        public ClassBuilder WithDependency(string type, string name)
        {
            _dependencies.Add(new (type,name));
            return this;
        }

        public ClassBuilder WithAttribute(string attribute)
        {
            _attributes.Add(attribute);
            return this;
        }

        public ClassBuilder WithConstructor(string[] constructor)
        {
            _constructor = constructor;
            return this;
        }

        public ClassBuilder WithMethod(string[] method)
        {
            _methods.Add(method);

            return this;
        }

        public ClassBuilder WithMethodSignature(string methodSignature)
        {
            _methods.Add(new string[1] { $"{methodSignature};" });
            return this;
        }

        private string GetInheritance()
        {
            StringBuilder inheritance = new StringBuilder();

            if(!string.IsNullOrEmpty(_baseClass))
            {
                inheritance.Append($": {_baseClass}");

                if (_interfaces.Count > 0)
                {
                    inheritance.Append($", {string.Join(", ", _interfaces)}");
                }
            }

            if (string.IsNullOrEmpty(_baseClass) && _interfaces.Count > 0)
            {
                inheritance.Append($": {string.Join(", ", _interfaces)}");
            }

            return inheritance.ToString();
        }

        public void Build()
        {

            if (_usings.Count > 0)
            {
                foreach (var @using in _usings)
                {
                    _content.Add($"using {@using};");
                }

                _content.Add("");
            }

            _content.Add($"namespace {_namespace}");
            _content.Add("{");

            Indent();

            foreach (var attribute in _attributes)
            {
                _content.Add(attribute.Indent(_indent));
            }

            if (_type == "class" && _static)
                _content.Add($"public static class {_name}{GetInheritance()}".Indent(_indent));

            if (_type == "class" && !_static)
                _content.Add($"public class {_name}{GetInheritance()}".Indent(_indent));

            if (_type == "interface")
                _content.Add($"public interface I{_name}{GetInheritance()}".Indent(_indent));

            if (_methods.Count == 0 && _dependencies.Count == 0 && _properties.Count == 0)
            {
                _content[_content.Count - 1] = _content[_content.Count - 1] + " { }";
            }
            else
            {
                _content.Add("{".Indent(_indent));

                Indent();

                if (_properties.Count > 0)
                {
                    foreach (var property in _properties)
                    {
                        _content.Add(property.Indent(_indent));
                    }
                }

                var ctorBuilder = new ConstructorBuilder(_name)
                    .WithIndent(_indent)
                    .WithParameters(new(_dependencies))
                    .WithBaseParameters(new(_baseDependencies))
                    .WithAccessModifier(Public);

                if (_dependencies.Count > 0 || _baseDependencies.Count > 0)
                {
                    if (_dependencies.Count > 0)
                    {
                        foreach (var dependency in _dependencies)
                        {
                            _content.Add(new FieldBuilder(dependency.Key, dependency.Value).WithReadonly().WithIndent(_indent).WithAccessModifier(Private).Build());
                        }

                        _content.Add("");
                    }

                    foreach (var line in ctorBuilder.Build())
                    {
                        _content.Add(line);
                    }
                }

                if (_methods.Count > 0)
                {
                    foreach (var method in _methods)
                    {
                        foreach (var line in method)
                        {
                            _content.Add(line.Indent(_indent));
                        }

                        _content.Add("".Indent(_indent));
                    }
                }

                Unindent();

                _content.Add("}".Indent(_indent));

            }

            Unindent();

            _content.Add("}".Indent(_indent));

            var path = $"{_directory}{Path.DirectorySeparatorChar}{_name}.cs";

            _context.Add(path, _content.ToArray());

            _fileSystem.WriteAllLines(path, _content.ToArray());
        }
    }
}
