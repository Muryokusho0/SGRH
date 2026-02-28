using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Base
{
    public abstract class AuditableEntity : EntityBase
    {
        public DateTime CreatedAtUtc { get; protected set; }
    }
}
