// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.ObjectRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Graphics.LowLevel;
using SonicOrca.Graphics.V2.Animation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SonicOrca.Drawing.Renderers
{

    public class ObjectRenderer : IObjectRenderer, IRenderer, IDisposable
    {
      private static readonly Dictionary<Renderer, ObjectRenderer> RendererDictionary = new Dictionary<Renderer, ObjectRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private readonly IVertexArray _vaoShadow;
      private readonly List<ObjectRenderer.Vertex> _vertices = new List<ObjectRenderer.Vertex>();
      private readonly ManagedShaderProgram _shaderProgram;
      private readonly ManagedShaderProgram _shadowShaderProgram;
      private Matrix4 _projectionMatrix;
      private readonly List<ObjectRenderer.BatchOperation> _batchOperations = new List<ObjectRenderer.BatchOperation>();
      private int _batchedVertexIndex;
      private int _batchedVertexCount;
      private Vector2 _scale;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];

      public Matrix4 ModelMatrix { get; set; }

      public BlendMode BlendMode { get; set; }

      public Rectangle ClipRectangle { get; set; }

      public Colour MultiplyColour { get; set; }

      public Colour AdditiveColour { get; set; }

      public ITexture Texture { get; set; }

      public Vector2 Scale
      {
        get => this._scale;
        set => this._scale = value;
      }

      public bool EmitsLight { get; set; }

      public bool Shadow { get; set; }

      public int Filter { get; set; }

      public double FilterAmount { get; set; }

      public static ObjectRenderer FromRenderer(Renderer renderer)
      {
        if (!ObjectRenderer.RendererDictionary.ContainsKey(renderer))
          ObjectRenderer.RendererDictionary.Add(renderer, new ObjectRenderer(renderer));
        return ObjectRenderer.RendererDictionary[renderer];
      }

      private ObjectRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/object.shader");
        this._shadowShaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/object_shadow.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vaoShadow = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (ObjectRenderer.Vertex));
        this._vaoShadow.DefineAttributes(this._shadowShaderProgram.Program, this._vbo, typeof (ObjectRenderer.Vertex));
        this.ModelMatrix = Matrix4.Identity;
        this.BlendMode = BlendMode.Alpha;
        Vector2i clientSize = this._renderer.Window.ClientSize;
        double x = (double) clientSize.X;
        clientSize = this._renderer.Window.ClientSize;
        double y = (double) clientSize.Y;
        this.ClipRectangle = new Rectangle(0.0, 0.0, x, y);
        this.MultiplyColour = Colours.White;
        this.Scale = new Vector2(1.0, 1.0);
      }

      public void Dispose()
      {
        this._vao.Dispose();
        this._vaoShadow.Dispose();
        this._vbo.Dispose();
        this._shaderProgram.Dispose();
        this._shadowShaderProgram.Dispose();
      }

      public void Deactivate() => this.Render();

      private void Render()
      {
        if (this._batchedVertexCount == 0)
          return;
        this.PushBatchOperation();
        this._vbo.SetData<ObjectRenderer.Vertex>(this._vertices.ToArray(), 0, this._vertices.Count);
        this._projectionMatrix = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        foreach (ObjectRenderer.BatchOperation batchOperation in this._batchOperations)
          batchOperation.Render();
        this._batchOperations.Clear();
        this._batchedVertexIndex = 0;
        this._batchedVertexCount = 0;
      }

      private void PushBatchOperation()
      {
        if (this._batchedVertexIndex == this._batchedVertexCount)
          return;
        this._batchOperations.Add(new ObjectRenderer.BatchOperation(this, this._batchedVertexIndex, this._batchedVertexCount - this._batchedVertexIndex, this.BlendMode, this.ClipRectangle, this.ModelMatrix, this.Texture, this.MultiplyColour, this.AdditiveColour, this.Shadow, this.EmitsLight ? 0 : this.Filter, this.FilterAmount));
        this._batchedVertexIndex = this._batchedVertexCount;
      }

      private void PushVertices(
        IReadOnlyList<Vector2> positions,
        IReadOnlyList<Vector2> textureMappings)
      {
        this._renderer.ActivateRenderer((IRenderer) this);
        if (this._batchedVertexCount == 0)
        {
          this._batchedVertexIndex = 0;
          this._vertices.Clear();
        }
        for (int index = 0; index < ((IReadOnlyCollection<Vector2>) positions).Count; ++index)
        {
          List<ObjectRenderer.Vertex> vertices = this._vertices;
          ObjectRenderer.Vertex vertex1 = new ObjectRenderer.Vertex();
          ref ObjectRenderer.Vertex local1 = ref vertex1;
          vec2 vec2_1 = new vec2();
          ref vec2 local2 = ref vec2_1;
          Vector2 vector2 = positions[index];
          double x1 = vector2.X;
          local2.x = (float) x1;
          ref vec2 local3 = ref vec2_1;
          vector2 = positions[index];
          double y1 = vector2.Y;
          local3.y = (float) y1;
          vec2 vec2_2 = vec2_1;
          local1.position = vec2_2;
          ref ObjectRenderer.Vertex local4 = ref vertex1;
          vec2_1 = new vec2();
          ref vec2 local5 = ref vec2_1;
          vector2 = textureMappings[index];
          double x2 = vector2.X;
          local5.s = (float) x2;
          ref vec2 local6 = ref vec2_1;
          vector2 = textureMappings[index];
          double y2 = vector2.Y;
          local6.t = (float) y2;
          vec2 vec2_3 = vec2_1;
          local4.texcoords = vec2_3;
          ObjectRenderer.Vertex vertex2 = vertex1;
          vertices.Add(vertex2);
        }
        this._batchedVertexCount += ((IReadOnlyCollection<Vector2>) positions).Count;
        this.PushBatchOperation();
        if (this._batchedVertexCount <= 1024 /*0x0400*/)
          return;
        this.Render();
      }

      public IDisposable BeginMatixState()
      {
        return (IDisposable) new ObjectRenderer.MatrixState(this, this.ModelMatrix, this.ClipRectangle);
      }

      public void SetDefault()
      {
        this.BlendMode = BlendMode.Alpha;
        this.MultiplyColour = Colours.White;
        this.AdditiveColour = Colours.Black;
        this.Shadow = false;
        this.EmitsLight = false;
      }

      public void Render(AnimationInstance animationInstance, bool flipX = false, bool flipY = false)
      {
        SonicOrca.Graphics.Animation.Frame currentFrame = animationInstance.CurrentFrame;
        this.Texture = animationInstance.CurrentTexture;
        this.Render((Rectangle) currentFrame.Source, (Vector2) currentFrame.Offset, flipX, flipY);
      }

      public void Render(
        AnimationInstance animationInstance,
        Vector2 destination,
        bool flipX = false,
        bool flipY = false)
      {
        SonicOrca.Graphics.Animation.Frame currentFrame = animationInstance.CurrentFrame;
        this.Texture = animationInstance.CurrentTexture;
        this.Render((Rectangle) currentFrame.Source, destination + (Vector2) currentFrame.Offset, flipX, flipY);
      }

      public void Render(
        CompositionInstance compositionInstance,
        Vector2 destination,
        bool flipX = false,
        bool flipY = false)
      {
      }

      public void Render(Vector2 destination = default (Vector2), bool flipX = false, bool flipY = false)
      {
        this.Render(new Rectangle(0.0, 0.0, (double) this.Texture.Width, (double) this.Texture.Height), destination, flipX, flipY);
      }

      public void Render(Rectangle destination, bool flipX = false, bool flipY = false)
      {
        this.Render(new Rectangle(0.0, 0.0, (double) this.Texture.Width, (double) this.Texture.Height), destination, flipX, flipY);
      }

      public void Render(Rectangle source, Vector2 offset, bool flipX = false, bool flipY = false)
      {
        this.Render(source, new Rectangle((offset.X - (double) (int) (source.Width / 2.0)) * this.Scale.X, (offset.Y - (double) (int) (source.Height / 2.0)) * this.Scale.Y, source.Width * this.Scale.X, source.Height * this.Scale.Y), flipX, flipY);
      }

      public void Render(Rectangle source, Rectangle destination, bool flipX = false, bool flipY = false)
      {
        Renderer.GetVertices(destination, this.vertexPositions);
        if (Math.Abs(this.ModelMatrix.M11) != 1.0 || Math.Abs(this.ModelMatrix.M22) != 1.0)
          Renderer.GetTextureMappingsHalfIn(this.Texture, (Rectanglei) source, ref this.vertexUVs, flipX, flipY);
        else
          Renderer.GetTextureMappings(this.Texture, (Rectanglei) source, this.vertexUVs, flipX, flipY);
        this.PushVertices((IReadOnlyList<Vector2>) this.vertexPositions, (IReadOnlyList<Vector2>) this.vertexUVs);
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
      }

      private class BatchOperation
      {
        private readonly ObjectRenderer _objectRenderer;
        private readonly int _vertexBufferIndex;
        private readonly int _vertexCount;
        private readonly BlendMode _blendMode;
        private readonly Rectangle _clipRectangle;
        private readonly Matrix4 _modelMatrix;
        private readonly ITexture _texture;
        private readonly Colour _multiplyColour;
        private readonly Colour _additiveColour;
        private readonly bool _shadow;
        private readonly int _filter;
        private readonly double _filterAmount;
        private Vector2[] vertexPositions = new Vector2[4];

        public BatchOperation(
          ObjectRenderer objectRenderer,
          int vertexBufferIndex,
          int vertexCount,
          BlendMode blendMode,
          Rectangle clipRectangle,
          Matrix4 modelMatrix,
          ITexture texture,
          Colour multiplyColour,
          Colour additiveColour,
          bool shadow,
          int filter,
          double filterAmount)
        {
          this._objectRenderer = objectRenderer;
          this._vertexBufferIndex = vertexBufferIndex;
          this._vertexCount = vertexCount;
          this._blendMode = blendMode;
          this._clipRectangle = clipRectangle;
          this._modelMatrix = modelMatrix;
          this._texture = texture;
          this._multiplyColour = multiplyColour;
          this._additiveColour = additiveColour;
          this._shadow = shadow;
          this._filter = filter;
          this._filterAmount = filterAmount;
        }

        public void Render()
        {
          IGraphicsContext graphicsContext = this._objectRenderer._graphicsContext;
          IShaderProgram shaderProgram1 = this._shadow ? this._objectRenderer._shadowShaderProgram.Program : this._objectRenderer._shaderProgram.Program;
          graphicsContext.BlendMode = this._blendMode;
          graphicsContext.SetTexture(this._texture);
          shaderProgram1.Activate();
          if (ShadowRenderer.IsShadowing)
            shaderProgram1.SetUniform("AlphaGrayscale", 1f);
          else
            shaderProgram1.SetUniform("AlphaGrayscale", 0.0f);
          shaderProgram1.SetUniform("ModelViewMatrix", this._modelMatrix);
          shaderProgram1.SetUniform("ProjectionMatrix", this._objectRenderer._projectionMatrix);
          shaderProgram1.SetUniform("InputTexture", 0);
          IShaderProgram shaderProgram2 = shaderProgram1;
          double x = this._clipRectangle.X;
          double y = this._clipRectangle.Y;
          Rectangle clipRectangle = this._clipRectangle;
          double right = clipRectangle.Right;
          clipRectangle = this._clipRectangle;
          double bottom = clipRectangle.Bottom;
          Vector4 vector4 = new Vector4(x, y, right, bottom);
          shaderProgram2.SetUniform("InputClipRectangle", vector4);
          shaderProgram1.SetUniform("InputColourMultiply", this._multiplyColour);
          shaderProgram1.SetUniform("InputColourAdd", this._additiveColour);
          shaderProgram1.SetUniform("InputEnableMask", this._blendMode == BlendMode.Additive ? 0 : 1);
          shaderProgram1.SetUniform("InputFilter", this._filter);
          shaderProgram1.SetUniform("InputFilterAmount", this._filterAmount);
          (this._shadow ? this._objectRenderer._vaoShadow : this._objectRenderer._vao).Render(PrimitiveType.Quads, this._vertexBufferIndex, this._vertexCount);
        }
      }

      private class MatrixState : IDisposable
      {
        private readonly ObjectRenderer _objectRenderer;
        private readonly Matrix4 _matrix;
        private readonly Rectangle _clipRectangle;

        public MatrixState(ObjectRenderer objectRenderer, Matrix4 matrix, Rectangle clipRectangle)
        {
          this._objectRenderer = objectRenderer;
          this._matrix = matrix;
          this._clipRectangle = clipRectangle;
        }

        public void Dispose()
        {
          this._objectRenderer.ModelMatrix = this._matrix;
          this._objectRenderer.ClipRectangle = this._clipRectangle;
        }
      }
    }
}
