namespace Chess.Shared
{
    public struct MoveEval
    {
        public Move move;
        public float eval;
    }
    public class Bot
    {
        int maxDepth = 2;
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

        public Move FindMove()
        {
            Move? bestMove = null;
            float bestMoveScore = float.MinValue;
            List<Move> moves = board.GetAllLegalMoves();
            foreach (Move move in moves)
            {
                Board node = board.MovePiece(move, true);
                node.CalculateThreats(null);
                float score = -NegaMaxAlphaBeta(node, maxDepth-1, float.MinValue, float.MaxValue);
                Console.WriteLine($"{move.GetMoveNotation()} - {score}");
                if(bestMove == null || score > bestMoveScore)
                {
                    bestMove = move;
                    bestMoveScore = score;
                }
            }
            if(bestMove == null) {
                throw new Exception("No Legal Moves!");
            }
            return bestMove;
        }

        float NegaMaxAlphaBeta(Board node, int depth, float alpha, float beta)
        {

            float value = float.MinValue;
            List<Move> moves = node.GetAllLegalMoves();
            if(moves.Count == 0)
            {
                Console.WriteLine($"{node.lastMove?.piece.colour} {node.lastMove?.GetMoveNotation()} - {moves.Count} moves for {node.currentTurn}");
                if (node.check) return float.MinValue;
                return 0;
            }
            if (depth == 0)
            {
                return GetHeuristicValue(node);
            }
            foreach (Move move in moves)
            {
                Board nextNode = node.MovePiece(move, true);
                nextNode.CalculateThreats(null);
                value = Math.Max(value, -NegaMaxAlphaBeta(nextNode, depth - 1, -beta, -alpha));
                alpha = Math.Max(alpha, value);
                if (alpha >= beta) break;
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
            }
            //float friendlyMobility = position.GetAllLegalMoves().Count / 10;
            //float opponentMobility = position.GetOpponentsLegalMoves().Count / 10;
            //score += friendlyMobility - opponentMobility;
            return score;
        }
    }
}
