using Microsoft.EntityFrameworkCore;
using MVC.FileUpload.Models;

namespace MVC.FileUpload.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<FileInDb> FilesInDb { get; set; }
        public DbSet<FileInFs> FilesInFs { get; set; }
    }
}