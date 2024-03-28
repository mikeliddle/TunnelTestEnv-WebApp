using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FormUpload.Models;
using FormUpload.Contexts;
using Microsoft.IdentityModel.Tokens;


namespace FormUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormUploadController : ControllerBase
    {
        private readonly FormUploadContext _context;
        private string uploadPath;
        private FormData? allData;

        public FormUploadController(FormUploadContext context)
        {
            _context = context;
            uploadPath = "uploads";
            try
            {
                InitializeFormTypes().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was a problem initializing form types: {e.Message}");
            }
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        private async Task InitializeFormTypes()
        {
            // Initialize the upload directory
            EnsureDirectoryExists(uploadPath);
            
            // Read from the database instead of a file
            allData = await _context.FormData
                .Include(fd => fd.DessertVotes)
                .Include(fd => fd.AuthorsList)
                .FirstOrDefaultAsync();
            
            if (allData == null) {
                // If there is no data in the database, create a new FormData object
                allData = new FormData(true);
                _context.FormData.Add(allData);
                await _context.SaveChangesAsync();
            }
        }

        // POST: api/formupload
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            var form = await this.Request.ReadFormAsync();

            // If the form is empty, return a 400 Bad Request
            if (form.IsNullOrEmpty() || form.Count == 0)
            {
                return BadRequest("Form is empty");
            }
            // If allData is null, initialize it
            if (allData == null) {
                await InitializeFormTypes();
            }

            // Handle file uploads
            IFormFileCollection files = new FormFileCollection();
            var allowedExtensions = new List<string> { ".pdf",".doc",".rtf",".jpg", ".png", ".gif", ".bmp", ".jpeg", ".txt"}; 

            if (form.Files.Count > 0) {

                files = form.Files;
                foreach (IFormFile file in files)
                {
                    // File sanitization 
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        continue; // Skip the file if the extension is not allowed
                    }

                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string path = Path.Combine(uploadPath, file.FileName);

                    // Check if a file with the same name already exists
                    if (System.IO.File.Exists(path))
                    {
                        // If a file with the same name exists, append a unique identifier to the file name
                        fileName = $"{fileName}_{Guid.NewGuid()}";
                        path = Path.Combine(uploadPath, fileName + fileExtension);
                    }

                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            try {
                string? uploaderName = form.ContainsKey("uploader") ? form["uploader"] : string.Empty;
                string? desserts = form.ContainsKey("dessert") ? form["dessert"] : string.Empty;
                string? weather = form.ContainsKey("weather") ? form["weather"] : string.Empty;
                string? country = form.ContainsKey("country") ? form["country"] : string.Empty;
                
                if (desserts != null) 
                { 
                    // Split the desserts string into an array of individual desserts
                    string[] strings = desserts.Split(',');
                    string[] dessertArray = strings.Select(s => s.Trim()).ToArray();

                    // Loop through the array and increment the vote for each dessert individually
                    foreach (var dessert in dessertArray)
                    {
                        if (!string.IsNullOrEmpty(dessert)) 
                        {
                            allData.IncrementDessertVote(dessert);
                        }
                    }
                }
                // Weather
                if (!string.IsNullOrEmpty(weather)) {
                    allData.UpdateWeather(weather);
                }

                // Author
                if (!string.IsNullOrEmpty(uploaderName) && !string.IsNullOrEmpty(country)) 
                {
                    allData.AddAuthor(uploaderName, country);
                }

                _context.FormData.Update(allData);
                await _context.SaveChangesAsync();
                return Ok();
            } catch (Exception e) {
                // Handle exceptions
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        public async Task<ActionResult<FormData>> GetAsync()
        {
            // Instead of reading from a file, get the data from the database
            var storedFormData = await _context.FormData.ToListAsync();

            if (storedFormData == null || !storedFormData.Any())
            {
                return NotFound();
            }

            // Get the list of files in the "uploads" directory
            var files = Directory.GetFiles(uploadPath).Select(Path.GetFileName).ToList();

            // Add the list of files to the response
            var response = new
            {
                storedFormData,
                files
            };

            return Ok(response);
        }

        // DELETE: api/formupload
        [HttpDelete]
        public async Task<ActionResult> ResetAll()
        {
            // Get the FormData object
            var formData = await _context.FormData.FirstOrDefaultAsync();

            if (formData != null)
            {
                // Reset the properties of the FormData object
                formData.Reset();

                // Update the FormData object in the context and save changes to the database
                _context.FormData.Update(formData);
                await _context.SaveChangesAsync();
            }

            // Delete all files from the "uploads" directory
            DirectoryInfo di = new DirectoryInfo(uploadPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // Return a success response
            return Ok("All data and files have been reset.");
        }
    }
}

