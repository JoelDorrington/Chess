using static Chess.Shared.Piece;

namespace Chess.Shared
{
    public class IllegalMoveException : Exception
    {
        public IllegalMoveException() : base("Illegal Move") { }
    }
    public class PromotionTypeException : Exception
    {
        public PromotionTypeException(string message) : base(message) { }
    }

    class Threat
    {
        public Piece attacker;
        public BitBoard attackMap;
        public Threat(Piece attacker, BitBoard attackMap)
        {
            this.attacker = attacker;
            this.attackMap = attackMap;
        }
    }
    public class Board
    {
        public Piece[] pieces;
        public Piece.Colour currentTurn {
            get
            {
                return MoveHistory.Count % 2 == 0 ? Piece.Colour.White : Piece.Colour.Black;
            }
        }
        public Piece.Colour opponentColour
        {
            get
            {
                return MoveHistory.Count % 2 == 0 ? Piece.Colour.Black : Piece.Colour.White;
            }
        }
        public List<Move> MoveHistory = new();
        public uint bitmask = 0b1111;
        //public bool wCastleShort = true; // now bitmap[0]
        //public bool wCastleLong = true; // now bitmap[1]
        //public bool bCastleShort = true; // now bitmap[2]
        //public bool bCastleLong = true; // now bitmap[4]
        private BitBoard occupiedSquares = new(0);
        private BitBoard friendlyPieces = new(0);
        private BitBoard opponentPieces = new(0);
        private BitBoard controlledSquares = new(0);
        public bool check = false;
        private List<Threat> checkingPieces = new();
        private List<Threat> pins = new();
        private List<King> kings = new();

        public Board()
        {
            this.pieces = new Piece[32];
            this.ResetBoard();
        }

        public static ulong a1h8Diagonal = 0x8040201008040201;
        public static ulong a8h1Diagonal = 0x0102040810204080;
        public static ulong aFile = 0x0101010101010101;
        public static ulong hFile = 0x8080808080808080;
        public static ulong firstRank = 0x00000000000000FF;
        public static ulong knightMoveTemplate = 0x0000000A1100110A;
        public static int knightMoveTemplateRank = 2;
        public static int knightMoveTemplateFile = 2;

        public void ResetBoard()
        {
            MoveHistory.Clear();
            pieces = new Piece[32];
            pieces[0] = new Rook(Piece.Colour.White, 1);
            pieces[1] = new Knight(Piece.Colour.White, 2);
            pieces[2] = new Bishop(Piece.Colour.White, 4);
            pieces[3] = new Queen(Piece.Colour.White, 8);
            pieces[4] = new King(Piece.Colour.White, 0x10);
            pieces[5] = new Bishop(Piece.Colour.White, 0x20);
            pieces[6] = new Knight(Piece.Colour.White, 0x40);
            pieces[7] = new Rook(Piece.Colour.White, 0x80);
            pieces[8] = new Pawn(Piece.Colour.White, 0x100);
            pieces[9] = new Pawn(Piece.Colour.White, 0x200);
            pieces[10] = new Pawn(Piece.Colour.White, 0x400);
            pieces[11] = new Pawn(Piece.Colour.White, 0x800);
            pieces[12] = new Pawn(Piece.Colour.White, 0x1000);
            pieces[13] = new Pawn(Piece.Colour.White, 0x2000);
            pieces[14] = new Pawn(Piece.Colour.White, 0x4000);
            pieces[15] = new Pawn(Piece.Colour.White, 0x8000);
            pieces[16] = new Pawn(Piece.Colour.Black, 0x1000000000000);
            pieces[17] = new Pawn(Piece.Colour.Black, 0x2000000000000);
            pieces[18] = new Pawn(Piece.Colour.Black, 0x4000000000000);
            pieces[19] = new Pawn(Piece.Colour.Black, 0x8000000000000);
            pieces[20] = new Pawn(Piece.Colour.Black, 0x10000000000000);
            pieces[21] = new Pawn(Piece.Colour.Black, 0x20000000000000);
            pieces[22] = new Pawn(Piece.Colour.Black, 0x40000000000000);
            pieces[23] = new Pawn(Piece.Colour.Black, 0x80000000000000);
            pieces[24] = new Rook(Piece.Colour.Black, 0x100000000000000);
            pieces[25] = new Knight(Piece.Colour.Black, 0x200000000000000);
            pieces[26] = new Bishop(Piece.Colour.Black, 0x400000000000000);
            pieces[27] = new Queen(Piece.Colour.Black, 0x800000000000000);
            pieces[28] = new King(Piece.Colour.Black, 0x1000000000000000);
            pieces[29] = new Bishop(Piece.Colour.Black, 0x2000000000000000);
            pieces[30] = new Knight(Piece.Colour.Black, 0x4000000000000000);
            pieces[31] = new Rook(Piece.Colour.Black, 0x8000000000000000);
            this.kings = new() { (King)pieces[4], (King)pieces[28] };
            this.bitmask = 0b1111;
            SetOccupiedSquares();
            CalculateThreats();
        }

        public Piece[][] GetLayout()
        {
            Piece[][] layout = new Piece[8][];
            for (int i = 0; i < 8; i++)
            {
                layout[i] = new Piece[8];
            }
            for (int i = 0; i < this.pieces.Length; i++)
            {
                Piece piece = this.pieces[i];
                if (!piece.position.IsEqual(0))
                {
                    int[] coords = piece.GetCoordinates();
                    layout[coords[0]][coords[1]] = piece;
                }
            }
            return layout;
        }

        public Piece? GetPieceAtSquare(BitBoard squareSingleBitset)
        {
            return GetPieceAtSquare(squareSingleBitset.board);
        }
        public Piece? GetPieceAtSquare(ulong squareSingleBitset)
        {
            for (int i = 0; i < this.pieces.Length; i++)
            {
                if (this.pieces[i] != null && this.pieces[i].position.IsEqual(squareSingleBitset))
                {
                    return this.pieces[i];
                }
            }
            return null;
        }
        
        public void SetOccupiedSquares()
        {
            occupiedSquares = new(0);
            friendlyPieces = new(0);
            opponentPieces = new(0);

            foreach (Piece piece in pieces)
            {
                occupiedSquares.board |= piece.position.board;
                if (piece.colour == currentTurn)
                {
                    friendlyPieces.board |= piece.position.board;
                } else
                {
                    opponentPieces.board |= piece.position.board;
                }
            }
        }

        public BitBoard GetOccupiedSquares()
        {
            BitBoard occupiedSquares = new(0);
            for (int i = 0; i < this.pieces.Length; i++)
            {
                occupiedSquares = occupiedSquares.UnionWith(this.pieces[i].position);
            }
            return occupiedSquares;
        }

        public BitBoard GetOccupiedSquares(Piece.Colour colour)
        {
            BitBoard occupiedSquares = new(0);
            for (int i = 0; i < this.pieces.Length; i++)
            {
                if (this.pieces[i].colour == colour)
                {
                    occupiedSquares.board |= this.pieces[i].position.board;
                }
            }
            return occupiedSquares;
        }

        private bool wCastleShort
        {
            get => (this.bitmask & 0b0001) != 0;
            set => SetBitmask(value, 0b0001);
        }
        private bool wCastleLong
        {
            get => (this.bitmask & 0b0010) != 0;
            set => SetBitmask(value, 0b0010);
        }
        private bool bCastleShort
        {
            get => (this.bitmask & 0b0100) != 0;
            set => SetBitmask(value, 0b0100);
        }
        private bool bCastleLong
        {
            get => (this.bitmask & 0b1000) != 0;
            set => SetBitmask(value, 0b1000);
        }

        public Move? LastMove {
            get
            {
                if (this.MoveHistory.Count == 0) return null;
                return this.MoveHistory.Last();
            }
            set
            {
                if (value == null) throw new Exception("Cannot set LastMove to null");
                this.MoveHistory.Add(value);
            }
        }

        public List<Move> GetLegalMovesForPiece<T>(T piece) where T : Piece
        {
            CalculateThreats();
            List<Move> moves = new() { };
            if (piece.position.IsEqual(0)) throw new Exception("Piece is captured");
            if (this.currentTurn != piece.colour) throw new Exception($"Not {piece.colour}'s turn");
            if (piece is King)
            {
                foreach (BitBoard kingMove in piece.EnumerateAttackMap(this.occupiedSquares)
                    .Where(x => !x.IsEqual(0)))
                {
                    if (kingMove.IntersectsWith(this.friendlyPieces.board | this.controlledSquares.board)) continue;
                    Move move = new(piece, kingMove);
                    move.isCapture = kingMove.IntersectsWith(this.opponentPieces);
                    moves.Add(move);
                }
                if (!this.check)
                {
                    if (piece.colour == Piece.Colour.White ? this.wCastleShort : this.bCastleShort)
                    {
                        BitBoard inBetween = new BitBoard(0x60).GenShift((int)piece.colour * 8 * 7);
                        if (!inBetween.IntersectsWith(this.occupiedSquares) && !inBetween.IntersectsWith(this.controlledSquares))
                        {
                            Move castleShort = new(piece, new BitBoard(piece.position).GenShift(2));
                            castleShort.castleDirection = Move.CastleDirection.Short;
                            moves.Add(castleShort);
                        }
                    }
                    if (piece.colour == Piece.Colour.White ? this.wCastleLong : this.bCastleLong)
                    {
                        BitBoard inBetween = new BitBoard(0xE).GenShift((int)piece.colour * 8 * 7);
                        if (!inBetween.IntersectsWith(this.occupiedSquares) && !inBetween.IntersectsWith(this.controlledSquares))
                        {
                            Move castleLong = new(piece, new BitBoard(piece.position).GenShift(-2));
                            castleLong.castleDirection = Move.CastleDirection.Long;
                            moves.Add(castleLong);
                        }
                    }
                }
            } else
            {
                moves = piece.GetLegalMoves(this.occupiedSquares, this.opponentPieces, this.LastMove);
            }
            //Console.WriteLine($"GetLegalMovesForPiece {MoveHistory.Count}. {LastMove}"); // Check Depth
            if (this.check || this.PieceIsPinned(piece))
            {
                List<Move> nonCheckMoves = new();
                for (int i = 0; i < moves.Count; i++)
                {
                    try
                    {
                        MovePiece(moves[i], true); // Will throw exception if in check
                        UnMove();
                        nonCheckMoves.Add(moves[i]);
                    } catch (Exception e) {
                        if (e.Message != "King has been left en prise")
                        {
                            Console.Error.WriteLine($"{MoveHistory.Count}. {moves[i]}"); // TODO REMOVE
                            Console.Error.WriteLine(e);
                        }
                    }
                }
                return nonCheckMoves;
            }
            return moves;
        }
        public IEnumerable<Move> GenerateLegalMovesForPiece<T>(T piece) where T : Piece
        {
            if (piece.position.IsEqual(0)) throw new Exception("Piece is captured");
            if (this.currentTurn != piece.colour) throw new Exception($"Not {piece.colour}'s turn");
            if (piece is King)
            {
                foreach (BitBoard kingMove in piece.EnumerateAttackMap(this.occupiedSquares))
                {
                    if (kingMove.IsEqual(0) || kingMove.IntersectsWith(this.friendlyPieces.board | this.controlledSquares.board)) continue;
                    Move move = new(piece, kingMove);
                    move.isCapture = kingMove.IntersectsWith(this.opponentPieces);
                    yield return move;
                }
                if (!this.check)
                {
                    if (piece.colour == Piece.Colour.White ? this.wCastleShort : this.bCastleShort)
                    {
                        BitBoard inBetween = new BitBoard(0x60).GenShift((int)piece.colour * 8 * 7);
                        if (!inBetween.IntersectsWith(this.occupiedSquares) && !inBetween.IntersectsWith(this.controlledSquares))
                        {
                            Move castleShort = new(piece, new BitBoard(piece.position).GenShift(2));
                            castleShort.castleDirection = Move.CastleDirection.Short;
                            yield return castleShort;
                        }
                    }
                    if (piece.colour == Piece.Colour.White ? this.wCastleLong : this.bCastleLong)
                    {
                        BitBoard inBetween = new BitBoard(0xE).GenShift((int)piece.colour * 8 * 7);
                        if (!inBetween.IntersectsWith(this.occupiedSquares) && !inBetween.IntersectsWith(this.controlledSquares))
                        {
                            Move castleLong = new(piece, new BitBoard(piece.position).GenShift(-2));
                            castleLong.castleDirection = Move.CastleDirection.Long;
                            yield return castleLong;
                        }
                    }
                }
            }
            else
            {
                foreach (Move move in piece.GenerateLegalMoves(this.occupiedSquares, this.opponentPieces, this.LastMove))
                {
                    yield return move;
                }
            }
        }

        public IEnumerable<Move> GenerateMoves()
        {
            foreach (Piece piece in GetFriendlyPieces())
            {
                foreach (Move move in GenerateLegalMovesForPiece(piece))
                {
                    yield return move;
                }
            }
        }

        public List<Move> GetAllLegalMoves()
        {
            List<Move> moves = new();
            foreach (Piece piece in GetFriendlyPieces())
            {
                moves.AddRange(GetLegalMovesForPiece(piece));
            }
            return moves;
        }
        public List<Move> GetOpponentsLegalMoves()
        {
            List<Move> moves = new();
            foreach (Piece piece in GetOpponentPieces())
            {
                moves.AddRange(GetLegalMovesForPiece(piece));
            }
            return moves;
        }

        public bool PieceIsPinned(Piece piece)
        {
            foreach (Threat pin in this.pins)
            {
                if (pin.attackMap.IntersectsWith(piece.position)) return true;
            }
            return false;
        }

        public int? CheckResult()
        {
            foreach (Piece piece in GetFriendlyPieces())
            {
                if (piece.position.IsEqual(0)) continue;
                List<Move> moves = this.GetLegalMovesForPiece(piece);
                if (moves.Count > 0)
                {
                    return null;
                }
            }
            return this.check ? 1 : 0;
        }

        public void Validate()
        {
            // Ensuring the current position is legal eg. opponent king must not be en prise
            Piece? opponentKing = this.GetKing(opponentColour); // The king of the player who played LastMove;
            foreach (Piece piece in GetFriendlyPieces())
            {
                if (!piece.position.IsEqual(0))
                {
                    BitBoard attacks = piece.GetAttackMap(this.occupiedSquares);
                    if (opponentKing != null)
                    {
                        if (attacks.IntersectsWith(opponentKing.position))
                        {
                            throw new Exception("King has been left en prise");
                        }
                    }
                }
            }
        }

        public void CalculateThreats()
        {
            check = false;
            Piece king = GetKing(currentTurn);
            controlledSquares = new(0);
            checkingPieces = new();
            pins = new();
            foreach (Piece piece in GetOpponentPieces())
            {
                if (piece.position.IsEqual(0)) continue;
                BitBoard attacks = piece.GetAttackMap(occupiedSquares);
                controlledSquares.board |= attacks.board;
                if (king == null) continue;
                if (attacks.IntersectsWith(king.position))
                {
                    check = true;
                    checkingPieces.Add(new Threat(piece, attacks));
                }
                else if (piece.type == Piece.Type.Bishop || piece.type == Piece.Type.Rook || piece.type == Piece.Type.Queen)
                {
                    BitBoard xRay = piece.GetXray(occupiedSquares);
                    if (xRay.IntersectsWith(king.position))
                    {
                        pins.Add(new Threat(piece, xRay));
                    }
                }
            }
        }

        private Piece GetKing(Colour? player)
        {
            return this.kings[(int)(player ?? this.currentTurn)];
        }
        public IEnumerable<Piece> GetFriendlyPieces()
        {
            foreach (Piece piece in pieces)
            {
                if (piece.colour == currentTurn && !piece.position.IsEqual(0))
                {
                    yield return piece;
                }
            }
        }

        private IEnumerable<Piece> GetOpponentPieces()
        {
            foreach (Piece piece in this.pieces)
            {
                if (piece.colour == opponentColour && !piece.position.IsEqual(0))
                {
                    yield return piece;
                }
            }
        }

        public Board MovePiece(Move move, bool dryRun=false)
        {
            if (move.piece.position.IsEqual(0))
            {
                throw new Exception("Piece is not on the board");
            }
            if (move.piece.colour != this.currentTurn)
            {
                throw new Exception($"Not {move.piece.colour}'s Turn");
            }
            if (move.piece is Pawn)
            {
                BitBoard promotionRank = new BitBoard(firstRank).GenShift((7 - ((int)move.piece.colour * 7)) * 8); // 0 for black and 7 for white.
                if (promotionRank.IntersectsWith(move.to))
                {
                    if (move.promoteTo is null)
                    {
                        throw new PromotionTypeException("Promotion Type Required");
                    }
                } else if (move.promoteTo is not null)
                {
                    throw new PromotionTypeException("Promotion Type Forbidden");
                }
            }
            else if (move.promoteTo is not null)
            {
                throw new PromotionTypeException("Promotion Type Forbidden");
            }
            move.bitmask &= (this.bitmask << 4) | 0b1111; // This.bitmask seems to always be 0
            move.occupiedSquares = occupiedSquares;
            move.friendlyPieces = friendlyPieces;
            move.opponentPieces = opponentPieces;
            this.LastMove = move;
            if (move.isCapture)
            {
                Piece? capturedPiece = move.captureSquare != null ? this.GetPieceAtSquare(move.captureSquare) : this.GetPieceAtSquare(move.to);
                if (capturedPiece != null)
                {
                    move.capturedPiece = capturedPiece;
                    capturedPiece.position = new(0);
                }
            }
            if (move.castleDirection != null)
            {
                Piece? castledRook = this.GetPieceAtSquare(new BitBoard(1)
                    .GenShift((int)move.castleDirection * 7)
                    .GenShift((int)move.piece.colour * 8 * 7));
                if (castledRook != null)
                {
                    move.castledRook = castledRook;
                    castledRook.position = new BitBoard(8).GenShift((int)move.castleDirection * 2)
                        .GenShift((int)move.piece.colour * 8 * 7);
                }
            }
            UpdateCastlingBitmask(move);
            move.piece.position = move.to;
            if (move.promoteTo != null)
            {
                Piece.Type promotionType = (Piece.Type)move.promoteTo;
                move.piece.type = promotionType;
            }

            try
            {
                SetOccupiedSquares();
                Validate();
                if (!dryRun)
                {
                    CalculateThreats();
                    move.isCheck = check;
                    int? result = CheckResult();
                    if (result == 0) move.isDraw = true;
                    else if (result == 1) move.isCheckMate = true;
                }
            }
            catch (Exception e)
            {
                this.UnMove();
                throw e;
            }

            return this;
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

        void UpdateCastlingBitmask(Move move)
        {

            if (move.piece.colour == Piece.Colour.White)
            {
                if (move.piece.type == Piece.Type.King)
                {
                    this.wCastleLong = false;
                    this.wCastleShort = false;
                }
                if (move.piece.type == Piece.Type.Rook)
                {
                    if (this.wCastleShort && move.piece.position.IntersectsWith(hFile))
                    {
                        this.wCastleShort = false;
                    }
                    if (this.wCastleLong && move.piece.position.IntersectsWith(aFile))
                    {
                        this.wCastleLong = false;
                    }
                }
            }
            else
            {
                if (move.piece.type == Piece.Type.King)
                {
                    this.bCastleLong = false;
                    this.bCastleShort = false;
                }
                if (move.piece.type == Piece.Type.Rook)
                {
                    if (this.bCastleShort && move.piece.position.IntersectsWith(hFile))
                    {
                        this.bCastleShort = false;
                    }
                    if (this.bCastleLong && move.piece.position.IntersectsWith(aFile))
                    {
                        this.bCastleLong = false;
                    }
                }
            }
        }

        public void UnMove()
        {
            Move? move = LastMove;
            if (move == null) throw new Exception("No moves to undo");
            if (move.promoteTo != null)
            {
                move.piece.type = Piece.Type.Pawn;
            }
            move.piece.position = move.from;
            if (move.capturedPiece != null)
            {
                move.capturedPiece.position = move.captureSquare ?? move.to;
            }
            if(move.castleDirection != null && move.castledRook != null)
            {
                move.castledRook.position = new BitBoard(1)
                    .GenShift((int)move.castleDirection * 7)
                    .GenShift((int)move.piece.colour * 8 * 7);
            }
            bitmask = move.bitmask >> 4;
            occupiedSquares = move.occupiedSquares ?? new(0);
            friendlyPieces = move.friendlyPieces ?? new(0);
            opponentPieces = move.opponentPieces ?? new(0);
            MoveHistory.Remove(move);
        }

        public string GetMoveNotation()
        {
            if (this.LastMove == null) return "...";
            return this.LastMove.GetMoveNotation();
        }
    }
}
