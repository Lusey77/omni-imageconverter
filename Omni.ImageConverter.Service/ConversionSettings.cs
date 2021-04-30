using System.Collections.Generic;

namespace Omni.ImageConverter
{
    public class ConversionSettings
    {
        public const string Key = "ConversionSettings";

        public string FolderPath { get; set; }
        public IEnumerable<string> ConvertFrom { get; set; }
        public string ConvertTo { get; set; }
    }
}