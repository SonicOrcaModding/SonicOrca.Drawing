// Decompiled with JetBrains decompiler
// Type: SonicOrca.Extensions.GraphicsExtensions
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using SonicOrca.Geometry;
using SonicOrca.Graphics;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SonicOrca.Extensions
{

    public static class GraphicsExtensions
    {
      public static Matrix4 CreateOrthographic(this IFramebuffer renderTarget)
      {
        return Matrix4.CreateOrthographicOffCenter(0.0, (double) renderTarget.Width, (double) renderTarget.Height, 0.0, 0.0, 1.0);
      }

      public static void SetData<T>(this IBuffer vbo, T[] data) => vbo.SetData<T>(data, 0, data.Length);

      public static void DefineAttributes(
        this IVertexArray vao,
        IShaderProgram program,
        IBuffer vbo,
        Type vertexType)
      {
        vao.Bind();
        vbo.Bind();
        int stride = Marshal.SizeOf(vertexType);
        foreach (FieldInfo field in vertexType.GetFields())
        {
          VertexAttributeAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<VertexAttributeAttribute>((MemberInfo) field);
          if (customAttribute != null)
          {
            Type fieldType = field.FieldType;
            VertexAttributeTypeAttribute attributeTypeAttribute = CustomAttributeExtensions.GetCustomAttribute<VertexAttributeTypeAttribute>((MemberInfo) fieldType);
            if (attributeTypeAttribute == null)
            {
              if (fieldType == typeof (float))
                attributeTypeAttribute = new VertexAttributeTypeAttribute(VertexAttributePointerType.Float, 1);
              else if (fieldType == typeof (int))
                attributeTypeAttribute = new VertexAttributeTypeAttribute(VertexAttributePointerType.Int, 1);
            }
            int offset = (int) Marshal.OffsetOf(vertexType, field.Name);
            vao.DefineAttribute(program.GetAttributeLocation(customAttribute.Name), attributeTypeAttribute.Type, attributeTypeAttribute.Size, stride, offset);
          }
        }
      }

      public static void Render<T>(this IVertexArray vao, PrimitiveType type, T[] vertices)
      {
        vao.Render(type, 0, vertices.Length);
      }
    }
}
