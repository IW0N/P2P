using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Header
{
    public enum STUNClass : byte
    {
        Request,//0b00
        Indication,//0b01
        SuccessResponse,//0b10
        ErrorResponse//0b11
    }
}
