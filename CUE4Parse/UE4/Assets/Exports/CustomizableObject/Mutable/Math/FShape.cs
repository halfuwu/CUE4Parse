using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.Assets.Exports.CustomizableObject.Mutable.Math;

public class FShape
{
    public FVector Position;
    public FVector Up;
    public FVector Side;
    public FVector Size;
    public Type ShapeType;
    
    public enum Type : byte
    {
        None = 0,
        Ellipse,
        AABox
    };
    
    public FShape(FArchive Ar)
    {
        Position = Ar.Read<FVector>();
        Up = Ar.Read<FVector>();
        Side = Ar.Read<FVector>();
        Size = Ar.Read<FVector>();
        ShapeType = Ar.Read<Type>();
    }
    
}