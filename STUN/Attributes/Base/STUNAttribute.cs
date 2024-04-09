namespace STUN.Attributes.Base;
public class STUNAttribute
{
    public AttributeType Type { get; }
    /// <summary>
    /// Размер поля Value в байтовом представлении
    /// </summary>
    public ushort ValueLength { get; }
    /// <summary>
    /// Минимальный общий размер аттрибута
    /// </summary>
    public const byte header_size = 4;
    public object Value { get; }
    public ushort TotalLength { get => (ushort)(ValueLength+header_size); }
    public STUNAttribute(AttributeType Type, ushort ValueLength, object Value)
    {
        this.Type = Type;
        this.ValueLength = ValueLength;
        this.Value = Value;
    }
}
    
