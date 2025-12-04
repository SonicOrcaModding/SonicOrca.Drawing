// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.LevelRendering.LevelRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using OpenTK.Graphics.OpenGL;
using SonicOrca.Core;
using SonicOrca.Core.Collision;
using SonicOrca.Drawing.Renderers;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using SonicOrca.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SonicOrca.Drawing.LevelRendering
{

    public class LevelRenderer : ILevelRenderer, IDisposable
    {
      private readonly SonicOrcaGameContext _gameContext;
      private readonly IGraphicsContext _graphicsContext;
      private readonly IVideoSettings _videoSettings;
      private readonly Level _level;
      private TileSet _tileSet;
      private ParticleManager _particleManager;
      private ResourceSession _resourceSession;
      private IFramebuffer _canvasFramebuffer;
      private IFramebuffer _effectFramebuffer;
      private RenderingLayer[] _renderingLayers;
      private RenderingTileList _renderingTileList;
      private RenderingTiler _tiler;
      private int _numLayers;
      private Rectanglei _previousCanvasViewport;
      private Rectanglei _screenBounds = new Rectanglei(0, 0, 1920, 1080);
      private Viewport _canvasViewport;
      private const string WaterfallDistortionTextureResourceKey = "SONICORCA/PARTICLE/WATERFALL";
      private ITexture _waterfallDistortionTexture;
      private readonly List<Rectanglei> _waterfallRegions = new List<Rectanglei>(16 /*0x10*/);

      public string[] LastDebugLog { get; private set; }

      public LevelRenderer(Level level, IVideoSettings videoSettings)
      {
        this._gameContext = level.GameContext;
        this._level = level;
        this._graphicsContext = this._gameContext.Window.GraphicsContext;
        this._videoSettings = videoSettings;
      }

      public void Dispose()
      {
        this.DisposeCanvas();
        this._resourceSession?.Dispose();
      }

      public async Task LoadAsync(CancellationToken ct = default (CancellationToken))
      {
        this._resourceSession = new ResourceSession(this._gameContext.ResourceTree);
        this._resourceSession.PushDependency("SONICORCA/PARTICLE/WATERFALL");
        await this._resourceSession.LoadAsync();
        this._waterfallDistortionTexture = this._gameContext.ResourceTree.GetLoadedResource<ITexture>("SONICORCA/PARTICLE/WATERFALL");
      }

      public void Initialise()
      {
        this._tileSet = this._level.TileSet;
        this._particleManager = this._level.ParticleManager;
        this._numLayers = this._level.Map.Layers.Count;
        this._renderingLayers = new RenderingLayer[this._numLayers];
        for (int layerIndex = 0; layerIndex < this._numLayers; ++layerIndex)
        {
          RenderingLayer renderingLayer = new RenderingLayer(this._level, layerIndex, this._videoSettings);
          renderingLayer.UpdateClippingMarkers();
          this._renderingLayers[layerIndex] = renderingLayer;
          renderingLayer.Layer.Index = layerIndex;
        }
        this._renderingTileList = new RenderingTileList();
        this._tiler = new RenderingTiler(this._level, this._renderingTileList);
        this.InitialiseShadowRendering();
        this._canvasViewport = new Viewport(this._screenBounds);
        this._canvasViewport.Bounds = this._screenBounds;
        this.InitialiseCanvas(this._canvasViewport.Destination.Width, this._canvasViewport.Destination.Height);
      }

      public void Render(Renderer renderer, Viewport screenViewport, LayerViewOptions layerViewOptions)
      {
        renderer.DeativateRenderer();
        IFramebuffer currentFramebuffer = this._graphicsContext.CurrentFramebuffer;
        Rectanglei rectanglei;
        if (this._screenBounds != screenViewport.Destination)
        {
          this._screenBounds = screenViewport.Destination;
          this._canvasViewport = new Viewport(this._screenBounds);
          int width = this._canvasViewport.Destination.Width;
          rectanglei = this._canvasViewport.Destination;
          int height = rectanglei.Height;
          this.InitialiseCanvas(width, height);
        }
        this._canvasViewport.Bounds = screenViewport.Bounds;
        rectanglei = this._canvasViewport.Bounds;
        if (rectanglei.X == this._previousCanvasViewport.X)
        {
          rectanglei = this._canvasViewport.Bounds;
          if (rectanglei.Y == this._previousCanvasViewport.Y)
            goto label_5;
        }
        this._previousCanvasViewport = this._canvasViewport.Bounds;
    label_5:
        this._canvasFramebuffer.Activate();
        this._graphicsContext.ClearBuffer();
        this.PrepareLayers(this._canvasViewport, layerViewOptions);
        this.PrepareObjects();
        this.DrawLayers(renderer, this._canvasViewport, layerViewOptions);
        if (layerViewOptions.ShowLandscapeCollision)
          this.DrawLandscapeCollision(renderer, layerViewOptions);
        this._level.WaterManager.Draw(renderer, this._canvasViewport);
        renderer.DeativateRenderer();
        RenderingHelpers.RenderToFramebuffer(renderer, this._canvasFramebuffer.Textures[0], currentFramebuffer, (SonicOrca.Geometry.Rectangle) this._canvasViewport.Destination, BlendMode.Opaque);
        renderer.DeativateRenderer();
      }

      private void InitialiseCanvas(int width, int height)
      {
        if (this._canvasFramebuffer != null && this._canvasFramebuffer.Width == width && this._canvasFramebuffer.Height == height)
          return;
        this.DisposeCanvas();
        this._canvasFramebuffer = this._graphicsContext.CreateFrameBuffer(width, height);
        this._effectFramebuffer = this._graphicsContext.CreateFrameBuffer(width, height);
      }

      private void DisposeCanvas()
      {
        this._canvasFramebuffer?.Dispose();
        this._effectFramebuffer?.Dispose();
      }

      private void PrepareLayers(Viewport canvasViewport, LayerViewOptions layerViewOptions)
      {
        this._renderingTileList.Clear();
        for (int index = 0; index < this._numLayers; ++index)
        {
          RenderingLayer renderingLayer = this._renderingLayers[index];
          renderingLayer.Clear();
          if (renderingLayer.Layer.Visible)
            this._tiler.PrepareTiles(renderingLayer, canvasViewport, layerViewOptions);
        }
      }

      private void PrepareObjects()
      {
        List<ActiveObject> list = this._level.ObjectManager.ActiveObjects.ToList<ActiveObject>();
        list.Sort((Comparison<ActiveObject>) ((a, b) => a.Priority - b.Priority));
        for (int index = 0; index < this._numLayers; ++index)
          this._renderingLayers[index].Clear();
        foreach (ActiveObject activeObject in list)
        {
          RenderingLayer renderingLayer = this._renderingLayers[activeObject.Layer.Index];
          int priority = activeObject.Priority;
          if (priority < 0)
            renderingLayer.AddLowObject(activeObject);
          else if (priority > 0)
            renderingLayer.AddHighObject(activeObject);
        }
      }

      private void DrawLayers(Renderer renderer, Viewport viewport, LayerViewOptions viewOptions)
      {
        I2dRenderer g = renderer.Get2dRenderer();
        Matrix4 modelMatrix = g.ModelMatrix;
        g.ModelMatrix = Matrix4.Identity;
        ITileRenderer tileRenderer = renderer.GetTileRenderer();
        tileRenderer.ClipRectangle = (SonicOrca.Geometry.Rectangle) viewport.Destination;
        tileRenderer.ModelMatrix = Matrix4.Identity;
        tileRenderer.Textures = ((IEnumerable<ITexture>) this._tileSet.Textures).ToArray<ITexture>();
        tileRenderer.Filter = viewOptions.Filter;
        tileRenderer.FilterAmount = viewOptions.FilterAmount;
        Stopwatch.StartNew();
        RenderingTiler tiler = this._tiler;
        foreach (RenderingLayer renderingLayer in this._renderingLayers)
        {
          if (renderingLayer.Layer.Visible)
          {
            this.BeginShadowStenciling(renderingLayer.HasShadows);
            this.SetLighting(renderingLayer.Layer, viewOptions);
            this.DrawBackObjects(renderer, viewport, viewOptions, renderingLayer);
            tiler.RenderTiles(g, tileRenderer, renderingLayer.RenderTileIndex, renderingLayer.RenderTileCount);
            this.DrawFrontObjects(renderer, viewport, viewOptions, renderingLayer);
            if (renderingLayer.HasShadows)
              this.DrawShadows(renderer, g, tileRenderer, viewport, viewOptions, renderingLayer);
            if (this._videoSettings.EnableWaterEffects)
              this.RenderWaterfallEffects(renderer, viewport, renderingLayer);
            this.RenderParticles(renderer, viewport, renderingLayer);
          }
        }
        g.ModelMatrix = modelMatrix;
      }

      private void DrawLandscapeCollision(Renderer renderer, LayerViewOptions layerViewOptions)
      {
        foreach (CollisionVector collisionVector in this._level.CollisionTable.InternalTree.Query(this._canvasViewport.Bounds))
          collisionVector.Draw(renderer, this._canvasViewport);
      }

      private void SetLighting(LevelLayer layer, LayerViewOptions viewOptions)
      {
        viewOptions.Filter = 0;
        viewOptions.FilterAmount = 0.0;
        switch (layer.Lighting.Type)
        {
          case LevelLayerLightingType.Outside:
            if (this._level.NightMode == 0.0)
              break;
            viewOptions.Filter = 1;
            viewOptions.FilterAmount = this._level.NightMode;
            break;
          case LevelLayerLightingType.Inside:
            viewOptions.Filter = 2;
            viewOptions.FilterAmount = 1.0 - layer.Lighting.Light;
            break;
        }
      }

      private void InitialiseShadowRendering()
      {
        GL.StencilMask((int) byte.MaxValue);
        GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
      }

      private void BeginShadowStenciling(bool shadowsEnabled)
      {
        if (shadowsEnabled)
        {
          GL.Enable(EnableCap.StencilTest);
          GL.Clear(ClearBufferMask.StencilBufferBit);
          GL.StencilFunc(StencilFunction.Always, 1, (int) byte.MaxValue);
          GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
        }
        else
          GL.Disable(EnableCap.StencilTest);
      }

      private void DrawShadows(
        Renderer renderer,
        I2dRenderer g,
        ITileRenderer tr,
        Viewport viewport,
        LayerViewOptions viewOptions,
        RenderingLayer rlayer)
      {
        GL.StencilFunc(StencilFunction.Equal, 1, (int) byte.MaxValue);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Zero);
        viewOptions.Shadows = true;
        ShadowRenderer.IsShadowing = true;
        Matrix4 modelMatrix = tr.ModelMatrix;
        RenderingTiler tiler = this._tiler;
        foreach (LevelLayerShadow shadow in (IEnumerable<LevelLayerShadow>) rlayer.Layer.Shadows)
        {
          RenderingLayer renderingLayer = this._renderingLayers[rlayer.LayerIndex + shadow.LayerIndexOffset];
          TileRenderer.ShadowColour = shadow.Colour;
          this.DrawBackObjects(renderer, viewport, viewOptions, renderingLayer);
          tr.ModelMatrix = Matrix4.CreateTranslation((Vector2) shadow.Displacement);
          tiler.RenderTiles(g, tr, renderingLayer.RenderTileIndex, renderingLayer.RenderTileCount);
          this.DrawFrontObjects(renderer, viewport, viewOptions, renderingLayer);
        }
        tr.ModelMatrix = modelMatrix;
        ShadowRenderer.IsShadowing = false;
        viewOptions.Shadows = false;
      }

      private void DrawBackObjects(
        Renderer renderer,
        Viewport viewport,
        LayerViewOptions viewOptions,
        RenderingLayer rlayer)
      {
        if (rlayer.LowObjects != null)
        {
          foreach (ActiveObject lowObject in rlayer.LowObjects)
            lowObject.Draw(renderer, viewport, viewOptions);
        }
        renderer.DeativateRenderer();
      }

      private void DrawFrontObjects(
        Renderer renderer,
        Viewport viewport,
        LayerViewOptions viewOptions,
        RenderingLayer rlayer)
      {
        if (rlayer.HighObjects != null)
        {
          foreach (ActiveObject highObject in rlayer.HighObjects)
            highObject.Draw(renderer, viewport, viewOptions);
        }
        renderer.DeativateRenderer();
      }

      public void RenderToClipboard(Viewport viewport, LayerViewOptions layerViewOptions)
      {
        Rectanglei destination1 = viewport.Destination;
        int width1 = destination1.Width;
        destination1 = viewport.Destination;
        int height1 = destination1.Height;
        int num1 = width1 * height1;
        int num2 = 4194304 /*0x400000*/;
        if (num1 > num2)
        {
          Trace.WriteLine($"Copying {num1} pixels, Max copy image limit: {num2} pixels");
        }
        else
        {
          int width2;
          int height2;
          byte[] argbData;
          try
          {
            IGraphicsContext graphicsContext = this._gameContext.Window.GraphicsContext;
            Rectanglei destination2 = viewport.Destination;
            int width3 = destination2.Width;
            destination2 = viewport.Destination;
            int height3 = destination2.Height;
            using (IFramebuffer frameBuffer = graphicsContext.CreateFrameBuffer(width3, height3))
            {
              using (Renderer renderer = (Renderer) new TheRenderer(this._gameContext.Window))
              {
                frameBuffer.Activate();
                this.Render(renderer, viewport, layerViewOptions);
              }
              ITexture texture = frameBuffer.Textures[0];
              width2 = texture.Width;
              height2 = texture.Height;
              argbData = texture.GetArgbData();
            }
          }
          catch (Exception ex)
          {
            return;
          }
          try
          {
            using (Bitmap bitmap = new Bitmap(width2, height2, width2 * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, Marshal.UnsafeAddrOfPinnedArrayElement((Array) argbData, 0)))
            {
              using (MemoryStream data1 = new MemoryStream())
              {
                bitmap.Save((System.IO.Stream) data1, ImageFormat.Png);
                File.WriteAllBytes(Path.Combine(this._gameContext.UserDataDirectory, "copiedtiles.png"), data1.ToArray());
                DataObject data2 = new DataObject();
                data2.SetData("PNG", false, (object) data1);
                Clipboard.SetDataObject((object) data2, true);
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
      }

      private void RenderWaterfallEffects(Renderer renderer, Viewport viewport, RenderingLayer rlayer)
      {
        this.GetWaterfallRegions(rlayer.Layer, viewport);
        if (this._waterfallRegions.Count <= 0)
          return;
        IFramebuffer canvasFramebuffer = this._canvasFramebuffer;
        ITexture texture = canvasFramebuffer.Textures[0];
        this._effectFramebuffer.Activate();
        this._graphicsContext.ClearBuffer();
        RenderingHelpers.RenderToFramebuffer(renderer, texture, this._effectFramebuffer, BlendMode.Opaque);
        this._waterfallDistortionTexture.Wrapping = TextureWrapping.Repeat;
        WaterfallRenderer waterfallRenderer = WaterfallRenderer.FromRenderer(renderer);
        waterfallRenderer.DistortionOffset = new Vector2(0.0, (double) (this._level.Ticks % 60) / -60.0);
        waterfallRenderer.DistortionTexture = this._waterfallDistortionTexture;
        waterfallRenderer.DistortionAmount = 16.0;
        waterfallRenderer.NonDistortionRadius = new Vector2i(4, 24);
        foreach (Rectanglei waterfallRegion in this._waterfallRegions)
        {
          Vector2i amount = new Vector2i(0, 1080 - waterfallRegion.Bottom - waterfallRegion.Top);
          Rectanglei source = waterfallRegion.OffsetBy(amount);
          waterfallRenderer.Render(texture, source, waterfallRegion, flipY: true);
        }
        RenderingHelpers.RenderToFramebuffer(renderer, this._effectFramebuffer.Textures[0], canvasFramebuffer);
      }

      private void GetWaterfallRegions(LevelLayer layer, Viewport viewport)
      {
        this._waterfallRegions.Clear();
        if (layer.WaterfallEffects == null)
          return;
        Rectanglei bounds = viewport.Bounds;
        Vector2i amount = new Vector2i(-bounds.X, -bounds.Y);
        foreach (Rectanglei waterfallEffect in layer.WaterfallEffects)
        {
          Rectanglei a = new Rectanglei(waterfallEffect.X * 64 /*0x40*/, waterfallEffect.Y * 64 /*0x40*/, waterfallEffect.Width * 64 /*0x40*/, waterfallEffect.Height * 64 /*0x40*/);
          a = Rectanglei.Intersect(a, bounds);
          if (a.Width > 0 && a.Height > 0)
          {
            a = a.OffsetBy(amount);
            this._waterfallRegions.Add(a);
          }
        }
      }

      private void RenderParticles(Renderer renderer, Viewport viewport, RenderingLayer rlayer)
      {
        renderer.DeativateRenderer();
        this._particleManager.Draw(renderer, viewport, rlayer.Layer, this._videoSettings);
      }
    }
}
