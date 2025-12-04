// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.LevelRendering.RenderingLayer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Core;
using SonicOrca.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SonicOrca.Drawing.LevelRendering
{

    internal class RenderingLayer
    {
      private readonly Level _level;
      private readonly IVideoSettings _videoSettings;
      private LevelMarker[] _clippingMarkers = new LevelMarker[0];

      public int LayerIndex { get; }

      public LevelLayer Layer { get; }

      public int RenderTileIndex { get; private set; }

      public int RenderTileCount { get; private set; }

      public List<ActiveObject> LowObjects { get; private set; }

      public List<ActiveObject> HighObjects { get; private set; }

      public bool HasShadows { get; private set; }

      public RenderingLayer(Level level, int layerIndex, IVideoSettings videoSettings)
      {
        this._level = level;
        this._videoSettings = videoSettings;
        this.LayerIndex = layerIndex;
        this.Layer = this._level.Map.Layers[layerIndex];
      }

      public void Clear()
      {
        this.HasShadows = this.Layer.Shadows.Count > 0 && this._videoSettings.EnableShadows;
        this.LowObjects?.Clear();
        this.HighObjects?.Clear();
      }

      public void SetRenderTileRange(int index, int count)
      {
        this.RenderTileIndex = index;
        this.RenderTileCount = count;
      }

      public void AddLowObject(ActiveObject obj)
      {
        if (this.LowObjects == null)
          this.LowObjects = new List<ActiveObject>();
        this.LowObjects.Add(obj);
      }

      public void AddHighObject(ActiveObject obj)
      {
        if (this.HighObjects == null)
          this.HighObjects = new List<ActiveObject>();
        this.HighObjects.Add(obj);
      }

      public void UpdateClippingMarkers()
      {
        this._clippingMarkers = this._level.Map.Markers.Where<LevelMarker>((Func<LevelMarker, bool>) (m => string.Equals(m.Tag, "clipping", StringComparison.OrdinalIgnoreCase))).Where<LevelMarker>((Func<LevelMarker, bool>) (m => m.Layer == this.Layer)).ToArray<LevelMarker>();
      }

      public bool IsAreaClipped(Viewport viewport, Rectanglei area)
      {
        if (this._clippingMarkers.Length == 0)
          return false;
        foreach (LevelMarker clippingMarker in this._clippingMarkers)
        {
          Rectanglei bounds = clippingMarker.Bounds;
          int x1 = bounds.X;
          bounds = viewport.Bounds;
          int x2 = bounds.X;
          double num1 = (double) (x1 - x2);
          Vector2 scale = viewport.Scale;
          double x3 = scale.X;
          double x4 = num1 * x3;
          bounds = clippingMarker.Bounds;
          int y1 = bounds.Y;
          bounds = viewport.Bounds;
          int y2 = bounds.Y;
          double num2 = (double) (y1 - y2);
          scale = viewport.Scale;
          double y3 = scale.Y;
          double y4 = num2 * y3;
          bounds = clippingMarker.Bounds;
          double width1 = (double) bounds.Width;
          scale = viewport.Scale;
          double x5 = scale.X;
          double width2 = width1 * x5;
          bounds = clippingMarker.Bounds;
          double height1 = (double) bounds.Height;
          scale = viewport.Scale;
          double y5 = scale.Y;
          double height2 = height1 * y5;
          if (((Rectanglei) new Rectangle(x4, y4, width2, height2)).IntersectsWith(area))
            return false;
        }
        return true;
      }
    }
}
