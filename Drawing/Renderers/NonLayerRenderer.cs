// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.NonLayerRenderer
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

    internal class NonLayerRenderer : INonLayerRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, NonLayerRenderer> RendererDictionary = new Dictionary<Renderer, NonLayerRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly List<NonLayerRenderer.Vertex> _vertices = new List<NonLayerRenderer.Vertex>();
      private Vector2[] vertexPositions = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;

      public static NonLayerRenderer FromRenderer(Renderer renderer)
      {
        if (!NonLayerRenderer.RendererDictionary.ContainsKey(renderer))
          NonLayerRenderer.RendererDictionary.Add(renderer, new NonLayerRenderer(renderer));
        return NonLayerRenderer.RendererDictionary[renderer];
      }

      private NonLayerRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/nonlayer.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (NonLayerRenderer.Vertex));
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

      public void Render(Rectanglei destination)
      {
        this.Render((IEnumerable<Rectanglei>) new Rectanglei[1]
        {
          destination
        });
      }

      public void Render(IEnumerable<Rectanglei> destinations)
      {
        this._vertices.Clear();
        foreach (Rectanglei destination in destinations)
        {
          Vector2[] vertices = new Vector2[4];
          Renderer.GetVertices((Rectangle) destination, vertices);
          for (int index = 0; index < 4; ++index)
            this._vertices.Add(new NonLayerRenderer.Vertex()
            {
              position = vertices[index].ToVec2()
            });
        }
        this._vbo.SetData<NonLayerRenderer.Vertex>(this._vertices.ToArray());
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        program.SetUniform("ProjectionMatrix", orthographic);
        this._vao.Render(PrimitiveType.Quads, 0, this._vertices.Count);
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
      }
    }
}
