using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///  A vector in 3D space
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable 660,661
public struct TSVector
#pragma warning restore 660, 661
{
    /// <summary>
    /// The x-coordinate of this <see cref="Vector"/> structure.
    /// </summary>
    public float X;
    /// <summary>
    /// The y-coordinate of this <see cref="Vector"/> structure.
    /// </summary>
    public float Y;
    /// <summary>
    /// The z-coordinate of this <see cref="Vector"/> structure.
    /// </summary>
    public float Z;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> structure. 
    /// </summary>
    /// <param name="x">The <see cref="X"/> value</param>
    /// <param name="y">The <see cref="Y"/> value</param>
    /// <param name="z">The <see cref="Z"/> value</param>
    public TSVector(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return string.Format("{0}, {1}, {2}", X, Y, Z);
    }

    public static bool operator ==(TSVector left, TSVector right)
    {
        if ((object)left == null && (object)right == null) return true;
        if ((object)left == null || (object)right == null) return false;
        return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
    }

    public static bool operator !=(TSVector left, TSVector right)
    {
        if ((object)left == null && (object)right == null) return false;
        if ((object)left == null || (object)right == null) return true;
        return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
    }

    public static TSVector operator -(TSVector left, TSVector right)
    {
        if ((object)left == null || (object)right == null) return new TSVector();
        return new TSVector(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static TSVector operator +(TSVector left, TSVector right)
    {
        if ((object)left == null || (object)right == null) return new TSVector();
        return new TSVector(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static TSVector operator *(TSVector left, float right)
    {
        if ((object)left == null) return new TSVector();
        return new TSVector(left.X * right, left.Y * right, left.Z * right);
    }

    public static TSVector operator /(TSVector left, float right)
    {
        if ((object)left == null) return new TSVector();
        return new TSVector(left.X / right, left.Y / right, left.Z / right);
    }

    public static float Distance(TSVector a, TSVector b)
    {
        return a.DistanceTo(b);
    }

    public static float DistanceSquared(TSVector a, TSVector b)
    {
        return a.DistanceToSquared(b);
    }

    public float DistanceToSquared(TSVector right)
    {
        if ((object)right == null) return 0f;

        var nX = X - right.X;
        var nY = Y - right.Y;
        var nZ = Z - right.Z;

        return nX * nX + nY * nY + nZ * nZ;
    }

    public float DistanceTo(TSVector right)
    {
        if ((object)right == null) return 0f;
        return (float)System.Math.Sqrt(DistanceToSquared(right));
    }
}
