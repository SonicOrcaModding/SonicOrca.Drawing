// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.LevelRendering.TileRenderInfo
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Core;
using SonicOrca.Geometry;

namespace SonicOrca.Drawing.LevelRendering
{

    internal struct TileRenderInfo
    {
      public LevelLayer Layer { get; set; }

      public int TileIndex { get; set; }

      public Rectanglei Source { get; set; }

      public Rectanglei Destination { get; set; }
    }
}
