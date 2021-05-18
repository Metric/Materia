﻿using Materia.Rendering;
using Materia.Rendering.Geometry;
using Materia.Rendering.Material;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public class MeshSceneObject : SceneObject
    {
        public Func<MeshRenderType, PBRMaterial> GetActiveMaterial { get; set; }
        public Func<GLTextureCube> GetActivePrefilterMap { get; set; }
        public Func<GLTextureCube> GetActiveIrradianceMap { get; set; }
        public Func<GLTextureCube> GetActiveEnvironmentMap { get; set; }

        public Func<Matrix4> GetActiveProjection { get; set; }
        public Func<Matrix4> GetActiveView { get; set; }
        public Func<Vector3> GetActiveEyePosition { get; set; }

        public Func<float> GetActiveNear { get; set; }
        public Func<float> GetActiveFar { get; set; }

        public Func<Light> GetActiveLight { get; set; }

        public MeshRenderType RenderMode { get; set; }

        public MeshRenderer Renderer { get; set; }

        public override void Dispose()
        {
            Renderer?.Dispose();
            base.Dispose();
        }

        public override void Draw()
        {
            if (!Visible) return;
            if (Renderer != null)
            {
                Renderer.RenderMode = RenderMode;
                Renderer.Mat = GetActiveMaterial?.Invoke(RenderMode);
                Renderer.IrradianceMap = GetActiveIrradianceMap?.Invoke();
                Renderer.PrefilterMap = GetActivePrefilterMap?.Invoke();
                Renderer.EnvironmentMap = GetActiveEnvironmentMap?.Invoke();

                if (GetActiveProjection != null)
                {
                    Renderer.Projection = GetActiveProjection.Invoke();
                }
                if (GetActiveView != null)
                {
                    Renderer.View = GetActiveView.Invoke();
                }
                if (GetActiveEyePosition != null)
                {
                    Renderer.CameraPosition = GetActiveEyePosition.Invoke();
                }
                Light light = GetActiveLight?.Invoke();

                if (light != null)
                {
                    if (RenderMode == MeshRenderType.Light)
                    {
                        LocalPosition = light.Position;
                        LocalScale = light.LocalScale;
                    }

                    Renderer.LightColor = light.Color;
                    Renderer.LightPosition = light.Position;
                    Renderer.LightPower = light.Power;
                }

                if (GetActiveNear != null)
                {
                    Renderer.Near = GetActiveNear.Invoke();
                }

                if (GetActiveFar != null)
                {
                    Renderer.Far = GetActiveFar.Invoke();
                }

                Renderer.Model = WorldMatrix;
                Renderer.Draw();
            }
        }
    }
}
