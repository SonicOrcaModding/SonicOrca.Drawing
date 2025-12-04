// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.DefaultRendererFactory
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Drawing.Renderers;
using SonicOrca.Graphics;

namespace SonicOrca.Drawing
{

    public class DefaultRendererFactory : IRendererFactory
    {
      private readonly IGraphicsContext _graphicsContext;

      private DefaultRendererFactory(IGraphicsContext graphicsContext)
      {
        this._graphicsContext = graphicsContext;
      }

      public static IRendererFactory Create(IGraphicsContext graphicsContext)
      {
        return (IRendererFactory) new DefaultRendererFactory(graphicsContext);
      }

      public I2dRenderer Create2dRenderer() => (I2dRenderer) null;

      public IFontRenderer CreateFontRenderer() => (IFontRenderer) null;

      public IFadeTransitionRenderer CreateFadeTransitionRenderer()
      {
        return (IFadeTransitionRenderer) new ClassicFadeTransitionRenderer(this._graphicsContext);
      }

      public ITileRenderer CreateTileRenderer() => (ITileRenderer) null;

      public IObjectRenderer CreateObjectRenderer() => (IObjectRenderer) null;

      public ICharacterRenderer CreateCharacterRenderer() => (ICharacterRenderer) null;

      public IWaterRenderer CreateWaterRenderer() => (IWaterRenderer) null;

      public IHeatRenderer CreateHeatRenderer() => (IHeatRenderer) null;

      public INonLayerRenderer CreateNonLayerRenderer() => (INonLayerRenderer) null;

      public IMaskRenderer CreateMaskRenderer() => (IMaskRenderer) null;
    }
}
