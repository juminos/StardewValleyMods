using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using FrenshipRings.Toolkit;

namespace FrenshipRings.Toolkit.Extensions
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Finds the Manhattan (taxicab) distance between two vectors.
        /// </summary>
        /// <param name="self">First vector.</param>
        /// <param name="other">Second vector.</param>
        /// <returns>Manhattan distance.</returns>
        public static float ManhattanDistance(this Vector2 self, Vector2 other)
            => Math.Abs(self.X - other.X) + Math.Abs(self.Y - other.Y);

        /// <summary>
        /// Finds the chessboard (Chebyshev) distance between two vectors.
        /// </summary>
        /// <param name="self">First vector.</param>
        /// <param name="other">Second vector.</param>
        /// <returns>Chessboard distance.</returns>
        public static float ChessboardDistance(this Vector2 self, Vector2 other)
            => Math.Max(Math.Abs(self.X - other.X), Math.Abs(self.Y - other.Y));

        /// <summary>
        /// Finds the midpoint between two vectors.
        /// </summary>
        /// <param name="self">First vector.</param>
        /// <param name="other">Second vector.</param>
        /// <returns>Midpoint.</returns>
        public static Vector2 Midpoint(this Vector2 self, Vector2 other)
            => new(self.X + (other.X - self.X) / 2, self.Y + (other.Y - self.Y) / 2);

        /// <summary>
        /// Tries to parse a vector2 from a string.
        /// </summary>
        /// <param name="str">the string.</param>
        /// <param name="vector">out param, the vector or default.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public static bool TryParseVector2(this string str, out Vector2 vector)
            => str.AsSpan().TryParseVector2(out vector);

        /// <summary>
        /// Tries to parse a vector2 from a ReadOnlySpan.
        /// </summary>
        /// <param name="span">the span.</param>
        /// <param name="vector">out param, the vector or default.</param>
        /// <returns>true if successful, false otherwise.</returns>
        public static bool TryParseVector2(this ReadOnlySpan<char> span, out Vector2 vector)
        {
            if (span.Trim().TrySplitOnce(',', out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
                && float.TryParse(first.Trim(), out float x) && float.TryParse(second.Trim(), out float y))
            {
                vector = new Vector2(x, y);
                return true;
            }

            vector = default;
            return false;
        }

        [JetBrains.Annotations.Pure]
        [MethodImpl(TKConstants.Hot)]
        public static bool TrySplitOnce(this ReadOnlySpan<char> str, char? deliminator, out ReadOnlySpan<char> first, out ReadOnlySpan<char> second)
        {
            int idx = deliminator is null ? str.GetIndexOfWhiteSpace() : str.IndexOf(deliminator.Value);

            if (idx < 0)
            {
                first = second = ReadOnlySpan<char>.Empty;
                return false;
            }

            first = str[..idx];
            second = str[(idx + 1)..];
            return true;
        }

        /// <summary>
        /// Gets the index of the next whitespace character.
        /// </summary>
        /// <param name="chars">ReadOnlySpan to search in.</param>
        /// <returns>Index of the whitespace character, or -1 if not found.</returns>
        [JetBrains.Annotations.Pure]
        [MethodImpl(TKConstants.Hot)]
        public static int GetIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsWhiteSpace(chars[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
