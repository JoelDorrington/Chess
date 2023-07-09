using System.Reflection;

namespace Chess.Shared
{
    public class IllegalMoveException : Exception
    {
        public IllegalMoveException() : base("Illegal Move") { }
    }
    public class BitBoard
    {
        public ulong board;
        public BitBoard(ulong board)
        {
            this.board = board;
        }
        public BitBoard(BitBoard board)
        {
            this.board = board.board;
        }

        public bool IsEqual(BitBoard other)
        {
            return board == other.board;
        }
        public bool IsEqual(ulong other)
        {
            return board == other;
        }

        public BitBoard UnionWith(BitBoard other)
        {
            return new BitBoard(this.board | other.board);
        }
        public BitBoard UnionWith(ulong other)
        {
            return new BitBoard(this.board | other);
        }
        public BitBoard IntersectionOf(BitBoard other)
        {
            return new BitBoard(this.board & other.board);
        }
        public BitBoard IntersectionOf(ulong other)
        {
            return new BitBoard(this.board & other);
        }

        public void GenShift(int shift)
        {
            this.board = (shift > 0) ? (this.board << shift) : (this.board >> -shift);
        }

        //public void RotateLeft(int shift)
        //{
        //    this.board = (this.board << shift) | (this.board >> -shift);
        //}
        //public void RotateRight(int shift)
        //{
        //    this.board = (this.board >> shift) | (this.board << -shift);
        //}

    }

    public class Piece
    {
        public enum Type
        {
            Pawn = 0,
            Knight = 1,
            Bishop = 2,
            Rook = 3,
            Queen = 4,
            King = 5
        }

        public enum Colour { White, Black }

        public Colour colour;
        public Type type;
        public BitBoard position;

        public Piece(Colour colour, Type type, ulong position)
        {
            this.colour = colour;
            this.type = type;
            this.position = new BitBoard(position);
        }
        public Piece(Colour colour, Type type, BitBoard position)
        {
            this.colour = colour;
            this.type = type;
            this.position = new BitBoard(position.board);
        }

        public Piece Clone()
        {
            return new Piece(this.colour, this.type, this.position);
        }
    }

    public class Board
    {
        public Piece[] pieces;
        public Piece.Colour currentTurn;
        public Board()
        {
            this.pieces = new Piece[32];
            this.currentTurn = Piece.Colour.White;
            this.ResetBoard();
        }
        public Board(Piece[] pieces, Piece.Colour lastTurn)
        {
            this.pieces = new Piece[32];
            this.currentTurn = lastTurn == Piece.Colour.White ? Piece.Colour.Black : Piece.Colour.White;
            Array.Copy(pieces, this.pieces, 32);
            for(int i = 0; i <  this.pieces.Length; i++)
            {
                this.pieces[i] = this.pieces[i].Clone();
            }
        }

        public const ulong a1h8Diagonal = 0x8040201008040201;
        public const ulong a8h1Diagonal = 0x0102040810204080;
        public const ulong a1File = 0x0101010101010101;
        public const ulong firstRank = 0x00000000000000FF;

        public void ResetBoard()
        {
            this.pieces = new Piece[32];
            this.pieces[0] = new Piece(Piece.Colour.White, Piece.Type.Rook, 1);
            this.pieces[1] = new Piece(Piece.Colour.White, Piece.Type.Knight, 2);
            this.pieces[2] = new Piece(Piece.Colour.White, Piece.Type.Bishop, 4);
            this.pieces[3] = new Piece(Piece.Colour.White, Piece.Type.King, 8);
            this.pieces[4] = new Piece(Piece.Colour.White, Piece.Type.Queen, 0x10);
            this.pieces[5] = new Piece(Piece.Colour.White, Piece.Type.Bishop, 0x20);
            this.pieces[6] = new Piece(Piece.Colour.White, Piece.Type.Knight, 0x40);
            this.pieces[7] = new Piece(Piece.Colour.White, Piece.Type.Rook, 0x80);
            this.pieces[8] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x100);
            this.pieces[9] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x200);
            this.pieces[10] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x400);
            this.pieces[11] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x800);
            this.pieces[12] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x1000);
            this.pieces[13] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x2000);
            this.pieces[14] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x4000);
            this.pieces[15] = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x8000);
            this.pieces[16] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x1000000000000);
            this.pieces[17] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x2000000000000);
            this.pieces[18] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x4000000000000);
            this.pieces[19] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x8000000000000);
            this.pieces[20] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x10000000000000);
            this.pieces[21] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x20000000000000);
            this.pieces[22] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x40000000000000);
            this.pieces[23] = new Piece(Piece.Colour.Black, Piece.Type.Pawn, 0x80000000000000);
            this.pieces[24] = new Piece(Piece.Colour.Black, Piece.Type.Rook, 0x100000000000000);
            this.pieces[25] = new Piece(Piece.Colour.Black, Piece.Type.Knight, 0x200000000000000);
            this.pieces[26] = new Piece(Piece.Colour.Black, Piece.Type.Bishop, 0x400000000000000);
            this.pieces[27] = new Piece(Piece.Colour.Black, Piece.Type.King, 0x800000000000000);
            this.pieces[28] = new Piece(Piece.Colour.Black, Piece.Type.Queen, 0x1000000000000000);
            this.pieces[29] = new Piece(Piece.Colour.Black, Piece.Type.Bishop, 0x2000000000000000);
            this.pieces[30] = new Piece(Piece.Colour.Black, Piece.Type.Knight, 0x4000000000000000);
            this.pieces[31] = new Piece(Piece.Colour.Black, Piece.Type.Rook, 0x8000000000000000);
        }

        public Piece? GetPieceAtSquare(BitBoard squareSingleBitset)
        {
            for (int i = 0; i < this.pieces.Length; i++){
                if (this.pieces[i] != null && this.pieces[i].position.IsEqual(squareSingleBitset)){
                    return this.pieces[i];
                }
            }
            return null;
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
                    occupiedSquares = occupiedSquares.UnionWith(this.pieces[i].position);
                }
            }
            return occupiedSquares;
        }

        public List<Move> GetLegalMovesForPiece(Piece piece)
        {
            List<Move> moves = new() { };
            BitBoard occupiedSquares = this.GetOccupiedSquares();
            BitBoard friendlyPieces = this.GetOccupiedSquares(piece.colour);
            BitBoard opponentPieces = new(occupiedSquares.board ^ friendlyPieces.board);
            if (piece.type == Piece.Type.Pawn)
            {
                BitBoard pushPawnSquare = new(piece.position);
                Console.WriteLine((int)piece.colour);
                pushPawnSquare.GenShift(8 - ((int)piece.colour * 16)); // -8 for black and 8 for white.
                Console.WriteLine(pushPawnSquare.board);
                if (pushPawnSquare.IntersectionOf(occupiedSquares).IsEqual(0)) // Square is not occupied;
                {
                    moves.Add(new Move(piece, pushPawnSquare));
                    
                    int homeRankShiftValue = 1 + (int)piece.colour * 5;
                    BitBoard homeRank = new(firstRank << homeRankShiftValue);
                    if (homeRank.IntersectionOf(piece.position).IsEqual(0))
                    {
                        BitBoard pushTwiceSquare = new(pushPawnSquare);
                        pushTwiceSquare.GenShift(8 - ((int)piece.colour * 16));
                        Console.WriteLine(pushTwiceSquare.board);
                        if (pushTwiceSquare.IntersectionOf(occupiedSquares).IsEqual(0))
                        {
                            moves.Add(new Move(piece, pushTwiceSquare));
                        }
                    }
                }

                //if (canPushTwice())
                //{
                //    BitBoard pushTwiceSquare = new(piece.position);
                //    pushTwiceSquare.GenShift((int)piece.colour * 16);
                //    moves.Add(new Move(piece, pushTwiceSquare));
                //}
                return moves;
            }
            return new List<Move>();
        }

        public Board MovePiece(Move move)
        {
            for(int i = 0; i < this.pieces.Length; i++)
            { 
                if(move.piece == this.pieces[i])
                {
                    if(move.piece.colour != this.currentTurn)
                    {
                        throw new Exception($"Not {move.piece.colour}'s Turn");
                    }
                    List<Move> legalMoves = GetLegalMovesForPiece(move.piece);
                    for(int j = 0; j < legalMoves.Count; j++)
                    {
                        Console.WriteLine(move.to.board);
                        if (legalMoves[j].piece == move.piece && legalMoves[j].to.IsEqual(move.to))
                        {
                            Board result = new(this.pieces, this.currentTurn);
                            result.pieces[i].position = move.to;
                            return result;
                        }
                    }
                    throw new IllegalMoveException();
                }
            }
            throw new Exception("Piece not from this Board");
        }
    }

    public class Move
    {
        public Piece piece;
        public BitBoard to;
        public Move(Piece piece, BitBoard to)
        {
            this.piece = piece;
            this.to = to;
        }
    }
}
