using Materia.Graph;
using Materia.Nodes.Atomic;
using MateriaCore.Components.GL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public static class GraphTemplate
    {
        public static void PBRFull(Graph result)
        {
            OutputNode baseColor = new OutputNode(result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 3;

            result.Add(baseColor);

            OutputNode metallic = new OutputNode(result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(metallic);

            OutputNode roughness = new OutputNode(result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            result.Add(roughness);

            OutputNode normal = new OutputNode(result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            result.Add(normal);

            OutputNode ao = new OutputNode(result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            result.Add(ao);

            OutputNode height = new OutputNode(result.DefaultTextureType);
            height.Name = "Height";
            height.OutType = OutputType.height;
            height.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(height);

            OutputNode emission = new OutputNode(result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 3;

            result.Add(emission);

            OutputNode thickness = new OutputNode(result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 4;

            result.Add(thickness);
        }

        public static void PBRNoHeight(Graph result)
        {
            OutputNode baseColor = new OutputNode(result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 3;

            result.Add(baseColor);

            OutputNode metallic = new OutputNode(result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(metallic);

            OutputNode roughness = new OutputNode(result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            result.Add(roughness);

            OutputNode normal = new OutputNode(result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            result.Add(normal);

            OutputNode ao = new OutputNode(result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            result.Add(ao);

            OutputNode emission = new OutputNode(result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(emission);

            OutputNode thickness = new OutputNode(result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 3;

            result.Add(thickness);
        }

        public static void PBRNoHeightAO(Graph result)
        {
            OutputNode baseColor = new OutputNode(result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(baseColor);

            OutputNode metallic = new OutputNode(result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            result.Add(metallic);

            OutputNode roughness = new OutputNode(result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = 5;

            result.Add(roughness);

            OutputNode normal = new OutputNode(result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            result.Add(normal);
        }


        public static void PBRNoHeightAONormal(Graph result)
        {
            OutputNode baseColor = new OutputNode(result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            result.Add(baseColor);

            OutputNode metallic = new OutputNode(result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = 5;

            result.Add(metallic);

            OutputNode roughness = new OutputNode(result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            result.Add(roughness);
        }
    }
}
