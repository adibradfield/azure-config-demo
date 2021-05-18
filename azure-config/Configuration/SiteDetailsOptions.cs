namespace azure_config.Configuration
{
    public class SiteDetailsOptions{
        public const string SECTION_NAME = "SiteDetails";

        public string Name { get; set; }
        public bool IsUnderMaintenance { get; set; }
    }
}