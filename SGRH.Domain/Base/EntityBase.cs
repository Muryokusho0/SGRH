using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Base
{
    public abstract class EntityBase
    {
        protected abstract object GetKey();

        public override bool Equals(object? obj)
        {
            if (obj is not EntityBase other)
                return false;

            return GetKey().Equals(other.GetKey());
        }

        public override int GetHashCode()
            => GetKey().GetHashCode();
    }
}
