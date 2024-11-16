using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;

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
        public static uint lastId = 0;
        public string Id = (Board.lastId++).ToString();
        public Piece[] pieces;
        public Piece.Colour currentTurn;
        public Move? movePlayed = null;
        public Move? lastMove = null;
        public bool wCastleShort = true;
        public bool wCastleLong = true;
        public bool bCastleShort = true;
        public bool bCastleLong = true;
        private BitBoard occupiedSquares = new(0);
        private BitBoard friendlyPieces = new(0);
        private BitBoard opponentPieces = new(0);
        private BitBoard controlledSquares = new(0);
        public bool check = false;
        private List<Threat> checkingPieces = new();
        private List<Threat> pins = new();

        public Board()
        {
            this.pieces = new Piece[32];
            this.currentTurn = Piece.Colour.White;
            this.ResetBoard();
        }
        public Board(Piece[] oldPieces, Move lastMove)
        {
            pieces = new Piece[32];
            this.lastMove = lastMove;
            Piece.Colour lastTurn = lastMove.piece.colour;
            this.currentTurn = lastTurn == Piece.Colour.White ? Piece.Colour.Black : Piece.Colour.White;
            Array.Copy(oldPieces, pieces, 32);
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] is Pawn) pieces[i] = new Pawn(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Knight) pieces[i] = new Knight(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Bishop) pieces[i] = new Bishop(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Rook) pieces[i] = new Rook(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Queen) pieces[i] = new Queen(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is King) pieces[i] = new King(pieces[i].colour, pieces[i].position.board);
            }
            this.CalculateThreats(this.currentTurn);
        }
        public Board(Board previousPosition)
        {
            if (previousPosition.movePlayed == null)
            {
                throw new Exception("previousPosition.movePlayed must not be null");
            }
            pieces = new Piece[32];
            Array.Copy(previousPosition.pieces, pieces, 32);
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i] is Pawn) pieces[i] = new Pawn(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Knight) pieces[i] = new Knight(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Bishop) pieces[i] = new Bishop(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Rook) pieces[i] = new Rook(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is Queen) pieces[i] = new Queen(pieces[i].colour, pieces[i].position.board);
                if (pieces[i] is King) pieces[i] = new King(pieces[i].colour, pieces[i].position.board);
            }
            this.lastMove = previousPosition.movePlayed;
            Piece.Colour lastTurn = this.lastMove.piece.colour;
            this.currentTurn = lastTurn == Piece.Colour.White ? Piece.Colour.Black : Piece.Colour.White;
            this.wCastleShort = previousPosition.wCastleShort;
            this.wCastleLong = previousPosition.wCastleLong;
            this.bCastleShort = previousPosition.bCastleShort;
            this.bCastleLong = previousPosition.bCastleLong;
            this.CalculateThreats(this.currentTurn);
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
            this.CalculateThreats(this.currentTurn); // TODO Remove for performance
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

        public List<Move> GetLegalMovesForPiece<T>(T piece) where T : Piece
        {
            List<Move> moves = new() { };
            if (piece.position.IsEqual(0)) return moves;
            if(this.currentTurn != piece.colour)
            {
                return moves;
            }
            if (piece is King)
            {
                BitBoard attacks = piece.GetAttackMap(this.occupiedSquares).AndNot(this.friendlyPieces);
                attacks = attacks.AndNot(this.controlledSquares);
                List<BitBoard> kingMoves = attacks.Enumerate();
                for (int i = 0; i < kingMoves.Count; i++)
                {
                    moves.Add(new Move(piece, kingMoves[i], kingMoves[i].IntersectsWith(this.opponentPieces)));
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
                moves = piece.GetLegalMoves(this.occupiedSquares, this.opponentPieces, this.lastMove);
            }
            if(this.check || this.PieceIsPinned(piece))
            {
                List<Move> nonCheckMoves = new();
                for(int i = 0; i < moves.Count; i++)
                {
                    try
                    {
                        if (!this.MoveResultsInCheck(moves[i]))
                        {
                            nonCheckMoves.Add(moves[i]);
                        }
                    } catch (PromotionTypeException e) {}
                }
                return nonCheckMoves;
            }
            return moves;
        }

        public List<Move> GetAllLegalMoves()
        {
            List<Move> moves = new();
            List<Piece> friendlyPieces = GetFriendlyPieces();
            foreach(Piece piece in friendlyPieces)
            {
                moves.AddRange(GetLegalMovesForPiece(piece));
            }
            return moves;
        }
        public List<Move> GetOpponentsLegalMoves()
        {
            List<Move> moves = new();
            List<Piece> opponentsPieces = GetOpponentPieces();
            Piece.Colour originalColour = currentTurn;
            currentTurn = originalColour == Piece.Colour.White ? Piece.Colour.Black : Piece.Colour.White;
            foreach (Piece piece in opponentsPieces)
            {
                moves.AddRange(GetLegalMovesForPiece(piece));
            }
            currentTurn = originalColour;
            return moves;
        }

        public bool MoveResultsInCheck(Move move) {
            Board result = this.MovePiece(move, true);
            return result.check;
        }

        public bool PieceIsPinned(Piece piece)
        {
            foreach(Threat pin in this.pins)
            {
                if (pin.attackMap.IntersectsWith(piece.position)) return true;
            }
            return false;
        }
        
        public int? CheckResult()
        {
            List<Piece> friendlyPieces = this.GetFriendlyPieces();
            foreach(Piece piece in friendlyPieces)
            {
                List<Move> moves = this.GetLegalMovesForPiece(piece);
                if(moves.Count > 0)
                {
                    return null;
                }
            }
            int result = this.check ? 1 : 0;
            if(this.lastMove != null) this.lastMove.result = result;
            return result;
        }

        public void CalculateThreats(Piece.Colour? player)
        {
            Piece? king = this.GetKing(player ?? currentTurn);
            this.controlledSquares = new(0);
            this.occupiedSquares = this.GetOccupiedSquares();
            this.friendlyPieces = this.GetOccupiedSquares(player ?? currentTurn);
            this.opponentPieces = new BitBoard(this.occupiedSquares.board).AndNot(this.friendlyPieces);
            this.checkingPieces = new();
            this.pins = new();
            List<Piece> opponentPieces = player == currentTurn ? this.GetOpponentPieces() : this.GetFriendlyPieces();
            foreach (Piece piece in opponentPieces)
            {
                if (!piece.position.IsEqual(0))
                {
                    BitBoard attacks = piece.GetAttackMap(this.occupiedSquares);
                    this.controlledSquares = this.controlledSquares.UnionWith(attacks);
                    if (king != null)
                    {
                        if (attacks.IntersectsWith(king.position))
                        {
                            this.check = true;
                            this.checkingPieces.Add(new Threat(piece, attacks));
                        }
                        else if (piece.type == Piece.Type.Bishop || piece.type == Piece.Type.Rook || piece.type == Piece.Type.Queen)
                        {
                            BitBoard xRay = piece.GetXray(this.occupiedSquares);
                            if (xRay.IntersectsWith(king.position))
                            {
                                this.pins.Add(new Threat(piece, xRay));
                            }
                        }
                    }
                }
            }
        }

        private Piece? GetKing(Piece.Colour? player)
        {
            foreach(Piece piece in pieces)
            {
                if(piece.colour == (player ?? currentTurn) && piece.type == Piece.Type.King)
                {
                    return piece;
                }
            }
            return null;
        }
        public List<Piece> GetFriendlyPieces()
        {
            List<Piece> friendlyPieces = new List<Piece>();
            foreach (Piece piece in pieces)
            {
                if (piece.colour == currentTurn && !piece.position.IsEqual(0))
                {
                    friendlyPieces.Add(piece);
                }
            }
            return friendlyPieces;
        }

        private List<Piece> GetOpponentPieces()
        {
            List<Piece> opponentPieces = new List<Piece>();
            foreach (Piece piece in this.pieces)
            {
                if (piece.colour != this.currentTurn && !piece.position.IsEqual(0))
                {
                    opponentPieces.Add(piece);
                }
            }
            return opponentPieces;
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
                } else if(move.promoteTo is not null)
                {
                    throw new PromotionTypeException("Promotion Type Forbidden");
                }
            }
            else if (move.promoteTo is not null)
            {
                throw new PromotionTypeException("Promotion Type Forbidden");
            }
            for (int i = 0; i < this.pieces.Length; i++)
            {
                if (move.piece == this.pieces[i])
                {
                    List<Move> legalMoves = new List<Move>();
                    if (dryRun)
                    {
                        legalMoves.Add(move);
                    } else
                    {
                        legalMoves = GetLegalMovesForPiece(move.piece);
                    }   
                    for (int j = 0; j < legalMoves.Count; j++)
                    {
                        if (legalMoves[j].piece == move.piece && legalMoves[j].to.IsEqual(move.to))
                        {
                            Move movePlayed = legalMoves[j];
                            this.movePlayed = movePlayed;
                            Board result = new(this);
                            if (movePlayed.isCapture)
                            {
                                Piece? capturedPiece = movePlayed.victim != null ? result.GetPieceAtSquare(movePlayed.victim) : result.GetPieceAtSquare(movePlayed.to);
                                if (capturedPiece != null)
                                {
                                    capturedPiece.position = new(0);
                                }
                            }
                            if (movePlayed.castleDirection != null)
                            {
                                Piece? castledRook = result.GetPieceAtSquare(new BitBoard(1)
                                    .GenShift((int)movePlayed.castleDirection * 7)
                                    .GenShift((int)movePlayed.piece.colour * 8*7));
                                if(castledRook != null)
                                {
                                    castledRook.position = new BitBoard(8).GenShift((int)movePlayed.castleDirection * 2)
                                        .GenShift((int)movePlayed.piece.colour * 8 * 7);
                                }
                            }
                            if(movePlayed.piece.colour == Piece.Colour.White)
                            {
                                if(movePlayed.piece.type == Piece.Type.King)
                                {
                                    result.wCastleLong = false;
                                    result.wCastleShort = false;
                                }
                                if(movePlayed.piece.type == Piece.Type.Rook)
                                {
                                    if (result.wCastleShort && movePlayed.piece.position.IntersectsWith(hFile))
                                    {
                                        result.wCastleShort = false;
                                    }
                                    if (result.wCastleLong && movePlayed.piece.position.IntersectsWith(aFile))
                                    {
                                        result.wCastleLong = false;
                                    }
                                }
                            } else
                            {
                                if(movePlayed.piece.type == Piece.Type.King)
                                {
                                    result.bCastleLong = false;
                                    result.bCastleShort = false;
                                }
                                if (movePlayed.piece.type == Piece.Type.Rook)
                                {
                                    if (result.wCastleShort && movePlayed.piece.position.IntersectsWith(hFile))
                                    {
                                        result.wCastleShort = false;
                                    }
                                    if (result.wCastleLong && movePlayed.piece.position.IntersectsWith(aFile))
                                    {
                                        result.wCastleLong = false;
                                    }
                                }
                            }
                            result.pieces[i].position = movePlayed.to;
                            if(move.promoteTo != null)
                            {
                                Piece.Type promotionType = (Piece.Type)move.promoteTo;
                                var newPiece = Piece.FromType(promotionType, result.pieces[i].colour, move.to);
                                result.pieces[i] = newPiece;
                            }

                            result.CalculateThreats(dryRun ? this.currentTurn : result.currentTurn);
                            movePlayed.isCheck = result.check;
                            // Needs work - bot can't see checkmate
                            if (dryRun) this.movePlayed = null;
                            else
                            {
                                movePlayed.result = result.CheckResult();
                                Console.WriteLine($"Move Result : {movePlayed.result}");
                            }
                            return result;
                        }
                    }
                    throw new IllegalMoveException();
                }
            }
            throw new Exception("Piece not from this Board");
        }

        public string GetMoveNotation()
        {
            if (this.movePlayed == null) return "...";
            return this.movePlayed.GetMoveNotation();
        }
    }
}
