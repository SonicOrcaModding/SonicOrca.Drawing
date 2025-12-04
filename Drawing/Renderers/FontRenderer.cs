// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.FontRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Collections.Generic;

namespace SonicOrca.Drawing.Renderers
{

    public class FontRenderer : IFontRenderer
    {
      private static readonly Dictionary<Renderer, FontRenderer> RendererDictionary = new Dictionary<Renderer, FontRenderer>();
      private readonly Renderer _renderer;
      private ITexture[] textures = new ITexture[2];

      public Font Font { get; set; }

      public Colour Colour { get; set; }

      public Rectangle Boundary { get; set; }

      public FontAlignment Alignment { get; set; }

      public Vector2 Shadow { get; set; }

      public int Overlay { get; set; }

      public string Text { get; set; }

      public static FontRenderer FromRenderer(Renderer renderer)
      {
        if (!FontRenderer.RendererDictionary.ContainsKey(renderer))
          FontRenderer.RendererDictionary.Add(renderer, new FontRenderer(renderer));
        return FontRenderer.RendererDictionary[renderer];
      }

      public FontRenderer(Renderer renderer) => this._renderer = renderer;

      public Rectangle Measure() => throw new NotImplementedException();

      public void RenderString(
        string text,
        Rectangle boundary,
        FontAlignment fontAlignment,
        Font font,
        int overlay)
      {
        this.RenderString(text, boundary, fontAlignment, font, Colours.White, new int?(overlay));
      }

      public void RenderString(
        string text,
        Rectangle boundary,
        FontAlignment fontAlignment,
        Font font,
        Colour colour,
        int? overlay = null)
      {
        this.RenderStringWithShadow(text, boundary, fontAlignment, font, colour, overlay, new Vector2i?(), Colours.Black, new int?());
      }

      public void RenderStringWithShadow(
        string text,
        Rectangle boundary,
        FontAlignment fontAlignment,
        Font font,
        int overlay)
      {
        this.RenderStringWithShadow(text, boundary, fontAlignment, font, Colours.White, new int?(overlay), font.DefaultShadow, Colours.Black, new int?());
      }

      public void RenderStringWithShadow(
        string text,
        Rectangle boundary,
        FontAlignment fontAlignment,
        Font font,
        Colour colour,
        int? overlay = null)
      {
        this.RenderStringWithShadow(text, boundary, fontAlignment, font, colour, overlay, font.DefaultShadow, new Colour(colour.Alpha, (byte) 0, (byte) 0, (byte) 0), new int?());
      }

      public void RenderStringWithShadow(
        string text,
        Rectangle boundary,
        FontAlignment fontAlignment,
        Font font,
        Colour colour,
        int? overlay,
        Vector2i? shadow,
        Colour shadowColour,
        int? shadowOverlay)
      {
        SimpleRenderer g = SimpleRenderer.FromRenderer(this._renderer);
        Rectangle rectangle = font.MeasureString(text, boundary, fontAlignment);
        Vector2 destination = new Vector2(rectangle.X, rectangle.Y);
        if (shadow.HasValue)
          this.RenderStringWithShadow(text, boundary + (Vector2) shadow.Value, fontAlignment, font, shadowColour, shadowOverlay, new Vector2i?(), new Colour(), new int?());
        foreach (char key in text)
        {
          Font.CharacterDefinition characterDefinition = font[key];
          if (characterDefinition == null)
          {
            destination.X += (double) font.DefaultWidth;
          }
          else
          {
            this.RenderCharacter(g, font, characterDefinition, destination, colour, overlay);
            destination.X += (double) characterDefinition.Width;
          }
          destination.X += (double) font.Tracking;
        }
      }

      private void RenderCharacter(
        SimpleRenderer g,
        Font font,
        Font.CharacterDefinition characterDefinition,
        Vector2 destination,
        Colour colour,
        int? overlay)
      {
        this.textures[0] = font.ShapeTexture;
        if (overlay.HasValue)
          this.textures[1] = font.OverlayTextures[overlay.Value];
        Rectangle sourceRectangle = (Rectangle) characterDefinition.SourceRectangle;
        Rectangle destination1 = new Rectangle(destination.X + (double) characterDefinition.Offset.X, destination.Y + (double) characterDefinition.Offset.Y, sourceRectangle.Width, sourceRectangle.Height);
        destination1.X *= this._renderer.GetObjectRenderer().Scale.X;
        destination1.Y *= this._renderer.GetObjectRenderer().Scale.Y;
        destination1.Width *= this._renderer.GetObjectRenderer().Scale.X;
        destination1.Height *= this._renderer.GetObjectRenderer().Scale.Y;
        g.BlendMode = BlendMode.Alpha;
        g.Colour = colour;
        if (overlay.HasValue)
          g.RenderTexture((IEnumerable<ITexture>) this.textures, sourceRectangle, destination1, false, false);
        else
          g.RenderTexture(this.textures[0], sourceRectangle, destination1, false, false);
      }

      public void Render() => SimpleRenderer.FromRenderer(this._renderer);
    }
}
