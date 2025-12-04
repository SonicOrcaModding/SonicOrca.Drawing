// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.RenderingHelpers
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Geometry;
using SonicOrca.Graphics;

namespace SonicOrca.Drawing.Renderers
{

    internal static class RenderingHelpers
    {
      public static void RenderToFramebuffer(
        Renderer renderer,
        IFramebuffer source,
        IFramebuffer destination,
        BlendMode blend = BlendMode.Alpha)
      {
        RenderingHelpers.RenderToFramebuffer(renderer, source.Textures[0], destination, new Rectangle(0.0, 0.0, (double) destination.Width, (double) destination.Height), blend);
      }

      public static void RenderToFramebuffer(
        Renderer renderer,
        ITexture source,
        IFramebuffer destination,
        BlendMode blend = BlendMode.Alpha)
      {
        RenderingHelpers.RenderToFramebuffer(renderer, source, destination, new Rectangle(0.0, 0.0, (double) destination.Width, (double) destination.Height), blend);
      }

      public static void RenderToFramebuffer(
        Renderer renderer,
        ITexture source,
        IFramebuffer destination,
        Rectangle destinationRect,
        BlendMode blend)
      {
        destination.Activate();
        I2dRenderer obj = renderer.Get2dRenderer();
        obj.BlendMode = blend;
        obj.Colour = Colours.White;
        obj.ClipRectangle = destinationRect;
        obj.ModelMatrix = Matrix4.Identity;
        obj.RenderTexture(source, new Rectangle(0.0, 0.0, (double) source.Width, (double) source.Height), destinationRect, flipy: true);
        obj.Deactivate();
      }
    }
}
