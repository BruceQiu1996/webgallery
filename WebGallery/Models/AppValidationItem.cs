namespace WebGallery.Models
{
    public class AppValidationItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string KeyInGlobalizationResources { get { return $"{Name}Status"; } }
        public string LanguageAndCountryCode { get; set; }
        public AppValidationItemType Type { get; set; }
    }

    public enum AppValidationItemType
    {
        Url,
        Package,
        Image
    }

    public enum ValidationResult
    {
        Pass = 0,
        Fail = 1,
        Unknown = 2
    }

    public class ImageValidationResult
    {
        public ValidationResult TypeStatus { get; set; }
        public ValidationResult DimensionStatus { get; set; }

        public static ImageValidationResult CreateUnknown()
        {
            return new ImageValidationResult
            {
                TypeStatus = ValidationResult.Unknown,
                DimensionStatus = ValidationResult.Unknown
            };
        }
    }
}