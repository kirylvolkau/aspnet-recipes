using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.FileUpload.Data;
using MVC.FileUpload.Models;

namespace MVC.FileUpload.Controllers
{
    public class FileController : Controller
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }
        // GET
        public async Task<IActionResult> Index()
        {
            var model = await LoadFiles();
            return View(model);
        }

        private async Task<FileUploadViewModel> LoadFiles()
        {
            var model = new FileUploadViewModel()
            {
                FilesInDb = await _context.FilesInDb.ToListAsync(),
                FilesInFs = await _context.FilesInFs.ToListAsync()
            };
            return model;
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadToFileSystem(List<IFormFile> files, string description)
        {
            foreach(var file in files)
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(),"Files");
                bool basePathExists = System.IO.Directory.Exists(basePath);
                if (!basePathExists)
                {
                    Directory.CreateDirectory(basePath);
                }
                
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(basePath, file.FileName);
                var extension = Path.GetExtension(file.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var fileModel = new FileInFs()
                    {
                        Created = DateTime.UtcNow,
                        FileType = file.ContentType,
                        Extension = extension,
                        Name = fileName,
                        Description = description,
                        FilePath = filePath
                    };
                    _context.FilesInFs.Add(fileModel);
                    _context.SaveChanges();
                }
            }

            TempData["Message"] = "File successfully uploaded to File System.";
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> DownloadFileFromFileSystem(int id)
        {

            var file = await _context.FilesInFs.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null)
            {
                return null;
            }
            var memory = new MemoryStream();
            using(var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, file.FileType, file.Name + file.Extension);
        }
        
        public async Task<IActionResult> DeleteFileFromFileSystem(int id)
        {

            var file = await _context.FilesInFs.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (file == null)
            {
                return null;
            }
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            } 
            _context.FilesInFs.Remove(file);
            _context.SaveChanges();
            TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from File System.";
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public async Task<IActionResult> UploadToDatabase(List<IFormFile> files, string description)
        {
            foreach(var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileModel = new FileInDb()
                {
                    Created = DateTime.UtcNow,
                    FileType = file.ContentType,
                    Extension = extension,
                    Name = fileName,
                    Description = description
                };
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    fileModel.Data = dataStream.ToArray();
                }
                _context.FilesInDb.Add(fileModel);
                _context.SaveChanges();
            }

            TempData["Message"] = "File successfully uploaded to InMemoryDb.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadFileFromDatabase(int id)
        {
            var file = await _context.FilesInDb.Where(f => f.Id == id).FirstOrDefaultAsync();
            return file is null ? null : File(file.Data, file.FileType, file.Name + file.Extension);
        }

        public async Task<IActionResult> DeleteFileFromDatabase(int id)
        {
            var file = await _context.FilesInDb.Where(f => f.Id == id).FirstOrDefaultAsync();
            _context.FilesInDb.Remove(file);
            _context.SaveChanges();
            TempData["Message"] = $"Removed {file.Name + file.Extension} successfully from Database.";
            return RedirectToAction("Index");
        }
    }
    
    
}