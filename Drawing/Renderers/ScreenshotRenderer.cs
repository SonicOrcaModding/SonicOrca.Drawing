// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.Renderers.ScreenshotRenderer
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SonicOrca.Drawing.Renderers
{

    public class ScreenshotRenderer
    {
      public static void GrabScreenshot()
      {
        int width = 1920;
        int height = 1080;
        Bitmap bitmap = new Bitmap(width, height);
        BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bitmapdata.Scan0);
        bitmap.UnlockBits(bitmapdata);
        bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
        string filename = $"Sonic 2 HD {(object) DateTime.Now.Year}-{(object) DateTime.Now.Month}-{(object) DateTime.Now.Day} {(object) DateTime.Now.Hour}-{(object) DateTime.Now.Minute}-{(object) DateTime.Now.Second}-{(object) DateTime.Now.Millisecond}.png";
        bitmap.Save(filename, ImageFormat.Png);
        bitmap.Dispose();
      }
    }
}
