using System;

namespace ImGuiSharp.Math;

/// <summary>
/// Minimal 4D vector struct used for colours and layout operations.
/// </summary>
public readonly struct Vec4 : IEquatable<Vec4>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vec4"/> struct.
    /// </summary>
    public Vec4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Gets a zero vector.
    /// </summary>
    public static Vec4 Zero => new(0f, 0f, 0f, 0f);

    /// <summary>
    /// Gets the X component.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y component.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets the Z component.
    /// </summary>
    public float Z { get; }

    /// <summary>
    /// Gets the W component.
    /// </summary>
    public float W { get; }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vec4 Lerp(Vec4 start, Vec4 end, float amount)
    {
        amount = System.Math.Clamp(amount, 0f, 1f);
        return new Vec4(
            start.X + ((end.X - start.X) * amount),
            start.Y + ((end.Y - start.Y) * amount),
            start.Z + ((end.Z - start.Z) * amount),
            start.W + ((end.W - start.W) * amount));
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    public static Vec4 operator +(Vec4 left, Vec4 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    public static Vec4 operator -(Vec4 left, Vec4 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);

    /// <summary>
    /// Multiplies the vector by a scalar.
    /// </summary>
    public static Vec4 operator *(Vec4 value, float scalar) => new(value.X * scalar, value.Y * scalar, value.Z * scalar, value.W * scalar);

    /// <inheritdoc />
    public bool Equals(Vec4 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vec4 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
