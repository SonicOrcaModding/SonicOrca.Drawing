// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.WaterRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Core;
using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Graphics.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SonicOrca.Drawing.Renderers
{

    internal class WaterRenderer : IWaterRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, WaterRenderer> RendererDictionary = new Dictionary<Renderer, WaterRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private readonly WaterRenderer.Vertex[] _vertices = new WaterRenderer.Vertex[4];
      private ManagedShaderProgram _shaderProgram;
      private IFramebuffer _canvas;

      public double HueTarget { get; set; }

      public double HueAmount { get; set; }

      public double SaturationChange { get; set; }

      public double LuminosityChange { get; set; }

      public double WavePhase { get; set; }

      public double NumWaves { get; set; }

      public double WaveSize { get; set; }

      public float Time { get; set; }

      public static WaterRenderer FromRenderer(Renderer renderer)
      {
        if (!WaterRenderer.RendererDictionary.ContainsKey(renderer))
          WaterRenderer.RendererDictionary.Add(renderer, new WaterRenderer(renderer));
        return WaterRenderer.RendererDictionary[renderer];
      }

      private WaterRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/water.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (WaterRenderer.Vertex));
        this._canvas = this._graphicsContext.CreateFrameBuffer(1920, 1080);
      }

      public void Dispose()
      {
        this._canvas.Dispose();
        this._vbo.Dispose();
        this._vao.Dispose();
        this._shaderProgram.Dispose();
      }

      public void Deactivate()
      {
      }

      public void Render(Rectanglei regionToFilter)
      {
        IFramebuffer currentFramebuffer = this._graphicsContext.CurrentFramebuffer;
        WaterManager.waveTexture.Wrapping = TextureWrapping.Repeat;
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 33071);
        int y = (int) ((double) WaterManager.viewportWaterLevel - 80.0);
        if (y < 0)
          y = 0;
        if (y > 1080)
          y = 1080;
        this._canvas.Activate();
        this.Render(currentFramebuffer.Textures[0], new Rectanglei(0, 0, 1920, 1080 - y), new Rectanglei(0, 0, 1920, 1080 - y));
        currentFramebuffer.Activate();
        SimpleRenderer simpleRenderer = SimpleRenderer.FromRenderer(this._renderer);
        simpleRenderer.BlendMode = BlendMode.Alpha;
        simpleRenderer.Colour = Colours.White;
        simpleRenderer.RenderTexture(this._canvas.Textures[0], (Rectangle) new Rectanglei(0, y, 1920, 1080), (Rectangle) new Rectanglei(0, y, 1920, 1080), false, false);
        simpleRenderer.BlendMode = BlendMode.Alpha;
        simpleRenderer.Deactivate();
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
        for (int index = 0; index < 4; ++index)
        {
          this._vertices[index].position = this.vertexPositions[index].ToVec2();
          this._vertices[index].texcoords = this.vertexUVs[index].ToVec2();
        }
        this._vbo.SetData<WaterRenderer.Vertex>(this._vertices, 0, 4);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        this._graphicsContext.SetTexture(0, texture);
        this._graphicsContext.SetTexture(1, WaterManager.waveTexture);
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        program.SetUniform("ProjectionMatrix", orthographic);
        program.SetUniform("InputTexture", 0);
        program.SetUniform("WaveTexture", 1);
        program.SetUniform("InputHueTarget", this.HueTarget);
        program.SetUniform("InputHueAmount", this.HueAmount);
        program.SetUniform("InputSaturationChange", this.SaturationChange);
        program.SetUniform("InputLuminosityChange", this.LuminosityChange);
        program.SetUniform("InputWavePhase", this.WavePhase);
        program.SetUniform("InputNumWaves", this.NumWaves);
        program.SetUniform("InputWaveSize", this.WaveSize);
        program.SetUniform("iGlobalTime", this.Time);
        program.SetUniform("InputPositionX", WaterManager.offsetX);
        program.SetUniform("InputPositionY", -WaterManager.offsetY);
        program.SetUniform("InputWaterLevel", WaterManager.viewportWaterLevel / 1080f);
        this._graphicsContext.BlendMode = BlendMode.Opaque;
        this._vao.Render(SonicOrca.Graphics.PrimitiveType.Quads, 0, 4);
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
      }
    }
}
