using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.Assets.Exports.CustomizableObject.Mutable.Physics.Bodies;

public class FBoxBody : FBodyShape
{
    public uint Version;
    public FVector Position;
    public FQuat Orientation;
    public FVector Size;
    
    public FBoxBody(FArchive Ar) : base(Ar)
    {
        Version = Ar.Read<uint>();
        Position = Ar.Read<FVector>();
        Orientation = Ar.Read<FQuat>();
        Size = Ar.Read<FVector>();
    }
}