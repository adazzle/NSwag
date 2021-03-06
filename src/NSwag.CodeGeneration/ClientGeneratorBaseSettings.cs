//-----------------------------------------------------------------------
// <copyright file="ClientGeneratorBaseSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using NJsonSchema.CodeGeneration;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace NSwag.CodeGeneration
{
    /// <summary>Settings for the ClientGeneratorBase.</summary>
    public abstract class ClientGeneratorBaseSettings
    {
        /// <summary>Initializes a new instance of the <see cref="ClientGeneratorBaseSettings"/> class.</summary>
        protected ClientGeneratorBaseSettings()
        {
            GenerateClientClasses = true;
            GenerateDtoTypes = true;
            OperationNameGenerator = new MultipleClientsFromOperationIdOperationNameGenerator();
        }

        /// <summary>Gets the code generator settings.</summary>
        public abstract CodeGeneratorSettingsBase CodeGeneratorSettings { get; }

        /// <summary>Gets or sets the class name of the service client or controller.</summary>
        public string ClassName { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate DTO classes (default: true).</summary>
        public bool GenerateDtoTypes { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate interfaces for the client classes (default: false).</summary>
        public bool GenerateClientInterfaces { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate client types (default: true).</summary>
        public bool GenerateClientClasses { get; set; }

        /// <summary>Gets or sets the operation name generator.</summary>
        public IOperationNameGenerator OperationNameGenerator { get; set; }
    }
}