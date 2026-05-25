using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/files")]
[Authorize(Roles = "admin")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FileController(IWebHostEnvironment env) => _env = env;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPEG, PNG, WebP, and GIF are allowed" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File must be under 5MB" });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "courts");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/images/courts/{fileName}";
        return Ok(new { url });
    }

    [HttpDelete]
    public IActionResult Delete([FromBody] DeleteFileRequest request)
    {
        if (string.IsNullOrEmpty(request.Url) || !request.Url.StartsWith("/images/"))
            return BadRequest(new { message = "Invalid file path" });

        var filePath = Path.Combine(_env.WebRootPath, request.Url.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            return Ok(new { message = "File deleted" });
        }

        return NotFound(new { message = "File not found" });
    }
}

public record DeleteFileRequest(string Url);