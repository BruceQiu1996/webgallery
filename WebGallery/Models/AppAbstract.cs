namespace WebGallery.Models
{
    public class AppAbstract
    {
        public System.DateTime ReleaseDate { get; set; }
        public int SubmissionId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string BriefDescription { get; set; }
        public string LogoUrl { get; set; }
    }
}