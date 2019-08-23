using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.Nodes.Attributes;
using Materia.Nodes.Helpers;
using System.Drawing;
using Materia.MathHelpers;
using Materia.Math3D;

namespace Materia.Nodes.Atomic
{
    public class TextNode : ImageNode
    {
        NodeOutput Output;

        GLTextuer2D character;

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
                    TryAndProcess();
                }
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
                TryAndProcess();
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
                TryAndProcess();
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
                TryAndProcess();
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
                TryAndProcess();
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
                TryAndProcess();
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
                    TryAndProcess();
                }
            }
        }

        public TextNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Text";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            fontSize = 32;
            fontFamily = "Arial";
            text = "";
            fonts = FontManager.GetAvailableFonts();
            position = new MVector();
            rotation = 0;
            scale = new MVector(1, 1);
            style = FontStyle.Regular;

            processor = new TextProcessor();

            //establish character holder texture
            character = new GLTextuer2D(GLInterfaces.PixelInternalFormat.Rgba);
            character.Bind();
            character.Linear();
            character.ClampToEdge();
            GLTextuer2D.Unbind();

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();

            Output = new NodeOutput(NodeType.Gray, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
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

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(character != null)
            {
                character.Release();
                character = null;
            }

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }

        public override void TryAndProcess()
        {
            if (!Async)
            {
                GetParams();
                TryAndGenerateCharacters();
                GetTransforms();
                Process();

                return;
            }

            if (ParentGraph != null)
            {
                ParentGraph.Schedule(this);
            }
        }

        void TryAndGenerateCharacters()
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(fontFamily) || pfontSize <= 0) return;
            map = FontManager.Generate(fontFamily, fontSize, text, style);
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                GetParams();
                TryAndGenerateCharacters();
                GetTransforms();
            }).ContinueWith(t =>
            {
                Process();
            }, Context);
        }

        private void GetParams()
        {
            pfontSize = fontSize;
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "FontSize"))
            { 
                pfontSize = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "FontSize"));
            }

            if(string.IsNullOrEmpty(text))
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
            transforms.Clear();
            if (map == null || map.Count == 0) return;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                for (int j = 0; j < line.Length; j++)
                {
                    MVector pPos = position;
                    float pcharRotation = rotation;
                    MVector pScale = scale;

                    if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rotation"))
                    {
                        if (ParentGraph.IsParameterValueFunction(Id, "Rotation"))
                        {
                            FunctionGraph func = ParentGraph.GetParameterRaw(Id, "Rotation").Value as FunctionGraph;
                            func.SetVar("character", j, NodeType.Float);
                            func.SetVar("maxCharacters", line.Length, NodeType.Float);
                            func.SetVar("line", i, NodeType.Float);
                            func.SetVar("maxLines", lines.Length, NodeType.Float);
                        }

                        pcharRotation = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rotation"));
                    }

                    if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
                    {
                        if (ParentGraph.IsParameterValueFunction(Id, "Scale"))
                        {
                            FunctionGraph func = ParentGraph.GetParameterRaw(Id, "Scale").Value as FunctionGraph;
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
                            FunctionGraph func = ParentGraph.GetParameterRaw(Id, "Position").Value as FunctionGraph;
                            func.SetVar("character", j, NodeType.Float);
                            func.SetVar("maxCharacters", line.Length, NodeType.Float);
                            func.SetVar("line", i, NodeType.Float);
                            func.SetVar("maxLines", lines.Length, NodeType.Float);
                        }

                        pPos = ParentGraph.GetParameterValue<MVector>(Id, "Position");
                    }

                    CharacterTransform ct = new CharacterTransform(pcharRotation * (float)(Math.PI / 180.0f), pPos, pScale);
                    transforms.Add(ct);
                }
            }
        }

        List<CharacterTransform> transforms = new List<CharacterTransform>();
        Dictionary<string, FontManager.CharData> map = new Dictionary<string, FontManager.CharData>();
        float pfontSize;
        string[] lines;
        void Process()
        {
            //need a clean buffer
            //when drawing
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }

            if (processor == null || lines == null) return;

            CreateBufferIfNeeded();

            processor.Prepare(width, height, character, buffer);

            float px = 1.0f / width;
            float py = 1.0f / height;

            if (map != null && map.Count > 0 && transforms.Count > 0)
            {
                int tindex = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    float left = 0;
                    for (int j = 0; j < line.Length; j++)
                    {
                        if (tindex >= transforms.Count) continue;

                        string ch = line.Substring(j, 1);
                        FontManager.CharData data = null;
                        if (map.TryGetValue(ch, out data))
                        {
                            if (data.texture == null)
                            {
                                tindex++;
                                continue;
                            }

                            CharacterTransform ct = transforms[tindex];
                            MVector finalPos = new MVector((ct.position.X + left * ct.scale.X) * width, (ct.position.Y + (i * data.bearing) * py * ct.scale.Y) * height);

                            left += (data.size.X + 1) * px;

                            character.Bind();
                            character.SetData(data.texture.Image, GLInterfaces.PixelFormat.Bgra, (int)Math.Ceiling(data.size.X), (int)Math.Ceiling(data.size.Y));
                            GLTextuer2D.Unbind();

                            processor.Translation = finalPos;
                            processor.Angle = ct.angle;
                            processor.Scale = ct.scale * (new MVector(data.size.X, data.size.Y) * 0.5f);
                            processor.ProcessCharacter(width, height, character, buffer);
                        }

                        tindex++;
                    }
                }
            }

            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }
    }
}
