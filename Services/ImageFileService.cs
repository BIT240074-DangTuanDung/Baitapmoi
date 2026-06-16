using BaithuchanhORM.Models;

namespace BaithuchanhORM.Services;

public sealed class ImageFileService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private readonly IWebHostEnvironment _env;

    public ImageFileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public (List<SavedImage> Saved, List<string> Errors) SaveBookImages(int bookId, IEnumerable<IFormFile> files)
    {
        var saved = new List<SavedImage>();
        var errors = new List<string>();

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "books", bookId.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add($"File \"{file.FileName}\" không hợp lệ. Chỉ chấp nhận JPG/PNG.");
                continue;
            }

            var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var physicalPath = Path.Combine(uploadDir, storedName);
            var relativePath = $"/uploads/books/{bookId}/{storedName}";

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            saved.Add(new SavedImage(file.FileName, relativePath));
        }

        return (saved, errors);
    }

    public void DeleteBookImages(IEnumerable<BookImage> images)
    {
        foreach (var image in images)
        {
            var physicalPath = Path.Combine(_env.WebRootPath, image.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
    }

    public void DeleteBookFolder(int bookId)
    {
        var folder = Path.Combine(_env.WebRootPath, "uploads", "books", bookId.ToString());
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}

public sealed record SavedImage(string FileName, string FilePath);
