// Decompiled with JetBrains decompiler
// Type: SonicOrca.Drawing.GreenList`1
// Assembly: SonicOrca.Drawing, Version=2.0.1012.10520, Culture=neutral, PublicKeyToken=null
// MVID: 31C48419-27DE-46EA-9D16-61FB91FF0FE1
// Assembly location: C:\Games\S2HD_2.0.1012-rc2\SonicOrca.Drawing.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace SonicOrca.Drawing
{

    internal class GreenList<T>
    {
      private T[] _items;

      public int Count { get; private set; }

      public int Capacity { get; private set; }

      public int GrowFactor { get; set; }

      public GreenList(int initialCapacity, int growFactor)
      {
        this.Capacity = initialCapacity;
        this.GrowFactor = growFactor;
        this._items = new T[initialCapacity];
        if (!typeof (T).IsClass)
          return;
        for (int index = 0; index < initialCapacity; ++index)
          this._items[index] = Activator.CreateInstance<T>();
      }

      public void EnsureCapacity(int targetCapacity)
      {
        if (this.Capacity >= targetCapacity)
          return;
        Array.Resize<T>(ref this._items, targetCapacity);
        if (typeof (T).IsClass)
        {
          for (int index = targetCapacity - this.Capacity; index < targetCapacity; ++index)
            this._items[index] = Activator.CreateInstance<T>();
        }
        this.Capacity = targetCapacity;
      }

      public void Add(T item)
      {
        if (this.Count == this.Capacity)
          this.EnsureCapacity(this.Capacity + this.GrowFactor);
        this._items[this.Count++] = item;
      }

      public void AddRange(IEnumerable<T> items)
      {
        switch (items)
        {
          case T[] items1:
            this.AddRange(items1);
            break;
          case IReadOnlyCollection<T> items2:
            this.AddRange(items2);
            break;
          default:
            this.AddRange(items.ToArray<T>());
            break;
        }
      }

      public void AddRange(IReadOnlyCollection<T> items)
      {
        int num = items.Count - (this.Capacity - this.Count);
        if (num > 0)
          this.EnsureCapacity(this.Capacity + (num + (this.GrowFactor - 1)) / this.GrowFactor);
        foreach (T obj in (IEnumerable<T>) items)
          this._items[this.Count++] = obj;
      }

      public void AddRange(T[] items)
      {
        int num1 = this.Capacity - this.Count;
        int num2 = items.Length - num1;
        if (num2 > 0)
          this.EnsureCapacity(this.Capacity + (num2 + (this.GrowFactor - 1)) / this.GrowFactor);
        Array.Copy((Array) items, 0, (Array) this._items, this.Count, items.Length);
        this.Count += items.Length;
      }

      public void Clear() => this.Count = 0;

      public T this[int index]
      {
        get => this._items[index];
        set => this._items[index] = value;
      }
    }
}
