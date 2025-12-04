// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.LevelRendering.RenderingTileList
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

namespace SonicOrca.Drawing.LevelRendering
{

    internal class RenderingTileList
    {
      private const int InitialTileRenderInfoCapacity = 128 /*0x80*/;
      private TileRenderInfo[] _buffer = new TileRenderInfo[128 /*0x80*/];
      private int _currentCount;

      public int Count => this._currentCount;

      public TileRenderInfo this[int index] => this._buffer[index];

      private void ExpandBuffer() => this._buffer = new TileRenderInfo[this._buffer.Length * 2];

      public void Clear() => this._currentCount = 0;

      public void AddTileRenderInfo(TileRenderInfo tileRenderInfo)
      {
        if (this._currentCount >= this._buffer.Length)
          this.ExpandBuffer();
        this._buffer[this._currentCount] = tileRenderInfo;
        ++this._currentCount;
      }
    }
}
