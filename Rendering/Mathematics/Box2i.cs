// Copyright (c) Open Toolkit library.
// This file is subject to the terms and conditions defined in
// file 'License.txt', which is part of this source code package.
using System;
using System.Runtime.InteropServices;
namespace Materia.Rendering.Mathematics
{
    /// <summary>
    /// Defines a 2d box (rectangle).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Box2i : IEquatable<Box2i>
    {
        /// <summary>
        /// The left boundary of the structure.
        /// </summary>
        public int Left;

        /// <summary>
        /// The right boundary of the structure.
        /// </summary>
        public int Right;

        /// <summary>
        /// The top boundary of the structure.
        /// </summary>
        public int Top;

        /// <summary>
        /// The bottom boundary of the structure.
        /// </summary>
        public int Bottom;

        public Box2i(Vector2i pos, int width, int height)
        {
            Left = pos.X;
            Top = pos.Y;
            Right = pos.X + width;
            Bottom = pos.Y + height;
        }

        /// <summary>
        /// Constructs a new Box2i with the specified dimensions.
        /// </summary>
        /// <param name="topLeft">An OpenTK.Vector2i describing the top-left corner of the Box2i.</param>
        /// <param name="bottomRight">An OpenTK.Vector2i describing the bottom-right corner of the Box2i.</param>
        public Box2i(Vector2i topLeft, Vector2i bottomRight)
        {
            Left = topLeft.X;
            Top = topLeft.Y;
            Right = bottomRight.X;
            Bottom = bottomRight.Y;
        }

        /// <summary>
        /// Constructs a new Box2i with the specified dimensions.
        /// </summary>
        /// <param name="left">The position of the left boundary.</param>
        /// <param name="top">The position of the top boundary.</param>
        /// <param name="right">The position of the right boundary.</param>
        /// <param name="bottom">The position of the bottom boundary.</param>
        public Box2i(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Creates a new Box2i with the specified dimensions.
        /// </summary>
        /// <param name="top">The position of the top boundary.</param>
        /// <param name="left">The position of the left boundary.</param>
        /// <param name="right">The position of the right boundary.</param>
        /// <param name="bottom">The position of the bottom boundary.</param>
        /// <returns>A new OpenTK.Box2i with the specfied dimensions.</returns>
        public static Box2i FromTLRB(int top, int left, int right, int bottom)
        {
            return new Box2i(left, top, right, bottom);
        }

        /// <summary>
        /// Creates a new Box2i with the specified dimensions.
        /// </summary>
        /// <param name="top">The position of the top boundary.</param>
        /// <param name="left">The position of the left boundary.</param>
        /// <param name="width">The width of the box.</param>
        /// <param name="height">The height of the box.</param>
        /// <returns>A new OpenTK.Box2i with the specfied dimensions.</returns>
        public static Box2i FromDimensions(int left, int top, int width, int height)
        {
            return new Box2i(left, top, left + width, top + height);
        }

        /// <summary>
        /// Creates a new Box2i with the specified dimensions.
        /// </summary>
        /// <param name="position">The position of the top left corner.</param>
        /// <param name="size">The size of the box.</param>
        /// <returns>A new OpenTK.Box2i with the specfied dimensions.</returns>
        public static Box2i FromDimensions(Vector2i position, Vector2i size)
        {
            return FromDimensions(position.X, position.Y, size.X, size.Y);
        }

        /// <summary>
        /// Gets a int describing the width of the Box2i structure.
        /// </summary>
        public int Width { get { return (int)System.Math.Abs(Right - Left); } }

        /// <summary>
        /// Gets a int describing the height of the Box2i structure.
        /// </summary>
        public int Height { get { return (int)System.Math.Abs(Bottom - Top); } }

        /// <summary>
        /// Returns whether the box contains the specified point on the closed region described by this Box2i.
        /// </summary>
        /// <param name="point">The point to query.</param>
        /// <returns>Whether this box contains the point.</returns>
        public bool Contains(Vector2i point, bool inverseY = false)
        {
            if (inverseY)
            {
                return point.X >= Left && point.X <= Right && point.Y <= Top && point.Y >= Bottom;
            }

            return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        }

        public bool Contains(Box2i b)
        {
            return b.Left >= Left && b.Right <= Right
                && b.Top >= Top && b.Bottom <= Bottom;
        }

        public bool Intersects(Box2i b)
        {
            return Left <= b.Right && Right >= b.Left &&
                Bottom >= b.Top && Top <= b.Bottom;
        }

        /// <summary>
        /// Returns a Box2i translated by the given amount.
        /// </summary>
        public Box2i Translated(Vector2i point)
        {
            return new Box2i(Left + point.X, Top + point.Y, Right + point.X, Bottom + point.Y);
        }

        /// <summary>
        /// Translates this Box2i by the given amount.
        /// </summary>
        public void Translate(Vector2i point)
        {
            Left += point.X;
            Right += point.X;
            Top += point.Y;
            Bottom += point.Y;
        }

        public void Encapsulate(Box2i b)
        {
            if (b.Left < Left)
            {
                Left = b.Left;
            }
            if (b.Right > Right)
            {
                Right = b.Right;
            }
            if (b.Top < Top)
            {
                Top = b.Top;
            }
            if (b.Bottom > Bottom)
            {
                Bottom = b.Bottom;
            }
        }

        /// <summary>
        /// Equality comparator.
        /// </summary>
        public static bool operator ==(Box2i left, Box2i right)
        {
            return left.Bottom == right.Bottom && left.Top == right.Top &&
                left.Left == right.Left && left.Right == right.Right;
        }

        /// <summary>
        /// Inequality comparator.
        /// </summary>
        public static bool operator !=(Box2i left, Box2i right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Functional equality comparator.
        /// </summary>
        public bool Equals(Box2i other)
        {
            return this == other;
        }

        /// <summary>
        /// Implements Object.Equals.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Box2i && Equals((Box2i)obj);
        }

        /// <summary>
        /// Gets the hash code for this Box2i.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Bottom.GetHashCode();
                return hashCode;
            }
        }


        private static string listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        /// <summary>
        /// Returns a <see cref="System.String"/> describing the current instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("({0}{4} {1}) - ({2}{4} {3})", Left, Top, Right, Bottom, listSeparator);
        }
    }
}
