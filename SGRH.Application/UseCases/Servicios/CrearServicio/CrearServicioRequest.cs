using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Servicios.CrearServicio;

public sealed record CrearServicioRequest(
    string NombreServicio,
    string Descripcion,
    AuditInfo AuditInfo);