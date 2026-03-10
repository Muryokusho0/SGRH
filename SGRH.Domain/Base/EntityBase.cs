using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Domain.Base;

public abstract class EntityBase
{
    protected abstract object GetKey();

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return GetKey().Equals(other.GetKey());
    }

    public override int GetHashCode() => GetKey().GetHashCode();

    public static bool operator ==(EntityBase? a, EntityBase? b)
        => a is null ? b is null : a.Equals(b);

    public static bool operator !=(EntityBase? a, EntityBase? b)
        => !(a == b);
}