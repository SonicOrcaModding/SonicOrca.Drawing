// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.ClassicFadeTransitionRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Extensions;
using SonicOrca.Graphics;
using SonicOrca.Graphics.LowLevel;
using System;
using System.Runtime.InteropServices;

namespace SonicOrca.Drawing.Renderers
{

    internal class ClassicFadeTransitionRenderer : IFadeTransitionRenderer, IDisposable
    {
      private readonly IGraphicsContext _graphicsContext;
      private ManagedShaderProgram _shaderProgram;
      private IBuffer _vbo;
      private IVertexArray _vao;
      private ClassicFadeTransitionRenderer.Vertex[] _vertices = new ClassicFadeTransitionRenderer.Vertex[4];
      private IFramebuffer _pingPongBuffer;

      public float Opacity { get; set; }

      public ClassicFadeTransitionRenderer(IGraphicsContext graphicsContext)
      {
        this._graphicsContext = graphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/transition.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (ClassicFadeTransitionRenderer.Vertex));
        this._shaderProgram.Program.SetUniform("InputTexture", 0);
      }

      public void Dispose()
      {
        this._vao.Dispose();
        this._vbo.Dispose();
        this._shaderProgram.Dispose();
        if (this._pingPongBuffer == null)
          return;
        this._pingPongBuffer.Dispose();
      }

      public void Render()
      {
        IFramebuffer currentFramebuffer = this._graphicsContext.CurrentFramebuffer;
        if (this._pingPongBuffer == null || this._pingPongBuffer.Width != currentFramebuffer.Width || this._pingPongBuffer.Height != currentFramebuffer.Height)
        {
          if (this._pingPongBuffer != null)
            this._pingPongBuffer.Dispose();
          this._pingPongBuffer = this._graphicsContext.CreateFrameBuffer(currentFramebuffer.Width, currentFramebuffer.Height);
        }
        this.Render(currentFramebuffer, this._pingPongBuffer);
        this.Opacity = 0.0f;
        this.Render(this._pingPongBuffer, currentFramebuffer);
      }

      public void Render(IFramebuffer sourceFramebuffer, IFramebuffer destFramebuffer)
      {
        IGraphicsContext graphicsContext = this._graphicsContext;
        IShaderProgram program = this._shaderProgram.Program;
        destFramebuffer.Activate();
        this._vertices[0].position.x = 0.0f;
        this._vertices[0].position.y = 0.0f;
        this._vertices[1].position.x = 0.0f;
        this._vertices[1].position.y = (float) destFramebuffer.Height;
        this._vertices[2].position.x = (float) destFramebuffer.Width;
        this._vertices[2].position.y = (float) destFramebuffer.Height;
        this._vertices[3].position.x = (float) destFramebuffer.Width;
        this._vertices[3].position.y = 0.0f;
        this._vertices[0].texcoords.s = 0.0f;
        this._vertices[0].texcoords.t = 1f;
        this._vertices[1].texcoords.s = 0.0f;
        this._vertices[1].texcoords.t = 0.0f;
        this._vertices[2].texcoords.s = 1f;
        this._vertices[2].texcoords.t = 0.0f;
        this._vertices[3].texcoords.s = 1f;
        this._vertices[3].texcoords.t = 1f;
        this._vbo.SetData<ClassicFadeTransitionRenderer.Vertex>(this._vertices);
        graphicsContext.BlendMode = BlendMode.Opaque;
        graphicsContext.SetTexture(sourceFramebuffer.Textures[0]);
        program.Activate();
        program.SetUniform("ProjectionMatrix", destFramebuffer.CreateOrthographic());
        program.SetUniform("InputDelta", this.Opacity);
        this._vao.Render<ClassicFadeTransitionRenderer.Vertex>(PrimitiveType.Quads, this._vertices);
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
