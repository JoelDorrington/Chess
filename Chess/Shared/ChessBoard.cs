﻿using System.Collections.Generic;
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
        public Move? lastMove = null;
        public Board()
        {
            this.pieces = new Piece[32];
            this.currentTurn = Piece.Colour.White;
            this.ResetBoard();
        }
        public Board(Piece[] pieces, Move lastMove)
        {
            this.pieces = new Piece[32];
            this.lastMove = lastMove;
            Piece.Colour lastTurn = lastMove.piece.colour;
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
        public const ulong knightMoveTemplate = 0x0000000A1100110A;
        public const int knightMoveTemplateRank = 2;
        public const int knightMoveTemplateFile = 2;

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
                int homeRankShiftValue = 8 + (int)piece.colour * (5 * 8);
                BitBoard homeRank = new BitBoard(firstRank).GenShift(homeRankShiftValue);
                BitBoard pushPawnSquare = new BitBoard(piece.position).GenShift(8 - ((int)piece.colour * 16)); // -8 for black and 8 for white.
                if (!pushPawnSquare.IntersectsWith(occupiedSquares)) // Square is not occupied;
                {
                    moves.Add(new Move(piece, pushPawnSquare));

                    if (homeRank.IntersectsWith(piece.position))
                    {
                        BitBoard pushTwiceSquare = new(pushPawnSquare);
                        pushTwiceSquare.GenShift(8 - ((int)piece.colour * 16));
                        if (!pushTwiceSquare.IntersectsWith(occupiedSquares))
                        {
                            moves.Add(new Move(piece, pushTwiceSquare));
                        }
                    }
                }

                BitBoard captureLeft = new BitBoard(piece.position).GenShift(7 - ((int)piece.colour * 16));
                if (!piece.position.IntersectsWith(aFile) && opponentPieces.IntersectsWith(captureLeft))
                {
                    moves.Add(new Move(piece, captureLeft, true));
                };
                BitBoard captureRight = new BitBoard(piece.position).GenShift(9 - ((int)piece.colour * 16));
                BitBoard hFile = new BitBoard(aFile).GenShift(7);
                if (!piece.position.IntersectsWith(hFile) && opponentPieces.IntersectsWith(captureRight))
                {
                    moves.Add(new Move(piece, captureRight, true));
                };

                BitBoard enPassantRank = new BitBoard(0x000000FF00000000).GenShift(-8 * (int)piece.colour);
                BitBoard opponentHomeRank = piece.colour == Piece.Colour.White 
                    ? new BitBoard(firstRank).GenShift(48) : new BitBoard(firstRank).GenShift(8);
                if (lastMove != null && lastMove.piece.type == Piece.Type.Pawn // Last move was a pawn move
                    && piece.position.IntersectsWith(enPassantRank) // This piece is on the enpassant rank
                    && lastMove.from.IntersectsWith(opponentHomeRank) // The pawn moved from its home square
                    && lastMove.to.IntersectsWith(enPassantRank)) // The pawn moved 2 squares
                {
                    BitBoard enPassantTarget = new(piece.position);
                    enPassantTarget.GenShift(-1);
                    if(lastMove.to.IsEqual(enPassantTarget))
                    {
                        moves.Add(new Move(piece, captureLeft, new BitBoard(enPassantTarget)));
                    }
                    enPassantTarget.GenShift(1);
                    enPassantTarget.GenShift(1);
                    if (lastMove.to.IsEqual(enPassantTarget))
                    {
                        moves.Add(new Move(piece, captureRight, new BitBoard(enPassantTarget)));
                    }

                }

                return moves;
            }
            if(piece.type == Piece.Type.Knight)
            {
                int[] pieceCoords = piece.GetCoordinates();
                BitBoard legalMoves = new(knightMoveTemplate);
                int verticalShiftCount = pieceCoords[1] - knightMoveTemplateRank;
                int horizontalShiftCount = pieceCoords[0] - knightMoveTemplateFile;
                for (int i = 0; i < Math.Abs(verticalShiftCount); i++)
                {
                    if (verticalShiftCount > 0) // Shift up
                    {
                        legalMoves.StepOne(8);
                    }
                    else // Shift down
                    {
                        legalMoves.StepOne(-8);
                    }
                }
                for (int i = 0; i < Math.Abs(horizontalShiftCount); i++)
                {
                    if (horizontalShiftCount > 0) // Shift right
                    {
                        legalMoves.StepOne(1);
                    }
                    else // Shift left
                    {
                        legalMoves.StepOne(-1);
                    }
                }
                legalMoves = new(legalMoves.board & ~friendlyPieces.board);
                List<BitBoard> knightMoves = legalMoves.Enumerate();
                for(int i = 0; i < knightMoves.Count; i++)
                {
                    moves.Add(new Move(piece, knightMoves[i], knightMoves[i].IntersectsWith(opponentPieces)));
                }
            }
            if(piece.type == Piece.Type.Bishop)
            {
                BitBoard attacks = piece.position.RayAttack(9, occupiedSquares)
                    .UnionWith(piece.position.RayAttack(7, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-9, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-7, occupiedSquares))
                    .AndNot(friendlyPieces);
                List<BitBoard> bishopMoves = attacks.Enumerate();
                for(int i = 0;i < bishopMoves.Count; i++)
                {
                    moves.Add(new Move(piece, bishopMoves[i], bishopMoves[i].IntersectsWith(opponentPieces)));
                }

            }
            if (piece.type == Piece.Type.Rook)
            {
                BitBoard attacks = piece.position.RayAttack(8, occupiedSquares)
                    .UnionWith(piece.position.RayAttack(1, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-8, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-1, occupiedSquares))
                    .AndNot(friendlyPieces);
                List<BitBoard> rookMoves = attacks.Enumerate();
                for (int i = 0; i < rookMoves.Count; i++)
                {
                    moves.Add(new Move(piece, rookMoves[i], rookMoves[i].IntersectsWith(opponentPieces)));
                }

            }
            if (piece.type == Piece.Type.Queen)
            {
                BitBoard attacks = piece.position.RayAttack(8, occupiedSquares)
                    .UnionWith(piece.position.RayAttack(1, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-8, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-1, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(9, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(7, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-9, occupiedSquares))
                    .UnionWith(piece.position.RayAttack(-7, occupiedSquares))
                    .AndNot(friendlyPieces);
                List<BitBoard> queenMoves = attacks.Enumerate();
                for (int i = 0; i < queenMoves.Count; i++)
                {
                    moves.Add(new Move(piece, queenMoves[i], queenMoves[i].IntersectsWith(opponentPieces)));
                }

            }
            if (piece.type == Piece.Type.King)
            {
                BitBoard attacks = new BitBoard(piece.position.board).StepOne(8)
                    .UnionWith(new BitBoard(piece.position.board).StepOne(1))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(-8))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(-1))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(9))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(7))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(-9))
                    .UnionWith(new BitBoard(piece.position.board).StepOne(-7))
                    .AndNot(friendlyPieces);
                List<BitBoard> kingMoves = attacks.Enumerate();
                for (int i = 0; i < kingMoves.Count; i++)
                {
                    moves.Add(new Move(piece, kingMoves[i], kingMoves[i].IntersectsWith(opponentPieces)));
                }

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
                            Board result = new(this.pieces, move);
                            this.movePlayed = legalMoves[j];
                            if (legalMoves[j].isCapture)
                            {
                                Piece? capturedPiece = move.victim != null ? result.GetPieceAtSquare(move.victim) : result.GetPieceAtSquare(move.to);
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
