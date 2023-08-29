﻿using System.Collections.Generic;
using CUE4Parse_Conversion.Meshes.PSK;
using CUE4Parse_Conversion.UnrealFormat;
using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Assets.Exports.Material;
using CUE4Parse.UE4.Assets.Exports.SkeletalMesh;
using CUE4Parse.UE4.Assets.Exports.StaticMesh;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Objects.Meshes;
using CUE4Parse.UE4.Objects.UObject;

namespace CUE4Parse_Conversion.Meshes.UnrealFormat;

public class UnrealModel : UnrealFormatExport
{
    protected override string Identifier { get; set; } = "UEMODEL";
    
    public UnrealModel(CStaticMeshLod lod, string name, ExporterOptions options) : base(name, options) 
    {
        SerializeStaticMeshData(lod.Verts, lod.Indices.Value, lod.VertexColors, lod.Sections.Value, lod.ExtraUV.Value);
    }
    
    public UnrealModel(CSkelMeshLod lod, string name, List<CSkelMeshBone> bones, FPackageIndex[]? morphTargets, FPackageIndex[] sockets, int lodIndex, ExporterOptions options) : base(name, options)
    {
        SerializeStaticMeshData(lod.Verts, lod.Indices.Value, lod.VertexColors, lod.Sections.Value, lod.ExtraUV.Value);
        SerializeSkeletalMeshData(lod.Verts, bones, morphTargets, sockets, lodIndex);
    }

    private void SerializeStaticMeshData(IReadOnlyCollection<CMeshVertex> verts, FRawStaticIndexBuffer indices, FColor[]? vertexColors, CMeshSection[] sections, FMeshUVFloat[][] extraUVs)
    {
        using var vertexChunk = new FDataChunk("VERTICES", verts.Count);
        using var normalsChunk = new FDataChunk("NORMALS", verts.Count);
        using var tangentsChunk = new FDataChunk("TANGENTS", verts.Count);

        var mainUVs = new List<FMeshUVFloat>();
        foreach (var vert in verts)
        {
            var position = vert.Position;
            position.Y = -position.Y;
            position.Serialize(vertexChunk);

            var normal = (FVector) vert.Normal;
            normal.Normalize();
            normal.Y = -normal.Y;
            normal.Serialize(normalsChunk);
            
            var tangent = (FVector) vert.Tangent;
            tangent.Normalize();
            tangent.Y = -tangent.Y;
            tangent.Serialize(tangentsChunk);
            
            var uv = vert.UV;
            mainUVs.Add(uv);
        }
        
        vertexChunk.Serialize(Ar);
        normalsChunk.Serialize(Ar);
        tangentsChunk.Serialize(Ar);
        
        using (var texCoordsChunk = new FDataChunk("TEXCOORDS"))
        {
            texCoordsChunk.WriteArray(mainUVs, uv =>
            {
                uv.V = 1-uv.V;
                uv.Serialize(texCoordsChunk);
            });
            texCoordsChunk.Count++;
            
            foreach (var extraUVSet in extraUVs)
            {
                texCoordsChunk.WriteArray(extraUVSet, uv => 
                {
                    uv.V = 1-uv.V;
                    uv.Serialize(texCoordsChunk);
                });
                texCoordsChunk.Count++;
            }
            
            texCoordsChunk.Serialize(Ar);
        }
        
        using (var indexChunk = new FDataChunk("INDICES", indices.Length))
        {
            for (var i = 0; i < indices.Length; i++)
            {
                indexChunk.Write(indices[i]);
            }
            
            indexChunk.Serialize(Ar);
        }
        
        if (vertexColors is not null)
        {
            using var vertexColorChunk = new FDataChunk("VERTEXCOLORS", vertexColors.Length);
            for (var i = 0; i < vertexColors.Length; i++)
            {
                vertexColors[i].Serialize(vertexColorChunk);
            }
            vertexColorChunk.Serialize(Ar);
        }
        
        using (var materialChunk = new FDataChunk("MATERIALS", sections.Length))
        {
            foreach (var section in sections)
            {
                var materialName = section.Material?.Load<UMaterialInterface>()?.Name ?? string.Empty;
                materialChunk.WriteFString(materialName);
                materialChunk.Write(section.FirstIndex);
                materialChunk.Write(section.NumFaces);
            }

            materialChunk.Serialize(Ar);
        }
    }

    private void SerializeSkeletalMeshData(CSkelMeshVertex[] verts, List<CSkelMeshBone> bones, FPackageIndex[]? morphTargets, FPackageIndex[] sockets, int lodIndex)
    {
        using (var weightsChunk = new FDataChunk("WEIGHTS"))
        {
            for (var vertexIndex = 0; vertexIndex < verts.Length; vertexIndex++)
            {
                var vert = verts[vertexIndex];
            
                var vertBones = vert.Bone;
                if (vertBones is null) continue;
            
                var weights = vert.UnpackWeights();
                for (var index = 0; index < weights.Length; index++)
                {
                    weightsChunk.Write(vertBones[index]);
                    weightsChunk.Write(vertexIndex);
                    weightsChunk.Write(weights[index]);
                    weightsChunk.Count++;
                }
            }
        
            weightsChunk.Serialize(Ar);
        }
        
        if (morphTargets is not null)
        {
            using var morphTargetsChunk = new FDataChunk("MORPHTARGETS", morphTargets.Length);
            foreach (var morphTarget in morphTargets)
            {
                var morph = morphTarget.Load<UMorphTarget>();
                if (morph?.MorphLODModels is null || lodIndex >= morph.MorphLODModels.Length) continue;

                var morphLod = morph.MorphLODModels[lodIndex];
            
                var morphData = new FMorphTarget(morph.Name, morphLod);
                morphData.Serialize(morphTargetsChunk);
            }
            morphTargetsChunk.Serialize(Ar);
        }
        
        using (var boneChunk = new FDataChunk("BONES", bones.Count))
        {
            foreach (var bone in bones)
            {
                var boneName = new FString(bone.Name.Text);
                boneName.Serialize(boneChunk);
            
                boneChunk.Write(bone.ParentIndex);

                var bonePos = bone.Position;
                bonePos.Y = -bonePos.Y;
                bonePos.Serialize(boneChunk);

                var boneRot = bone.Orientation;
                boneRot.Y = -boneRot.Y;
                boneRot.W = -boneRot.W;
                boneRot.Serialize(boneChunk);
            }
            boneChunk.Serialize(Ar);
        }
        
        using (var socketChunk = new FDataChunk("SOCKETS", sockets.Length))
        {
            foreach (var socketObject in sockets)
            {
                var socket = socketObject.Load<USkeletalMeshSocket>();
                if (socket is null) continue;

                socketChunk.WriteFString(socket.SocketName.Text);
                socketChunk.WriteFString(socket.BoneName.Text);
            
                var bonePos = socket.RelativeLocation;
                bonePos.Y = -bonePos.Y;
                bonePos.Serialize(socketChunk);

                var boneRot = socket.RelativeRotation.Quaternion();
                boneRot.Y = -boneRot.Y;
                boneRot.W = -boneRot.W;
                boneRot.Serialize(socketChunk);
            
                var boneScale = socket.RelativeScale;
                boneScale.Y = -boneScale.Y;
                boneScale.Serialize(socketChunk);
            }

            socketChunk.Serialize(Ar);
        }

    }
    
}