using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.Assets.Exports.CustomizableObject.Mutable.Physics.Bodies;

public class FConvexBody : FBodyShape
{
    public uint Version;
    public FVector[] Vertices;
    public int[] Indices;
    public FTransform Transform;
    
    public FConvexBody(FArchive Ar) : base(Ar)
    {
        Version = Ar.Read<uint>();
        Vertices = Ar.ReadArray<FVector>();
        Indices = Ar.ReadArray<int>();
        Transform = Ar.Read<FTransform>();
    }
}