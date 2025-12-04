// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.LevelRendering.RenderingTiler
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Core;
using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;

namespace SonicOrca.Drawing.LevelRendering
{

    internal class RenderingTiler
    {
      private readonly Level _level;
      private readonly TileSet _tileSet;
      private readonly RenderingTileList _renderingTileList;

      public RenderingTiler(Level level, RenderingTileList renderingTileList)
      {
        this._level = level;
        this._tileSet = level.TileSet;
        this._renderingTileList = renderingTileList;
      }

      public void PrepareTiles(
        RenderingLayer renderingLayer,
        Viewport viewport,
        LayerViewOptions layerViewOptions)
      {
        int count1 = this._renderingTileList.Count;
        if (layerViewOptions.ShowLandscape)
          this.PrepareTilesVertical(renderingLayer, viewport);
        int count2 = this._renderingTileList.Count - count1;
        renderingLayer.SetRenderTileRange(count1, count2);
      }

      private void PrepareTilesVertical(RenderingLayer renderingLayer, Viewport viewport)
      {
        LevelLayer layer = renderingLayer.Layer;
        if (layer.Rows == 0 || layer.Columns == 0)
          return;
        int num1 = layer.Editing ? 1 : 0;
        bool flag = num1 == 0 && layer.WrapY;
        int offsetY = num1 != 0 ? 0 : layer.OffsetY;
        double num2 = num1 != 0 ? 1.0 : layer.ParallaxY;
        int num3 = layer.Rows * 64 /*0x40*/;
        Rectanglei rectanglei = viewport.Bounds;
        int num4 = (int) ((double) rectanglei.Y * num2);
        if (!flag && num4 >= num3)
          return;
        int sourceY = num4 % num3;
        rectanglei = viewport.Destination;
        int destinationY = rectanglei.Y + offsetY;
        while (true)
        {
          do
          {
            double num5 = (double) destinationY;
            rectanglei = viewport.Destination;
            double num6 = (double) rectanglei.Bottom / viewport.Scale.Y;
            if (num5 < num6)
            {
              int height = num3 - sourceY;
              this.PrepareTilesHorizontal(renderingLayer, viewport, sourceY, destinationY, ref height);
              if (height > 0)
              {
                sourceY += height;
                destinationY += height;
                if (flag)
                  goto label_7;
              }
              else
                goto label_3;
            }
            else
              goto label_12;
          }
          while (sourceY <= num3);
          goto label_11;
    label_7:
          sourceY %= num3;
        }
    label_3:
        return;
    label_11:
        return;
    label_12:;
      }

      private void PrepareTilesHorizontal(
        RenderingLayer renderingLayer,
        Viewport viewport,
        int sourceY,
        int destinationY,
        ref int height)
      {
        LevelLayer layer = renderingLayer.Layer;
        bool editing = layer.Editing;
        int num1 = 64 /*0x40*/;
        int num2 = layer.Columns * num1;
        int num3 = layer.Rows * num1;
        bool flag = !editing && layer.WrapX;
        double num4 = editing ? 1.0 : layer.ParallaxY;
        int num5 = 0;
        int top1;
        LayerRowDefinition rowDefinitionAt = layer.GetRowDefinitionAt(sourceY, out top1);
        if (rowDefinitionAt != null && !editing)
        {
          num4 = rowDefinitionAt.Parallax;
          num5 = (int) rowDefinitionAt.CurrentOffset;
          if (rowDefinitionAt.Width != 0)
          {
            int width = rowDefinitionAt.Width;
            if (width < num2)
              num2 = width;
          }
          height = Math.Min(height, top1 + rowDefinitionAt.Height - sourceY);
          height = Math.Min(height, 64 /*0x40*/ - sourceY % 64 /*0x40*/);
          if (height <= 0)
          {
            height = 1;
            return;
          }
        }
        if (height > num1)
          height = num1 - sourceY % num1;
        int num6 = destinationY + height;
        Rectanglei rectanglei = viewport.Destination;
        int top2 = rectanglei.Top;
        if (num6 < top2)
          return;
        rectanglei = viewport.Bounds;
        int num7 = (int) ((double) rectanglei.X * num4) - num5;
        if (flag)
        {
          while (num7 < 0)
            num7 += num2;
        }
        if (!flag && num7 > num2)
          return;
        if (flag)
          num7 %= num2;
        int num8 = num7;
        rectanglei = viewport.Destination;
        int num9 = (int) ((double) rectanglei.X / viewport.Scale.X);
        if (!editing)
        {
          double parallaxY = layer.ParallaxY;
        }
        TileSet tileSet = this._tileSet;
        int[,] tiles = layer.Tiles;
        while (true)
        {
          do
          {
            double num10 = (double) num9;
            rectanglei = viewport.Destination;
            double right = (double) rectanglei.Right;
            Vector2 scale = viewport.Scale;
            double x1 = scale.X;
            double num11 = right / x1;
            if (num10 < num11)
            {
              int num12 = 0;
              if (num8 >= 0 && sourceY >= 0 && num8 < num2 && sourceY < num3)
                num12 = tiles[num8 / num1, sourceY / num1];
              int num13 = -(num8 % num1);
              int num14 = num1 - num8 % num1;
              if (num14 != 0)
              {
                if (tileSet.TryGetValue(num12 & 4095 /*0x0FFF*/, out ITile _))
                {
                  Rectangle rectangle = new Rectangle(0.0, (double) (sourceY % num1), 64.0, (double) height);
                  double num15 = (double) (num9 + num13);
                  scale = viewport.Scale;
                  double x2 = scale.X;
                  double x3 = num15 * x2;
                  double num16 = (double) destinationY;
                  scale = viewport.Scale;
                  double y1 = scale.Y;
                  double y2 = num16 * y1;
                  double num17 = (double) num1;
                  scale = viewport.Scale;
                  double x4 = scale.X;
                  double width = num17 * x4;
                  double num18 = (double) height;
                  scale = viewport.Scale;
                  double y3 = scale.Y;
                  double height1 = num18 * y3;
                  Rectanglei area = (Rectanglei) new Rectangle(x3, y2, width, height1);
                  TileRenderInfo tileRenderInfo = new TileRenderInfo()
                  {
                    Layer = layer,
                    TileIndex = num12,
                    Source = (Rectanglei) rectangle,
                    Destination = area
                  };
                  if (!renderingLayer.IsAreaClipped(viewport, area))
                    this._renderingTileList.AddTileRenderInfo(tileRenderInfo);
                }
                num8 += num14;
                num9 += num14;
                if (flag)
                  goto label_28;
              }
              else
                goto label_15;
            }
            else
              goto label_32;
          }
          while (num8 <= num2);
          goto label_31;
    label_28:
          num8 %= num2;
        }
    label_15:
        return;
    label_31:
        return;
    label_32:;
      }

      public void RenderTiles(I2dRenderer g, ITileRenderer tr, int startIndex, int count)
      {
        if (count == 0)
          return;
        int num = startIndex + count - 1;
        TileRenderInfo renderingTile;
        for (int index1 = 1; index1 <= 2; ++index1)
        {
          TileBlendMode tileBlendMode = (TileBlendMode) index1;
          tr.BeginRender();
          for (int index2 = startIndex; index2 <= num; ++index2)
          {
            renderingTile = this._renderingTileList[index2];
            ITile tile1;
            if (this._tileSet.TryGetValue(renderingTile.TileIndex, out tile1) && tile1 is Tile tile2 && tile2.Blend == tileBlendMode)
              tile2.Draw(tr, renderingTile.TileIndex, renderingTile.Source, renderingTile.Destination);
          }
          tr.EndRender();
        }
        for (int index = startIndex; index <= num; ++index)
        {
          renderingTile = this._renderingTileList[index];
          ITile tile;
          if (this._tileSet.TryGetValue(renderingTile.TileIndex, out tile) && tile is TileSequence tileSequence)
            tileSequence.Draw(g, renderingTile.TileIndex, renderingTile.Source, renderingTile.Destination);
        }
      }
    }
}
