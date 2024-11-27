using System.Runtime.CompilerServices;

namespace Chess.Shared
{
    public struct MoveEval
    {
        public Move move;
        public float eval;
    }
    public class Bot
    {
        private static ulong center = 0x0000001818000000;
        private static ulong semiCenter = 0x00003C24243C0000;
        private static ulong edge = 0xFF818181818181FF;
        private static ulong castled = 0xC7000000000000C7;
        private static ulong[] homeRanks = { 0x00000000000000FF, 0xFF00000000000000 };
        int maxDepth = 3;
        public bool thinking = false;
        public Piece.Colour colour = Piece.Colour.Black;
        Board board;
        public Bot(Board board)
        {
            this.board = board;
        }
        public Bot(Board board, Piece.Colour colour)
        {
            this.board = board;
            this.colour = colour;
        }

        public async Task<Move> FindMoveAsync()
        {
            return await Task.Run(FindMove);
        }

        public Move FindMove()
        {
            thinking = true;
            Move? bestMove = null;
            float bestMoveScore = float.MinValue;
            foreach (Move move in board.GenerateMoves())
            {
                try
                {
                    board.MovePiece(move, true);
                    float score = -NegaMaxAlphaBeta(maxDepth - 1, float.MinValue, float.MaxValue);
                    board.UnMove();
                    if (bestMove == null || score > bestMoveScore)
                    {
                        bestMove = move;
                        bestMoveScore = score;
                    }
                } catch (Exception e)
                {
                    if (e.Message != "King has been left en prise")
                    {
                        Console.Error.WriteLine(e);
                    }
                    continue;
                }
            }
            if(bestMove == null) {
                throw new Exception("No Legal Moves!");
            }
            thinking = false;
            return bestMove;
        }

        float NegaMaxAlphaBeta(int depth, float alpha, float beta)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            if (board.LastMove?.isCheckMate == true) return float.MinValue;
            if (board.LastMove?.isDraw == true) return 0;
            if (depth <= 0) return GetHeuristicValue(board);

            float value = float.MinValue;
            foreach (Move move in board.GenerateMoves())
            {
                try
                {
                    board.MovePiece(move, true);
                    value = Math.Max(value, -NegaMaxAlphaBeta(depth - 1, -beta, -alpha));
                    board.UnMove();
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta) break;
                } catch (Exception e)
                {
                    if(e.Message != "King has been left en prise")
                    {
                        Console.Error.WriteLine(e);
                    }
                    continue;
                }
            }
            return value;
        }

        /*
         *   f(p) = 200(K-K')
                    + 9(Q-Q')
                    + 5(R-R')
                    + 3(B-B' + N-N')
                    + 1(P-P')
                    - 0.5(D-D' + S-S' + I-I')
                    + 0.1(M-M') + ...

            KQRBNP = number of kings, queens, rooks, bishops, knights and pawns
            D,S,I = doubled, blocked and isolated pawns TODO
            M = Mobility (the number of legal moves)
        */
        float GetHeuristicValue(Board? position = null)
        {
            position ??= board;
            float score = 0;
            for (uint i = 0; i < position.pieces.Length; i += 1)
            {
                Piece piece = position.pieces[i];
                if (piece.position.board == 0) continue;
                int modifier = piece.colour == position.currentTurn ? 1 : -1;
                if(piece.type == Piece.Type.King)
                {
                    score += 200 * modifier;
                }
                if (piece.type == Piece.Type.Queen)
                {
                    score += 9 * modifier;
                }
                if (piece.type == Piece.Type.Rook)
                {
                    score += 5 * modifier;
                }
                if (piece.type == Piece.Type.Bishop || piece.type == Piece.Type.Knight)
                {
                    score += 3 * modifier;
                }
                if (piece.type == Piece.Type.Pawn)
                {
                    score += 1 * modifier;
                }
                if (piece.position.IntersectsWith(center))
                {
                    if (piece.type == Piece.Type.Pawn) score += (float)0.5 * modifier;
                    else score += (float)0.2 * modifier;
                }
                if (piece.position.IntersectsWith(semiCenter))
                {
                    score += (float)0.1 * modifier;
                }
                if (piece.type == Piece.Type.Knight && piece.position.IntersectsWith(edge))
                {
                    score += (float)-0.1 * modifier;
                }
                if (piece.type == Piece.Type.King && piece.position.IntersectsWith(castled))
                {
                    score += (float)0.5 * modifier;
                }
                if(piece.type != Piece.Type.King && piece.type != Piece.Type.Rook 
                    && piece.type != Piece.Type.Pawn 
                    && piece.position.IntersectsWith(homeRanks[(int)piece.colour]))
                {
                    score += (float)-0.1 * modifier;
                }
            }
            //float friendlyMobility = position.GetAllLegalMoves().Count / 10;
            //float opponentMobility = position.GetOpponentsLegalMoves().Count / 10;
            //score += friendlyMobility - opponentMobility;
            return score;
        }
    }
}
