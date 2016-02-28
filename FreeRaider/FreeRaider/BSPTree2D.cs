namespace FreeRaider
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

        /// <summary>
        /// Split this node along its Y axis (X is split).
        /// </summary>
        /// <param name="splitLocation">Local X coordinate of the split point</param>
        public void SplitHorizontally(uint splitLocation)
        {
            Left = new BSPTree2DNode(X, Y, splitLocation, Height);
            Right = new BSPTree2DNode(X + splitLocation, Y, Width - splitLocation, Height);
        }

        /// <summary>
        /// Split this node along its X axis (Y is split).
        /// </summary>
        /// <param name="splitLocation">Local Y coordinate of the split point</param>
        public void SplitVertically(uint splitLocation)
        {
            Left = new BSPTree2DNode(X, Y, Width, splitLocation);
            Right = new BSPTree2DNode(X, Y + splitLocation, Width, Height - splitLocation);
        }

        public bool Fits(uint w, uint h)
        {
            return !IsFilled && w <= Width && h <= Height;
        }

        /// <summary>
        /// Find a free space in this node or its children
        /// </summary>
        /// <param name="needleWidth">Needle width</param>
        /// <param name="needleHeight">Needle height</param>
        /// <param name="destX">Destination X coordinate</param>
        /// <param name="destY">Destination Y coordinate</param>
        /// <returns></returns>
        public bool FindSpaceFor(uint needleWidth, uint needleHeight, ref uint destX, ref uint destY)
        {
            // Could this possibly fit?
            if (!Fits(needleWidth, needleHeight))
                return false;

            if(IsSplit)
            {
                // This node is already split -> Recurse!
                var found = false;
                if(needleWidth <= Left.Width && needleHeight <= Left.Height)
                {
                    found = Left.FindSpaceFor(needleWidth, needleHeight, ref destX, ref destX);
                }
                if (!found && needleWidth <= Right.Width && needleHeight <= Right.Height)
                {
                    found = Right.FindSpaceFor(needleWidth, needleHeight, ref destX, ref destX);
                }

                // If both children are filled, mark this as filled and discard the children
                if(Left.IsFilled && Right.IsFilled)
                {
                    IsFilled = true;
                    Left = null;
                    Right = null;
                }

                return found;
            }

            // We may split this node
            if(Height == needleHeight && Width == needleWidth)
            {
                // Perfecft match
                IsFilled = true;
                destX = X;
                destY = Y;
                return true;
            }
            else if(Height == needleHeight)
            {
                // Split horizontally
                SplitHorizontally(needleWidth);

                // Height already fits, width fits too now, so this is the result
                Left.IsFilled = true;
                destX = Left.X;
                destY = Left.Y;
                return true;
            }
            else
            {
                // In case of doubt do a vertical split
                SplitVertically(needleHeight);

                // Recurse, because the width may not match
                return Left.FindSpaceFor(needleWidth, needleHeight, ref destX, ref destY);
            }
        }
    }
}
