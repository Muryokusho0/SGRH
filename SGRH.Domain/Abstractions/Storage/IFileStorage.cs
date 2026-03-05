using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Abstractions.Storage;

public interface IFileStorage
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken ct = default);

    Task<FileDownloadResult> DownloadAsync(StoragePath path, CancellationToken ct = default);

    Task<bool> DeleteAsync(StoragePath path, CancellationToken ct = default);

    Task<bool> ExistsAsync(StoragePath path, CancellationToken ct = default);
}
