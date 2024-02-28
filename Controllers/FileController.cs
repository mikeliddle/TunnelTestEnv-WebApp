using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic; 
using System.Globalization;
using static FormUpload.Models.FormUpload;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private string uploadPath;
        private const string FilePath = "formDataStorage.json";
        private Dictionary<string, int> dessertVotes = new Dictionary<string, int>();
        private string currentWeather = "clear";
        private List<Author> authorsList = new List<Author> {};
        private FormData allData;
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        private async Task<FormData> ReadFormDataFromFile()
        {
            var json = await System.IO.File.ReadAllTextAsync(FilePath);
            if (string.IsNullOrEmpty(json))
            {
                throw new Exception("File is empty");
            }
            return JsonSerializer.Deserialize<FormData>(json);
        }
        private async Task WriteFormDataToFile(FormData data)
        {
            var json = JsonSerializer.Serialize(data);
            await System.IO.File.WriteAllTextAsync(FilePath, json);
        }
        private FormData CreateFormData(IFormFileCollection files, Dictionary<string, int> dessertVotes, string currentWeather, List<Author> authorsList)
        {
            return new FormData
            {
                Files = files.Select(f => f.FileName).ToArray(),
                DessertVotes = dessertVotes,
                CurrentWeather = currentWeather ?? "clear",
                AuthorsList = authorsList
            };
        }
        private async Task InitializeFormTypes()
        {
            // Initialize the upload directory
            EnsureDirectoryExists(uploadPath);

            if (System.IO.File.Exists(FilePath))
            {
                allData = await ReadFormDataFromFile();
                dessertVotes = allData?.DessertVotes ?? dessertVotes;
                authorsList = allData?.AuthorsList ?? authorsList;
                currentWeather = allData?.CurrentWeather ?? currentWeather;
            } 
            else 
            {
                allData = new FormData
                {
                    Files = new string[] {},
                    DessertVotes = new Dictionary<string, int>()
                        {
                            {"Red Velvet Cake", 0},
                            {"Peach Pie", 0},
                            {"Lemon Bars", 0}
                        },
                    CurrentWeather = "clear",
                    AuthorsList = new List<Author> {}
                };
                await WriteFormDataToFile(allData);
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
            uploadPath = settings?.Value?.UploadPath ?? "uploads";
            try
            {
                InitializeFormTypes().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was a problem initializing form types: {e.Message}");
            }
        }

        // POST: api/File
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            var form = await this.Request.ReadFormAsync();
            if (form.IsNullOrEmpty()) {
                return BadRequest();
            }
            IFormFileCollection files = new FormFileCollection();
            if (form.Files.Count > 0) {
                files = form.Files;

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
            }

            string? uploaderName = form.ContainsKey("uploader") ? form["uploader"] : string.Empty;
            string? desserts = form.ContainsKey("dessert") ? form["dessert"] : string.Empty;
            string? weather = form.ContainsKey("weather") ? form["weather"] : string.Empty;
            string? country = form.ContainsKey("country") ? form["country"] : string.Empty;

            if (desserts != null) { 
                // Split the desserts string into an array of individual desserts
                string[] strings = desserts.Split(',');
                string[] dessertArray = strings.Select(s => s.Trim()).ToArray();

                // Loop through the array and add each dessert individually
                foreach (var dessert in dessertArray)
                {
                    if (!string.IsNullOrEmpty(dessert)) {
                        IncrementDessertVote(dessert);
                    }
                }
            }
            
            // Update the weather
            if (string.IsNullOrEmpty(weather)) {
                currentWeather = "clear";
            } else {
                currentWeather = weather;
            }

            // If author is new add them, their country of origin and update latest submission timestamp
            if (string.IsNullOrEmpty(uploaderName)) {
                Console.WriteLine("Uploader name is empty. Skipping author addition.");
            } else {
                var returnAuthor = authorsList.Find(a => a.Name == uploaderName);
                if (returnAuthor == null && country != null)
                {
                    authorsList.Add(new Author 
                    { 
                        Name = uploaderName != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(uploaderName) : string.Empty,
                        CountryOfOrigin = country != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country) : string.Empty,
                        LastSubmissionTimestamp = DateTime.Now
                    }); 
                }
                else
                {
                    returnAuthor.LastSubmissionTimestamp = DateTime.Now;
                    returnAuthor.CountryOfOrigin = country != null ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(country) : string.Empty;
                }
            }

            // Update allData before serializing it
            allData = CreateFormData(files, dessertVotes, currentWeather, authorsList);

            // Write the updated data to the file
            await WriteFormDataToFile(allData);

            return Ok();
        }
        // GET: api/File
        [HttpGet]
        public async Task<ActionResult<FormData>> GetAsync()
        {
            EnsureDirectoryExists(uploadPath);
            try
            {
                // Read the file and return the data
                allData = await ReadFormDataFromFile();
                if (allData == null)
                {
                    return NotFound();
                }
                return allData;
            }
            catch (Exception e)
            {
                // Log the exception and return a 500 Internal Server Error status
                Console.WriteLine(e);
                return StatusCode(500, "There was a problem reading the form data from the file.");
            }          
        }
    }
}

