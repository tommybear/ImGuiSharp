using System;

namespace ImGuiSharp.Math;

/// <summary>
/// Minimal 2D vector implementation for UI layout calculations.
/// </summary>
public readonly struct Vec2 : IEquatable<Vec2>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vec2"/> struct.
    /// </summary>
    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the X component.
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Gets the Y component.
    /// </summary>
    public float Y { get; }

    /// <summary>
    /// Gets a zero vector.
    /// </summary>
    public static Vec2 Zero => new(0f, 0f);

    /// <summary>
    /// Gets a vector of ones.
    /// </summary>
    public static Vec2 One => new(1f, 1f);

    /// <summary>
    /// Gets the squared length of the vector.
    /// </summary>
    public float LengthSquared => (X * X) + (Y * Y);

    /// <summary>
    /// Gets the length of the vector.
    /// </summary>
    public float Length => System.MathF.Sqrt(LengthSquared);

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    public static Vec2 operator +(Vec2 left, Vec2 right) => new(left.X + right.X, left.Y + right.Y);

    /// <summary>
    /// Subtracts two vectors.
    /// </summary>
    public static Vec2 operator -(Vec2 left, Vec2 right) => new(left.X - right.X, left.Y - right.Y);

    /// <summary>
    /// Multiplies the vector by a scalar.
    /// </summary>
    public static Vec2 operator *(Vec2 value, float scalar) => new(value.X * scalar, value.Y * scalar);

    /// <summary>
    /// Divides the vector by a scalar.
    /// </summary>
    public static Vec2 operator /(Vec2 value, float scalar) => new(value.X / scalar, value.Y / scalar);

    /// <summary>
    /// Computes the dot product between two vectors.
    /// </summary>
    public static float Dot(Vec2 left, Vec2 right) => (left.X * right.X) + (left.Y * right.Y);

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vec2 Lerp(Vec2 start, Vec2 end, float amount)
    {
        amount = System.Math.Clamp(amount, 0f, 1f);
        return new Vec2(start.X + ((end.X - start.X) * amount), start.Y + ((end.Y - start.Y) * amount));
    }

    /// <inheritdoc />
    public bool Equals(Vec2 other) => X.Equals(other.X) && Y.Equals(other.Y);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vec2 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc />
    public override string ToString() => $"({X}, {Y})";
}
