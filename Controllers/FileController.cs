using Microsoft.AspNetCore.Mvc;
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
        public string[]? Files { get; set; }
        public Dictionary<string, int>? DessertVotes { get; set; }
        public string? CurrentWeather { get; set; }
        public List<Author>? AuthorsList { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        
        private const string FilePath = "formDataStorage.json";
        private Dictionary<string, int>? dessertVotes = new Dictionary<string, int>();
        private string? currentWeather = "clear";
        private List<Author> authorsList = new List<Author> {};
        private dynamic allData;

        private async Task InitializeFormTypes()
        {
            if (System.IO.File.Exists(FilePath))
            {
                var json = await System.IO.File.ReadAllTextAsync(FilePath);
                Console.WriteLine(json);
                allData = JsonSerializer.Deserialize<FormData>(json);
                dessertVotes = allData?.DessertVotes ?? new Dictionary<string, int>();
                authorsList = allData?.AuthorsList ?? new List<Author>();
                currentWeather = allData?.CurrentWeather ?? "clear";
            }
            else
            {
                dessertVotes = new Dictionary<string, int>()
                {
                    {"Red Velvet Cake", 0},
                    {"Peach Pie", 0},
                    {"Lemon Bars", 0}
                };
                authorsList = new List<Author>();
                currentWeather = "clear";
            }
        }

        public FileController()
        {
            InitializeFormTypes().Wait();
        }

        // POST: api/File
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            var form = await this.Request.ReadFormAsync();
            var files = form.Files;

            if (files.Count == 0)
            {
                return BadRequest("No files found");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files)
            {
                var path = Path.Combine(uploadPath, file.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            string uploaderName = form["uploader"];
            string desserts = form["dessert"];  // Assuming this is a string like "dessert1,dessert2,dessert3"
            string weather = form["weather"];
            string country = form["country"];

            // Split the desserts string into an array of individual desserts
            string[] dessertArray = desserts.Split(',');

            // Loop through the array and add each dessert individually
            foreach (string dessert in dessertArray)
            {
                // Increment a vote counter for the type of dessert
                if (this.dessertVotes.ContainsKey(dessert))
                {
                    this.dessertVotes[dessert] += 1;
                }
                else
                {
                    this.dessertVotes[dessert] = 1;
                }
            }
            // Update the weather
            currentWeather = weather;

            // If author is new add them, their country of origin and update latest submission timestamp
            var author = authorsList.Find(a => a.Name == uploaderName);
            if (author == null)
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
                author.LastSubmissionTimestamp = DateTime.Now;
            }

            // Update allData before serializing it
            allData = new
            {
                DessertVotes = this.dessertVotes,
                CurrentWeather = currentWeather,
                AuthorsList = authorsList
            };

            // Serialize the allData object to JSON
            var json = JsonSerializer.Serialize(allData);
            Console.WriteLine(json);
            // Write the JSON to the file
            await System.IO.File.WriteAllTextAsync(FilePath, json);

            return Ok();
        }

        // GET: api/File
        [HttpGet]
        public ActionResult<FormData> Get()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
            }

            var files = Directory.GetFiles(uploadPath).Select(e => Path.GetFileName(e)).ToArray();

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