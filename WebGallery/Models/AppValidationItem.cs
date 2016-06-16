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

    public enum ValiadationStatus
    {
        Unknown,
        Pass,
        Fail
    }

    public class PackageValidationResult
    {
        public ValiadationStatus HashStatus { get; set; }
        public ValiadationStatus ManifestStatus { get; set; }

        public static PackageValidationResult CreateFail()
        {
            return new PackageValidationResult
            {
                HashStatus = ValiadationStatus.Fail,
                ManifestStatus = ValiadationStatus.Fail
            };
        }
    }

    public class ImageValidationResult
    {
        public ValiadationStatus TypeStatus { get; set; }
        public ValiadationStatus DimensionStatus { get; set; }

        public static ImageValidationResult CreateUnknown()
        {
            return new ImageValidationResult
            {
                TypeStatus = ValiadationStatus.Unknown,
                DimensionStatus = ValiadationStatus.Unknown
            };
        }
    }
}