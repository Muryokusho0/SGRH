using SGRH.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Categorias.ModificarCategoria;

public sealed record ModificarCategoriaRequest(
    int CategoriaHabitacionId,
    string NombreCategoria,
    int Capacidad,
    string Descripcion,
    decimal PrecioBase,
    AuditInfo AuditInfo);