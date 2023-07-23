using System.Collections.Generic;
namespace Chess.Shared
{
    public class IllegalMoveException : Exception
    {
        public IllegalMoveException() : base("Illegal Move") { }
    }
    public class Board
    {
        public Piece[] pieces;
        public Piece.Colour currentTurn;
        public Move? movePlayed = null;
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
            for (int i = 0; i < this.pieces.Length; i++)
            {
                this.pieces[i] = this.pieces[i].Clone();
            }
        }

        public const ulong a1h8Diagonal = 0x8040201008040201;
        public const ulong a8h1Diagonal = 0x0102040810204080;
        public const ulong aFile = 0x0101010101010101;
        public const ulong firstRank = 0x00000000000000FF;

        public void ResetBoard()
        {
            this.pieces = new Piece[32];
            this.pieces[0] = new Piece(Piece.Colour.White, Piece.Type.Rook, 1);
            this.pieces[1] = new Piece(Piece.Colour.White, Piece.Type.Knight, 2);
            this.pieces[2] = new Piece(Piece.Colour.White, Piece.Type.Bishop, 4);
            this.pieces[3] = new Piece(Piece.Colour.White, Piece.Type.Queen, 8);
            this.pieces[4] = new Piece(Piece.Colour.White, Piece.Type.King, 0x10);
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
            this.pieces[27] = new Piece(Piece.Colour.Black, Piece.Type.Queen, 0x800000000000000);
            this.pieces[28] = new Piece(Piece.Colour.Black, Piece.Type.King, 0x1000000000000000);
            this.pieces[29] = new Piece(Piece.Colour.Black, Piece.Type.Bishop, 0x2000000000000000);
            this.pieces[30] = new Piece(Piece.Colour.Black, Piece.Type.Knight, 0x4000000000000000);
            this.pieces[31] = new Piece(Piece.Colour.Black, Piece.Type.Rook, 0x8000000000000000);
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
                    Console.WriteLine($"GetLayout coords {coords[0]},{coords[1]}");
                    layout[coords[0]][coords[1]] = piece;
                }
            }
            return layout;
        }

        public Piece? GetPieceAtSquare(BitBoard squareSingleBitset)
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
            if(this.currentTurn != piece.colour)
            {
                return moves;
            }
            BitBoard occupiedSquares = this.GetOccupiedSquares();
            BitBoard friendlyPieces = this.GetOccupiedSquares(piece.colour);
            BitBoard opponentPieces = new(occupiedSquares.board ^ friendlyPieces.board);
            if (piece.type == Piece.Type.Pawn)
            {
                BitBoard pushPawnSquare = new(piece.position);
                pushPawnSquare.GenShift(8 - ((int)piece.colour * 16)); // -8 for black and 8 for white.
                if (pushPawnSquare.IntersectionOf(occupiedSquares).IsEqual(0)) // Square is not occupied;
                {
                    moves.Add(new Move(piece, pushPawnSquare));

                    int homeRankShiftValue = 8 + (int)piece.colour * (5 * 8);
                    BitBoard homeRank = new(firstRank << homeRankShiftValue);
                    Console.WriteLine($"HomeRank {homeRank.board:X}");
                    if (!homeRank.IntersectionOf(piece.position).IsEqual(0))
                    {
                        BitBoard pushTwiceSquare = new(pushPawnSquare);
                        pushTwiceSquare.GenShift(8 - ((int)piece.colour * 16));
                        if (pushTwiceSquare.IntersectionOf(occupiedSquares).IsEqual(0))
                        {
                            moves.Add(new Move(piece, pushTwiceSquare));
                        }
                    }
                }

                BitBoard captureLeft = new(piece.position);
                captureLeft.GenShift(7 - ((int)piece.colour * 16));
                if (piece.position.IntersectionOf(aFile).IsEqual(0) && !opponentPieces.IntersectionOf(captureLeft).IsEqual(0))
                {
                    moves.Add(new Move(piece, captureLeft, true));
                };
                BitBoard captureRight = new(piece.position);
                BitBoard hFile = new(aFile);
                hFile.GenShift(7);
                captureRight.GenShift(9 - ((int)piece.colour * 16));
                if (piece.position.IntersectionOf(hFile).IsEqual(0) && !opponentPieces.IntersectionOf(captureRight).IsEqual(0))
                {
                    moves.Add(new Move(piece, captureRight, true));
                };

                return moves;
            }
            return moves;
        }

        public Board MovePiece(Move move)
        {
            for (int i = 0; i < this.pieces.Length; i++)
            {
                if (move.piece == this.pieces[i])
                {
                    if (move.piece.colour != this.currentTurn)
                    {
                        throw new Exception($"Not {move.piece.colour}'s Turn");
                    }
                    List<Move> legalMoves = GetLegalMovesForPiece(move.piece);
                    for (int j = 0; j < legalMoves.Count; j++)
                    {
                        if (legalMoves[j].piece == move.piece && legalMoves[j].to.IsEqual(move.to))
                        {
                            Board result = new(this.pieces, this.currentTurn);
                            this.movePlayed = legalMoves[j];
                            if (legalMoves[j].isCapture)
                            {
                                Piece? capturedPiece = result.GetPieceAtSquare(move.to);
                                if (capturedPiece != null)
                                {
                                    capturedPiece.position = new(0);
                                }
                            }
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
}
