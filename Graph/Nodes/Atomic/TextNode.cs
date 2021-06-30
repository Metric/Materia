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
using static Materia.Rendering.Fonts.FontManager;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    //todo: fix this node since we changed font manager
    //to a atlas based approach
    public class TextNode : ImageNode
    {
        NodeOutput Output;

        #region Internal Value Holders
        List<CharacterTransform> transforms = new List<CharacterTransform>();
        float pfontSize;
        float pspacing;
        string[] lines;
        TextAlignment palignment;
        FontStyle pstyle;
        List<float> adjustments = new List<float>();
        GLTexture2D characters;
        CharAtlas map;
        #endregion

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

        protected FontStyle style = FontStyle.Regular;
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

        protected TextAlignment alignment = TextAlignment.Center;
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

        protected float spacing = 1;
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

        protected MVector position = MVector.Zero;
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

        protected float rotation = 0;
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

        protected MVector scale = new MVector(1,1);
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

        protected string fontFamily = "Arial";
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

        protected float fontSize = 32;
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

        //TODO: add in a string node to Function based Nodes
        protected string text = "";
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
            defaultName = Name = "Text";

            width = w;
            height = h;

            fonts = FontManager.FamilyNames;       

            internalPixelType = p;

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
            public byte style;
            public byte alignment;
            public float spacing;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(text);
                w.Write(fontFamily);
                w.Write(style);
                w.Write(alignment);
                w.Write(fontSize);
                w.Write(spacing);
                w.Write(rotation);
                w.Write(positionX);
                w.Write(positionY);
                w.Write(scaleX);
                w.Write(scaleY);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                text = r.NextString();
                fontFamily = r.NextString();
                style = r.NextByte();
                alignment = r.NextByte();
                fontSize = r.NextFloat();
                spacing = r.NextFloat();
                rotation = r.NextFloat();
                positionX = r.NextFloat();
                positionY = r.NextFloat();
                scaleX = r.NextFloat();
                scaleY = r.NextFloat();
            }
        }

        private void SetData(TextNodeData d)
        {
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

        private void FillData(TextNodeData d)
        {
            d.fontFamily = fontFamily;
            d.fontSize = fontSize;
            d.text = text;
            d.style = (byte)style;
            d.rotation = rotation;
            d.positionX = position.X;
            d.positionY = position.Y;
            d.scaleX = scale.X;
            d.scaleY = scale.Y;
            d.alignment = (byte)alignment;
            d.spacing = spacing;
        }

        public override void GetBinary(Writer w)
        {
            TextNodeData d = new TextNodeData();
            FillBaseNodeData(d);
            FillData(d);
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            TextNodeData d = new TextNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override void FromJson(string data)
        {
            TextNodeData d = JsonConvert.DeserializeObject<TextNodeData>(data);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override string GetJson()
        {
            TextNodeData d = new TextNodeData();
            FillBaseNodeData(d);
            FillData(d);

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }

        void TryAndGenerateCharacters()
        {
            if (isDisposing) return;
            if (string.IsNullOrEmpty(text)
                || string.IsNullOrEmpty(fontFamily)
                || pfontSize <= 0)
            {
                return;
            }

            map = FontManager.GetAtlas(fontFamily, pfontSize, pstyle);
            if (map != null)
            {
                characters = map.atlas;
            }
        }

        private void GetParams()
        {
            if (isDisposing) return;
            pfontSize = GetParameter("FontSize", fontSize);
            palignment = (TextAlignment)GetParameter("Alignment", (int)alignment);
            pstyle = (FontStyle)GetParameter("Style", (int)style);
            pspacing = GetParameter("Spacing", spacing);

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
            if (isDisposing) return;
            if (map == null) return;

            transforms.Clear();
            adjustments.Clear();

            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                float alignmentAdjustment = 0;
                for (int j = 0; j < line.Length; ++j)
                {
                    char ch = line[j];
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

                    CharacterTransform ct = new CharacterTransform(pcharRotation * MathHelper.Deg2Rad, pPos, pScale);
                    transforms.Add(ct);

                    //for these two alignments we need to calculate the 
                    //actual full line width first before we do final
                    //positing and rendering
                    //to apply the proper adjustment
                    //for right alignment all we need is the total
                    //for center we need the halfway point
                    //we have to do left here otherwise it is reversed on the texture due to the transform on render
                    if (palignment == TextAlignment.Center || palignment == TextAlignment.Left)
                    {
                        var data = map.Get(ch);
                        if (data != null)
                        {
                            alignmentAdjustment += data.info.size.Width + pspacing;
                        }
                    }
                }

                if (palignment == TextAlignment.Center)
                {
                    alignmentAdjustment *= 0.5f;
                }

                adjustments.Add(alignmentAdjustment);
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            TryAndGenerateCharacters();
            GetTransforms();
            Process();
        }

        void Process()
        {
            if (isDisposing || lines == null 
                || characters == null || map == null) return;

            CreateBufferIfNeeded();

            processor ??= new TextProcessor();

            processor.PrepareView(buffer);

            float px = 1.0f / width;
            float py = 1.0f / height;

            MVector pivot = new MVector(-1, 0);

            if (transforms.Count > 0)
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

                        char ch = line[j];
                        var cdat = map.Get(ch);

                        if (cdat == null)
                        {
                            ++tindex;
                            continue;
                        }

                        CharacterTransform ct = transforms[tindex];
                        MVector finalPos = new MVector((ct.position.X + left * ct.scale.X) * width - alignmentAdjustment, (ct.position.Y + (i * cdat.info.lineHeight) * py * ct.scale.Y) * height);
                        left += (cdat.info.size.Width + pspacing) * px;

                        processor.Translation = finalPos;
                        processor.Angle = ct.angle;
                        processor.Pivot = pivot;
                        processor.Scale = ct.scale * (new MVector(cdat.info.size.Width, cdat.info.size.Height) * 0.5f);

                        processor.Process(map.atlas, cdat.uv);

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
