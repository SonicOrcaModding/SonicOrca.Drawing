// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.WaterfallRenderer
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

    internal class WaterfallRenderer : IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, WaterfallRenderer> RendererDictionary = new Dictionary<Renderer, WaterfallRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly WaterfallRenderer.Vertex[] _vertices = new WaterfallRenderer.Vertex[4];
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private Vector2[] distortionUVs = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;
      private Vector2[] vertexNormalisation = new Vector2[4];

      public ITexture DistortionTexture { get; set; }

      public double DistortionAmount { get; set; }

      public Vector2 DistortionOffset { get; set; }

      public Vector2i NonDistortionRadius { get; set; }

      public static WaterfallRenderer FromRenderer(Renderer renderer)
      {
        if (!WaterfallRenderer.RendererDictionary.ContainsKey(renderer))
          WaterfallRenderer.RendererDictionary.Add(renderer, new WaterfallRenderer(renderer));
        return WaterfallRenderer.RendererDictionary[renderer];
      }

      private WaterfallRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/waterfall.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (WaterfallRenderer.Vertex));
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
        ITexture distortionTexture = this.DistortionTexture;
        Vector2 distortionOffset = this.DistortionOffset;
        double x1 = distortionOffset.X * (double) this.DistortionTexture.Width;
        distortionOffset = this.DistortionOffset;
        double y1 = distortionOffset.Y * (double) this.DistortionTexture.Height;
        double width = (double) destination.Width;
        double height = (double) destination.Height;
        Rectanglei source1 = (Rectanglei) new Rectangle(x1, y1, width, height);
        Vector2[] distortionUvs = this.distortionUVs;
        Renderer.GetTextureMappings(distortionTexture, source1, distortionUvs);
        Renderer.GetVertices(new Rectangle(0.0, 0.0, 1.0, 1.0), this.vertexNormalisation);
        for (int index1 = 0; index1 < 4; ++index1)
        {
          WaterfallRenderer.Vertex[] vertices = this._vertices;
          int index2 = index1;
          WaterfallRenderer.Vertex vertex1 = new WaterfallRenderer.Vertex();
          ref WaterfallRenderer.Vertex local1 = ref vertex1;
          vec2 vec2_1 = new vec2();
          vec2_1.x = (float) this.vertexPositions[index1].X;
          vec2_1.y = (float) this.vertexPositions[index1].Y;
          vec2 vec2_2 = vec2_1;
          local1.position = vec2_2;
          ref WaterfallRenderer.Vertex local2 = ref vertex1;
          vec2_1 = new vec2();
          vec2_1.s = (float) this.vertexUVs[index1].X;
          vec2_1.t = (float) this.vertexUVs[index1].Y;
          vec2 vec2_3 = vec2_1;
          local2.texcoords = vec2_3;
          ref WaterfallRenderer.Vertex local3 = ref vertex1;
          vec2_1 = new vec2();
          vec2_1.s = (float) this.distortionUVs[index1].X;
          vec2_1.t = (float) this.distortionUVs[index1].Y;
          vec2 vec2_4 = vec2_1;
          local3.distortiontexcoords = vec2_4;
          ref WaterfallRenderer.Vertex local4 = ref vertex1;
          vec2_1 = new vec2();
          vec2_1.x = (float) this.vertexNormalisation[index1].X;
          vec2_1.y = (float) this.vertexNormalisation[index1].Y;
          vec2 vec2_5 = vec2_1;
          local4.normalisation = vec2_5;
          WaterfallRenderer.Vertex vertex2 = vertex1;
          vertices[index2] = vertex2;
        }
        this._vbo.SetData<WaterfallRenderer.Vertex>(this._vertices, 0, 4);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Opaque;
        this._graphicsContext.SetTextures((IEnumerable<ITexture>) new ITexture[2]
        {
          texture,
          this.DistortionTexture
        });
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        program.SetUniform("ProjectionMatrix", orthographic);
        program.SetUniform("InputDistortionAmount", 1.0 / (double) texture.Height * this.DistortionAmount);
        double num1 = 1.0 / (double) destination.Width;
        Vector2i distortionRadius = this.NonDistortionRadius;
        double x2 = (double) distortionRadius.X;
        double x3 = num1 * x2;
        double num2 = 1.0 / (double) destination.Height;
        distortionRadius = this.NonDistortionRadius;
        double y2 = (double) distortionRadius.Y;
        double y3 = num2 * y2;
        program.SetUniform("InputNonDistortionRadius", new Vector2(x3, y3));
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
        [VertexAttribute("VertexNormalisation")]
        public vec2 normalisation;
      }
    }
}
