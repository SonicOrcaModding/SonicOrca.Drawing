// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.MaskRenderer
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

    internal class MaskRenderer : IMaskRenderer, IDisposable, IRenderer
    {
      private readonly IGraphicsContext _graphicsContext;
      private ManagedShaderProgram _shaderProgram;
      private IBuffer _vbo;
      private IVertexArray _vao;
      private MaskRenderer.Vertex[] _vertices = new MaskRenderer.Vertex[4];
      private Matrix4 _maskModelMatrix;
      private Matrix4 _targetModelMatrix;
      private Matrix4 _intersectionModelMatrix;
      private static readonly Dictionary<Renderer, MaskRenderer> RendererDictionary = new Dictionary<Renderer, MaskRenderer>();

      public ITexture Texture { get; set; }

      public Rectanglei Source { get; set; }

      public Rectanglei Destination { get; set; }

      public ITexture MaskTexture { get; set; }

      public Rectanglei MaskSource { get; set; }

      public Rectanglei MaskDestination { get; set; }

      public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

      public Colour Colour { get; set; } = Colours.White;

      public Matrix4 MaskModelMatrix
      {
        get => this._maskModelMatrix;
        set
        {
          if (!(this._maskModelMatrix != value))
            return;
          this._maskModelMatrix = value;
        }
      }

      public Matrix4 IntersectionModelMatrix
      {
        get => this._intersectionModelMatrix;
        set
        {
          if (!(this._intersectionModelMatrix != value))
            return;
          this._intersectionModelMatrix = value;
        }
      }

      public Matrix4 TargetModelMatrix
      {
        get => this._targetModelMatrix;
        set
        {
          if (!(this._targetModelMatrix != value))
            return;
          this._targetModelMatrix = value;
        }
      }

      public static MaskRenderer FromRenderer(Renderer renderer)
      {
        if (!MaskRenderer.RendererDictionary.ContainsKey(renderer))
          MaskRenderer.RendererDictionary.Add(renderer, new MaskRenderer(renderer));
        return MaskRenderer.RendererDictionary[renderer];
      }

      public MaskRenderer(Renderer renderer)
      {
        this._graphicsContext = renderer.Window.GraphicsContext;
        renderer.RegisterRenderer((IRenderer) this);
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/mask.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (MaskRenderer.Vertex));
        this._shaderProgram.Program.Activate();
        this._shaderProgram.Program.SetUniform("InputTexture", 0);
        this._shaderProgram.Program.SetUniform("InputTextureMask", 1);
        this._intersectionModelMatrix = Matrix4.Identity;
        this._targetModelMatrix = Matrix4.Identity;
        this._maskModelMatrix = Matrix4.Identity;
      }

      public void Dispose()
      {
        this._vao.Dispose();
        this._vbo.Dispose();
        this._shaderProgram.Dispose();
      }

      public IDisposable BeginMatixState()
      {
        return (IDisposable) new MaskRenderer.MatrixState(this, this._intersectionModelMatrix, this._maskModelMatrix, this._targetModelMatrix);
      }

      public void Render(bool maskColorMultiply = false)
      {
        IGraphicsContext graphicsContext = this._graphicsContext;
        IShaderProgram program = this._shaderProgram.Program;
        Rectanglei clippedDestination = Rectanglei.Intersect(this.Destination, this.MaskDestination);
        if (this.Texture == null)
          throw new InvalidOperationException("Texture was null.");
        int num = 0;
        if (this.MaskTexture != null)
        {
          ++num;
          if (clippedDestination.Width <= 0 || clippedDestination.Height <= 0)
            return;
        }
        else
          clippedDestination = this.Destination;
        this._vertices[0].position.x = (float) clippedDestination.Left;
        this._vertices[0].position.y = (float) clippedDestination.Top;
        this._vertices[1].position.x = (float) clippedDestination.Left;
        this._vertices[1].position.y = (float) clippedDestination.Bottom;
        this._vertices[2].position.x = (float) clippedDestination.Right;
        this._vertices[2].position.y = (float) clippedDestination.Bottom;
        this._vertices[3].position.x = (float) clippedDestination.Right;
        this._vertices[3].position.y = (float) clippedDestination.Top;
        float left1;
        float top1;
        float right1;
        float bottom1;
        this.GetClippedSourceRect(this.Texture, this.Source, this.Destination, clippedDestination, out left1, out top1, out right1, out bottom1);
        ITexture texture = this.Texture;
        int width = texture.Width;
        int height = texture.Height;
        this._vertices[0].texcoords.s = left1;
        this._vertices[0].texcoords.t = top1;
        this._vertices[1].texcoords.s = left1;
        this._vertices[1].texcoords.t = bottom1;
        this._vertices[2].texcoords.s = right1;
        this._vertices[2].texcoords.t = bottom1;
        this._vertices[3].texcoords.s = right1;
        this._vertices[3].texcoords.t = top1;
        if (num > 0)
        {
          float left2;
          float top2;
          float right2;
          float bottom2;
          this.GetClippedSourceRect(this.MaskTexture, this.MaskSource, this.MaskDestination, clippedDestination, out left2, out top2, out right2, out bottom2);
          this._vertices[0].masktexcoords.s = left2;
          this._vertices[0].masktexcoords.t = top2;
          this._vertices[1].masktexcoords.s = left2;
          this._vertices[1].masktexcoords.t = bottom2;
          this._vertices[2].masktexcoords.s = right2;
          this._vertices[2].masktexcoords.t = bottom2;
          this._vertices[3].masktexcoords.s = right2;
          this._vertices[3].masktexcoords.t = top2;
        }
        this._vbo.SetData<MaskRenderer.Vertex>(this._vertices);
        graphicsContext.BlendMode = this.BlendMode;
        graphicsContext.SetTexture(0, this.Texture);
        graphicsContext.SetTexture(1, this.MaskTexture);
        program.Activate();
        program.SetUniform("IntersectionModelViewMatrix", this._intersectionModelMatrix);
        program.SetUniform("MaskModelViewMatrix", this._maskModelMatrix);
        program.SetUniform("TargetModelViewMatrix", this._targetModelMatrix);
        program.SetUniform("ProjectionMatrix", this._graphicsContext.CurrentFramebuffer.CreateOrthographic());
        program.SetUniform("MaskInput", num);
        program.SetUniform("InputColour", this.Colour);
        program.SetUniform("MaskColorMultiply", maskColorMultiply ? 1 : 0);
        this._vao.Render<MaskRenderer.Vertex>(PrimitiveType.Quads, this._vertices);
      }

      private void GetClippedSourceRect(
        ITexture sourceTexture,
        Rectanglei source,
        Rectanglei destination,
        Rectanglei clippedDestination,
        out float left,
        out float top,
        out float right,
        out float bottom)
      {
        left = (float) source.Left / (float) sourceTexture.Width;
        top = (float) source.Top / (float) sourceTexture.Height;
        right = (float) source.Right / (float) sourceTexture.Width;
        bottom = (float) source.Bottom / (float) sourceTexture.Height;
        float num1 = right - left;
        float num2 = bottom - top;
        left += (float) (clippedDestination.Left - destination.Left) / (float) destination.Width / num1;
        top += (float) (clippedDestination.Top - destination.Top) / (float) destination.Height / num2;
        right -= (float) (destination.Right - clippedDestination.Right) / (float) destination.Width / num2;
        bottom -= (float) (destination.Bottom - clippedDestination.Bottom) / (float) destination.Height / num2;
      }

      public void Deactivate()
      {
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
        [VertexAttribute("VertexMaskTextureMapping")]
        public vec2 masktexcoords;
      }

      private class MatrixState : IDisposable
      {
        private readonly MaskRenderer _maskRenderer;
        private readonly Matrix4 _maskMatrix;
        private readonly Matrix4 _targetMatrix;
        private readonly Matrix4 _intersectionMatrix;

        public MatrixState(
          MaskRenderer maskRenderer,
          Matrix4 intersectionMatrix,
          Matrix4 maskMatrix,
          Matrix4 targetMatrix)
        {
          this._maskRenderer = maskRenderer;
          this._intersectionMatrix = intersectionMatrix;
          this._maskMatrix = maskMatrix;
          this._targetMatrix = targetMatrix;
        }

        public void Dispose()
        {
          this._maskRenderer.IntersectionModelMatrix = this._intersectionMatrix;
          this._maskRenderer.MaskModelMatrix = this._maskMatrix;
          this._maskRenderer.TargetModelMatrix = this._targetMatrix;
        }
      }
    }
}
