namespace STUN.Interfaces;
internal interface IBytesConvertable<T>:IParsable<T>
{
    void CopyToBytes(T source, byte[] destination);
}