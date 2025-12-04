// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.ShadowRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace SonicOrca.Drawing.Renderers
{

    internal class ShadowRenderer : IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, ShadowRenderer> RendererDictionary = new Dictionary<Renderer, ShadowRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      public static bool IsShadowing = false;
      private ManagedShaderProgram _shaderProgram;
      private VertexBuffer _vertexBuffer;

      public static ShadowRenderer FromRenderer(Renderer renderer)
      {
        if (!ShadowRenderer.RendererDictionary.ContainsKey(renderer))
          ShadowRenderer.RendererDictionary.Add(renderer, new ShadowRenderer(renderer));
        return ShadowRenderer.RendererDictionary[renderer];
      }

      private ShadowRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/shadow.shader");
        this._vertexBuffer = this._graphicsContext.CreateVertexBuffer(2, 2);
      }

      public void Dispose()
      {
        this._vertexBuffer.Dispose();
        this._shaderProgram.Dispose();
      }

      public void Deactivate()
      {
      }
    }
}
