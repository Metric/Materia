using Materia.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Graph
{
    public enum TextAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum SwitchInput
    {
        Input0 = 0,
        Input1 = 1
    }

    public enum FXPivot
    {
        Center = 0,
        Min = 1,
        Max = 2,
        MinX = 3,
        MaxX = 4,
        MinY = 5,
        MaxY = 6
    }

    public enum FXBlend
    {
        Blend = 0,
        Add = 1,
        Max = 2,
        AddSub = 3
    }


    public enum BlendType
    {
        AddSub = 0,
        Copy = 1,
        Multiply = 2,
        Screen = 3,
        Overlay = 4,
        HardLight = 5,
        SoftLight = 6,
        ColorDodge = 7,
        LinearDodge = 8,
        ColorBurn = 9,
        LinearBurn = 10,
        VividLight = 11,
        Divide = 12,
        Subtract = 13,
        Difference = 14,
        Darken = 15,
        Lighten = 16,
        Hue = 17,
        Saturation = 18,
        Color = 19,
        Luminosity = 20,
        LinearLight = 21,
        PinLight = 22,
        HardMix = 23,
        Exclusion = 24
    }

    public enum AlphaModeType
    {
        Background = 0,
        Foreground = 1,
        Min = 2,
        Max = 3,
        Average = 4,
        Add = 5
    }

    public enum GraphPixelType
    {
        RGBA = PixelInternalFormat.Rgba8,
        RGBA16F = PixelInternalFormat.Rgba16f,
        RGBA32F = PixelInternalFormat.Rgba32f,
        RGB = PixelInternalFormat.Rgb8,
        RGB16F = PixelInternalFormat.Rgb16f,
        RGB32F = PixelInternalFormat.Rgb32f,
        Luminance16F = PixelInternalFormat.R16f,
        Luminance32F = PixelInternalFormat.R32f,
    }

    public enum GraphState
    {
        Loading,
        Ready
    }

    public enum OutputType
    {
        basecolor,
        height,
        occlusion,
        roughness,
        metallic,
        normal,
        thickness,
        emission
    }

    public enum NodeDataType
    {
        AtomicAONode = 0,
        AtomicBitmapNode = 1,
        AtomicBlendNode = 2,
        AtomicBlurNode = 3,
        AtomicChannelSwitchNode = 4,
        AtomicCircleNode = 5,
        AtomicCurvesNode = 6,
        AtomicDirectionalWarpNode = 7,
        AtomicDistanceNode = 8,
        AtomicEmbossNode = 9,
        AtomicFXNode = 10,
        AtomicGammaNode = 11,
        AtomicGradientDynamicNode = 12,
        AtomicGradientMapNode = 13,
        AtomicGrayscaleConversionNode = 14,
        AtomicHSLNode = 15,
        AtomicInputNode = 16,
        AtomicInvertNode = 17,
        AtomicLevelsNode = 18,
        AtomicMeshDepthNode = 19,
        AtomicMeshNode = 20,
        AtomicMotionBlurNode = 21,
        AtomicNormalNode = 22,
        AtomicOutputNode = 23,
        AtomicPixelProcessorNode = 24,
        AtomicSequenceNode = 25,
        AtomicSharpenNode = 26,
        AtomicSwitchNode = 27,
        AtomicTextNode = 28,
        AtomicTransformNode = 29,
        AtomicUniformColorNode = 30,
        AtomicWarpNode = 31,
        ItemsCommentNode = 32,
        ItemsPinNode = 33,
        MathNodesAbsoluteNode = 34,
        MathNodesAddNode = 35,
        MathNodesAndNode = 36,
        MathNodesArcTangentNode = 37,
        MathNodesArgNode = 38,
        MathNodesBooleanConstantNode = 39,
        MathNodesBreakFloat2Node = 40,
        MathNodesBreakFloat3Node = 41,
        MathNodesBreakFloat4Node = 42,
        MathNodesCallNode = 43,
        MathNodesCartesianNode = 44,
        MathNodesCeilNode = 45,
        MathNodesClampNode = 46,
        MathNodesCosineNode = 47,
        MathNodesDistanceNode = 48,
        MathNodesDivideNode = 49,
        MathNodesDotProductNode = 50,
        MathNodesEqualNode = 51,
        MathNodesExecuteNode = 52,
        MathNodesExponentialNode = 53,
        MathNodesFloat2ConstantNode = 54,
        MathNodesFloat3ConstantNode = 55,
        MathNodesFloat4ConstantNode = 56,
        MathNodesFloatConstantNode = 57,
        MathNodesFloorNode = 58,
        MathNodesForLoopNode = 59,
        MathNodesFractNode = 60,
        MathNodesGetBoolVarNode = 61,
        MathNodesGetFloat2VarNode = 62,
        MathNodesGetFloat3VarNode = 63,
        MathNodesGetFloat4VarNode = 64,
        MathNodesGetFloatVarNode = 65,
        MathNodesGreaterThanEqualNode = 66,
        MathNodesGreaterThanNode = 67,
        MathNodesIfElseNode = 68,
        MathNodesLengthNode = 69,
        MathNodesLerpNode = 70,
        MathNodesLessThanEqualNode = 71,
        MathNodesLessThanNode = 72,
        MathNodesLog2Node = 73,
        MathNodesLogNode = 74,
        MathNodesMakeFloat2Node = 75,
        MathNodesMakeFloat3Node = 76,
        MathNodesMakeFloat4Node = 77,
        MathNodesMatrixNode = 78,
        MathNodesMaxNode = 79,
        MathNodesMinNode = 80,
        MathNodesModuloNode = 81,
        MathNodesMultiplyNode = 82,
        MathNodesNegateNode = 83,
        MathNodesNormalizeNode = 84,
        MathNodesNotEqualNode = 85,
        MathNodesNotNode = 86,
        MathNodesOrNode = 87,
        MathNodesPolarNode = 88,
        MathNodesPow2Node = 89,
        MathNodesPowNode = 90,
        MathNodesRandom2Node = 91,
        MathNodesRandomNode = 92,
        MathNodesRotateMatrixNode = 93,
        MathNodesRoundNode = 94,
        MathNodesSamplerNode = 95,
        MathNodesScaleMatrixNode = 96,
        MathNodesSetVarNode = 97,
        MathNodesShearMatrixNode = 98,
        MathNodesSineNode = 99,
        MathNodesSqrtNode = 100,
        MathNodesSubtractNode = 101,
        MathNodesTangentNode = 102,
        MathNodesTranslateMatrixNode = 103,
        AtomicGraphInstanceNode = 104
    }
}
