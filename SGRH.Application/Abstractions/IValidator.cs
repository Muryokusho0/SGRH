using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGRH.Application.Abstractions;


namespace SGRH.Application.Abstractions;

public interface IValidator<TRequest>
{
    Task<ValidacionResultado> ValidateAsync(
        TRequest request, CancellationToken ct = default);
}
