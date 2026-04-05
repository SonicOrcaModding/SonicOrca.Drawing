// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.TileRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

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

    public class TileRenderer : ITileRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, TileRenderer> RendererDictionary = new Dictionary<Renderer, TileRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private TileRenderer.Vertex[] _vertices = new TileRenderer.Vertex[20];
      public int numVertices;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];
      private ManagedShaderProgram _shaderProgram;
      public static Colour ShadowColour;
      private TileBlendMode _tempLastBlendMode;
      private Matrix4 _cachedModelMatrix;
      private Matrix4 _cachedProjectionMatrix;
      private Colour _cachedInputColour;
      private Vector4 _cachedClipRectangle;
      private Colour _cachedShadowColour;
      private int _cachedFilter;
      private float _cachedFilterAmount;
      private float _cachedAlphaGrayscale = -1f;
      private bool _tileUniformStateValid;

      public Matrix4 ModelMatrix { get; set; }

      public ITexture[] Textures { get; set; }

      public Colour Colour { get; set; }

      public Rectangle ClipRectangle { get; set; }

      public int Filter { get; set; }

      public double FilterAmount { get; set; }

      public bool Rendering { get; private set; }

      public int NumTiles { get; private set; }

      public static TileRenderer FromRenderer(Renderer renderer)
      {
        if (!TileRenderer.RendererDictionary.ContainsKey(renderer))
          TileRenderer.RendererDictionary.Add(renderer, new TileRenderer(renderer));
        return TileRenderer.RendererDictionary[renderer];
      }

      private TileRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/tile.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (TileRenderer.Vertex));
        this.ModelMatrix = Matrix4.Identity;
        this.Colour = Colours.White;
        this.ClipRectangle = new Rectangle(0.0, 0.0, 1920.0, 1080.0);
        this._shaderProgram.Program.Activate();
        for (int index = 0; index < 4; ++index)
          this._shaderProgram.Program.SetUniform($"InputTexture[{(object) index}]", index);
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

      public void BeginRender()
      {
        this.Rendering = !this.Rendering ? true : throw new InvalidOperationException("Already renderering.");
        this.NumTiles = 0;
        this.numVertices = 0;
      }

      public void AddTile(
        Rectanglei source,
        Rectanglei destination,
        int textureIndex,
        bool flipX = false,
        bool flipY = false,
        float opacity = 1f,
        TileBlendMode blend = TileBlendMode.Alpha)
      {
        if (!this.Rendering)
          throw new Exception("Not currently renderering");
        ITexture texture = this.Textures[textureIndex];
        Renderer.GetVertices((Rectangle) destination, this.vertexPositions);
        Rectanglei source1 = source;
        Vector2[] vertexUvs = this.vertexUVs;
        int num1 = flipX ? 1 : 0;
        int num2 = flipY ? 1 : 0;
        Renderer.GetTextureMappings(texture, source1, vertexUvs, num1 != 0, num2 != 0);
        for (int index = 0; index < 4; ++index)
        {
          this._vertices[this.numVertices].position.x = (float) this.vertexPositions[index].X;
          this._vertices[this.numVertices].position.y = (float) this.vertexPositions[index].Y;
          this._vertices[this.numVertices].texcoords.s = (float) this.vertexUVs[index].X;
          this._vertices[this.numVertices].texcoords.t = (float) this.vertexUVs[index].Y;
          this._vertices[this.numVertices].texindex = (float) textureIndex;
          this._vertices[this.numVertices].opacity = opacity;
          ++this.numVertices;
          if (this.numVertices >= this._vertices.Length)
          {
            TileRenderer.Vertex[] destinationArray = new TileRenderer.Vertex[this._vertices.Length * 2];
            Array.Copy((Array) this._vertices, (Array) destinationArray, this._vertices.Length);
            this._vertices = destinationArray;
          }
        }
        ++this.NumTiles;
        this._tempLastBlendMode = blend;
      }

      public void EndRender()
      {
        if (!this.Rendering)
          throw new Exception("Not currently renderering");
        this._vbo.SetData<TileRenderer.Vertex>(this._vertices, 0, this.numVertices);
        this._renderer.ActivateRenderer((IRenderer) this);
        this._graphicsContext.BlendMode = this._tempLastBlendMode == TileBlendMode.Additive ? BlendMode.Additive : BlendMode.Alpha;
        foreach (ITexture texture in this.Textures)
          texture.Filtering = TextureFiltering.NearestNeighbour;
        this._graphicsContext.SetTextures((IEnumerable<ITexture>) this.Textures);
        Matrix4 orthographic = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographicCached();
        IShaderProgram program = this._shaderProgram.Program;
        program.Activate();
        float alphaGrayscale = ShadowRenderer.IsShadowing ? 1f : 0f;
        Rectangle clipRectangle = this.ClipRectangle;
        Vector4 clipVec = new Vector4(clipRectangle.X, clipRectangle.Y, clipRectangle.Right, clipRectangle.Bottom);
        if (!this._tileUniformStateValid
            || this.ModelMatrix != this._cachedModelMatrix
            || orthographic != this._cachedProjectionMatrix
            || this.Colour != this._cachedInputColour
            || clipVec != this._cachedClipRectangle
            || TileRenderer.ShadowColour != this._cachedShadowColour
            || this.Filter != this._cachedFilter
            || Math.Abs(this.FilterAmount - (double) this._cachedFilterAmount) > 1E-9
            || Math.Abs(alphaGrayscale - this._cachedAlphaGrayscale) > 1E-9)
        {
          program.SetUniform("AlphaGrayscale", alphaGrayscale);
          program.SetUniform("ModelViewMatrix", this.ModelMatrix);
          program.SetUniform("ProjectionMatrix", orthographic);
          program.SetUniform("InputColour", this.Colour);
          program.SetUniform("InputClipRectangle", clipVec);
          program.SetUniform("ShadowColour", TileRenderer.ShadowColour);
          program.SetUniform("InputFilter", this.Filter);
          program.SetUniform("InputFilterAmount", this.FilterAmount);
          this._cachedModelMatrix = this.ModelMatrix;
          this._cachedProjectionMatrix = orthographic;
          this._cachedInputColour = this.Colour;
          this._cachedClipRectangle = clipVec;
          this._cachedShadowColour = TileRenderer.ShadowColour;
          this._cachedFilter = this.Filter;
          this._cachedFilterAmount = (float) this.FilterAmount;
          this._cachedAlphaGrayscale = alphaGrayscale;
          this._tileUniformStateValid = true;
        }
        this._vao.Render(PrimitiveType.Quads, 0, this.numVertices);
        this.Rendering = false;
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
        [VertexAttribute("VertexTextureIndex")]
        public float texindex;
        [VertexAttribute("VertexOpacity")]
        public float opacity;
      }
    }
}
