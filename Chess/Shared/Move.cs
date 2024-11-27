namespace Chess.Shared
{
    public class Move
    {
        public Piece piece;
        public BitBoard from;
        public BitBoard to;
        public uint bitmask = 0b11110000;
        private const uint CaptureMask = 0b0001;
        private const uint CheckMask = 0b0010;
        private const uint CheckMateMask = 0b0100;
        private const uint DrawMask = 0b1000;
        public BitBoard? occupiedSquares;
        public BitBoard? friendlyPieces;
        public BitBoard? opponentPieces;
        public Piece? castledRook;
        // little endian(?) isCapture, isCheck, isCheckMate, isDraw, wCastleShort, wCastleLong, bCastleShort, bCastleLong
        public bool isCapture
        {
            get => (this.bitmask & CaptureMask) != 0;
            set => SetBitmask(value, CaptureMask);
        }
        public bool isCheck
        {
            get => (this.bitmask & CheckMask) != 0;
            set => SetBitmask(value, CheckMask);
        }
        public bool isCheckMate
        {
            get => (this.bitmask & CheckMateMask) != 0;
            set => SetBitmask(value, CheckMateMask);
        }
        public bool isDraw
        {
            get => (this.bitmask & DrawMask) != 0;
            set => SetBitmask(value, DrawMask);
        }
        private void SetBitmask(bool value, uint mask)
        {
            if (value)
            {
                this.bitmask |= mask;
            }
            else
            {
                this.bitmask &= ~mask;
            }
        }
        public Piece? capturedPiece = null;
        public BitBoard? captureSquare;
        public Piece.Type? promoteTo = null;
        public enum CastleDirection {Long, Short};
        public CastleDirection? castleDirection = null;
        public Move(Piece piece, BitBoard to)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.captureSquare = null;
        }
        public Move(Piece piece, BitBoard to, uint bitmask, BitBoard? captureSquare)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.bitmask = bitmask;
            this.captureSquare = captureSquare;
            this.isCapture = true;
        }
        public Move(Piece piece, BitBoard to, BitBoard? captureSquare)
        {
            this.piece = piece;
            this.from = piece.position;
            this.to = to;
            this.captureSquare = captureSquare;
            this.isCapture = true;
        }

        public Move SelectPromotionType(Piece.Type type)
        {
            Move newMove = new Move(this.piece, this.to, this.bitmask, this.captureSquare);
            newMove.promoteTo = type;
            return newMove;
        }

        public string GetMoveNotation()
        {
            string[] destinationNotation = this.to.GetFileAndRank();
            string[] sourceNotation = this.piece.position.GetFileAndRank();
            // TODO Qualifier notation - find other Knight and differentiate either file or rank
            string checkNotation = this.isCheck ? "+" : "";
            if (this.isCheckMate) checkNotation = "#";
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

        public override string ToString()
        {
            return GetMoveNotation();
        }
    }
}
