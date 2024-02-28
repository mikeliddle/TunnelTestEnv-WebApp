namespace FormUpload.Models
{
    public class FormUpload
    {
        public class Author
        {
            public string? Name { get; set; }
            public string? CountryOfOrigin { get; set; }
            public DateTime LastSubmissionTimestamp { get; set; }
        }
        public class FormData
        {
            public required string[] Files { get; set; }
            public required Dictionary<string, int> DessertVotes { get; set; }
            public required string CurrentWeather { get; set; }
            public required List<Author> AuthorsList { get; set; }
        }
        public class AppSettings
        {
            public required string UploadPath { get; set; }
        }
    }
}