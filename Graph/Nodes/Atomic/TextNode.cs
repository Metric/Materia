using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using System.Drawing;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Fonts;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public enum TextAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    //todo: fix this node since we changed font manager
    //to a atlas based approach
    public class TextNode : ImageNode
    {
        NodeOutput Output;

        GLTexture2D character;

        TextProcessor processor;

        protected class CharacterTransform
        {
            public float angle;
            public MVector position;
            public MVector scale;

            public CharacterTransform(float ang, MVector pos, MVector sc)
            {
                position = pos;
                angle = ang;
                scale = sc;
            }
        }

        protected string[] fonts;
        [Dropdown("FontFamily")]
        [Editable(ParameterInputType.Dropdown, "Font")]
        public string[] Fonts
        {
            get
            {
                return fonts;
            }
        }

        protected FontStyle style;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Style")]
        public FontStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                if(style != value)
                {
                    style = value;
                    TriggerValueChange();
                }
            }
        }

        protected TextAlignment alignment;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Alignment")]
        public TextAlignment Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
                TriggerValueChange();
            }
        }

        protected float spacing;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Spacing")]
        public float Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                spacing = value;
                TriggerValueChange();
            }
        }

        protected MVector position;
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Position")]
        public MVector Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                TriggerValueChange();
            }
        }

        protected float rotation;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Rotation", "Default", 0, 360)]
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                TriggerValueChange();
            }
        }

        protected MVector scale;
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Scale")]
        public MVector Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                TriggerValueChange();
            }
        }

        protected string fontFamily;
        public string FontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                fontFamily = value;
                TriggerValueChange();
            }
        }

        protected float fontSize;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Font Size")]
        public float FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = value;
                TriggerValueChange();
            }
        }

        protected string text;
        [Editable(ParameterInputType.MultiText, "Text")]
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (!text.Equals(value))
                {
                    text = value;
                    TriggerValueChange();
                }
            }
        }

        public TextNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Text";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            fontSize = 32;
            fontFamily = "Arial";
            text = "";
            fonts = FontManager.FamilyNames;
            position = new MVector();
            rotation = 0;
            scale = new MVector(1, 1);
            style = FontStyle.Regular;
            alignment = TextAlignment.Center;
            spacing = 1;
            

            processor = new TextProcessor();

            //establish character holder texture
            character = new GLTexture2D(PixelInternalFormat.Rgba);
            character.Bind();
            character.Linear();
            character.ClampToEdge();
            GLTexture2D.Unbind();

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();

            Output = new NodeOutput(NodeType.Gray, this);
            Outputs.Add(Output);
        }

        public class TextNodeData : NodeData
        {
            public string text;
            public float fontSize;
            public string fontFamily;
            public float rotation;
            public float positionX;
            public float positionY;
            public float scaleX;
            public float scaleY;
            public int style;
            public int alignment;
            public float spacing;
        }

        public override void FromJson(string data)
        {
            TextNodeData d = JsonConvert.DeserializeObject<TextNodeData>(data);
            SetBaseNodeDate(d);
            text = d.text;
            fontSize = d.fontSize;
            fontFamily = d.fontFamily;
            style = (FontStyle)d.style;
            rotation = d.rotation;
            scale = new MVector(d.scaleX, d.scaleY);
            position = new MVector(d.positionX, d.positionY);
            alignment = (TextAlignment)d.alignment;
            spacing = d.spacing;
        }

        public override string GetJson()
        {
            TextNodeData d = new TextNodeData();
            FillBaseNodeData(d);
            d.fontFamily = fontFamily;
            d.fontSize = fontSize;
            d.text = text;
            d.style = (int)style;
            d.rotation = rotation;
            d.positionX = position.X;
            d.positionY = position.Y;
            d.scaleX = scale.X;
            d.scaleY = scale.Y;
            d.alignment = (int)alignment;
            d.spacing = spacing;

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();

            if(character != null)
            {
                character.Dispose();
                character = null;
            }

            if(processor != null)
            {
                processor.Dispose();
                processor = null;
            }
        }

        void TryAndGenerateCharacters()
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(fontFamily) || pfontSize <= 0) return;
            //map = FontManager.Generate(fontFamily, fontSize, text, style);
        }

        private void GetParams()
        {
            pfontSize = fontSize;
            palignment = alignment;
            pstyle = style;
            pspacing = spacing;
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "FontSize"))
            { 
                pfontSize = ParentGraph.GetParameterValue(Id, "FontSize").ToFloat();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Spacing"))
            {
                pspacing = ParentGraph.GetParameterValue(Id, "Spacing").ToFloat();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Style"))
            {
                pstyle = (FontStyle)ParentGraph.GetParameterValue(Id, "Style").ToInt();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Alignment"))
            {
                palignment = (TextAlignment)ParentGraph.GetParameterValue(Id, "Alignment").ToInt();
            }

            if (string.IsNullOrEmpty(text))
            {
                lines = new string[0];
            }
            else
            {
                lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private void GetTransforms()
        {
            /*transforms.Clear();
            if (map == null || map.Count == 0) return;

            adjustments.Clear();

            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                float alignmentAdjustment = 0;
                for (int j = 0; j < line.Length; ++j)
                {
                    string ch = line.Substring(j, 1);
                    FontManager.CharData data = null;

                    MVector pPos = position;
                    float pcharRotation = rotation;
                    MVector pScale = scale;

                    if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
                    {
                        if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                        {
                            Function func = ParentGraph.GetParameterRaw(Id, "Rotation").Value as Function;
                            func.SetVar("character", j, NodeType.Float);
                            func.SetVar("maxCharacters", line.Length, NodeType.Float);
                            func.SetVar("line", i, NodeType.Float);
                            func.SetVar("maxLines", lines.Length, NodeType.Float);
                        }

                        pcharRotation = ParentGraph.GetParameterValue(Id, "Rotation").ToFloat();
                    }

                    if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
                    {
                        if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                        {
                            Function func = ParentGraph.GetParameterRaw(Id, "Scale").Value as Function;
                            func.SetVar("character", j, NodeType.Float);
                            func.SetVar("maxCharacters", line.Length, NodeType.Float);
                            func.SetVar("line", i, NodeType.Float);
                            func.SetVar("maxLines", lines.Length, NodeType.Float);
                        }

                        pScale = ParentGraph.GetParameterValue<MVector>(Id, "Position");
                    }

                    if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Position"))
                    {
                        if (ParentGraph.IsParameterValueFunction(Id, "Position"))
                        {
                            Function func = ParentGraph.GetParameterRaw(Id, "Position").Value as Function;
                            func.SetVar("character", j, NodeType.Float);
                            func.SetVar("maxCharacters", line.Length, NodeType.Float);
                            func.SetVar("line", i, NodeType.Float);
                            func.SetVar("maxLines", lines.Length, NodeType.Float);
                        }

                        pPos = ParentGraph.GetParameterValue<MVector>(Id, "Position");
                    }

                    CharacterTransform ct = new CharacterTransform(pcharRotation * (float)(Math.PI / 180.0f), pPos, pScale);
                    transforms.Add(ct);

                    //for these two alignments we need to calculate the 
                    //actual full line width first before we do final
                    //positing and rendering
                    //to apply the proper adjustment
                    //for right alignment all we need is the total
                    //for center we need the halfway point
                    if (palignment == TextAlignment.Center || palignment == TextAlignment.Right)
                    {  
                        if (map.TryGetValue(ch, out data))
                        {
                            alignmentAdjustment += data.size.X + pspacing;
                        }
                    }
                }

                if (palignment == TextAlignment.Center)
                {
                    alignmentAdjustment *= 0.5f;
                }

                adjustments.Add(alignmentAdjustment);
            }*/
        }

        public override void TryAndProcess()
        {
            GetParams();
            TryAndGenerateCharacters();
            GetTransforms();
            Process();
        }

        List<CharacterTransform> transforms = new List<CharacterTransform>();
        Dictionary<string, FontManager.CharData> map = new Dictionary<string, FontManager.CharData>();
        float pfontSize;
        float pspacing;
        string[] lines;
        TextAlignment palignment;
        FontStyle pstyle;
        List<float> adjustments = new List<float>();
        void Process()
        {
            //need a clean buffer
            //when drawing
            if(buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            if (processor == null || lines == null) return;

            CreateBufferIfNeeded();

            processor.Prepare(width, height, character, buffer);

            float px = 1.0f / width;
            float py = 1.0f / height;

            MVector pivot = new MVector(-1, 0);

            if (map != null && map.Count > 0 && transforms.Count > 0)
            {
                int tindex = 0;
                for (int i = 0; i < lines.Length; ++i)
                {
                    string line = lines[i];
                    float left = 0;
                    float alignmentAdjustment = adjustments[i];

                    for (int j = 0; j < line.Length; ++j)
                    {
                        if (tindex >= transforms.Count) continue;

                        string ch = line.Substring(j, 1);
                        /*FontManager.CharData data = null;
                        if (map.TryGetValue(ch, out data))
                        {
                            if (data.texture == null)
                            {
                                ++tindex;
                                continue;
                            }

                            CharacterTransform ct = transforms[tindex];
                            MVector finalPos = new MVector((ct.position.X + left * ct.scale.X) * width - alignmentAdjustment, (ct.position.Y + (i * data.bearing) * py * ct.scale.Y) * height);

                            left += (data.size.X + pspacing) * px;

                            character.Bind();
                            character.SetData(data.texture.Image, PixelFormat.Bgra, (int)Math.Ceiling(data.size.X), (int)Math.Ceiling(data.size.Y));
                            GLTexture2D.Unbind();

                            processor.Translation = finalPos;
                            processor.Angle = ct.angle;
                            processor.Pivot = pivot;
                            processor.Scale = ct.scale * (new MVector(data.size.X, data.size.Y) * 0.5f);
                            processor.ProcessCharacter(width, height, character, buffer);
                        }*/

                        ++tindex;
                    }
                }
            }

            processor.Complete();
            Output.Data = buffer;
            TriggerTextureChange();
        }
    }
}
