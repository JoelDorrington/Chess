namespace Chess.Shared
{
    public class Move
    {
        public Piece piece;
        public BitBoard from;
        public BitBoard to;
        public bool isCapture;
        public bool isCheck;
        public int? result;
        public BitBoard? victim;
        public Piece.Type? promoteTo = null;
        public enum CastleDirection {Long, Short};
        public CastleDirection? castleDirection = null;
        public Move(Piece piece, BitBoard to)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = false;
            this.victim = null;
            this.isCheck = false;
            this.result = null;
        }
        public Move(Piece piece, BitBoard to, bool isCapture)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = isCapture;
            this.victim = null;
            this.isCheck = false;
            this.result = null;
        }
        public Move(Piece piece, BitBoard to, BitBoard victim)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.isCapture = true;
            this.victim = victim;
            this.isCheck = false;
            this.result = null;
        }

        public Move SelectPromotionType(Piece.Type type)
        {
            Move newMove = new Move(this.piece, this.to);
            newMove.isCapture = this.isCapture;
            newMove.victim = this.victim;
            newMove.promoteTo = type;
            newMove.isCheck = this.isCheck;
            newMove.result = this.result;
            return newMove;
        }
    }
}
