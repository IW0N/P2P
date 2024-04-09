using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Header
{
    /// <summary>
    /// This is 96-bits record, that describe transaction id. 
    /// It uses BIG-ENDIAN Order
    /// </summary>
    /// <param name="Pice1">The first pice of 96-bit's big-endian number</param>
    /// <param name="Pice2">The second pice of 96-bit's big-endian number</param>
    /// <param name="Pice3">The third pice of 96-bit's big-endian number</param>    
    public record struct TransactionID(int Pice1, int Pice2, int Pice3)
    {
        private static int GetIntRandom()
        {
            return RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        }
        public static TransactionID GetRandom()
        {
            int first = GetIntRandom();
            int second = GetIntRandom();
            int third = GetIntRandom();
            return new TransactionID(first, second, third);
        }
    }
}
