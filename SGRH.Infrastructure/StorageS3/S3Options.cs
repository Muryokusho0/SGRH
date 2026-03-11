using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.StorageS3;

public sealed class S3Options
{
    public const string Section = "AWS:S3";

    public string BucketName { get; init; } = default!;
    public string Region { get; init; } = default!;
}