using System.IO.Pipelines;

namespace Chess.Shared
{
    public abstract class AbstractPiece
    {
        public abstract List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove);

        public abstract BitBoard GetAttackMap(BitBoard occupiedSquares);

        public abstract BitBoard GetXray(BitBoard occupiedSquares);

        public abstract string GetInitial();
    }
    public class Piece : AbstractPiece
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

        public static Piece FromType(Piece.Type type, Piece.Colour colour, BitBoard position)
        {
            switch (type)
            {
                case Type.Pawn:
                    return new Pawn(colour, position.board);
                case Type.Knight:
                    return new Knight(colour, position.board);
                case Type.Bishop:
                    return new Bishop(colour, position.board);
                case Type.Rook:
                    return new Rook(colour, position.board);
                case Type.Queen:
                    return new Queen(colour, position.board);
                case Type.King:
                    return new King(colour, position.board);
                default:
                    throw new Exception("Invalid Piece Type");
            }
        }

        public int[] GetCoordinates()
        {
            return this.position.GetCoordinates();
        }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            throw new NotImplementedException();
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            throw new NotImplementedException();
        }

        public override BitBoard GetXray(BitBoard occupiedSquares)
        {
            throw new NotImplementedException();
        }

        public override string GetInitial()
        {
            return "";
        }
    }

    public class Pawn : Piece
    {
        public Pawn(Colour colour, ulong position) : base (colour, Type.Pawn, position) { }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            List<Move> moves = new();
            int homeRankShiftValue = 8 + (int)this.colour * (5 * 8);
            BitBoard homeRank = new BitBoard(Board.firstRank).GenShift(homeRankShiftValue);
            BitBoard pushPawnSquare = new BitBoard(this.position).GenShift(8 - ((int)this.colour * 16)); // -8 for black and 8 for white.
            if (!pushPawnSquare.IntersectsWith(occupiedSquares)) // Square is not occupied;
            {
                moves.Add(new Move(this, pushPawnSquare));

                if (homeRank.IntersectsWith(this.position))
                {
                    BitBoard pushTwiceSquare = new(pushPawnSquare);
                    pushTwiceSquare.GenShift(8 - ((int)this.colour * 16));
                    if (!pushTwiceSquare.IntersectsWith(occupiedSquares))
                    {
                        moves.Add(new Move(this, pushTwiceSquare));
                    }
                }
            }

            List<BitBoard> attacks = this.GetAttackMap(occupiedSquares).Enumerate();
            foreach (BitBoard attack in attacks)
            {
                if (attack.IntersectsWith(opponentPieces))
                {
                    moves.Add(new Move(this, attack, true));
                }
            }

            BitBoard captureLeft = new BitBoard(this.position).GenShift(7 - ((int)this.colour * 16));
            BitBoard captureRight = new BitBoard(this.position).GenShift(9 - ((int)this.colour * 16));
            BitBoard enPassantRank = new BitBoard(0x000000FF00000000).GenShift(-8 * (int)this.colour);
            BitBoard opponentHomeRank = this.colour == Colour.White
                ? new BitBoard(Board.firstRank).GenShift(48) : new BitBoard(Board.firstRank).GenShift(8);
            if (lastMove != null && lastMove.piece.type == Type.Pawn // Last move was a pawn move
            && this.position.IntersectsWith(enPassantRank) // This this is on the enpassant rank
                && lastMove.from.IntersectsWith(opponentHomeRank) // The pawn moved from its home square
                && lastMove.to.IntersectsWith(enPassantRank)) // The pawn moved 2 squares
            {
                BitBoard enPassantTarget = new(this.position);
                enPassantTarget.GenShift(-1);
                if (lastMove.to.IsEqual(enPassantTarget))
                {
                    moves.Add(new Move(this, captureLeft, new BitBoard(enPassantTarget)));
                }
                enPassantTarget.GenShift(1);
                enPassantTarget.GenShift(1);
                if (lastMove.to.IsEqual(enPassantTarget))
                {
                    moves.Add(new Move(this, captureRight, new BitBoard(enPassantTarget)));
                }

            }

            return moves;
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            BitBoard attackMap = new(0);
            BitBoard captureLeft = new BitBoard(this.position).GenShift(7 - ((int)this.colour * 16));
            if (!this.position.IntersectsWith(Board.aFile))
            {
                attackMap = attackMap.UnionWith(captureLeft);
            };
            BitBoard captureRight = new BitBoard(this.position).GenShift(9 - ((int)this.colour * 16));
            BitBoard hFile = new BitBoard(Board.aFile).GenShift(7);
            if (!this.position.IntersectsWith(hFile))
            {
                attackMap = attackMap.UnionWith(captureRight);
            };
            return attackMap;
        }

    }

    public class Knight : Piece
    {
        public Knight(Colour colour, ulong position) : base(colour, Type.Knight, position) { }

        public override string GetInitial()
        {
            return "N";
        }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            List<Move> moves = new() { };
            BitBoard friendlyPieces = new BitBoard(occupiedSquares).AndNot(opponentPieces);
            BitBoard attacks = this.GetAttackMap(occupiedSquares).AndNot(friendlyPieces);
            List<BitBoard> knightMoves = attacks.Enumerate();
            for (int i = 0; i < knightMoves.Count; i++)
            {
                moves.Add(new Move(this, knightMoves[i], knightMoves[i].IntersectsWith(opponentPieces)));
            }
            return moves;
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {

            int[] pieceCoords = this.GetCoordinates();
            BitBoard attackMap = new(Board.knightMoveTemplate);
            int verticalShiftCount = pieceCoords[1] - Board.knightMoveTemplateRank;
            int horizontalShiftCount = pieceCoords[0] - Board.knightMoveTemplateFile;
            for (int i = 0; i < Math.Abs(verticalShiftCount); i++)
            {
                if (verticalShiftCount > 0) // Shift up
                {
                    attackMap.StepOne(8);
                }
                else // Shift down
                {
                    attackMap.StepOne(-8);
                }
            }
            for (int i = 0; i < Math.Abs(horizontalShiftCount); i++)
            {
                if (horizontalShiftCount > 0) // Shift right
                {
                    attackMap.StepOne(1);
                }
                else // Shift left
                {
                    attackMap.StepOne(-1);
                }
            }
            return attackMap;
        }
    }

    public class Bishop : Piece
    {
        public override string GetInitial()
        {
            return "B";
        }
        public Bishop(Colour colour, ulong position) : base(colour, Type.Bishop, position) { }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            List<Move> moves = new() { };
            BitBoard friendlyPieces = new BitBoard(occupiedSquares).AndNot(opponentPieces);
            BitBoard attacks = this.GetAttackMap(occupiedSquares).AndNot(friendlyPieces);
            List<BitBoard> bishopMoves = attacks.Enumerate();
            for (int i = 0; i < bishopMoves.Count; i++)
            {
                moves.Add(new Move(this, bishopMoves[i], bishopMoves[i].IntersectsWith(opponentPieces)));
            }
            return moves;
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            return this.position.RayAttack(9, occupiedSquares)
                .UnionWith(this.position.RayAttack(7, occupiedSquares))
                .UnionWith(this.position.RayAttack(-9, occupiedSquares))
                .UnionWith(this.position.RayAttack(-7, occupiedSquares));
        }

        public override BitBoard GetXray(BitBoard occupiedSquares)
        {   
            BitBoard result = this.position.XRay(9, occupiedSquares)
                .UnionWith(this.position.XRay(7, occupiedSquares))
                .UnionWith(this.position.XRay(-9, occupiedSquares))
                .UnionWith(this.position.XRay(-7, occupiedSquares));
            return result;
        }
    }

    public class Rook : Piece
    {
        public override string GetInitial()
        {
            return "R";
        }
        public Rook(Colour colour, ulong position) : base(colour, Type.Rook, position) { }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            List<Move> moves = new() { };
            BitBoard friendlyPieces = new BitBoard(occupiedSquares).AndNot(opponentPieces);
            BitBoard attacks = this.GetAttackMap(occupiedSquares).AndNot(friendlyPieces);
            List<BitBoard> rookMoves = attacks.Enumerate();
            for (int i = 0; i < rookMoves.Count; i++)
            {
                moves.Add(new Move(this, rookMoves[i], rookMoves[i].IntersectsWith(opponentPieces)));
            }
            return moves;
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            return this.position.RayAttack(8, occupiedSquares)
                .UnionWith(this.position.RayAttack(1, occupiedSquares))
                .UnionWith(this.position.RayAttack(-8, occupiedSquares))
                .UnionWith(this.position.RayAttack(-1, occupiedSquares));
        }

        public override BitBoard GetXray(BitBoard occupiedSquares)
        {
            return this.position.XRay(8, occupiedSquares)
                .UnionWith(this.position.XRay(1, occupiedSquares))
                .UnionWith(this.position.XRay(-8, occupiedSquares))
                .UnionWith(this.position.XRay(-1, occupiedSquares));
        }
    }

    public class Queen : Piece
    {
        public override string GetInitial()
        {
            return "Q";
        }
        public Queen(Colour colour, ulong position) : base(colour, Type.Queen, position) { }

        public override List<Move> GetLegalMoves(BitBoard occupiedSquares, BitBoard opponentPieces, Move? lastMove)
        {
            List<Move> moves = new() { };
            BitBoard friendlyPieces = new BitBoard(occupiedSquares).AndNot(opponentPieces);
            BitBoard attacks = this.GetAttackMap(occupiedSquares).AndNot(friendlyPieces);
            List<BitBoard> queenMoves = attacks.Enumerate();
            for (int i = 0; i < queenMoves.Count; i++)
            {
                moves.Add(new Move(this, queenMoves[i], queenMoves[i].IntersectsWith(opponentPieces)));
            }
            return moves;
        }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            return this.position.RayAttack(8, occupiedSquares)
                .UnionWith(this.position.RayAttack(1, occupiedSquares))
                .UnionWith(this.position.RayAttack(-8, occupiedSquares))
                .UnionWith(this.position.RayAttack(-1, occupiedSquares))
                .UnionWith(this.position.RayAttack(9, occupiedSquares))
                .UnionWith(this.position.RayAttack(7, occupiedSquares))
                .UnionWith(this.position.RayAttack(-9, occupiedSquares))
                .UnionWith(this.position.RayAttack(-7, occupiedSquares));
        }
        public override BitBoard GetXray(BitBoard occupiedSquares)
        {
            return this.position.XRay(8, occupiedSquares)
                .UnionWith(this.position.XRay(1, occupiedSquares))
                .UnionWith(this.position.XRay(-8, occupiedSquares))
                .UnionWith(this.position.XRay(-1, occupiedSquares))
                .UnionWith(this.position.XRay(9, occupiedSquares))
                .UnionWith(this.position.XRay(7, occupiedSquares))
                .UnionWith(this.position.XRay(-9, occupiedSquares))
                .UnionWith(this.position.XRay(-7, occupiedSquares));
        }
    }

    public class King : Piece
    {
        public override string GetInitial()
        {
            return "K";
        }

        public King(Colour colour, ulong position) : base(colour, Type.King, position) { }

        public override BitBoard GetAttackMap(BitBoard occupiedSquares)
        {
            return new BitBoard(this.position.board).StepOne(8)
                .UnionWith(new BitBoard(this.position.board).StepOne(1))
                .UnionWith(new BitBoard(this.position.board).StepOne(-8))
                .UnionWith(new BitBoard(this.position.board).StepOne(-1))
                .UnionWith(new BitBoard(this.position.board).StepOne(9))
                .UnionWith(new BitBoard(this.position.board).StepOne(7))
                .UnionWith(new BitBoard(this.position.board).StepOne(-9))
                .UnionWith(new BitBoard(this.position.board).StepOne(-7));
        }
    }
}
