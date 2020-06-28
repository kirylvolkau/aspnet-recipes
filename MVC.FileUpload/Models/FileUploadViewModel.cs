using System.Collections.Generic;

namespace MVC.FileUpload.Models
{
    public class FileUploadViewModel
    {
        public List<FileInDb> FilesInDb { get; set; }
        public List<FileInFs> FilesInFs { get; set; }
    }
}