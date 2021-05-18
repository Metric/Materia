using Avalonia.Controls;
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
using System.Threading.Tasks;

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
        SceneObject activeMesh;
        SceneObject cube;
        SceneObject skyBox;
        SceneObject light;
        SceneObject roundedCube;
        SceneObject cubeSphere;
        SceneObject cylinder;
        SceneObject plane;
        SceneObject custom;
        SceneObject sphere;

        string customFilePath;
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
                sceneRenderer.UV();
                Utils.GlobalEvents.Emit(Utils.GlobalEvent.Preview2DUV, this, sceneRenderer.Image);
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
            //these are all the base ones we need to load to start with
            //we will load the others on demand when the user selects it
            skyBox = scene.CreateMeshFromEmbeddedFile("Geometry/cube.obj", typeof(UI3DPreview), Materia.Rendering.Geometry.MeshRenderType.Skybox);
            activeMesh = cube = scene.CreateMeshFromEmbeddedFile("Geometry/cube.obj", typeof(UI3DPreview));
            light = scene.CreateMeshFromEmbeddedFile("Geometry/sphere-standard.obj", typeof(UI3DPreview), Materia.Rendering.Geometry.MeshRenderType.Light);

            scene.Root.Add(skyBox);
            scene.Root.Add(cube);
            scene.Root.Add(light);

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
                    .Add("Cube", GeometryView_Click)
                    .Add("Sphere", GeometryView_Click)
                    .Add("Cube Sphere", GeometryView_Click)
                    .Add("Rounded Cube", GeometryView_Click)
                    .Add("Plane", GeometryView_Click)
                    .Add("Cylinder", GeometryView_Click)
                    .Separator()
                    .Add("Custom", GeometryView_Click)
                    .Separator()
                    .Add("Reset", ResetScene_Click)
                .FinishSubMenu()
                .Add(loc.Get("Camera"))
                .StartSubMenu()
                    .Add("Top", CameraView_Click)
                    .Add("Bottom", CameraView_Click)
                    .Add("Left", CameraView_Click)
                    .Add("Right", CameraView_Click)
                    .Add("Front", CameraView_Click)
                    .Add("Back", CameraView_Click)
                    .Add("Angled", CameraView_Click)
                    .Separator()
                    .Add("Orthographic", CameraMode_Click)
                    .Add("Perspective", CameraMode_Click)
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
                    .Add("Wireframe", PolygonMode_Click)
                    .Add("Solid", PolygonMode_Click)
                    .Separator()
                    .Add("Reset")
                .FinishSubMenu()
                .Finilize();
            content.AddChild(menu);
        }

        #region custom mesh helpers
        private void UnloadCustomMesh()
        {
            if (custom != null)
            {
                scene.Root.Remove(custom);
                scene.Delete(custom);
                custom = null;
            }

            if (!string.IsNullOrEmpty(customFilePath))
            {
                scene.DeleteCache(customFilePath);
                customFilePath = null;
            }
        }

        private void LoadCustomMesh()
        {
            string[] f = null;
            Task.Run(async () =>
            {
                try
                {
                    var dialog = new OpenFileDialog();
                    dialog.AllowMultiple = false;
                    FileDialogFilter filter = new FileDialogFilter();
                    filter.Name = "Mesh Files";
                    filter.Extensions.Add("fbx");
                    filter.Extensions.Add("obj");
                    dialog.Filters.Add(filter);
                    dialog.Title = "Import Mesh";
                    f = await dialog.ShowAsync(MainWindow.Instance); //will need a window handle I believe
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }).ContinueWith(t =>
            {
                if (f == null || f.Length == 0) return;
                UnloadCustomMesh();
                customFilePath = f[0];
                custom = scene.CreateMeshFromFile(customFilePath);
                scene.Root.Add(custom);

                activeMesh.Visible = false;
                activeMesh = custom;
                activeMesh.Visible = true;

                scene.IsModified = true;

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion

        #region Menu Callbacks
        private void ResetScene_Click(InfinityUI.Controls.Button item)
        {
            scene.Root.LocalPosition = new Vector3(0, 0, 0);
            scene.CameraView = PreviewCameraPosition.Front;
            scene.IsModified = true;
            UI.Focus = null;
        }

        private void GeometryView_Click(InfinityUI.Controls.Button item)
        {
            PreviewGeometryType v;
            Enum.TryParse<PreviewGeometryType>(item.Text.Replace(" ", ""), out v);
            switch(v)
            {
                case PreviewGeometryType.Cube:
                    activeMesh.Visible = false;
                    activeMesh = cube;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.CubeSphere:
                    if (cubeSphere == null)
                    {
                        cubeSphere = scene.CreateMeshFromEmbeddedFile("Geometry/cube-sphere.obj", typeof(UI3DPreview));
                        scene.Root.Add(cubeSphere);
                    }
                    activeMesh.Visible = false;
                    activeMesh = cubeSphere;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.Cylinder:
                    if (cylinder == null)
                    {
                        cylinder = scene.CreateMeshFromEmbeddedFile("Geometry/cylinder.obj", typeof(UI3DPreview));
                        scene.Root.Add(cylinder);
                    }
                    activeMesh.Visible = false;
                    activeMesh = cylinder;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.Plane:
                    if (plane == null)
                    {
                        plane = scene.CreateMeshFromEmbeddedFile("Geometry/plane.obj", typeof(UI3DPreview));
                        plane.LocalRotation = Quaternion.FromEulerAngles(new Vector3(180 * MathHelper.Deg2Rad, 0, 0));
                        plane.LocalScale = new Vector3(2, 2, 2);
                        scene.Root.Add(plane);
                    }
                    activeMesh.Visible = false;
                    activeMesh = plane;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.RoundedCube:
                    if (roundedCube == null)
                    {
                        roundedCube = scene.CreateMeshFromEmbeddedFile("Geometry/cube-rounded.obj", typeof(UI3DPreview));
                        scene.Root.Add(roundedCube);
                    }
                    activeMesh.Visible = false;
                    activeMesh = roundedCube;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.Sphere:
                    if (sphere == null)
                    {
                        sphere = scene.CreateMeshFromEmbeddedFile("Geometry/sphere-standard.obj", typeof(UI3DPreview));
                        scene.Root.Add(sphere);
                    }
                    activeMesh.Visible = false;
                    activeMesh = sphere;
                    activeMesh.Visible = true;
                    break;
                case PreviewGeometryType.Custom:
                    LoadCustomMesh();
                    break;
            }
            UI.Focus = null;
            scene.IsModified = true;
        }
        
        private void PolygonMode_Click(InfinityUI.Controls.Button item)
        {
            PreviewRenderMode v;
            Enum.TryParse<PreviewRenderMode>(item.Text, out v);
            sceneRenderer.PolyMode = v;
            UI.Focus = null;
        }

        private void CameraView_Click(InfinityUI.Controls.Button item)
        {
            PreviewCameraPosition v;
            Enum.TryParse<PreviewCameraPosition>(item.Text, out v);
            scene.CameraView = v;
            scene.IsModified = true;
            UI.Focus = null;
        }
        #endregion

        private void CameraMode_Click(InfinityUI.Controls.Button item)
        {
            PreviewCameraMode v;
            Enum.TryParse<PreviewCameraMode>(item.Text, out v);
            scene.CameraMode = v;
            scene.IsModified = true;
            UI.Focus = null;
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

            scene.CameraView = PreviewCameraPosition.Custom;

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
