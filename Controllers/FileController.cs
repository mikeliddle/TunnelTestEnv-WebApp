using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
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

            return Ok();
        }

        // GET: api/File
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
            }

            return Directory.GetFiles(uploadPath).Select(e => Path.GetFileName(e)).ToArray();
        }
    }
}