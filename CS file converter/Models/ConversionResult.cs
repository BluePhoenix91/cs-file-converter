namespace CSFileConverter.Models
{
    /// <summary>
    /// Results of a file conversion operation
    /// </summary>
    public class ConversionResult
    {
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }
}
