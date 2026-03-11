using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SGRH.Domain.Abstractions.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.StorageS3;

// Implementa IFileStorage usando Amazon S3.
// Las credenciales se resuelven por la cadena estándar de AWS SDK
// (env vars → ~/.aws/credentials → IAM role en EC2/ECS).
public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public S3FileStorage(IAmazonS3 s3, IOptions<S3Options> options)
    {
        _s3 = s3;
        _bucket = options.Value.BucketName;
    }

    // ── Upload ────────────────────────────────────────────────────────────
    public async Task<FileUploadResult> UploadAsync(
        FileUploadRequest request, CancellationToken ct = default)
    {
        try
        {
            using var stream = new MemoryStream(request.Content);

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = request.Path.Value,
                InputStream = stream,
                ContentType = request.ContentType,
                // Fuerza lectura del stream completo para obtener ETag
                UseChunkEncoding = false
            };

            var response = await _s3.PutObjectAsync(putRequest, ct);

            var storedObject = new StoredObject(
                path: request.Path,
                contentType: request.ContentType,
                sizeBytes: request.Content.LongLength,
                eTag: response.ETag,
                lastModifiedUtc: DateTimeOffset.UtcNow);

            return new FileUploadResult(Success: true, StoredObject: storedObject);
        }
        catch (AmazonS3Exception ex)
        {
            return new FileUploadResult(
                Success: false,
                Error: $"S3 error [{ex.ErrorCode}]: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new FileUploadResult(
                Success: false,
                Error: $"Error inesperado al subir archivo: {ex.Message}");
        }
    }

    // ── Download ──────────────────────────────────────────────────────────
    public async Task<FileDownloadResult> DownloadAsync(
        StoragePath path, CancellationToken ct = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = path.Value
            };

            using var response = await _s3.GetObjectAsync(getRequest, ct);
            using var ms = new MemoryStream();

            await response.ResponseStream.CopyToAsync(ms, ct);

            return new FileDownloadResult(
                Success: true,
                ContentType: response.Headers.ContentType,
                Content: ms.ToArray());
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new FileDownloadResult(
                Success: false,
                Error: $"Archivo no encontrado: {path.Value}");
        }
        catch (AmazonS3Exception ex)
        {
            return new FileDownloadResult(
                Success: false,
                Error: $"S3 error [{ex.ErrorCode}]: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new FileDownloadResult(
                Success: false,
                Error: $"Error inesperado al descargar archivo: {ex.Message}");
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(
        StoragePath path, CancellationToken ct = default)
    {
        try
        {
            await _s3.DeleteObjectAsync(_bucket, path.Value, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Exists ────────────────────────────────────────────────────────────
    public async Task<bool> ExistsAsync(
        StoragePath path, CancellationToken ct = default)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_bucket, path.Value, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
