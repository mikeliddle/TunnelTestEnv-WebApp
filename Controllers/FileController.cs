using Microsoft.AspNetCore.Mvc;
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
            var file = form.Files.FirstOrDefault();

            if (file == null)
            {
                return BadRequest("No file found");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            if (!Directory.Exists(uploadPath)) {
                Directory.CreateDirectory(uploadPath);
            }

            var path = Path.Combine(uploadPath, file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
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