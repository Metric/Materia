namespace Materia.Rendering.Attributes
{
    public enum NodeType
    {
        Color = 2,
        Gray = 4,
        Float = 8,
        Float2 = 16,
        Float3 = 32,
        Float4 = 64,
        Bool = 128,
        Execute = 256,
        Matrix = 512,
    }

    public enum ParameterInputType
    {
        FloatSlider = 0,
        FloatInput = 1,
        IntSlider = 2,
        IntInput = 3,
        Float2Slider = 4,
        Float2Input = 5,
        Float3Slider = 6,
        Float3Input = 7,
        Float4Slider = 8,
        Float4Input = 9,
        Int2Slider = 10,
        Int2Input = 11,
        Int3Slider = 12,
        Int3Input = 13,
        Int4Slider = 14,
        Int4Input = 16,
        Toggle = 17,
        Color = 18,
        Gradient = 19,
        ImageFile = 20,
        MeshFile = 21,
        GraphFile = 22,
        Levels = 23,
        Curves = 24,
        Text = 25,
        Dropdown = 26,
        Map = 27,
        MapEdit = 28,
        MultiText = 29
    }
}
