using Microsoft.Extensions.Logging;
using SGRH.Domain.Abstractions.Repositories;
using SGRH.Domain.Abstractions.Services;
using SGRH.Domain.Abstractions.Storage;
using SGRH.Domain.Entities.Auditoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SGRH.Infrastructure.Services;

public sealed class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorage _storage;
    private readonly ILogger<AuditoriaService> _logger;

    // Ruta en S3: auditoria/{año}/{mes}/{dia}/{AuditoriaEventoId}.json
    // Ejemplo:   auditoria/2025/12/25/0000000042.json
    private static StoragePath BuildS3Path(AuditoriaEvento evento)
    {
        var fecha = evento.FechaUtc;
        return new StoragePath(
            $"auditoria/{fecha:yyyy}/{fecha:MM}/{fecha:dd}/{evento.AuditoriaEventoId:D10}.json");
    }

    public AuditoriaService(
        IAuditoriaRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorage storage,
        ILogger<AuditoriaService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _storage = storage;
        _logger = logger;
    }

    public async Task RegistrarAsync(AuditoriaEvento evento, CancellationToken ct = default)
    {
        // ── 1. Persistir en SQL Server ────────────────────────────────────
        await _repository.AddAsync(evento, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ── 2. Subir copia JSON a S3 (best-effort) ────────────────────────
        try
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(evento, _jsonOptions);

            var request = new FileUploadRequest(
                path: BuildS3Path(evento),
                contentType: "application/json",
                content: json);

            var result = await _storage.UploadAsync(request, ct);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "AuditoriaService: S3 upload falló para evento {EventoId}. Error: {Error}",
                    evento.AuditoriaEventoId, result.Error);
            }
        }
        catch (Exception ex)
        {
            // S3 no debe romper el flujo principal
            _logger.LogError(ex,
                "AuditoriaService: excepción al subir evento {EventoId} a S3.",
                evento.AuditoriaEventoId);
        }
    }

    // Opciones de serialización: nombres en camelCase, enums como string,
    // fechas en ISO 8601, sin ciclos de referencia.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };
}
