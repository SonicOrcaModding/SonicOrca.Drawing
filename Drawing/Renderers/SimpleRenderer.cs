// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.SimpleRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Extensions;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Graphics.LowLevel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SonicOrca.Drawing.Renderers
{

    public class SimpleRenderer : I2dRenderer, IDisposable, IRenderer
    {
      private static readonly Dictionary<Renderer, SimpleRenderer> RendererDictionary = new Dictionary<Renderer, SimpleRenderer>();
      private readonly Renderer _renderer;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IBuffer _vbo;
      private readonly IVertexArray _vao;
      private SimpleRenderer.Vertex[] _verticesData = new SimpleRenderer.Vertex[20];
      private int numVerticesData;
      private ManagedShaderProgram _shaderProgram;
      private Matrix4 _projectionMatrix;
      private Matrix4 _modelMatrix;
      private PrimitiveType _primitiveType = PrimitiveType.Quads;
      private ITexture[] _textures = new ITexture[0];
      private Vector2[] _vertices;
      private BlendMode _blendMode;
      private Rectangle _clipRectangle;
      private Colour _colour;
      private Colour _additiveColour;
      private SimpleRenderer.BatchOperation[] _batchOperations = new SimpleRenderer.BatchOperation[4];
      private int numBatchOperations;
      private int _batchedVertexIndex;
      private int _batchedVertexCount;
      private Vector2[] vertexPositions = new Vector2[4];
      private Vector2[] vertexUVs = new Vector2[4];

      public Matrix4 ModelMatrix
      {
        get => this._modelMatrix;
        set
        {
          if (!(this._modelMatrix != value))
            return;
          this.PushBatchOperation();
          this._modelMatrix = value;
        }
      }

      public BlendMode BlendMode
      {
        get => this._blendMode;
        set
        {
          if (this._blendMode == value)
            return;
          this.PushBatchOperation();
          this._blendMode = value;
        }
      }

      public Rectangle ClipRectangle
      {
        get => this._clipRectangle;
        set
        {
          if (!(this._clipRectangle != value))
            return;
          this.PushBatchOperation();
          this._clipRectangle = value;
        }
      }

      public Colour Colour
      {
        get => this._colour;
        set
        {
          if (!(this._colour != value))
            return;
          this.PushBatchOperation();
          this._colour = value;
        }
      }

      public Colour AdditiveColour
      {
        get => this._additiveColour;
        set
        {
          if (!(this._additiveColour != value))
            return;
          this.PushBatchOperation();
          this._additiveColour = value;
        }
      }

      public ITexture Texture
      {
        get
        {
          if (this._textures == null)
            return (ITexture) null;
          return this._textures.Length == 0 ? (ITexture) null : this._textures[0];
        }
        set
        {
          if (value == null)
          {
            if (this._textures.Length == 0)
              return;
            this.PushBatchOperation();
            this._textures = new ITexture[0];
          }
          else if (this._textures.Length == 1)
          {
            if (this._textures[0] != value)
              return;
            this.PushBatchOperation();
            this._textures[0] = value;
          }
          else
          {
            this.PushBatchOperation();
            this._textures = new ITexture[1]{ value };
          }
        }
      }

      public IEnumerable<ITexture> Textures
      {
        get => (IEnumerable<ITexture>) this._textures;
        set
        {
          if (value == null)
          {
            this.Texture = (ITexture) null;
          }
          else
          {
            ITexture[] array = value.ToArray<ITexture>();
            if (this._textures.Length != array.Length)
            {
              this.PushBatchOperation();
              this._textures = array;
            }
            else
            {
              bool flag = false;
              for (int index = 0; index < this._textures.Length; ++index)
              {
                if (this._textures[index] != array[index])
                {
                  flag = true;
                  break;
                }
              }
              if (!flag)
                return;
              this.PushBatchOperation();
              this._textures = array;
            }
          }
        }
      }

      public static SimpleRenderer FromRenderer(Renderer renderer)
      {
        if (!SimpleRenderer.RendererDictionary.ContainsKey(renderer))
          SimpleRenderer.RendererDictionary.Add(renderer, new SimpleRenderer(renderer));
        return SimpleRenderer.RendererDictionary[renderer];
      }

      private SimpleRenderer(Renderer renderer)
      {
        this._renderer = renderer;
        renderer.RegisterRenderer((IRenderer) this);
        this._graphicsContext = renderer.Window.GraphicsContext;
        this._shaderProgram = OrcaShader.CreateFromFile(this._graphicsContext, "shaders/simple.shader");
        this._vbo = this._graphicsContext.CreateBuffer();
        this._vao = this._graphicsContext.CreateVertexArray();
        this._vao.DefineAttributes(this._shaderProgram.Program, this._vbo, typeof (SimpleRenderer.Vertex));
        this._modelMatrix = Matrix4.Identity;
        this.BlendMode = BlendMode.Alpha;
        Vector2i clientSize = this._renderer.Window.ClientSize;
        double x = (double) clientSize.X;
        clientSize = this._renderer.Window.ClientSize;
        double y = (double) clientSize.Y;
        this.ClipRectangle = new Rectangle(0.0, 0.0, x, y);
        this.Colour = Colours.White;
      }

      public void Dispose()
      {
        this._vbo.Dispose();
        this._vao.Dispose();
        this._shaderProgram.Dispose();
      }

      public void Deactivate() => this.Render();

      private void Render()
      {
        if (this._batchedVertexCount == 0)
          return;
        this.PushBatchOperation();
        this._vbo.SetData<SimpleRenderer.Vertex>(this._verticesData, 0, this.numVerticesData);
        this._projectionMatrix = this._renderer.Window.GraphicsContext.CurrentFramebuffer.CreateOrthographic();
        for (int index = 0; index < this.numBatchOperations; ++index)
          this._batchOperations[index].Render();
        this.numBatchOperations = 0;
        this._batchedVertexIndex = 0;
        this._batchedVertexCount = 0;
      }

      private void PushBatchOperation()
      {
        if (this._batchedVertexIndex == this._batchedVertexCount)
          return;
        if (this.numBatchOperations >= this._batchOperations.Length)
        {
          SimpleRenderer.BatchOperation[] batchOperationArray = new SimpleRenderer.BatchOperation[this._batchOperations.Length * 2];
          for (int index = 0; index < this._batchOperations.Length; ++index)
            batchOperationArray[index] = this._batchOperations[index];
          this._batchOperations = batchOperationArray;
        }
        if (this._batchOperations[this.numBatchOperations] == null)
        {
          this._batchOperations[this.numBatchOperations] = new SimpleRenderer.BatchOperation(this, this._primitiveType, this._batchedVertexIndex, this._batchedVertexCount - this._batchedVertexIndex, this._blendMode, this._clipRectangle, (IEnumerable<ITexture>) this._textures, this._modelMatrix, this._additiveColour);
        }
        else
        {
          this._batchOperations[this.numBatchOperations]._simpleRenderer = this;
          this._batchOperations[this.numBatchOperations]._primitiveType = this._primitiveType;
          this._batchOperations[this.numBatchOperations]._vertexBufferIndex = this._batchedVertexIndex;
          this._batchOperations[this.numBatchOperations]._vertexCount = this._batchedVertexCount - this._batchedVertexIndex;
          this._batchOperations[this.numBatchOperations]._blendMode = this._blendMode;
          this._batchOperations[this.numBatchOperations]._clipRectangle = this._clipRectangle;
          this._batchOperations[this.numBatchOperations]._textures = (IReadOnlyList<ITexture>) ((IEnumerable<ITexture>) this._textures).ToArray<ITexture>();
          this._batchOperations[this.numBatchOperations]._modelMatrix = this._modelMatrix;
          this._batchOperations[this.numBatchOperations]._additiveColour = this._additiveColour;
        }
        ++this.numBatchOperations;
        this._batchedVertexIndex = this._batchedVertexCount;
      }

      private void PushVertices(IReadOnlyList<Vector2> positions)
      {
        Colour[] colours = new Colour[((IReadOnlyCollection<Vector2>) positions).Count];
        Vector2[] textureMappings = new Vector2[((IReadOnlyCollection<Vector2>) positions).Count];
        for (int index = 0; index < ((IReadOnlyCollection<Vector2>) positions).Count; ++index)
          colours[index] = this.Colour;
        this.PushVertices(positions, (IReadOnlyList<Colour>) colours, (IReadOnlyList<Vector2>) textureMappings);
      }

      public void PushVertices(
        IReadOnlyList<Vector2> positions,
        IReadOnlyList<Colour> colours,
        IReadOnlyList<Vector2> textureMappings)
      {
        this._renderer.ActivateRenderer((IRenderer) this);
        if (this._batchedVertexCount == 0)
        {
          this._batchedVertexIndex = 0;
          this.numVerticesData = 0;
        }
        for (int index = 0; index < ((IReadOnlyCollection<Vector2>) positions).Count; ++index)
        {
          this._verticesData[this.numVerticesData].position.x = (float) positions[index].X;
          this._verticesData[this.numVerticesData].position.y = (float) positions[index].Y;
          this._verticesData[this.numVerticesData].colour.r = (float) colours[index].Red / (float) byte.MaxValue;
          this._verticesData[this.numVerticesData].colour.g = (float) colours[index].Green / (float) byte.MaxValue;
          this._verticesData[this.numVerticesData].colour.b = (float) colours[index].Blue / (float) byte.MaxValue;
          this._verticesData[this.numVerticesData].colour.a = (float) colours[index].Alpha / (float) byte.MaxValue;
          this._verticesData[this.numVerticesData].texcoords.s = (float) textureMappings[index].X;
          this._verticesData[this.numVerticesData].texcoords.t = (float) textureMappings[index].Y;
          ++this.numVerticesData;
          if (this.numVerticesData >= this._verticesData.Length)
          {
            SimpleRenderer.Vertex[] destinationArray = new SimpleRenderer.Vertex[this._verticesData.Length * 2];
            Array.Copy((Array) this._verticesData, (Array) destinationArray, this._verticesData.Length);
            this._verticesData = destinationArray;
          }
        }
        this._batchedVertexCount += ((IReadOnlyCollection<Vector2>) positions).Count;
        if (this._batchedVertexCount <= 1024 /*0x0400*/)
          return;
        this.Render();
      }

      public IDisposable BeginMatixState()
      {
        return (IDisposable) new SimpleRenderer.MatrixState(this, this._modelMatrix, this._clipRectangle);
      }

      public void SetVertices(Rectangle rectangle)
      {
        this._vertices = new Vector2[4]
        {
          new Vector2(rectangle.Left, rectangle.Top),
          new Vector2(rectangle.Left, rectangle.Bottom),
          new Vector2(rectangle.Right, rectangle.Bottom),
          new Vector2(rectangle.Right, rectangle.Top)
        };
      }

      public void SetVerticesFromTextureSize(Vector2 position)
      {
        ITexture texture = this.Texture;
        if (texture == null)
          throw new InvalidOperationException("Texture not set");
        this.SetVertices(new Rectangle(position.X, position.Y, (double) texture.Width, (double) texture.Height));
      }

      public void RenderTexture(ITexture texture, Vector2 destination, bool flipX = false, bool flipY = false)
      {
        this.RenderTexture(texture, new Rectangle(0.0, 0.0, (double) texture.Width, (double) texture.Height), destination, flipX, flipY);
      }

      public void RenderTexture(ITexture texture, Rectangle destination, bool flipX = false, bool flipY = false)
      {
        this.RenderTexture(texture, new Rectangle(0.0, 0.0, (double) texture.Width, (double) texture.Height), destination, flipX, flipY);
      }

      public void RenderTexture(
        ITexture texture,
        Rectangle source,
        Vector2 destination,
        bool flipX = false,
        bool flipY = false)
      {
        this.RenderTexture(texture, source, new Rectangle(destination.X - (double) (int) ((double) texture.Width / 2.0), destination.Y - (double) (int) ((double) texture.Height / 2.0), (double) texture.Width, (double) texture.Height), flipX, flipY);
      }

      public void RenderTexture(IEnumerable<ITexture> textures, Vector2 destination)
      {
        ITexture texture = textures.First<ITexture>();
        this.RenderTexture(textures, new Rectangle(0.0, 0.0, (double) texture.Width, (double) texture.Height), destination);
      }

      public void RenderTexture(IEnumerable<ITexture> textures, Rectangle destination)
      {
        ITexture texture = textures.First<ITexture>();
        this.RenderTexture(textures, new Rectangle(0.0, 0.0, (double) texture.Width, (double) texture.Height), destination, false, false);
      }

      public void RenderTexture(IEnumerable<ITexture> textures, Rectangle source, Vector2 destination)
      {
        ITexture texture = textures.First<ITexture>();
        this.RenderTexture(textures, source, new Rectangle(destination.X - (double) texture.Width / 2.0, destination.Y - (double) texture.Height / 2.0, (double) texture.Width, (double) texture.Height), false, false);
      }

      public void RenderTexture(
        ITexture texture,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        this.RenderTexture((IEnumerable<ITexture>) new ITexture[1]
        {
          texture
        }, source, destination, flipX, flipY);
      }

      public void RenderTexture(
        IEnumerable<ITexture> textures,
        Rectangle source,
        Rectangle destination,
        bool flipX = false,
        bool flipY = false)
      {
        if (textures.Count<ITexture>() == 0)
          throw new ArgumentException("No textures specified", nameof (textures));
        Renderer.GetVertices(destination, this.vertexPositions);
        ITexture texture = textures.First<ITexture>();
        if (this._modelMatrix.M11 != 1.0 || this._modelMatrix.M22 != 1.0)
          Renderer.GetTextureMappingsHalfIn(texture, (Rectanglei) source, ref this.vertexUVs, flipX, flipY);
        else
          Renderer.GetTextureMappings(texture, (Rectanglei) source, this.vertexUVs, flipX, flipY);
        this.Textures = textures;
        this.PushVertices((IReadOnlyList<Vector2>) this.vertexPositions, (IReadOnlyList<Colour>) new Colour[4]
        {
          this.Colour,
          this.Colour,
          this.Colour,
          this.Colour
        }, (IReadOnlyList<Vector2>) this.vertexUVs);
      }

      public void RenderQuad(Colour colour, Rectangle rectangle)
      {
        this.Colour = colour;
        this.Texture = (ITexture) null;
        this.PushVertices((IReadOnlyList<Vector2>) new Vector2[4]
        {
          new Vector2(rectangle.Left, rectangle.Top),
          new Vector2(rectangle.Left, rectangle.Bottom),
          new Vector2(rectangle.Right, rectangle.Bottom),
          new Vector2(rectangle.Right, rectangle.Top)
        });
      }

      public void RenderQuad(Colour colour, Vector2[] points)
      {
        this.Colour = colour;
        this.Texture = (ITexture) null;
        this.PushVertices((IReadOnlyList<Vector2>) points);
      }

      public void RenderEllipse(
        Colour colour,
        Vector2 centre,
        double innerRadius,
        double outerRadius,
        int sectors)
      {
        this.Colour = colour;
        this.Texture = (ITexture) null;
        Vector2[] positions = new Vector2[sectors * 4];
        int num1 = 0;
        for (int index1 = 0; index1 < sectors; ++index1)
        {
          double num2 = (double) index1 * (2.0 * Math.PI) / (double) sectors;
          double num3 = (double) (index1 + 1) * (2.0 * Math.PI) / (double) sectors;
          Vector2[] vector2Array1 = positions;
          int index2 = num1;
          int num4 = index2 + 1;
          Vector2 vector2_1 = new Vector2(Math.Sin(num2) * outerRadius, -Math.Cos(num2) * outerRadius) + centre;
          vector2Array1[index2] = vector2_1;
          Vector2[] vector2Array2 = positions;
          int index3 = num4;
          int num5 = index3 + 1;
          Vector2 vector2_2 = new Vector2(Math.Sin(num2) * innerRadius, -Math.Cos(num2) * innerRadius) + centre;
          vector2Array2[index3] = vector2_2;
          Vector2[] vector2Array3 = positions;
          int index4 = num5;
          int num6 = index4 + 1;
          Vector2 vector2_3 = new Vector2(Math.Sin(num3) * innerRadius, -Math.Cos(num3) * innerRadius) + centre;
          vector2Array3[index4] = vector2_3;
          Vector2[] vector2Array4 = positions;
          int index5 = num6;
          num1 = index5 + 1;
          Vector2 vector2_4 = new Vector2(Math.Sin(num3) * outerRadius, -Math.Cos(num3) * outerRadius) + centre;
          vector2Array4[index5] = vector2_4;
        }
        this.PushVertices((IReadOnlyList<Vector2>) positions);
      }

      public void RenderRectangle(Colour colour, Rectangle rect, double thickness)
      {
        double width = Math.Min(thickness, rect.Width);
        double height = Math.Min(thickness, rect.Height);
        this.RenderQuad(colour, new Rectangle(rect.X, rect.Y, width, rect.Height));
        this.RenderQuad(colour, new Rectangle(rect.Right - width, rect.Y, width, rect.Height));
        this.RenderQuad(colour, new Rectangle(rect.X + width, rect.Y, rect.Width - width * 2.0, height));
        this.RenderQuad(colour, new Rectangle(rect.X + width, rect.Bottom - height, rect.Width - width * 2.0, height));
      }

      public void RenderLine(Colour colour, Vector2 a, Vector2 b, double thickness)
      {
        this.Colour = colour;
        this.Texture = (ITexture) null;
        a += new Vector2(0.5, 0.5);
        b += new Vector2(0.5, 0.5);
        thickness /= 2.0;
        double num1 = Math.Atan2(a.Y - b.Y, a.X - b.X) + Math.PI / 2.0;
        double num2 = Math.Atan2(a.Y - b.Y, a.X - b.X) - Math.PI / 2.0;
        this.PushVertices((IReadOnlyList<Vector2>) new Vector2[4]
        {
          a + new Vector2(Math.Cos(num1) * thickness, Math.Sin(num1) * thickness),
          a + new Vector2(Math.Cos(num2) * thickness, Math.Sin(num2) * thickness),
          b + new Vector2(Math.Cos(num1) * thickness, Math.Sin(num1) * thickness),
          b + new Vector2(Math.Cos(num2) * thickness, Math.Sin(num2) * thickness)
        });
      }

      public Rectangle RenderText(TextRenderInfo textRenderInfo)
      {
        return TextRenderingHelpers.RenderWith2d((I2dRenderer) this, textRenderInfo);
      }

      public Rectangle MeasureText(TextRenderInfo textRenderInfo)
      {
        return TextRenderingHelpers.MeasureWith2d((I2dRenderer) this, textRenderInfo);
      }

      [StructLayout(LayoutKind.Sequential, Pack = 1)]
      private struct Vertex
      {
        [VertexAttribute("VertexPosition")]
        public vec2 position;
        [VertexAttribute("VertexColour")]
        public vec4 colour;
        [VertexAttribute("VertexTextureMapping")]
        public vec2 texcoords;
      }

      private class BatchOperation
      {
        public SimpleRenderer _simpleRenderer;
        public PrimitiveType _primitiveType;
        public int _vertexBufferIndex;
        public int _vertexCount;
        public BlendMode _blendMode;
        public Rectangle _clipRectangle;
        public IReadOnlyList<ITexture> _textures;
        public Matrix4 _modelMatrix;
        public Colour _additiveColour;
        private Vector2[] vertexPositions = new Vector2[4];

        public BatchOperation(
          SimpleRenderer simpleRenderer,
          PrimitiveType primitiveType,
          int vertexBufferIndex,
          int vertexCount,
          BlendMode blendMode,
          Rectangle clipRectangle,
          IEnumerable<ITexture> textures,
          Matrix4 modelMatrix,
          Colour additiveColour)
        {
          this._simpleRenderer = simpleRenderer;
          this._primitiveType = primitiveType;
          this._vertexBufferIndex = vertexBufferIndex;
          this._vertexCount = vertexCount;
          this._blendMode = blendMode;
          this._clipRectangle = clipRectangle;
          this._textures = (IReadOnlyList<ITexture>) textures.ToArray<ITexture>();
          this._modelMatrix = modelMatrix;
          this._additiveColour = additiveColour;
        }

        public void Render()
        {
          IGraphicsContext graphicsContext = this._simpleRenderer._graphicsContext;
          IShaderProgram program = this._simpleRenderer._shaderProgram.Program;
          graphicsContext.BlendMode = this._blendMode;
          graphicsContext.SetTextures((IEnumerable<ITexture>) this._textures);
          program.Activate();
          program.SetUniform("ModelViewMatrix", this._modelMatrix);
          program.SetUniform("ProjectionMatrix", this._simpleRenderer._projectionMatrix);
          program.SetUniform("InputTextureCount", ((IReadOnlyCollection<ITexture>) this._textures).Count);
          for (int index = 0; index < ((IReadOnlyCollection<ITexture>) this._textures).Count; ++index)
            program.SetUniform($"InputTexture[{(object) index}]", index);
          program.SetUniform("InputClipRectangle", new Vector4(this._clipRectangle.X, this._clipRectangle.Y, this._clipRectangle.Right, this._clipRectangle.Bottom));
          program.SetUniform("InputAdditiveColour", this._additiveColour);
          this._simpleRenderer._vao.Render(this._primitiveType, this._vertexBufferIndex, this._vertexCount);
        }
      }

      private class MatrixState : IDisposable
      {
        private readonly SimpleRenderer _simpleRenderer;
        private readonly Matrix4 _matrix;
        private readonly Rectangle _clipRectangle;

        public MatrixState(SimpleRenderer simpleRenderer, Matrix4 matrix, Rectangle clipRectangle)
        {
          this._simpleRenderer = simpleRenderer;
          this._matrix = matrix;
          this._clipRectangle = clipRectangle;
        }

        public void Dispose()
        {
          this._simpleRenderer.ModelMatrix = this._matrix;
          this._simpleRenderer.ClipRectangle = this._clipRectangle;
        }
      }
    }
}
