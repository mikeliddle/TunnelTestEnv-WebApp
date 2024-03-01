using Microsoft.EntityFrameworkCore;
using FormUpload.Models;
using sampleWebService;

namespace FormUpload.Contexts
{
    public class FormUploadContext : DbContext
    {
        public FormUploadContext(DbContextOptions<FormUploadContext> options)
            : base(options)
        {
        }

        public DbSet<FormData> FormData { get; set; }
    }
}