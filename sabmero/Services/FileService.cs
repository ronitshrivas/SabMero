namespace sabmero.Services;

// Saves uploaded images/documents to wwwroot/uploads/<subfolder>/ on the server,
// and returns a relative URL the Flutter app can load directly.
//
// NOTE on Railway: the container filesystem is EPHEMERAL — files saved here are lost
// when the app restarts/redeploys. That's fine for testing. For permanent storage,
// switch SaveAsync to upload to a cloud bucket (Cloudinary, AWS S3, Supabase Storage)
// and return the public URL it gives back. Callers won't need to change.
public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileService> _logger;

    // Only allow safe image/document types and cap size at 5 MB.
    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };
    private const long MaxBytes = 5 * 1024 * 1024;

    public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, string? Path)> SaveAsync(IFormFile file, string subfolder)
    {
        if (file == null || file.Length == 0)
            return (false, "No file uploaded.", null);
        if (file.Length > MaxBytes)
            return (false, "File too large (max 5 MB).", null);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return (false, "Unsupported file type. Allowed: jpg, png, webp, gif, pdf.", null);

        // Sanitize the subfolder so callers can't escape the uploads directory.
        var safeSub = string.Concat(subfolder.Where(char.IsLetterOrDigit));
        if (string.IsNullOrEmpty(safeSub)) safeSub = "misc";

        // Save under {ContentRoot}/wwwroot/uploads/... — the SAME absolute base
        // that UseStaticFiles serves from in Program.cs. Relying on WebRootPath
        // is unsafe because it's null in a published/Docker build.
        var webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", safeSub);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{safeSub}/{fileName}";
        _logger.LogInformation("File saved: {Path}", relativeUrl);

        return (true, "File uploaded.", relativeUrl);
    }
}