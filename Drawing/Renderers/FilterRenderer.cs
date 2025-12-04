// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.FilterRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace SonicOrca.Drawing.Renderers
{

    internal class FilterRenderer : IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, FilterRenderer> RendererDictionary = new Dictionary<Renderer, FilterRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private IShader _vertexShader;
      private IShader _fragmentShader;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private IShaderProgram _shaderProgram;
      private VertexBuffer _vertexBuffer;

      public static FilterRenderer FromRenderer(Renderer renderer)
      {
        if (!FilterRenderer.RendererDictionary.ContainsKey(renderer))
          FilterRenderer.RendererDictionary.Add(renderer, new FilterRenderer(renderer));
        return FilterRenderer.RendererDictionary[renderer];
      }

      private FilterRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        string vertexOuput;
        string fragmentOutput;
        OrcaShader.Parse(File.ReadAllText("shaders/greyscale_filter.shader"), out vertexOuput, out fragmentOutput);
        this._vertexShader = this._graphicsContext.CreateShader(ShaderType.Vertex, vertexOuput);
        this._fragmentShader = this._graphicsContext.CreateShader(ShaderType.Fragment, fragmentOutput);
        this._shaderProgram = this._graphicsContext.CreateShaderProgram(this._vertexShader, this._fragmentShader);
        this._vertexBuffer = this._graphicsContext.CreateVertexBuffer(2, 2);
      }

      public void Dispose()
      {
        this._vertexBuffer.Dispose();
        this._vertexShader.Dispose();
        this._fragmentShader.Dispose();
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
        this._vertexBuffer.Begin();
        for (int index = 0; index < 4; ++index)
        {
          this._vertexBuffer.AddValue(0, this.vertexPositions[index]);
          this._vertexBuffer.AddValue(0, this.vertexUVs[index]);
        }
        this._vertexBuffer.End();
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = BlendMode.Alpha;
        this._graphicsContext.SetTexture(texture);
        this._shaderProgram.Activate();
        this._shaderProgram.SetUniform("ProjectionMatrix", orthographic);
        this._shaderProgram.SetUniform("InputTexture", 0);
        this._graphicsContext.BlendMode = BlendMode.Opaque;
        this._vertexBuffer.Render(PrimitiveType.Quads);
      }
    }
}
