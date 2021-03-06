﻿//-----------------------------------------------------------------------
// <copyright file="CSharpOperationModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NSwag.CodeGeneration.Models;

namespace NSwag.CodeGeneration.CSharp.Models
{
    /// <summary>The CSharp operation model.</summary>
    public class CSharpOperationModel : OperationModelBase<CSharpParameterModel, CSharpResponseModel>
    {
        private static readonly string[] ReservedKeywords =
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
            "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float",
            "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object",
            "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while"
        };

        private readonly SwaggerToCSharpGeneratorSettings _settings;
        private readonly SwaggerOperation _operation;
        private readonly SwaggerToCSharpGeneratorBase _generator;

        /// <summary>Initializes a new instance of the <see cref="CSharpOperationModel" /> class.</summary>
        /// <param name="operation">The operation.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="resolver">The resolver.</param>
        public CSharpOperationModel(
            SwaggerOperation operation,
            SwaggerToCSharpGeneratorSettings settings,
            SwaggerToCSharpGeneratorBase generator,
            SwaggerToCSharpTypeResolver resolver)
            : base(resolver.ExceptionSchema, operation, resolver, generator, settings)
        {
            _settings = settings;
            _operation = operation;
            _generator = generator;

            var parameters = _operation.ActualParameters.ToList();
            if (settings.GenerateOptionalParameters)
                parameters = parameters.OrderBy(p => !p.IsRequired).ToList();

            Parameters = parameters.Select(parameter =>
                new CSharpParameterModel(parameter.Name, GetParameterVariableName(parameter, _operation.Parameters), 
                    ResolveParameterType(parameter), parameter, parameters,
                    _settings.CodeGeneratorSettings,
                    _generator))
                .ToList();
        }

        /// <summary>Gets or sets the type of the result.</summary>
        public override string ResultType
        {
            get
            {
                if (UnwrappedResultType == "FileResponse")
                    return "System.Threading.Tasks.Task<FileResponse>";

                if (_settings != null && _settings.WrapResponses)
                    return UnwrappedResultType == "void"
                        ? "System.Threading.Tasks.Task<" + _settings.ResponseClass.Replace("{controller}", ControllerName) + ">"
                        : "System.Threading.Tasks.Task<" + _settings.ResponseClass.Replace("{controller}", ControllerName) + "<" + UnwrappedResultType + ">>";

                return UnwrappedResultType == "void"
                    ? "System.Threading.Tasks.Task"
                    : "System.Threading.Tasks.Task<" + UnwrappedResultType + ">";
            }
        }

        /// <summary>Gets or sets the type of the exception.</summary>
        public override string ExceptionType
        {
            get
            {
                if (_operation.Responses.Count(r => !HttpUtilities.IsSuccessStatusCode(r.Key)) != 1)
                    return "System.Exception";

                var response = _operation.Responses.Single(r => !HttpUtilities.IsSuccessStatusCode(r.Key)).Value;
                return _generator.GetTypeName(response.ActualResponseSchema, response.IsNullable(_settings.CodeGeneratorSettings.NullHandling), "Exception");
            }
        }

        /// <summary>Gets the name of the parameter variable.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="allParameters">All parameters.</param>
        /// <returns>The parameter variable name.</returns>
        protected override string GetParameterVariableName(SwaggerParameter parameter, IEnumerable<SwaggerParameter> allParameters)
        {
            var name = base.GetParameterVariableName(parameter, allParameters);
            return ReservedKeywords.Contains(name) ? "@" + name : name;
        }

        /// <summary>Resolves the type of the parameter.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The parameter type name.</returns>
        protected override string ResolveParameterType(SwaggerParameter parameter)
        {
            var schema = parameter.ActualSchema;
            if (schema.Type == JsonObjectType.File)
            {
                if (parameter.CollectionFormat == SwaggerParameterCollectionFormat.Multi && !schema.Type.HasFlag(JsonObjectType.Array))
                    return "System.Collections.Generic.IEnumerable<FileParameter>";

                return "FileParameter";
            }

            return base.ResolveParameterType(parameter)
                .Replace(_settings.CSharpGeneratorSettings.ArrayType + "<", "System.Collections.Generic.IEnumerable<")
                .Replace(_settings.CSharpGeneratorSettings.DictionaryType + "<", "System.Collections.Generic.IDictionary<");
        }

        /// <summary>Creates the response model.</summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="response">The response.</param>
        /// <param name="exceptionSchema">The exception schema.</param>
        /// <param name="generator">The generator.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        protected override CSharpResponseModel CreateResponseModel(string statusCode, SwaggerResponse response, JsonSchema4 exceptionSchema, IClientGenerator generator, ClientGeneratorBaseSettings settings)
        {
            return new CSharpResponseModel(statusCode, response, response == GetSuccessResponse(), exceptionSchema, generator, settings.CodeGeneratorSettings);
        }
    }
}
