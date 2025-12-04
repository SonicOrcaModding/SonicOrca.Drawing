// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.GaussianBlurRenderer
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

    public class GaussianBlurRenderer : IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, GaussianBlurRenderer> RendererDictionary = new Dictionary<Renderer, GaussianBlurRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly GaussianBlurRenderer.Vertex[] _vertices = new GaussianBlurRenderer.Vertex[4];
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;
      private IFramebuffer _intermediateTarget;
      private double[] _gaussianWeights;
      private double _softness;

      public double Softness
      {
        get => this._softness;
        set
        {
          if (this._softness == value)
            return;
          this._softness = value != 0.0 ? value : throw new ArgumentException("Softness can not be 0.", nameof (value));
          this._gaussianWeights = GaussianBlurRenderer.CalculateWeights(value);
        }
      }

      public static GaussianBlurRenderer FromRenderer(Renderer renderer)
      {
        if (!GaussianBlurRenderer.RendererDictionary.ContainsKey(renderer))
          GaussianBlurRenderer.RendererDictionary.Add(renderer, new GaussianBlurRenderer(renderer));
        return GaussianBlurRenderer.RendererDictionary[renderer];
      }

      private GaussianBlurRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/gaussian_blur.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (GaussianBlurRenderer.Vertex));
        this.Softness = 8.0;
        this._shaderProgram.Program.Activate();
        this._shaderProgram.Program.SetUniform("InputTexture", 0);
      }

      public void Dispose()
      {
        if (this._intermediateTarget != null)
          this._intermediateTarget.Dispose();
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
        for (int index1 = 0; index1 < 4; ++index1)
        {
          GaussianBlurRenderer.Vertex[] vertices = this._vertices;
          int index2 = index1;
          GaussianBlurRenderer.Vertex vertex1 = new GaussianBlurRenderer.Vertex();
          ref GaussianBlurRenderer.Vertex local1 = ref vertex1;
          vec2 vec2_1 = new vec2();
          vec2_1.x = (float) this.vertexPositions[index1].X;
          vec2_1.y = (float) this.vertexPositions[index1].Y;
          vec2 vec2_2 = vec2_1;
          local1.position = vec2_2;
          ref GaussianBlurRenderer.Vertex local2 = ref vertex1;
          vec2_1 = new vec2();
          vec2_1.s = (float) this.vertexUVs[index1].X;
          vec2_1.t = (float) this.vertexUVs[index1].Y;
          vec2 vec2_3 = vec2_1;
          local2.texcoords = vec2_3;
          GaussianBlurRenderer.Vertex vertex2 = vertex1;
          vertices[index2] = vertex2;
        }
        this._vbo.SetData<GaussianBlurRenderer.Vertex>(this._vertices, 0, 4);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        this._graphicsContext.SetTexture(texture);
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        program.SetUniform("ProjectionMatrix", orthographic);
        program.SetUniform("InputAxis", 0);
        for (int index = 0; index < 5; ++index)
          program.SetUniform($"InputWeight[{(object) index}]", this._gaussianWeights[index]);
        IFramebuffer currentFramebuffer = this._graphicsContext.CurrentFramebuffer;
        if (this._intermediateTarget == null || this._intermediateTarget.Width < destination.Width || this._intermediateTarget.Height < destination.Height)
        {
          if (this._intermediateTarget != null)
            this._intermediateTarget.Dispose();
          this._intermediateTarget = this._graphicsContext.CreateFrameBuffer(destination.Width, destination.Height);
        }
        this._intermediateTarget.Activate();
        this._graphicsContext.ClearBuffer();
        this._graphicsContext.BlendMode = BlendMode.Opaque;
        this._vao.Render(PrimitiveType.Quads, 0, 4);
        this._graphicsContext.SetTexture(this._intermediateTarget.Textures[0]);
        program.SetUniform("InputAxis", 1);
        currentFramebuffer.Activate();
        this._vao.Render(PrimitiveType.Quads, 0, 4);
      }

      private static double[] CalculateWeights(double sigma2)
      {
        double[] weights = new double[5]
        {
          GaussianBlurRenderer.Gaussian(0.0, sigma2),
          0.0,
          0.0,
          0.0,
          0.0
        };
        double num = weights[0];
        for (int x = 1; x < weights.Length; ++x)
        {
          weights[x] = GaussianBlurRenderer.Gaussian((double) x, sigma2);
          num += 2.0 * weights[x];
        }
        for (int index = 0; index < weights.Length; ++index)
          weights[index] /= num;
        return weights;
      }

      private static double Gaussian(double x, double sigma2)
      {
        return 1.0 / (2.0 * Math.PI * sigma2) * Math.Exp(-(x * x) / (2.0 * sigma2));
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
