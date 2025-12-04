// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.CharacterRenderer
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

    public class CharacterRenderer : ICharacterRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, CharacterRenderer> RendererDictionary = new Dictionary<Renderer, CharacterRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly IVertexArray _vaoGhost;
      private CharacterRenderer.Vertex[] _vertices = new CharacterRenderer.Vertex[4];
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;
      private ManagedShaderProgram _ghostShaderProgram;

      public Matrix4 ModelMatrix { get; set; }

      public Rectangle ClipRectangle { get; set; }

      public int Filter { get; set; }

      public double FilterAmount { get; set; }

      public float Brightness { get; set; }

      public static CharacterRenderer FromRenderer(Renderer renderer)
      {
        if (!CharacterRenderer.RendererDictionary.ContainsKey(renderer))
          CharacterRenderer.RendererDictionary.Add(renderer, new CharacterRenderer(renderer));
        return CharacterRenderer.RendererDictionary[renderer];
      }

      private CharacterRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/character.shader");
        this._ghostShaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/ghost.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vaoGhost = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (CharacterRenderer.Vertex));
        this._vaoGhost.DefineAttributes(this._ghostShaderProgram.Program, this._vbo, typeof (CharacterRenderer.Vertex));
        this.ModelMatrix = Matrix4.Identity;
        this.ClipRectangle = new Rectangle(0.0, 0.0, 1920.0, 1080.0);
        this._shaderProgram.Program.Activate();
        this._shaderProgram.Program.SetUniform("InputTextureSkin", 0);
        this._shaderProgram.Program.SetUniform("InputTextureBody", 1);
      }

      public void Dispose()
      {
        this._vao.Dispose();
        this._vaoGhost.Dispose();
        this._vbo.Dispose();
        this._shaderProgram.Dispose();
        this._ghostShaderProgram.Dispose();
      }

      public void Deactivate()
      {
      }

      public void RenderTexture(
        ITexture skinTexture,
        ITexture bodyTexture,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        this.RenderTexture(skinTexture, bodyTexture, 0.0, 0.0, 0.0, source, destination, flipX, flipY);
      }

      public void RenderTexture(
        ITexture skinTexture,
        ITexture bodyTexture,
        double hueShift,
        double satuationShift,
        double luminosityShift,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        this.RenderTexture(this._shaderProgram, skinTexture, bodyTexture, hueShift, satuationShift, luminosityShift, source, destination, flipX, flipY);
      }

      public void RenderTextureGhost(
        ITexture skinTexture,
        ITexture bodyTexture,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        this.RenderTexture(this._ghostShaderProgram, skinTexture, bodyTexture, 0.0, 0.0, 0.0, source, destination, flipX, flipY);
      }

      private void RenderTexture(
        ManagedShaderProgram managedShaderProgram,
        ITexture skinTexture,
        ITexture bodyTexture,
        double hueShift,
        double satuationShift,
        double luminosityShift,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        Renderer.GetVertices(destination, this.vertexPositions);
        Renderer.GetTextureMappings(skinTexture, (Rectanglei) source, this.vertexUVs, flipX, flipY);
        for (int index1 = 0; index1 < 4; ++index1)
        {
          CharacterRenderer.Vertex[] vertices = this._vertices;
          int index2 = index1;
          CharacterRenderer.Vertex vertex1 = new CharacterRenderer.Vertex();
          ref CharacterRenderer.Vertex local1 = ref vertex1;
          vec2 vec2_1 = new vec2();
          vec2_1.x = (float) this.vertexPositions[index1].X;
          vec2_1.y = (float) this.vertexPositions[index1].Y;
          vec2 vec2_2 = vec2_1;
          local1.position = vec2_2;
          ref CharacterRenderer.Vertex local2 = ref vertex1;
          vec2_1 = new vec2();
          vec2_1.s = (float) this.vertexUVs[index1].X;
          vec2_1.t = (float) this.vertexUVs[index1].Y;
          vec2 vec2_3 = vec2_1;
          local2.texcoords = vec2_3;
          CharacterRenderer.Vertex vertex2 = vertex1;
          vertices[index2] = vertex2;
        }
        this._vbo.SetData<CharacterRenderer.Vertex>(this._vertices, 0, 4);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        this._graphicsContext.SetTextures((IEnumerable<ITexture>) new ITexture[2]
        {
          skinTexture,
          bodyTexture
        });
        IShaderProgram program = managedShaderProgram.Program;
        program.Activate();
        if (ShadowRenderer.IsShadowing)
          program.SetUniform("AlphaGrayscale", 1f);
        else
          program.SetUniform("AlphaGrayscale", 0.0f);
        program.SetUniform("ModelViewMatrix", this.ModelMatrix);
        program.SetUniform("ProjectionMatrix", orthographic);
        program.SetUniform("InputHSLShift", new Vector3(hueShift, satuationShift, luminosityShift));
        IShaderProgram shaderProgram = program;
        double x = this.ClipRectangle.X;
        double y = this.ClipRectangle.Y;
        Rectangle clipRectangle = this.ClipRectangle;
        double right = clipRectangle.Right;
        clipRectangle = this.ClipRectangle;
        double bottom = clipRectangle.Bottom;
        Vector4 vector4 = new Vector4(x, y, right, bottom);
        shaderProgram.SetUniform("InputClipRectangle", vector4);
        program.SetUniform("InputFilter", this.Filter);
        program.SetUniform("InputFilterAmount", this.FilterAmount);
        if (managedShaderProgram == this._shaderProgram)
          program.SetUniform("InputBrightness", this.Brightness);
        (managedShaderProgram == this._ghostShaderProgram ? this._vaoGhost : this._vao).Render(PrimitiveType.Quads, 0, 4);
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
