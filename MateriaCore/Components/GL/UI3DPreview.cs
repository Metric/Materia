using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.GL.Menu;
using MateriaCore.Components.GL.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UI3DPreview : UIWindow
    {
        protected Scene scene;
        public Scene Scene { get => scene;  }

        protected const float DELTA_STAMP = 1f / 60f;
        protected const float CAM_SPEED = 16 * DELTA_STAMP;

        #region UI Components
        protected UIObject internalContainer;
        protected UIImage internalBackground;
        protected MovablePane previewArea;
        protected UIImage preview;

        protected UIMenu menu;
        #endregion

        #region 3D Objects
        ISceneRenderer sceneRenderer;
        SceneObject cube;
        SceneObject skyBox;
        SceneObject sphere;
        #endregion

        //note we need to come back and use 
        //localization for the title
        public UI3DPreview() : base(new Vector2(956f, 512f), "3D Preview")
        {
            RelativeTo = Anchor.BottomRight;
            Position = new Vector2(956f, 0);
            InitializeComponents();
            InitializeScene();
            InitializeMenu();
        }

        public void Render()
        {
            //do rendering here instead from MainGLWindow
            if (sceneRenderer != null && scene != null && Visible)
            {
                scene.ViewSize = previewArea.WorldSize;
                sceneRenderer.Render();
                preview.Texture = sceneRenderer.Image;
            }
        }

        private void InitializeScene()
        {
            scene = new Scene
            {
                Root = new SceneObject()
            };

            //load meshes
            //testing skybox data
            skyBox = scene.CreateMeshFromEmbeddedFile("Geometry/cube.obj", typeof(UI3DPreview), Materia.Rendering.Geometry.MeshRenderType.Skybox);
            cube = scene.CreateMeshFromEmbeddedFile("Geometry/cube.obj", typeof(UI3DPreview));

            scene.Root.Add(skyBox);
            scene.Root.Add(cube);

            //create scene renderer
            sceneRenderer = new SceneStackRenderer(scene);
        }

        private void InitializeComponents()
        {
            internalContainer = new UIObject
            {
                RaycastTarget = true,
                RelativeTo = Anchor.Fill
            };
            internalBackground = internalContainer.AddComponent<UIImage>();
            internalBackground.Color = new Vector4(0.05f, 0.05f, 0.05f, 1); //todo: use theme class
            //internalBackground.Clip = true;

            //size doesn't matter at this point
            //since it it will be calculated by fill
            previewArea = new MovablePane(new Vector2(1,1))
            {
                MoveAxis = Axis.None,
                SnapMode = MovablePaneSnapMode.None,
                RelativeTo = Anchor.Fill
            };
   
            preview = previewArea.GetComponent<UIImage>();
            preview.Color = Vector4.One;
            previewArea.Moved += PreviewArea_Moved;

            selectable.BubbleEvents = false;
            selectable.Wheel += Selectable_Wheel;

            internalContainer.AddChild(previewArea);
            content.AddChild(internalContainer);
        }

        private void InitializeMenu()
        {
            Localization.Local loc = new Localization.Local();
            UIMenuBuilder builder = new UIMenuBuilder();
            menu = builder.Add(loc.Get("Scene"))
                .StartSubMenu()
                    .Add("Cube")
                    .Add("Sphere")
                    .Add("Cube Sphere")
                    .Add("Rounded Cube")
                    .Add("Plane")
                    .Add("Cylinder")
                    .Separator()
                    .Add("Custom")
                    .Separator()
                    .Add("Reset")
                .FinishSubMenu()
                .Add(loc.Get("Camera"))
                .StartSubMenu()
                    .Add("Top")
                    .Add("Bottom")
                    .Add("Left")
                    .Add("Right")
                    .Add("Front")
                    .Add("Back")
                    .Add("Angled")
                    .Separator()
                    .Add("Orthographic")
                    .Add("Perspective")
                    .Separator()
                    .Add("Settings")
                .FinishSubMenu()
                .Add(loc.Get("Light"))
                .StartSubMenu()
                    .Add("Default Position")
                    .Add("Set to Origin")
                    .Add("Reset")
                    .Separator()
                    .Add("Settings")
                .FinishSubMenu()
                .Add(loc.Get("Material"))
                .StartSubMenu()
                    .Add("Settings")
                    .Separator()
                    .Add("Wireframe")
                    .Add("Solid")
                    .Separator()
                    .Add("Reset")
                .FinishSubMenu()
                .Finilize();
            content.AddChild(menu);
        }

        private void Selectable_Wheel(UISelectable arg1, MouseWheelArgs e)
        {
            if (scene == null) return;
            Debug.WriteLine("3d preview mouse wheel");
            Vector3 pos = scene.Cam.LocalPosition;
            pos.Z += e.Delta.Y * CAM_SPEED;
            scene.Cam.LocalPosition = pos;
            scene.IsModified = true;
        }

        private Quaternion GetViewAngle(ref Vector2 delta)
        {
            Quaternion rot = scene.Cam.LocalRotation;
            Quaternion deltaRot = Quaternion.FromEulerAngles(new Vector3(delta.Y, -delta.X, 0) * DELTA_STAMP);

            rot *= deltaRot;

            return rot;
        }

        private Vector3 GetViewTranslation(ref Vector2 delta)
        {
            Vector3 pos = scene.Cam.LocalPosition;
            Vector3 up = scene.Cam.Up;
            Vector3 right = scene.Cam.Right;

            float z = 1.0f / pos.Z;
            
            float dx = delta.X * DELTA_STAMP * z;
            float dy = delta.Y * DELTA_STAMP * z;

            return right * dx + up * dy;
        }

        private void PreviewArea_Moved(MovablePane arg1, Vector2 delta, MouseEventArgs e)
        {
            if (scene == null) return;

            if (e == null)
            {
                scene.Cam.LocalRotation = GetViewAngle(ref delta);
                scene.IsModified = true;
                return;
            }

            if (e.Button.HasFlag(MouseButton.Left))
            {
                Quaternion direction = GetViewAngle(ref delta);
                scene.Cam.LocalRotation = direction;
                scene.IsModified = true;
            }
            else if(e.Button.HasFlag(MouseButton.Middle))
            {
                scene.Root.LocalPosition += GetViewTranslation(ref delta);
                scene.IsModified = true;
            }
        }

        public override void Dispose(bool disposing = true)
        {
            sceneRenderer?.Dispose();
            sceneRenderer = null;

            scene?.Dispose();
            scene = null;

            base.Dispose(disposing);
        }
    }
}
