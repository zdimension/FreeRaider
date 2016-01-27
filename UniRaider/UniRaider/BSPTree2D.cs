using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider
{
    public class BSPTree2DNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BSPTree2DNode"/> class.
        /// </summary>
        public BSPTree2DNode(uint x, uint y, uint width, uint height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public BSPTree2DNode Left { get; set; }

        public BSPTree2DNode Right { get; set; }

        public bool IsFilled { get; set; }

        /// <summary>
        /// The X coordinate of this node in 2D space.
        /// </summary>
        public uint X { get; set; }

        /// <summary>
        /// The Y coordinate of this node in 2D space.
        /// </summary>
        public uint Y { get; set; }

        /// <summary>
        /// The width of this node in 2D space.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// The height of this node in 2D space.
        /// </summary>
        public uint Height { get; set; }

        public bool IsSplit => Left != null && Right != null;

        public void SplitHorizontally(uint splitLocation)
        {
            Left = new BSPTree2DNode(X, Y, splitLocation, Height);
            Right = new BSPTree2DNode(X + splitLocation, Y, Width - splitLocation, Height);
        }

        public void SplitVertically(uint splitLocation)
        {
            Left = new BSPTree2DNode(X, Y, Width, splitLocation);
            Right = new BSPTree2DNode(X, Y + splitLocation, Width, Height - splitLocation);
        }

        public bool Fitz(uint w, uint h)
        {
            return !IsFilled && w <= Width && h <= Height;
        }
    }
}
