namespace AuthSystem.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadProfilePictureAsync(IFormFile file);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadDirectory;

        public FileUploadService(IWebHostEnvironment env)
        {
            _uploadDirectory = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(_uploadDirectory);
        }

        public async Task<string> UploadProfilePictureAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_uploadDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
