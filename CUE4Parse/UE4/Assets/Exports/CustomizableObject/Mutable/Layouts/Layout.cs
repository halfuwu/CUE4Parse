using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;

namespace CUE4Parse.UE4.Assets.Exports.CustomizableObject.Mutable.Layouts;

public class Layout : IMutablePtr
{
    public TIntVector2<ushort> Size;
    public FBlock[] Blocks;
    public TIntVector2<ushort> MaxSize;
    public EPackStrategy Strategy;
    public EReductionMethod ReductionMethod;
    public bool FirstLODToIgnoreWarnings;
    
    public int Version { get; set; }
    
    public void Deserialize(FArchive Ar)
    {
        Size = Ar.Read<TIntVector2<ushort>>();
        Blocks = Ar.ReadArray(() => new FBlock(Ar));
        MaxSize = Ar.Read<TIntVector2<ushort>>();

        Strategy = Ar.Read<EPackStrategy>();
        FirstLODToIgnoreWarnings = Ar.Read<int>() == 1;
        ReductionMethod = Ar.Read<EReductionMethod>();
    }
}

public enum EPackStrategy : uint
{
    RESIZABLE_LAYOUT,
    FIXED_LAYOUT,
    OVERLAY_LAYOUT
}

public enum EReductionMethod : uint
{
    HALVE_REDUCTION,	// Divide axis by 2
    UNITARY_REDUCTION	// Reduces 1 block the axis 
}