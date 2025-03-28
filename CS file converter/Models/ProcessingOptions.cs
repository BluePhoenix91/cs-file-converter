namespace CSFileConverter.Models
{
    /// <summary>
    /// User-defined options for file processing
    /// </summary>
    public class ProcessingOptions
    {
        /// <summary>
        /// Gets or sets whether to include database migration files
        /// </summary>
        public bool IncludeMigrations { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include interface files
        /// </summary>
        public bool IncludeInterfaces { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include test files
        /// </summary>
        public bool IncludeTestFiles { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include generated files
        /// </summary>
        public bool IncludeGeneratedFiles { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to remove XML documentation
        /// </summary>
        public bool RemoveXmlDocs { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to remove empty lines
        /// </summary>
        public bool RemoveEmptyLines { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to optimize whitespace
        /// </summary>
        public bool RemoveWhitespace { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to remove regions
        /// </summary>
        public bool RemoveRegions { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the migrations folder
        /// </summary>
        public string MigrationsFolder { get; set; } = "Migrations";

        /// <summary>
        /// Gets or sets the output directory path
        /// </summary>
        public string OutputDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected output structure
        /// </summary>
        public OutputStructure SelectedOutputStructure { get; set; } = OutputStructure.Structured;
    }
}