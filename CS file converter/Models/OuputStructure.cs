namespace CSFileConverter.Models
{
    /// <summary>
    /// Defines the structure of output files
    /// </summary>
    public enum OutputStructure
    {
        /// <summary>All files in root directory with project name prefix</summary>
        SuperFlat,

        /// <summary>Files organized in project folders</summary>
        Flat,

        /// <summary>Preserves original directory structure</summary>
        Structured
    }
}