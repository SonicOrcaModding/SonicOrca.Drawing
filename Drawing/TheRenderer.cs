// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.TheRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Drawing.Renderers;
using SonicOrca.Graphics;

namespace SonicOrca.Drawing
{
    public class TheRenderer : Renderer
    {
        public TheRenderer(WindowContext windowContext) : base(windowContext)
        {
        }

        public override I2dRenderer Get2dRenderer()
        {
            return (I2dRenderer)SimpleRenderer.FromRenderer(this);
        }

        public override IFontRenderer GetFontRenderer()
        {
            return (IFontRenderer)FontRenderer.FromRenderer(this);
        }

        public override ITileRenderer GetTileRenderer()
        {
            return (ITileRenderer)TileRenderer.FromRenderer(this);
        }

        public override IObjectRenderer GetObjectRenderer()
        {
            return (IObjectRenderer)ObjectRenderer.FromRenderer(this);
        }

        public override ICharacterRenderer GetCharacterRenderer()
        {
            return (ICharacterRenderer)CharacterRenderer.FromRenderer(this);
        }

        public override IWaterRenderer GetWaterRenderer()
        {
            return (IWaterRenderer)WaterRenderer.FromRenderer(this);
        }

        public override IHeatRenderer GetHeatRenderer()
        {
            return (IHeatRenderer)HeatRenderer.FromRenderer(this);
        }

        public override INonLayerRenderer GetNonLayerRenderer()
        {
            return (INonLayerRenderer)NonLayerRenderer.FromRenderer(this);
        }

        public override IMaskRenderer GetMaskRenderer()
        {
            return (IMaskRenderer)MaskRenderer.FromRenderer(this);
        }

        public override IFadeTransitionRenderer CreateFadeTransitionRenderer()
        {
            return new ClassicFadeTransitionRenderer(this.Window.GraphicsContext);
        }
    }
}
