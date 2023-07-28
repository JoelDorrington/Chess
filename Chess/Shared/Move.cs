namespace Chess.Shared
{
    public class Move
    {
        public Piece piece;
        public BitBoard from;
        public BitBoard to;
        public bool isCapture;
        public BitBoard? victim;
        public Move(Piece piece, BitBoard to)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = false;
            this.victim = null;
        }
        public Move(Piece piece, BitBoard to, bool isCapture)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = isCapture;
            this.victim = null;
        }
        public Move(Piece piece, BitBoard to, BitBoard victim)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = true;
            this.victim = victim;
        }
    }
}
