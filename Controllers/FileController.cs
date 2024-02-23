using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic; 
using System.Globalization;

namespace TodoApi.Controllers
{
    public class Author
    {
        public string Name { get; set; }
        public string CountryOfOrigin { get; set; }
        public DateTime LastSubmissionTimestamp { get; set; }
    }
    public class FormData
    {
        public string[] Files { get; set; }
        public Dictionary<string, int> DessertVotes { get; set; }
        public string CurrentWeather { get; set; }
        public List<Author> AuthorsList { get; set; }
    }
    public class AppSettings
    {
        public string UploadPath { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private string uploadPath;
        private const string FilePath = "formDataStorage.json";
        private Dictionary<string, int> dessertVotes = new Dictionary<string, int>();
        private string? currentWeather = "clear";
        private List<Author> authorsList = new List<Author> {};
        private FormData allData;
        
        private FormData CreateFormData()
        {
            return new FormData
            {
                Files = Directory.GetFiles(uploadPath).Select(e => Path.GetFileName(e)).ToArray(),
                DessertVotes = dessertVotes,
                CurrentWeather = currentWeather ?? "clear",
                AuthorsList = authorsList
            };
        }
        private async Task InitializeFormTypes()
        {
            dessertVotes = new Dictionary<string, int>()
            {
                {"Red Velvet Cake", 0},
                {"Peach Pie", 0},
                {"Lemon Bars", 0}
            };
            authorsList = new List<Author>();
            currentWeather = "clear";

            if (System.IO.File.Exists(FilePath))
            {
                var json = await System.IO.File.ReadAllTextAsync(FilePath);
                Console.WriteLine(json);
                allData = JsonSerializer.Deserialize<FormData>(json);
                dessertVotes = allData?.DessertVotes ?? dessertVotes;
                authorsList = allData?.AuthorsList ?? authorsList;
                currentWeather = allData?.CurrentWeather ?? currentWeather;
            } 
            else 
            {
                allData = new FormData
                {
                    Files = new string[] {},
                    DessertVotes = dessertVotes,
                    CurrentWeather = currentWeather,
                    AuthorsList = authorsList
                };
                var json = JsonSerializer.Serialize(allData);
                await System.IO.File.WriteAllTextAsync(FilePath, json);
            }
        }
        private void IncrementDessertVote(string dessert) {
            if (this.dessertVotes.ContainsKey(dessert))
            {
                this.dessertVotes[dessert] += 1;
            }
            else
            {
                this.dessertVotes[dessert] = 1;
            }
        }
        public FileController(IOptions<AppSettings> settings)
        {
            uploadPath = string.IsNullOrEmpty(settings?.Value?.UploadPath) 
                ? "wwwroot/upload" 
                : settings.Value.UploadPath;

            try
            {
                InitializeFormTypes().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // POST: api/File
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            IFormCollection form = await this.Request.ReadFormAsync();
            IFormFileCollection files = form.Files;

             if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (IFormFile file in files)
            {
                // File sanitization 
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = new List<string> { ".jpg", ".png", ".gif", ".bmp", ".jpeg", ".txt"}; 

                if (!allowedExtensions.Contains(fileExtension))
                {
                    continue;
                }

                string path = Path.Combine(uploadPath, file.FileName);

                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }


            string uploaderName = form.ContainsKey("uploader") ? form["uploader"] : string.Empty;
            string desserts = form.ContainsKey("dessert") ? form["dessert"] : string.Empty;
            string weather = form.ContainsKey("weather") ? form["weather"] : string.Empty;
            string country = form.ContainsKey("country") ? form["country"] : string.Empty;

            // Split the desserts string into an array of individual desserts
            string[] dessertArray = desserts.Split(',');

            // Loop through the array and add each dessert individually
            foreach (var dessert in dessertArray)
            {
                if (string.IsNullOrEmpty(dessert)) {
                    continue;
                } 
                else {
                    IncrementDessertVote(dessert);
                }
            }
            
            // Update the weather
            currentWeather = weather ?? "clear";

            // If author is new add them, their country of origin and update latest submission timestamp
            if (string.IsNullOrEmpty(uploaderName)) {
                Console.WriteLine("Uploader name is empty. Skipping author addition.");
            } else {
                var returnAuthor = authorsList.Find(a => a.Name == uploaderName);
                if (returnAuthor == null)
                {
                    authorsList.Add(new Author 
                    { 
                        Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(uploaderName),
                        CountryOfOrigin = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country),
                        LastSubmissionTimestamp = DateTime.Now
                    }); 
                }
                else
                {
                    returnAuthor.LastSubmissionTimestamp = DateTime.Now;
                    returnAuthor.CountryOfOrigin = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country);
                }
            }

            // Update allData before serializing it
            allData = CreateFormData();

            // Serialize the allData object to JSON
            var json = JsonSerializer.Serialize(allData);

            // Write the JSON to the file
            await System.IO.File.WriteAllTextAsync(FilePath, json);

            return Ok();
        }

        // GET: api/File
        [HttpGet]
        public ActionResult<FormData> Get()
        {            
            if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
                // save the directory path to the appsettings.json
                var json = JsonSerializer.Serialize(new { UploadPath = uploadPath });
                System.IO.File.WriteAllText("appsettings.json", json);
            }

            string[] files = Directory.GetFiles(uploadPath).Select(e => Path.GetFileName(e)).ToArray();

            return new FormData
            {
                Files = files,
                DessertVotes = dessertVotes,
                CurrentWeather = currentWeather,
                AuthorsList = authorsList
            };
        }
    }

}

