namespace Chess.Shared
{
    public class Move
    {
        public Piece piece;
        public BitBoard to;
        public bool isCapture;
        public Move(Piece piece, BitBoard to)
        {
            this.piece = piece;
            this.to = to;
            this.isCapture = false;
        }
        public Move(Piece piece, BitBoard to, bool isCapture)
        {
            this.piece = piece;
            this.to = to;
            this.isCapture = isCapture;
        }
    }
}
