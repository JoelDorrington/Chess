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

        public string GetMoveNotation()
        {
            string[] destinationNotation = this.to.GetFileAndRank();
            string[] sourceNotation = this.piece.position.GetFileAndRank();
            // TODO Qualifier notation - find other Knight and differentiate either file or rank
            string checkNotation = this.isCheck ? "+" : "";
            if (this.result == 1) checkNotation = "#";
            string captureNotation = "";
            if (this.isCapture)
            {
                captureNotation = "x";
                if (this.piece is Pawn)
                {
                    captureNotation = sourceNotation[0] + captureNotation;
                }
            }
            return $"{this.piece.GetInitial()}{captureNotation}{destinationNotation[0]}{destinationNotation[1]}{checkNotation}";
        }
    }
}
