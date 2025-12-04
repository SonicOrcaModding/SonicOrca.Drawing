// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.HeatRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Graphics.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SonicOrca.Drawing.Renderers
{

    internal class HeatRenderer : IHeatRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, HeatRenderer> RendererDictionary = new Dictionary<Renderer, HeatRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly HeatRenderer.Vertex[] _vertices = new HeatRenderer.Vertex[4];
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private Vector2[] distortionUVs = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;

      public ITexture DistortionTexture { get; set; }

      public double DistortionAmount { get; set; }

      public static HeatRenderer FromRenderer(Renderer renderer)
      {
        if (!HeatRenderer.RendererDictionary.ContainsKey(renderer))
          HeatRenderer.RendererDictionary.Add(renderer, new HeatRenderer(renderer));
        return HeatRenderer.RendererDictionary[renderer];
      }

      private HeatRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/heat.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (HeatRenderer.Vertex));
        this._shaderProgram.Program.Activate();
        this._shaderProgram.Program.SetUniform("InputTexture", 0);
        this._shaderProgram.Program.SetUniform("InputDistortionTexture", 1);
      }

      public void Dispose()
      {
        this._vbo.Dispose();
        this._vao.Dispose();
        this._shaderProgram.Dispose();
      }

      public void Deactivate()
      {
      }

      public void Render(ITexture texture, Rectanglei destination, bool flipX = false, bool flipY = false)
      {
        this.Render(texture, new Rectanglei(0, 0, texture.Width, texture.Height), destination, flipX, flipY);
      }

      public void Render(
        ITexture texture,
        Rectanglei source,
        Rectanglei destination,
        bool flipX = false,
        bool flipY = false)
      {
        Renderer.GetVertices((Rectangle) destination, this.vertexPositions);
        Renderer.GetTextureMappings(texture, source, this.vertexUVs, flipX, flipY);
        Renderer.GetTextureMappings(this.DistortionTexture, new Rectanglei(0, 0, this.DistortionTexture.Width, this.DistortionTexture.Height), this.distortionUVs);
        for (int index = 0; index < 4; ++index)
          this._vertices[index] = new HeatRenderer.Vertex()
          {
            position = this.vertexPositions[index].ToVec2(),
            texcoords = this.vertexUVs[index].ToVec2(),
            distortiontexcoords = this.distortionUVs[index].ToVec2()
          };
        this._vbo.SetData<HeatRenderer.Vertex>(this._vertices, 0, 4);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        this._graphicsContext.SetTextures((IEnumerable<ITexture>) new ITexture[2]
        {
          texture,
          this.DistortionTexture
        });
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        program.SetUniform("ProjectionMatrix", orthographic);
        program.SetUniform("InputDistortionAmount", this.DistortionAmount);
        this._vao.Render(PrimitiveType.Quads, 0, 4);
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
        [VertexAttribute("DistortionTextureMapping")]
        public vec2 distortiontexcoords;
      }
    }
}
