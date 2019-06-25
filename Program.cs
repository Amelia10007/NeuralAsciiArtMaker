using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace NeuralAsciiArtMaker
{
  public static class Extension
  {
    /// <summary>
    /// 指定した定義域を持つ一様分布に従う乱数を返します。
    /// </summary>
    /// <param name="random"></param>
    /// <param name="min">最小値。</param>
    /// <param name="max">最大値。</param>
    /// <returns>min以上max未満の実数。</returns>
    public static double NextDouble(this Random random, int min, int max)
    {
      return min + (max - min) * random.Next(int.MaxValue) / (double)int.MaxValue;
    }
    /// <summary>
    /// 指定した平均値、分散のガウス分布に従う乱数を返します。
    /// </summary>
    /// <param name="random"></param>
    /// <param name="average">ガウス分布の平均。</param>
    /// <param name="variance">ガウス分布の分散。</param>
    /// <returns></returns>
    public static double NextGauss(this Random random, double average, double variance)
    {
      double rnd1 = random.NextDouble(0, 1);
      double rnd2 = random.NextDouble(0, 1);
      double r = Math.Sqrt(-2 * Math.Log(rnd1)) * Math.Cos(2.0 * Math.PI * rnd2);
      return r * Math.Sqrt(variance) + average;
    }
  }
  /// <summary>
  /// 指定した型のオブジェクトと座標のペアを表現します。
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Correspond<T>
  {
    private class Element
    {
      public int[] Location;
      public T Value;
      public Element(T value, int[] location)
      {
        this.Location = new int[location.Length];
        for (int i = 0; i < location.Length; i++)
        {
          this.Location[i] = location[i];
        }
        this.Value = value;
      }
      public bool PositionEquals(params int[] location)
      {
        if (location.Length != this.Location.Length) return false;
        for (int i = 0; i < this.Location.Length; i++)
        {
          if (this.Location[i] != location[i])
            return false;
        }
        return true;
      }
    }
    private List<Element> elements;
    /// <summary>
    /// 指定した位置の要素を取得または設定します。
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public T this[params int[] location]
    {
      get
      {
        return this.elements.Find(element => element.PositionEquals(location)).Value;
      }
      set
      {
        if (this.elements.Exists(element => element.PositionEquals(location)))
        {
          var e = this.elements.Find(element => element.Value.Equals(value));
          this.elements[this.elements.IndexOf(e)] = new Element(value, location);
        }
        else
        {
          this.elements.Add(new Element(value, location));
        }
      }
    }
    /// <summary>
    /// 空の状態で新しいインスタンスを初期化します。
    /// </summary>
    public Correspond()
    {
      this.elements = new List<Element>();
    }
    /// <summary>
    /// 指定した位置に要素が存在するか返します。
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool Exists(params int[] location)
    {
      return this.elements.Exists(element => element.PositionEquals(location));
    }
    /// <summary>
    /// 指定した位置の要素を削除します。
    /// </summary>
    /// <param name="location"></param>
    public void RemoveAt(params int[] location)
    {
      var e = this.elements.Find(element => element.PositionEquals(location));
      this.elements.RemoveAt(this.elements.IndexOf(e));
    }
    /// <summary>
    /// このインスタンスが持つ要素をリストにして返します。
    /// </summary>
    /// <returns></returns>
    public List<T> ToList()
    {
      return this.elements.ConvertAll(e => e.Value);
    }
  }
  /// <summary>
  /// 実数を要素とする行列を表します。
  /// </summary>
  public class Matrix
  {
    /// <summary>
    /// 行列の各成分。elements[i,j]は、行列の第i行第j列の要素を表します。
    /// </summary>
    protected double[,] elements;
    /// <summary>
    /// この行列の行数を取得します。
    /// </summary>
    public int Line
    {
      get
      {
        return this.elements.GetLength(0);
      }
    }
    /// <summary>
    /// この行列の列数を取得します。
    /// </summary>
    public int Column
    {
      get
      {
        return this.elements.GetLength(1);
      }
    }
    /// <summary>
    /// この行列の第i行j列目の要素を取得または設定します。
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    public double this[int i, int j]
    {
      get
      {
        return this.elements[i, j];
      }
      set
      {
        this.elements[i, j] = value;
      }
    }
    /// <summary>
    /// 成分を持たない行列を初期化します。
    /// </summary>
    public Matrix()
    {
      this.elements = new double[0, 0];
    }
    /// <summary>
    /// 指定した行数、列数を持つ行列を初期化します。
    /// </summary>
    /// <param name="line"></param>
    /// <param name="column"></param>
    public Matrix(int line, int column)
    {
      this.elements = new double[line, column];
    }
    /// <summary>
    /// 指定した行列と等しい成分を持つ行列を初期化します。
    /// </summary>
    /// <param name="matrix"></param>
    public Matrix(Matrix matrix)
    {
      this.elements = new double[matrix.Line, matrix.Column];
      for (int i = 0; i < matrix.Line; i++)
      {
        for (int j = 0; j < matrix.Column; j++)
        {
          this.elements[i, j] = matrix[i, j];
        }
      }
    }
    /// <summary>
    /// この行列の転置行列を返します。
    /// </summary>
    /// <returns></returns>
    public Matrix Transpose()
    {
      Matrix value = new Matrix(this.Column, this.Line);
      for (int i = 0; i < this.Line; i++)
      {
        for (int j = 0; j < this.Column; j++)
        {
          value[j, i] = this[i, j];
        }
      }
      return value;
    }
    /// <summary>
    /// 指定した行列のアダマール積を返します。
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Matrix Hadamard(Matrix left, Matrix right)
    {
      if (left.Line != right.Line || left.Column != right.Column)
        throw new ArgumentException("行数または列数が異なる行列の間にはアダマール積を定義できません。");
      Matrix value = new Matrix(left.Line, left.Column);
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          value[i, j] = left[i, j] * right[i, j];
        }
      }
      return value;
    }
    public static Matrix operator +(Matrix left, Matrix right)
    {
      if (left.Line != right.Line || left.Column != right.Column)
        throw new ArgumentException("行数または列数が異なる行列の間には和を定義できません。");
      Matrix value = new Matrix(left.Line, left.Column);
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          value[i, j] = left[i, j] + right[i, j];
        }
      }
      return value;
    }
    public static Matrix operator -(Matrix left, Matrix right)
    {
      if (left.Line != right.Line || left.Column != right.Column)
        throw new ArgumentException("行数または列数が異なる行列の間には差を定義できません。");
      Matrix value = new Matrix(left.Line, left.Column);
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          value[i, j] = left[i, j] - right[i, j];
        }
      }
      return value;
    }
    public static Matrix operator *(double left, Matrix right)
    {
      Matrix value = new Matrix(right.Line, right.Column);
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          value[i, j] = left * right[i, j];
        }
      }
      return value;
    }
    public static Matrix operator *(Matrix left, double right)
    {
      Matrix value = new Matrix(left.Line, left.Column);
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          value[i, j] = left[i, j] * right;
        }
      }
      return value;
    }
    public static Matrix operator *(Matrix left, Matrix right)
    {
      if (left.Column != right.Line)
      {
        throw new ArgumentException("行列の積を定義できません。");
      }
      Matrix value = new Matrix(left.Line, right.Column);
      value.elements.Initialize();
      for (int i = 0; i < value.Line; i++)
      {
        for (int j = 0; j < value.Column; j++)
        {
          for (int k = 0; k < left.Column; k++)
          {
            value[i, j] += left[i, k] * right[k, j];
          }
        }
      }
      return value;
    }
    public static Vector operator *(Matrix left, Vector right)
    {
      if (left.Column != right.Dimension)
      {
        throw new ArgumentException("行列の列数とベクトルの次元が一致しません。");
      }
      Vector value = new Vector(left.Line);
      for (int i = 0; i < left.Line; i++)
      {
        for (int j = 0; j < left.Column; j++)
        {
          value[i] += left[i, j] * right[j];
        }
      }
      return value;
    }
    /// <summary>
    /// この行列を表す文字列を返します。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string value = "";
      for (int i = 0; i < elements.GetLength(0); i++)
      {
        for (int j = 0; j < elements.GetLength(1); j++)
        {
          value += this.elements[i, j];
          if (j < this.elements.GetLength(1) - 1)
          {
            value += ",";
          }
        }
        value += "\n";
      }
      return value;
    }
  }
  /// <summary>
  /// 縦ベクトルを表します。
  /// </summary>
  public class Vector
  {
    /// <summary>
    /// このベクトルの要素を取得または設定します。
    /// </summary>
    public double[] Elements
    {
      get; private set;
    }
    /// <summary>
    /// このベクトルの次元を取得します。
    /// </summary>
    public int Dimension
    {
      get
      {
        return this.Elements.Length;
      }
    }
    /// <summary>
    /// 指定した位置にある要素を取得または設定します。
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public double this[int index]
    {
      get
      {
        return this.Elements[index];
      }
      set
      {
        this.Elements[index] = value;
      }
    }
    /// <summary>
    /// 指定した次元を持つベクトルとしてこのインスタンスを初期化します。
    /// </summary>
    /// <param name="dimension"></param>
    public Vector(int dimension)
    {
      this.Initialize(new double[dimension]);
    }
    /// <summary>
    /// 指定したコレクションでこのインスタンスを初期化します。
    /// </summary>
    /// <param name="collection"></param>
    public Vector(IEnumerable<double> collection)
    {
      this.Initialize(collection);
    }
    /// <summary>
    /// 指定した要素を持つベクトルとしてこのインスタンスを初期化します。
    /// </summary>
    /// <param name="parameters"></param>
    public Vector(params double[] parameters)
    {
      this.Initialize(parameters);
    }
    /// <summary>
    /// 指定したコレクションと等価なベクトルとしてこのインスタンスを初期化します。
    /// </summary>
    /// <param name="collection"></param>
    private void Initialize(IEnumerable<double> collection)
    {
      int count = collection.Count();
      this.Elements = new double[count];
      for (int i = 0; i < count; i++)
      {
        this.Elements[i] = collection.ElementAt(i);
      }
    }
    /// <summary>
    /// 指定したベクトルのアダマール積を返します。
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Vector Hadamard(Vector left, Vector right)
    {
      if (left.Dimension != right.Dimension)
      {
        throw new ArgumentException("次元の異なるベクトルのアダマール積は定義されません。");
      }
      Vector value = new Vector(left.Dimension);
      for (int i = 0; i < left.Dimension; i++)
      {
        value[i] = left[i] * right[i];
      }
      return value;
    }
    public static Vector operator +(Vector left, Vector right)
    {
      if (left.Dimension != right.Dimension)
      {
        throw new ArgumentException("ベクトルの次元が一致しません。");
      }
      Vector value = new Vector(left.Dimension);
      for (int i = 0; i < value.Dimension; i++)
      {
        value[i] = left[i] + right[i];
      }
      return value;
    }
    public static Vector operator -(Vector left, Vector right)
    {
      if (left.Dimension != right.Dimension)
      {
        throw new ArgumentException("ベクトルの次元が一致しません。");
      }
      Vector value = new Vector(left.Dimension);
      for (int i = 0; i < value.Dimension; i++)
      {
        value[i] = left[i] - right[i];
      }
      return value;
    }
    public static Vector operator *(double left, Vector right)
    {
      Vector value = new Vector(right.Dimension);
      for (int i = 0; i < value.Dimension; i++)
      {
        value[i] = left * right[i];
      }
      return value;
    }
    public static Vector operator *(Vector left, double right)
    {
      Vector value = new Vector(left.Dimension);
      for (int i = 0; i < value.Dimension; i++)
      {
        value[i] = left[i] * right;
      }
      return value;
    }
    public static Vector operator /(Vector left, double right)
    {
      Vector value = new Vector(left.Dimension);
      for (int i = 0; i < value.Dimension; i++)
      {
        value[i] = left[i] / right;
      }
      return value;
    }
    /// <summary>
    /// このベクトルを表す文字列を返します。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string value = "(";
      for (int i = 0; i < this.Dimension; i++)
      {
        value += this[i];
        if (i != this.Dimension - 1) value += ",";
      }
      value += ")";
      return value;
    }
  }
  /// <summary>
  /// 画素を表します。
  /// </summary>
  public struct Pixel
  {
    public static readonly Pixel TrueColor = new Pixel(0);
    public static readonly Pixel FalseColor = new Pixel(1);
    /// <summary>
    /// 正規化された赤色の濃度値を取得または設定します。
    /// </summary>
    public double R
    {
      get; set;
    }
    /// <summary>
    /// 正規化された緑色の濃度値を取得または設定します。
    /// </summary>
    public double G
    {
      get; set;
    }
    /// <summary>
    /// 正規化された青色の濃度値を取得または設定します。
    /// </summary>
    public double B
    {
      get; set;
    }
    /// <summary>
    /// この画素の明るさを取得または設定します。
    /// </summary>
    public double Brightness
    {
      get
      {
        return (this.R + this.G + this.B) / 3;
      }
      set
      {
        this.R = this.G = this.B = value;
      }
    }
    /// <summary>
    /// この画素の2値画素値を取得または設定します。
    /// </summary>
    public bool Binary
    {
      get
      {
        return this == TrueColor;
      }
      set
      {
        this = value ? TrueColor : FalseColor;
      }
    }
    /// <summary>
    /// 指定したSystem.Drawimg.Colorと等価な画素としてインスタンスを初期化します。
    /// </summary>
    /// <param name="c"></param>
    public Pixel(Color c)
    {
      this.R = c.R / (double)byte.MaxValue;
      this.G = c.G / (double)byte.MaxValue;
      this.B = c.B / (double)byte.MaxValue;
    }
    /// <summary>
    /// 指定したRGB値を持つ画素としてインスタンスを初期化します。
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    public Pixel(double r, double g, double b)
    {
      this.R = r;
      this.G = g;
      this.B = b;
    }
    /// <summary>
    /// 指定した明るさを持つ画素としてインスタンスを初期化します。
    /// </summary>
    /// <param name="brightness"></param>
    public Pixel(double brightness)
    {
      this.R = this.G = this.B = brightness;
    }
    /// <summary>
    /// 指定した2値画素値を持つ画素としてインスタンスを初期化します。
    /// </summary>
    /// <param name="binary"></param>
    public Pixel(bool binary)
    {
      this = binary ? TrueColor : FalseColor;
    }
    /// <summary>
    /// この画素のRGB値を正規化したもの(各濃度値を0～1にしたもの)を返します。
    /// </summary>
    /// <returns></returns>
    public Pixel Normalize()
    {
      var value = this;
      //負の濃度値があれば、0にする
      value.R = Math.Max(0, value.R);
      value.G = Math.Max(0, value.G);
      value.B = Math.Max(0, value.B);
      //1より大きい濃度値があれば、最大値が1になるように各濃度値を正規化する
      if (value.R > 1 || value.G > 1 || value.B > 1)
      {
        double max = Math.Max(value.R, Math.Max(value.G, value.B));
        value.R /= max;
        value.G /= max;
        value.B /= max;
      }
      return value;
    }
    /// <summary>
    /// 指定された2色の各成分について、2乗和を2で割ったものを返します。
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static Pixel EuclideanDistance(Pixel p1, Pixel p2)
    {
      var r = Math.Sqrt((p1.R * p1.R + p2.R * p2.R) / 2);
      var g = Math.Sqrt((p1.G * p1.G + p2.G * p2.G) / 2);
      var b = Math.Sqrt((p1.B * p1.B + p2.B * p2.B) / 2);
      return new Pixel(r, g, b);
    }
    public static Pixel operator +(Pixel p)
    {
      return p;
    }
    public static Pixel operator -(Pixel p)
    {
      return -1 * p;
    }
    public static Pixel operator +(Pixel left, Pixel right)
    {
      return new Pixel(left.R + right.R, left.G + right.G, left.B + right.B);
    }
    public static Pixel operator -(Pixel left, Pixel right)
    {
      return new Pixel(left.R - right.R, left.G - right.G, left.B - right.B);
    }
    public static Pixel operator *(Pixel left, double right)
    {
      return new Pixel(left.R * right, left.G * right, left.B * right);
    }
    public static Pixel operator *(double left, Pixel right)
    {
      return new Pixel(left * right.R, left * right.G, left * right.B);
    }
    public static Pixel operator /(Pixel left, double right)
    {
      return new Pixel(left.R / right, left.G / right, left.B / right);
    }
    public static bool operator ==(Pixel left, Pixel right)
    {
      return left.R == right.R && left.G == right.G && left.B == right.B;
    }
    public static bool operator !=(Pixel left, Pixel right)
    {
      return left.R != right.R || left.G != right.G || left.B != right.B;
    }
    public static explicit operator Color(Pixel p)
    {
      var c = p.Normalize() * byte.MaxValue;
      return Color.FromArgb((int)c.R, (int)c.G, (int)c.B);
    }
    public static explicit operator bool (Pixel p)
    {
      return p == TrueColor;
    }
    public override bool Equals(object obj)
    {
      return obj is Pixel && this == (Pixel)obj;
    }
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      //RGB値
      sb.AppendFormat("({0},{1},{2})", this.R, this.G, this.B);
      sb.Append("/");
      //明るさ及び2値画素としての真偽値
      sb.AppendFormat("{0}/{1}", this.Brightness, this.Binary);
      return sb.ToString();
    }
  }
  /// <summary>
  /// 画像を表現します。
  /// </summary>
  public class PixelImage
  {
    /// <summary>
    /// 画素の二次元配列として表現されたこの画像を取得または設定します。
    /// </summary>
    public Pixel[,] Pixels
    {
      get; set;
    }
    /// <summary>
    /// この画像の幅を取得します。
    /// </summary>
    public int Width
    {
      get
      {
        return this.Pixels.GetLength(0);
      }
    }
    /// <summary>
    /// この画像の高さを取得します。
    /// </summary>
    public int Height
    {
      get
      {
        return this.Pixels.GetLength(1);
      }
    }
    /// <summary>
    /// 大きさ0の画像としてこのインスタンスを初期化します。
    /// </summary>
    public PixelImage()
    {
      this.Pixels = new Pixel[0, 0];
    }
    /// <summary>
    /// 指定した幅と高さを持つ画像としてこのインスタンスを初期化します。
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public PixelImage(int width, int height)
    {
      this.Pixels = new Pixel[width, height];
    }
    /// <summary>
    /// 指定した画像からCompoundImage型の新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="image"></param>
    public PixelImage(PixelImage image)
    {
      this.Pixels = new Pixel[image.Width, image.Height];
      for (int x = 0; x < image.Width; x++)
      {
        for (int y = 0; y < image.Height; y++)
        {
          this.Pixels[x, y] = image[x, y];
        }
      }
    }
    /// <summary>
    /// 指定したビットマップ画像で新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="bitmap"></param>
    public PixelImage(Bitmap bitmap)
    {
      this.Pixels = new Pixel[bitmap.Width, bitmap.Height];
      for (int x = 0; x < bitmap.Width; x++)
      {
        for (int y = 0; y < bitmap.Height; y++)
        {
          this.Pixels[x, y] = new Pixel(bitmap.GetPixel(x, y));
        }
      }
    }
    /// <summary>
    /// 指定した画像ファイルから新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="filename"></param>
    public PixelImage(string filename)
    {
      Bitmap bitmap = new Bitmap(filename);
      this.Pixels = new Pixel[bitmap.Width, bitmap.Height];
      for (int x = 0; x < bitmap.Width; x++)
      {
        for (int y = 0; y < bitmap.Height; y++)
        {
          this.Pixels[x, y] = new Pixel(bitmap.GetPixel(x, y));
        }
      }
    }
    /// <summary>
    /// 指定した位置の画素値を取得または設定します。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Pixel this[int x, int y]
    {
      get
      {
        return this.Pixels[x, y];
      }
      set
      {
        this.Pixels[x, y] = value;
      }
    }
    /// <summary>
    /// この画像を白黒濃淡画像に変換したものを返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage Monochromize()
    {
      var value = new PixelImage(this.Width, this.Height);
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          value[x, y] = new Pixel(this[x, y].Brightness);
        }
      }
      return value;
    }
    /// <summary>
    /// 指定した閾値を用いてこの画像を2値化して返します。
    /// </summary>
    /// <param name="threhold"></param>
    /// <returns></returns>
    public PixelImage Binarize(double threhold)
    {
      //この画像を白黒濃淡画像に変換
      var value = this.Monochromize();
      //
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          //明るさが閾値に達しているかどうかで2値化する
          value[x, y] = value[x, y].Brightness >= threhold ? Pixel.TrueColor : Pixel.FalseColor;
        }
      }
      return value;
    }
    /// <summary>
    /// P-Tile法でこの画像を2値画像に変換したものを返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage BinarizePTile(double ratio)
    {
      //明度ヒストグラム。明度は256段階とする
      int[] num = new int[byte.MaxValue + 1];
      //処理にはこの画像を白黒濃淡画像に変換したものを使う。
      var temp = this.Monochromize();
      //明度ヒストグラムを作成
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          num[(int)(temp[x, y].Normalize().Brightness * byte.MaxValue)]++;
        }
      }
      //全体面積に対する、TrueColorとなる点の面積の比が引数以上になるなら、その時の閾値で2値化して結果を返す
      for (double threhold = 0; threhold < 1; threhold += 1.0 / (byte.MaxValue + 1))
      {
        var a = num.Take((int)((threhold + 1.0 / (byte.MaxValue + 1)) * num.Length)).Sum();
        var b = num.Take((int)((threhold + 1.0 / (byte.MaxValue + 1)) * num.Length)).Sum() / (double)(this.Width * this.Height);
        if (num.Take((int)(threhold * num.Length)).Sum() / (double)(this.Width * this.Height) >= 1 - ratio)
        {
          return this.Binarize(threhold);
        }
      }
      return this.Binarize(1);
    }
    /// <summary>
    /// この画像と等価なBitmapインスタンスを返します。
    /// </summary>
    /// <returns></returns>
    public Bitmap ToBitmap()
    {
      Bitmap value = new Bitmap(this.Width, this.Height);
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          value.SetPixel(x, y, (Color)this[x, y]);
        }
      }
      return value;
    }
    /// <summary>
    /// この画像をSobelのエッジ検出フィルタにかけたときの結果を返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage SobelEdge()
    {
      var value = new PixelImage(this.Width, this.Height);
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          //近傍画素を抽出
          var sur = this.GetSurroundings(x, y, 3);
          //x,y方向それぞれの画素の重み付き総和を計算
          var difx = -sur[-1, -1] - 2 * sur[-1, 0] - sur[-1, 1] +
            sur[1, -1] + 2 * sur[1, 0] + sur[1, 1];
          var dify = -sur[-1, -1] - 2 * sur[0, -1] - sur[1, -1] +
            sur[-1, 1] + 2 * sur[0, 1] + sur[1, 1];
          //画素値を決定
          value[x, y] = Pixel.EuclideanDistance(difx, dify);
        }
      }
      return value;
    }
    /// <summary>
    /// この2値画像中の図形を指定した画素分広げたものを返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage Expand(int count)
    {
      PixelImage value = count > 1 ? this.Expand(count - 1) : new PixelImage(this.Width, this.Height);
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          //近傍画素を取得
          var surroundings = this.GetSurroundings(x, y, 3);
          //近傍画素のうち、一つでも真の画素があれば、注目画素を真にする
          value[x, y] = surroundings.ToList().Any(p => p == Pixel.TrueColor) ? Pixel.TrueColor : Pixel.FalseColor;
        }
      }
      return value;
    }
    /// <summary>
    /// この2値画像中の図形を指定した画素分縮小したものを返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage Reduce(int count)
    {
      PixelImage value = count > 1 ? this.Reduce(count - 1) : new PixelImage(this.Width, this.Height);
      for (int x = 0; x < this.Width; x++)
      {
        for (int y = 0; y < this.Height; y++)
        {
          //近傍画素を取得
          var surroundings = this.GetSurroundings(x, y, 3);
          //近傍画素のうち、一つでも偽の画素があれば、注目画素を偽にする
          value[x, y] = surroundings.ToList().Any(p => p == Pixel.FalseColor) ? Pixel.FalseColor : Pixel.TrueColor;
        }
      }
      return value;
    }
    /// <summary>
    /// この画像に細線化処理を施したものを返します。
    /// </summary>
    /// <returns></returns>
    public PixelImage Thin()
    {
      PixelImage value = new PixelImage(this);
      PixelImage temp = new PixelImage(this);
      int RemovedCount;
      do
      {
        RemovedCount = 0;
        for (int x = 0; x < this.Width; x++)
        {
          for (int y = 0; y < this.Height; y++)
          {
            //もし注目画素が消去可能なら消去し、消去数をカウント。
            //ただし、近傍画素が一つだけ真なら消去しないでおく
            if (temp.IsDeletable(x, y) && this.GetSurroundings(x, y, 3).ToList().Count(c => c == Pixel.TrueColor) != 2)
            {
              //value[x, y] = Pixel.FalseColor;
              temp[x, y] = Pixel.FalseColor;
              RemovedCount++;
            }
          }
        }
        //結果をコピー
        //temp = new CompoundImage(value);
        Console.WriteLine("{0} pixels were removed", RemovedCount);
        //消去可能な点がなくなるまで行う
      } while (RemovedCount != 0);
      return temp;
    }
    /// <summary>
    /// 指定した座標を中心に、画像から指定した大きさの領域を切り出して返します。
    /// </summary>
    /// <param name="CenterX"></param>
    /// <param name="CenterY"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private Correspond<Pixel> GetSurroundings(int CenterX, int CenterY, int size)
    {
      var value = new Correspond<Pixel>();
      for (int x = -size / 2; x <= size / 2; x++)
      {
        for (int y = -size / 2; y <= size / 2; y++)
        {
          int SourceX = CenterX + x;
          int SourceY = CenterY + y;
          if (SourceX < 0) SourceX = 0;
          else if (SourceX >= this.Width) SourceX = this.Width - 1;
          if (SourceY < 0) SourceY = 0;
          else if (SourceY >= this.Height) SourceY = this.Height - 1;
          value[x, y] = this[SourceX, SourceY];
        }
      }
      return value;
    }
    /// <summary>
    /// 指定した位置の点が除去可能であるか8連結の場合で返します。
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsDeletable(int x, int y)
    {
      //偽の画素は調べなくて良い
      if (!(bool)this[x, y]) return false;
      //近傍画素を取得
      var sur = this.GetSurroundings(x, y, 3);
      //輪郭線追跡と同じ順に画素を並べ、計算のために整数のリストに変形
      var arr = new List<Pixel>()
      {
        sur[0,0], sur[1,0], sur[1,-1],
          sur[0,-1], sur[-1,-1], sur[-1,0],
          sur[-1,1], sur[0,1], sur[1,1],
      }.ConvertAll(p => (bool)p ? 0 : 1);
      arr.Add(arr[1]);
      //連結数を計算
      int num = 0;
      for (int i = 1; i < 8; i += 2)
      {
        num += arr[i] - arr[i] * arr[i + 1] * arr[i + 2];
      }
      //
      return num == 1;
    }
  }
  /// <summary>
  /// ニューラルネットワークを表します。
  /// </summary>
  public class NeuralNetwork
  {
    public class Teacher
    {
      public Vector Input;
      public Vector CorrectOutput;
      public Teacher()
      {
        this.Input = new Vector();
        this.CorrectOutput = new Vector();
      }
      public Teacher(Vector input, Vector output)
      {
        this.Input = input;
        this.CorrectOutput = output;
      }
    }
    /// <summary>
    /// ニューラルネットワークの構成要素であるニューロン層を定義します。
    /// </summary>
    private class NeuronLayer
    {
      /// <summary>
      /// この層に含まれるニューロンの数を取得します。
      /// </summary>
      public int Count
      {
        get
        {
          return this.Inputs.Dimension;
        }
      }
      /// <summary>
      /// Inputs[i]は、i番目のニューロンへの入力値を表します。
      /// </summary>
      public Vector Inputs;
      /// <summary>
      /// Inputs[i]は、i番目のニューロンへの出力値を表します。
      /// </summary>
      public Vector Outputs;
      /// <summary>
      /// Inputs[i]は、i番目のニューロンへのバイアス値を表します。
      /// </summary>
      public Vector Biases;
      /// <summary>
      /// 指定した数のニューロンを持つ新しい層を初期化します。
      /// </summary>
      /// <param name="num"></param>
      public NeuronLayer(int num)
      {
        this.Inputs = new Vector(num);
        this.Outputs = new Vector(num);
        this.Biases = new Vector(num);
      }
    }
    /// <summary>
    /// ニューロンの活性化関数。
    /// </summary>
    public enum ActivationFunction
    {
      /// <summary>
      /// シグモイド。
      /// </summary>
      Sigmoid,
      /// <summary>
      /// ハイパボリックタンジェント。
      /// </summary>
      HyperbolicTangent,
      /// <summary>
      /// ソフトサイン。
      /// </summary>
      Softsign,
      /// <summary>
      /// ソフトプラス。
      /// </summary>
      Softplus,
      /// <summary>
      /// ランプ関数。
      /// </summary>
      ReLU,
      /// <summary>
      /// leaky rectified linear。
      /// </summary>
      LReL,
    }
    /// <summary>
    /// layers[i]は、第i層のニューロンを表します。
    /// </summary>
    private List<NeuronLayer> layers;
    /// <summary>
    /// ニューロン間の伝達重み。weights[i][j,k]は、i-1層k番目のニューロンから、i層j番目のニューロンへの伝達重みを表します。
    /// </summary>
    private List<Matrix> weights;
    /// <summary>
    /// ニューロンの活性化関数の種類を取得または設定します。
    /// </summary>
    public ActivationFunction ActivationKind
    {
      get; set;
    }
    /// <summary>
    /// 学習に使用する乱数系列を取得または設定します。
    /// </summary>
    public Random Random
    {
      get; set;
    }
    /// <summary>
    /// 確率的勾配降下法を用いて学習を行うときのミニバッチの要素数を取得または設定します。
    /// </summary>
    public int BatchNum
    {
      get; set;
    }
    /// <summary>
    /// ニューロンの学習率を取得または設定します。
    /// </summary>
    public double LearnRatio
    {
      get; set;
    }
    /// <summary>
    /// 学習時のL1正規化項の寄与度を取得または設定します。
    /// </summary>
    public double L1Normalization
    {
      get; set;
    }
    /// <summary>
    /// 学習時のL2正規化項の寄与度を取得または設定します。
    /// </summary>
    public double L2Normalization
    {
      get; set;
    }
    /// <summary>
    /// 既定の状態でニューラルネットワークを初期化します。
    /// </summary>
    public NeuralNetwork()
    {
      this.layers = new List<NeuronLayer>();
      this.weights = new List<Matrix>();
      this.ActivationKind = ActivationFunction.HyperbolicTangent;
      this.Random = new Random((int)DateTime.Now.Ticks);
      this.BatchNum = 1;
      this.LearnRatio = 1;
      this.L1Normalization = 0;
      this.L2Normalization = 0;
    }
    /// <summary>
    /// 指定した大きさを持つ層を作成します。
    /// </summary>
    /// <param name="LayerNums"></param>
    public void CreateLayers(params int[] LayerNums)
    {
      if (LayerNums == null)
      {
        throw new ArgumentNullException("LayerNums");
      }
      if (LayerNums.Length < 2)
      {
        throw new ArgumentException("層の数が少なすぎます。");
      }
      //ニューロン層を初期化
      this.layers.Clear();
      foreach (var num in LayerNums)
      {
        this.layers.Add(new NeuronLayer(num));
      }
      //重み行列リストを初期化
      this.weights.Add(new Matrix(0, 0)); //ダミー行列を追加
      for (int i = 1; i < LayerNums.Length; i++)
      {
        this.weights.Add(new Matrix(LayerNums[i], LayerNums[i - 1]));
      }
    }
    /// <summary>
    /// ニューロン間の結合重みを初期化します。
    /// </summary>
    public void InitializeWeight()
    {
      for (int i = 1; i < this.weights.Count; i++)
      {
        int InputNum = this.layers[i - 1].Count;
        for (int to = 0; to < this.weights[i].Line; to++)
        {
          for (int from = 0; from < this.weights[i].Column; from++)
          {
            double w = this.Random.NextGauss(0, 1.0 / InputNum);
            this.weights[i][to, from] = w;
          }
        }
      }
    }
    /// <summary>
    /// ニューロンのバイアスを初期化します。
    /// </summary>
    public void InitalizeBias()
    {
      //1層から始めるのは、最初の層(入力層)がバイアスを持たないため
      for (int layer = 1; layer < this.layers.Count; layer++)
      {
        for (int i = 0; i < this.layers[layer].Count; i++)
        {
          this.layers[layer].Biases[i] = this.Random.NextGauss(0, 1);
        }
      }
    }
    /// <summary>
    /// 指定した教師データに確率的勾配降下法をして、ネットワークの学習を1エポック分行います。
    /// </summary>
    /// <param name="teachers"></param>
    /// <param name="Dropout"></param>
    public void LearnEpoch(List<Teacher> teachers)
    {
      //シャッフル
      var TempTeachers = new List<Teacher>(teachers);
      TempTeachers.Sort((t1, t2) =>
      {
        int r = this.Random.Next(3);
        if (r == 0) return -1;
        else if (r < 0) return 1;
        else return 0;
      });
      //ソートしたデータが無くなるまで繰り返す
      while (TempTeachers.Count > 0)
      {
        //ミニバッチの数分だけ要素を取り出す
        var batch = TempTeachers.Take(this.BatchNum).ToList();
        //修正
        this.GradientDescent(batch);
        //使った分を削除
        TempTeachers.RemoveRange(0, Math.Min(TempTeachers.Count, this.BatchNum));
      }
    }
    /// <summary>
    /// 指定した入力に対するこのネットワークの出力を返します。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Vector GetOutput(Vector input)
    {
      this.SetVectors(input);
      return this.layers.Last().Outputs;
    }
    /// <summary>
    /// このニューラルネットワークを指定したファイルに保存します。
    /// </summary>
    /// <param name="filename"></param>
    public void Save(string filename)
    {
      List<byte> data = new List<byte>();
      //活性化関数の種類をint型で記録
      data.AddRange(BitConverter.GetBytes((int)this.ActivationKind));
      //層の数をint型で記録
      data.AddRange(BitConverter.GetBytes(this.layers.Count));
      //各層のニューロン数をint型で記録
      for (int i = 0; i < this.layers.Count; i++)
      {
        data.AddRange(BitConverter.GetBytes(this.layers[i].Count));
      }
      //各ニューロンのバイアスをdouble型で記録
      for (int i = 0; i < this.layers.Count; i++)
      {
        for (int j = 0; j < this.layers[i].Count; j++)
        {
          data.AddRange(BitConverter.GetBytes(this.layers[i].Biases[j]));
        }
      }
      //重みをdouble型で記録
      for (int i = 0; i < this.weights.Count; i++)
      {
        for (int j = 0; j < this.weights[i].Line; j++)
        {
          for (int k = 0; k < this.weights[i].Column; k++)
          {
            data.AddRange(BitConverter.GetBytes(this.weights[i][j, k]));
          }
        }
      }
      //ファイルを新規作成モードで開く
      using (FileStream fs = new FileStream(filename, FileMode.Create))
      {
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
          //データをファイルに書き込む
          bw.Write(data.ToArray());
        }
      }
    }
    /// <summary>
    /// 指定したファイルからニューラルネットワークを作成します。
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static NeuralNetwork FromFile(string filename)
    {
      using (FileStream fs = new FileStream(filename, FileMode.Open))
      {
        return FromStream(fs);
      }
      /*
      NeuralNetwork nn = new NeuralNetwork();
      //ファイルを読み込みモードで開く
      using (FileStream fs = new FileStream(filename, FileMode.Open))
      {
        using (BinaryReader br = new BinaryReader(fs))
        {
          //層の数を読み込む
          int num = br.ReadInt32();
          int[] LayerNums = new int[num];
          //各層のニューロン数を読み込む
          for (int i = 0; i < num; i++)
          {
            LayerNums[i] = br.ReadInt32();
          }
          //層を作成
          nn.CreateLayers(LayerNums);
          //各ニューロンのバイアスを読み込む
          for (int i = 0; i < num; i++)
          {
            for (int j = 0; j < LayerNums[i]; j++)
            {
              nn.layers[i].Biases[j] = br.ReadDouble();
            }
          }
          //重みを読み込む
          for (int i = 0; i < nn.weights.Count; i++)
          {
            for (int j = 0; j < nn.weights[i].Line; j++)
            {
              for (int k = 0; k < nn.weights[i].Column; k++)
              {
                nn.weights[i][j, k] = br.ReadDouble();
              }
            }
          }
        }
      }
      return nn;
      */
    }
    /// <summary>
    /// 指定したストリームからニューラルネットワークを作成します。
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static NeuralNetwork FromStream(Stream stream)
    {
      NeuralNetwork nn = new NeuralNetwork();
      using (BinaryReader br = new BinaryReader(stream))
      {
        //活性化関数の種類を読み込む
        nn.ActivationKind = (ActivationFunction)br.ReadInt32();
        //層の数を読み込む
        int num = br.ReadInt32();
        int[] LayerNums = new int[num];
        //各層のニューロン数を読み込む
        for (int i = 0; i < num; i++)
        {
          LayerNums[i] = br.ReadInt32();
        }
        //層を作成
        nn.CreateLayers(LayerNums);
        //各ニューロンのバイアスを読み込む
        for (int i = 0; i < num; i++)
        {
          for (int j = 0; j < LayerNums[i]; j++)
          {
            nn.layers[i].Biases[j] = br.ReadDouble();
          }
        }
        //重みを読み込む
        for (int i = 0; i < nn.weights.Count; i++)
        {
          for (int j = 0; j < nn.weights[i].Line; j++)
          {
            for (int k = 0; k < nn.weights[i].Column; k++)
            {
              nn.weights[i][j, k] = br.ReadDouble();
            }
          }
        }
      }
      return nn;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="teachers"></param>
    /// <returns></returns>
    public int CorrectNum(List<Teacher> teachers)
    {
      int num = 0;
      foreach (var t in teachers)
      {
        var output = this.GetOutput(t.Input);
        if (output.Elements.ToList().IndexOf(output.Elements.Max()) == t.CorrectOutput.Elements.ToList().IndexOf(t.CorrectOutput.Elements.Max()))
        {
          num++;
        }
      }
      return num;
    }
    /// <summary>
    /// 2乗コスト関数を返します。
    /// </summary>
    /// <param name="teachers"></param>
    /// <returns></returns>
    public double SqureCost(List<Teacher> teachers)
    {
      double cost = 0;
      foreach (var t in teachers)
      {
        this.SetVectors(t.Input);
        for (int i = 0; i < t.CorrectOutput.Dimension; i++)
        {
          cost += (this.layers.Last().Outputs[i] - t.CorrectOutput[i])
            * (this.layers.Last().Outputs[i] - t.CorrectOutput[i]) / 2;
        }
      }
      return cost;
    }
    /// <summary>
    /// 指定した入力をネットワークに与えたときの、各ニューロンの入出力を格納します。
    /// </summary>
    /// <param name="input"></param>
    private void SetVectors(Vector input)
    {
      if (this.layers[0].Inputs.Dimension != input.Dimension)
      {
        throw new ArgumentException("渡されたベクトルの次元が正しくありません。");
      }
      this.layers[0].Inputs = input;
      for (int i = 0; i < this.layers.Count - 1; i++)
      {
        //出力とバイアスから次の層の入力を計算
        this.ActivateLayer(this.layers[i]);
        this.layers[i + 1].Inputs =
          this.weights[i + 1] * this.layers[i].Outputs
          + this.layers[i + 1].Biases;
      }
      //最後の層の出力を計算
      this.ActivateLayer(this.layers[this.layers.Count - 1]);
    }
    /// <summary>
    /// 指定した正しい出力から、各ニューロンの誤差を返します。
    /// </summary>
    /// <remarks>このメソッドを使用する前に、ネットワークの全ニューロンの入出力が計算済みである必要があります。</remarks>
    /// <param name="input"></param>
    /// <param name="CorrectOutput"></param>
    /// <returns></returns>
    private List<Vector> GetError(Vector CorrectOutput)
    {
      List<Vector> errors = new List<Vector>();
      foreach (var layer in this.layers)
      {
        errors.Add(new Vector(layer.Count));
      }
      //出力層の誤差を計算
      errors[errors.Count - 1] = this.layers.Last().Outputs - CorrectOutput;
      //誤差逆伝播
      for (int i = errors.Count - 2; i >= 0; i--)
      {
        errors[i] = Vector.Hadamard(
          this.weights[i + 1].Transpose() * errors[i + 1],
          this.GetActivationDifferences(this.layers[i].Inputs));
      }
      return errors;
    }
    /// <summary>
    /// 最急降下法によって伝達重みとバイアスを修正します。
    /// </summary>
    /// <param name="errors"></param>
    private void GradientDescent(List<Teacher> teachers)
    {
      //誤差ベクトルリストを作成
      List<Vector> ErrorSum = new List<Vector>();
      foreach (var layer in this.layers)
      {
        ErrorSum.Add(new Vector(layer.Count));
      }
      //誤差の合計を計算
      for (int i = 0; i < teachers.Count; i++)
      {
        this.SetVectors(teachers[i].Input);
        var error = this.GetError(teachers[i].CorrectOutput);
        for (int j = 0; j < error.Count; j++)
        {
          ErrorSum[j] += error[j];
        }
      }
      //重みの修正
      for (int i = 1; i < this.layers.Count; i++)
      {
        for (int j = 0; j < this.layers[i].Count; j++)
        {
          for (int k = 0; k < this.layers[i - 1].Count; k++)
          {
            this.weights[i][j, k] =
              (1 - this.LearnRatio * this.L2Normalization / this.BatchNum) * this.weights[i][j, k]
              - this.LearnRatio * this.layers[i - 1].Outputs[k] * ErrorSum[i][j] / this.BatchNum
              - this.LearnRatio * this.L1Normalization * Math.Sign(this.weights[i][j, k]) / this.BatchNum;
          }
        }
      }
      //バイアス修正
      for (int i = 0; i < this.layers.Count; i++)
      {
        for (int j = 0; j < this.layers[i].Count; j++)
        {
          this.layers[i].Biases[j] =
            this.layers[i].Biases[j]
            - this.LearnRatio * ErrorSum[i][j] / this.BatchNum;
        }
      }
    }
    /// <summary>
    /// 指定した層の活度を計算します。
    /// </summary>
    /// <param name="layer">入力値が既知の層。</param>
    private void ActivateLayer(NeuronLayer layer)
    {
      layer.Outputs = this.GetActivations(layer.Inputs);
    }
    /// <summary>
    /// ニューロンの活性化関数。
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private double GetActivation(double x)
    {
      double value;
      switch (this.ActivationKind)
      {
        case ActivationFunction.Sigmoid:
          value = 1 / (1 + Math.Exp(-x));
          break;
        case ActivationFunction.HyperbolicTangent:
          value = Math.Tanh(x);
          break;
        case ActivationFunction.Softsign:
          value = x / (1 + Math.Abs(x));
          break;
        case ActivationFunction.Softplus:
          value = Math.Log(1 + Math.Exp(x));
          break;
        case ActivationFunction.ReLU:
          value = x > 0 ? x : 0;
          break;
        case ActivationFunction.LReL:
          value = x > 0 ? x : 0.01 * x;
          break;
        default:
          throw new InvalidOperationException(this.ActivationKind.ToString());
      }
      return value;
    }
    private Vector GetActivations(Vector v)
    {
      Vector value = new Vector(v.Dimension);
      for (int i = 0; i < v.Dimension; i++)
      {
        value[i] = this.GetActivation(v[i]);
      }
      return value;
    }
    private double GetActivationDifference(double x)
    {
      double temp;
      double value;
      switch (this.ActivationKind)
      {
        case ActivationFunction.Sigmoid:
          temp = Math.Exp(-x);
          value = temp / ((1 + temp) * (1 + temp));
          break;
        case ActivationFunction.HyperbolicTangent:
          temp = Math.Tanh(x);
          value = 1 - temp * temp;
          break;
        case ActivationFunction.Softsign:
          temp = Math.Abs(x);
          value = (1 + temp - x * Math.Sign(x)) / ((1 + temp) * (1 + temp));
          break;
        case ActivationFunction.Softplus:
          temp = Math.Exp(x);
          value = temp / (1 + temp);
          break;
        case ActivationFunction.ReLU:
          value = x > 0 ? 1 : 0;
          break;
        case ActivationFunction.LReL:
          value = x > 0 ? 1 : 0.01;
          break;
        default:
          throw new InvalidOperationException(this.ActivationKind.ToString());
      }
      return value;
    }
    private Vector GetActivationDifferences(Vector v)
    {
      Vector value = new Vector(v.Dimension);
      for (int i = 0; i < v.Dimension; i++)
      {
        value[i] = this.GetActivationDifference(v[i]);
      }
      return value;
    }
  }
  /// <summary>
  /// 
  /// </summary>
  public class AsciiArtMaker
  {
    private class LearnTeacher : NeuralNetwork.Teacher
    {
      public char C;
      public int Width;
    }
    private static readonly Size candidateImageSize = new Size(16, 16);
    private static readonly Encoding fileEncoding = Encoding.GetEncoding("unicode");
    private List<char> candidates;
    private List<LearnTeacher> teachers;
    private NeuralNetwork nn;
    public AsciiArtMaker()
    {
      this.candidates = new List<char>();
      this.teachers = new List<LearnTeacher>();
      this.nn = new NeuralNetwork()
      {
        ActivationKind = NeuralNetwork.ActivationFunction.HyperbolicTangent,
        BatchNum = 1,
        LearnRatio = 0.005,
        L1Normalization = 0,
        L2Normalization = 0,
        Random = new Random((int)DateTime.Now.Ticks)
      };
    }
    public void LoadCandidate(string directory)
    {
      //以前のデータを消去
      this.candidates.Clear();
      this.teachers.Clear();
      //子ディレクトリを取得
      var children = Directory.EnumerateDirectories(directory).ToList();
      //各子ディレクトリの中身を調べる
      foreach (string child in children)
      {
        char c;
        int width;
        //テキストファイルを開き、文字と文字幅を読み込む
        using (StreamReader sr = new StreamReader(Directory.EnumerateFiles(child).First(name => name.ToLower().Contains(".txt")), Encoding.GetEncoding("unicode")))
        {
          c = sr.ReadLine()[0];
          width = int.Parse(sr.ReadLine());
          //候補文字をリストに追加
          this.candidates.Add(c);
        }
        //子ディレクトリ内のビットマップファイルを読み込んでいく
        foreach (string imagefilename in Directory.EnumerateFiles(child).Where(name => name.ToLower().Contains(".bmp")))
        {
          this.teachers.AddRange(this.ToTeachers(c, width, children.Count, new PixelImage(imagefilename)));
        }
      }
    }
    private List<LearnTeacher> ToTeachers(char c, int width, int OutputDimension, PixelImage PrimitiveImage)
    {
      List<LearnTeacher> teachers = new List<LearnTeacher>();
      //元画像と、元画像を1ピクセルだけ平行移動した画像を配列に
      PixelImage[] images = new PixelImage[5];
      int[] TrueNums = new int[images.Length];
      for (int i = 0; i < images.Length; i++)
      {
        images[i] = new PixelImage(PrimitiveImage.Width, PrimitiveImage.Height);
      }
      for (int x = 0; x < PrimitiveImage.Width; x++)
      {
        for (int y = 0; y < PrimitiveImage.Height; y++)
        {
          images[0][x, y] = PrimitiveImage[x, y];
          images[1][x, y] = x + 1 >= PrimitiveImage.Width ? Pixel.FalseColor : PrimitiveImage[x + 1, y];
          images[2][x, y] = x - 1 < 0 ? Pixel.FalseColor : PrimitiveImage[x - 1, y];
          images[3][x, y] = y + 1 >= PrimitiveImage.Height ? Pixel.FalseColor : PrimitiveImage[x, y + 1];
          images[4][x, y] = y - 1 < 0 ? Pixel.FalseColor : PrimitiveImage[x, y - 1];
          //画像の真画素の数を取得
          for (int j = 0; j < images.Length; j++)
          {
            if ((bool)images[j][x, y])
            {
              TrueNums[j]++;
            }
          }
        }
      }
      for (int i = 0; i < images.Length; i++)
      {
        //元画像と真画素の数が異なるものは教師データに使用しない
        if (TrueNums[i] != TrueNums[0]) continue;
        LearnTeacher teacher = new LearnTeacher();
        teacher.C = c;
        teacher.Width = width;
        teacher.Input = ToInput(images[i], new Point(0, 0));
        teacher.CorrectOutput = new Vector(OutputDimension);
        for (int j = 0; j < OutputDimension; j++)
        {
          teacher.CorrectOutput[j] = j == this.candidates.IndexOf(c) ? 1 : 0;
        }
        //教師データを追加
        teachers.Add(teacher);
      }
      return teachers;
    }
    public void LoadNetwork(string filename)
    {
      this.nn = NeuralNetwork.FromFile(filename);
    }
    public void LearnNetwork(string OutputDirectory, NeuralNetwork.ActivationFunction activation)
    {
      //ネットワーク学習の準備
      this.nn.ActivationKind = activation;
      this.nn.BatchNum = 1;
      this.nn.LearnRatio = 0.03;
      this.nn.L1Normalization = 0.00;
      this.nn.L2Normalization = 0.00;
      this.nn.CreateLayers(
        this.teachers.First().Input.Dimension,
        this.teachers.First().Input.Dimension * 2 / 3,
        this.teachers.First().CorrectOutput.Dimension);
      this.nn.InitializeWeight();
      this.nn.InitalizeBias();
      //学習
      int step = 0;
      int MaxStep = 120;
      var t = this.teachers.ConvertAll(teacher => (NeuralNetwork.Teacher)teacher);
      do
      {
        Console.WriteLine(step + "/" + MaxStep);
        this.nn.LearnEpoch(t);
        #region test code
        if (step % 10 == 0)
        {
          if (step > 0)
          {
            this.nn.Save(OutputDirectory + "\\network" + step + ".dat");
          }
          Console.WriteLine("step{0} square{1} correct:{2}/{3}",
            step, this.nn.SqureCost(t),
            this.nn.CorrectNum(t), this.teachers.Count);
        }
        #endregion
      } while (step++ < MaxStep);
    }
    public string ToHtmlAsciiArt(PixelImage image)
    {
      string value = this.ToAsciiArt(image);
      string htmlBegin = "<html>\n<head>\n";
      htmlBegin += "<meta http-equiv=\"Content - Type\" content=\"text / html; charset = unicode\">\n";
      htmlBegin += "<title>AA</title>\n</head>\n<body>\n";
      htmlBegin += "<font size=\"3\" face=\"MS PGothic\">";
      string htmlEnd = "\n</font>\n</body>\n</html>";
      return htmlBegin + "<br>" + value.Replace("\n", "<br>") + htmlEnd;
    }
    public string ToAsciiArt(PixelImage image)
    {
      string value = "";
      Point point = new Point(0, 0);
      while (point.Y < image.Height)
      {
        var cs = this.ToCharacters(image, point);
        char c = cs[0];
        //現在の位置が行の先頭の場合、半角スペースが選択されないようにする
        if (point.X == 0 && c == ' ')
        {
          c = cs[1];
        }
        //現在の最有力候補と、直前の文字が半角スペースの場合、直前の文字を消して全角スペースを追加する
        if (value.Length > 0 && value.Last() == ' ' && c == ' ')
        {
          value = value.Remove(value.Length - 1, 1);
          point.X -= this.WidthOf(' ');
          c = '　';
        }
        /*
        //もし、直前の文字が半角スペースか、現在位置が行の先頭なら、半角スペースは用いない
        if (c == ' ' && (value.Length == 0 || point.X == 0 || value.Last() == ' '))
        {
          c = cs[1];
        }
        */
        /*
        char c = this.ToCharacter(image, point);
        int width = this.teachers.Find(t => t.C == c).Width;
        //可能なら、全角スペースではなく半角スペースにする
        if (c == '　' && value.Length > 1 && value.Last() != ' ' && value.Last() != '\n')
        {
          value += ' ';
          width = 5;
        }
        else
        {
          value += c;
        }
        */
        //更新
        value += c;
        point.X += this.WidthOf(c);
        if (point.X >= image.Width)
        {
          value += '\n';
          point.X = 0;
          point.Y += AsciiArtMaker.candidateImageSize.Height;
        }
      }
      return value;
    }
    private int WidthOf(char c)
    {
      if (c == '　')
      {
        return 11;
      }
      return this.teachers.Find(t => t.C == c).Width;
    }
    /// <summary>
    /// 候補文字を、指定した画像への合致度順に並び替えて返します。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private List<char> ToCharacters(PixelImage image, Point point)
    {
      Vector input = ToInput(image, point);
      var output = this.nn.GetOutput(input);
      var q = from c in this.candidates
              orderby output[this.candidates.IndexOf(c)] descending
              select c;
      return q.ToList();
    }
    private char ToCharacter(PixelImage image, Point point)
    {
      Vector input = ToInput(image, point);
      var output = this.nn.GetOutput(input);
      return this.candidates[output.Elements.ToList().IndexOf(output.Elements.Max())];
    }
    private static Vector ToInput(PixelImage image, Point point)
    {
      Vector input = new Vector(candidateImageSize.Width * candidateImageSize.Height);
      for (int x = 0; x < candidateImageSize.Width; x++)
      {
        for (int y = 0; y < candidateImageSize.Height; y++)
        {
          bool b;
          if (point.X + x >= image.Width || point.Y + y >= image.Height) b = false;
          else b = (bool)image[point.X + x, point.Y + y];
          input[x * candidateImageSize.Height + y] = b ? 1 : 0;
        }
      }
      return input;
    }
  }
  class Program
  {
    static void Main(string[] args)
    {
      AsciiArtization();
    }
    static void AsciiArtization()
    {
      AsciiArtMaker aam = new AsciiArtMaker();
      while (true)
      {
        Console.Write("input command:");
        string[] cmd = Console.ReadLine().Split(' ');
        try
        {
          switch (cmd[0].ToLower())
          {
            case "exit":
              goto ProgramEnd;
            case "candidate":
              aam.LoadCandidate(cmd[1]);
              Console.WriteLine("succeeded to load candidates {0}", cmd[1]);
              break;
            case "network":
              aam.LoadNetwork(cmd[1]);
              Console.WriteLine("succeeded to load network {0}", cmd[1]);
              break;
            case "learn":
              aam.LearnNetwork(cmd[1], NeuralNetwork.ActivationFunction.ReLU);
              Console.WriteLine("succeeded to learn network {0}", cmd[1]);
              break;
            case "aa":
              var filename = Path.GetFileNameWithoutExtension(cmd[1]) + ".html";
              var directory = Path.GetDirectoryName(cmd[1]);
              using (StreamWriter sw = new StreamWriter(directory + "//" + filename))
              {
                var s = aam.ToAsciiArt(new PixelImage(cmd[1]));
                sw.Write(s);
              }
              Console.WriteLine("succeeded to save " + filename);
              break;
            case "html":
              var _filename = Path.GetFileNameWithoutExtension(cmd[1]) + ".html";
              var _directory = Path.GetDirectoryName(cmd[1]);
              using (StreamWriter sw = new StreamWriter(_directory + "//" + _filename, false, Encoding.GetEncoding("unicode")))
              {
                var s = aam.ToHtmlAsciiArt(new PixelImage(cmd[1]));
                sw.Write(s);
              }
              Console.WriteLine("succeeded to save " + _filename);
              break;
            default:
              Console.WriteLine("invalid command:{0}", cmd[0]);
              break;
          }
        }
        catch
        {
          Console.WriteLine("failed");
        }
      }
      ProgramEnd:
      return;
    }
  }
}
