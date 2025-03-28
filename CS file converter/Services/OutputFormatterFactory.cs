using CSFileConverter.Interfaces;
using CSFileConverter.Models;
using CSFileConverter.OutputFormatters;

namespace CSFileConverter.Services
{
    /// <summary>
    /// Factory for creating output formatters based on the selected structure
    /// </summary>
    public class OutputFormatterFactory
    {
        /// <summary>
        /// Gets an output formatter for the specified structure
        /// </summary>
        public IOutputFormatter GetFormatter(OutputStructure structure)
        {
            return structure switch
            {
                OutputStructure.SuperFlat => new SuperFlatOutputFormatter(),
                OutputStructure.Flat => new FlatOutputFormatter(),
                OutputStructure.Structured => new StructuredOutputFormatter(),
                _ => throw new ArgumentOutOfRangeException(nameof(structure), "Invalid output structure")
            };
        }
    }
}
