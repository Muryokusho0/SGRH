using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.Abstractions;
// Datos de contexto HTTP necesarios para registrar auditoría.
// Se agrupa en un record para no repetir los 3 campos en cada Request mutante.
// El Controller lo construye desde HttpContext y lo pasa al Request.
public sealed record AuditInfo(
    Guid RequestId,
    string IpOrigen,
    string UserAgent);
