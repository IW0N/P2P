using STUN.Interfaces;

namespace STUN.Header
{
    public record MessageType(STUNClass Class, Method Method = Method.Binding)
    {
        
    }
}
