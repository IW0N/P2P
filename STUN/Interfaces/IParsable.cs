using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Interfaces
{
    internal interface IParsable<T>
    {
        T ParseBytes(byte[] source);
    }
}
