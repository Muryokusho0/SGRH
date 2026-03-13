using SGRH.Application.Dtos.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Application.UseCases.Usuarios.GetUsuario;

public sealed record GetUsuarioResponse(UsuarioDto Usuario);