using Microsoft.AspNetCore.Mvc;
using SGRH.Domain.Abstractions.Storage;

namespace SGRH.Api.Controllers;

[ApiController]
[Route("api/test-s3")]
public class TestS3Controller : ControllerBase
{
    private readonly IFileStorage _storage;

    public TestS3Controller(IFileStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping(CancellationToken ct)
    {
        var path = new StoragePath("test/conexion-prueba.txt");
        var content = System.Text.Encoding.UTF8.GetBytes(
            $"Prueba de conexión SGRH – {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        // 1. Upload
        var upload = await _storage.UploadAsync(
            new FileUploadRequest(path, "text/plain", content), ct);

        if (!upload.Success)
            return StatusCode(500, new
            {
                Paso = "Upload",
                Ok = false,
                Error = upload.Error
            });

        // 2. Exists
        var exists = await _storage.ExistsAsync(path, ct);

        // 3. Delete (limpieza)
        await _storage.DeleteAsync(path, ct);

        return Ok(new
        {
            Conectado = true,
            Bucket = upload.StoredObject!.Path.Value,
            TamañoBytes = upload.StoredObject.SizeBytes,
            ETag = upload.StoredObject.ETag,
            ArchivoExistio = exists
        });
    }
}