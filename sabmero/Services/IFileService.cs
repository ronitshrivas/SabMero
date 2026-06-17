namespace sabmero.Services;

// Contract for handling uploaded files (images / documents).
public interface IFileService
{
    // Saves the uploaded file under a subfolder (e.g. "products", "damage", "kyc")
    // and returns the public relative URL path the app can use (e.g. "/uploads/products/abc.jpg").
    Task<(bool Success, string Message, string? Path)> SaveAsync(IFormFile file, string subfolder);
}