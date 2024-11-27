using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Chess.Shared;
using System.Diagnostics;

namespace ChessTest
{
    [TestClass]
    public class BitBoardTest
    {

        private readonly BitBoard board1 = new(0);
        private readonly BitBoard board2 = new(0);
        private readonly BitBoard board3 = new(1);

        [TestMethod]
        public void ConstructorTest()
        {
            ulong d2Uint = 1 << 11;
            BitBoard d2 = new BitBoard("d2");
            Assert.AreEqual(d2Uint, d2.board);
        }

        [TestMethod]
        public void IsEqual()
        {
            Assert.IsTrue(board1.IsEqual(board2));
            Assert.IsFalse(board1.IsEqual(board3));
        }

        [TestMethod]
        public void UnionWith()
        {
            BitBoard aPawnPosition = new("a2");
            BitBoard bPawnPosition = new("b2");
            BitBoard result = aPawnPosition.UnionWith(bPawnPosition);
            BitBoard expected = new(0x0300);
            Assert.IsTrue(result.IsEqual(expected));
        }

        [TestMethod]
        public void UnionWith2()
        {
            BitBoard bPawnPosition = new("b2");
            BitBoard cPawnPosition = new("c2");
            BitBoard result = bPawnPosition.UnionWith(cPawnPosition);
            BitBoard expected = new(0x0600);
            Assert.IsTrue(result.IsEqual(expected));
        }

        [TestMethod]
        public void IntersectionOf()
        {
            BitBoard secondRank = new(Board.firstRank << 8);
            BitBoard c2Pawn = new("c2");
            BitBoard result = secondRank.IntersectionOf(c2Pawn);
            Assert.IsFalse(result.IsEqual(0));
            Assert.IsTrue(result.IsEqual(c2Pawn));
        }

        [TestMethod]
        public void IntersectionOf2()
        {
            BitBoard secondRank = new(Board.firstRank << 8);
            BitBoard c3 = new("c3");
            BitBoard result = secondRank.IntersectionOf(c3);
            Assert.IsTrue(result.IsEqual(0));
            Assert.IsFalse(result.IsEqual(c3));
        }

        [TestMethod]
        public void StepOne()
        {
            BitBoard test = new("c5");
            BitBoard d6 = new("d6");
            BitBoard e6 = new("e6");
            BitBoard f5 = new("f5");
            BitBoard f4 = new("f4");
            BitBoard e3 = new("e3");
            BitBoard d3 = new("d3");
            BitBoard c4 = new("c4");
            BitBoard c5 = new("c5");
            test.StepOne(9);
            Assert.AreEqual(d6.board, test.board);
            test.StepOne(1);
            Assert.AreEqual(e6.board, test.board);
            test.StepOne(-7);
            Assert.AreEqual(f5.board, test.board);
            test.StepOne(-8);
            Assert.AreEqual(f4.board, test.board);
            test.StepOne(-9);
            Assert.AreEqual(e3.board, test.board);
            test.StepOne(-1);
            Assert.AreEqual(d3.board, test.board);
            test.StepOne(7);
            Assert.AreEqual(c4.board, test.board);
            test.StepOne(8);
            Assert.AreEqual(c5.board, test.board);

        }

        [TestMethod]
        public void EnumeratedRayAttack()
        {
            BitBoard symmetricalBishops = new(0xDFEF00141400EFDF);
            BitBoard c5 = new("c5");
            IEnumerable<BitBoard> attacks = c5.EnumeratedRayAttack(9, symmetricalBishops);
            Assert.AreEqual(3, attacks.Count());
        }

        [TestMethod]
        public void Enumerate()
        {
            BitBoard knightTemplate = new(Board.knightMoveTemplate);
            List<BitBoard> moves = knightTemplate.Enumerate();
            Assert.AreEqual(8, moves.Count);
        }
}

    [TestClass]
    public class PieceTest
    {

        [TestMethod]
        public void IsEqual()
        {
            Piece pawn1 = new(Piece.Colour.White, Piece.Type.Pawn, 0x0100);
            Piece pawn2 = new(Piece.Colour.White, Piece.Type.Pawn, 0x0200);
            Assert.IsFalse(pawn1.position.IsEqual(pawn2.position));
        }
        
        [TestMethod]
        public void GetCoordinates()
        {
            Piece pawn1 = new(Piece.Colour.White, Piece.Type.Pawn, 0x0100);
            Piece pawn2 = new(Piece.Colour.White, Piece.Type.Pawn, 0x0200);
            int[] coordinates1 = pawn1.GetCoordinates();
            int[] coordinates2 = pawn2.GetCoordinates();
            Assert.AreEqual(0, coordinates1[0]);
            Assert.AreEqual(1, coordinates1[1]);
            Assert.AreEqual(1, coordinates2[0]);
            Assert.AreEqual(1, coordinates2[1]);
        }

        [TestMethod]
        public void GetEnumeratedAttackMap()
        {
            BitBoard fullBoard = new(0xFFFF00000000FFFF);
            BitBoard scandinavianPosition = new(0xFFF700081000EFFF);
            BitBoard symmetricalBishops = new(0xDFEF00141400EFDF);
            BitBoard d5 = new(1);
            d5.GenShift(35);
            Pawn pawn = new Pawn(Piece.Colour.White, d5.board);
            IEnumerable<BitBoard> boards = pawn.EnumerateAttackMap(scandinavianPosition);
            Assert.AreEqual(2, boards.Count());

            BitBoard c5 = new(0x0000000400000000);
            Bishop bishop = new Bishop(Piece.Colour.Black, c5.board);
            var bishopAttacks = bishop.EnumerateAttackMap(symmetricalBishops);
            symmetricalBishops.LogPosition();
            Assert.AreEqual(10, bishopAttacks.Count());
        }

        [TestMethod]
        public void GetLegalMoves()
        {
            BitBoard symmetricalBishops = new(0xDFEF00141400EFDF);
            BitBoard opponentPieces = new(0x000000001400EFDF);
            BitBoard c5 = new(0x0000000400000000);
            Bishop bishop = new Bishop(Piece.Colour.Black, c5.board);
            List<Move> moves = bishop.GetLegalMoves(symmetricalBishops, opponentPieces, null);
            Assert.AreEqual(9, moves.Count);
        }
    }

    [TestClass]
    public class BoardTest
    {
        private readonly BitBoard fullBoard = new(0xFFFF00000000FFFF);
        [TestMethod]
        public void ResetBoard()
        {
            Board testBoard = new();
            BitBoard occupiedSquares = testBoard.GetOccupiedSquares();
            Assert.IsTrue(occupiedSquares.IsEqual(fullBoard));
            Piece? aRookBlack = testBoard.GetPieceAtSquare(new BitBoard(0x100000000000000));
            Piece? aPawnWhite = testBoard.GetPieceAtSquare(new BitBoard(0x100));
            Assert.IsNotNull(aPawnWhite);
            Assert.AreEqual(aPawnWhite.colour, Piece.Colour.White);
            Assert.AreEqual(aPawnWhite.type, Piece.Type.Pawn);
            Assert.IsNotNull(aRookBlack);
            Assert.AreEqual(aRookBlack.colour, Piece.Colour.Black);
            Assert.AreEqual(aRookBlack.type, Piece.Type.Rook);

        }

        [TestMethod]
        public void GetLayout()
        {
            Board testBoard = new();
            Piece[][] layout = testBoard.GetLayout();
            Assert.AreEqual(Piece.Type.Rook, layout[0][0].type);
            Assert.AreEqual(Piece.Type.Knight, layout[1][0].type);
            Assert.AreEqual(Piece.Type.Bishop, layout[2][0].type);
            Assert.AreEqual(Piece.Type.Queen, layout[3][0].type);
            Assert.AreEqual(Piece.Type.King, layout[4][0].type);
            Assert.AreEqual(Piece.Type.Bishop, layout[5][0].type);
            Assert.AreEqual(Piece.Type.Knight, layout[6][0].type);
            Assert.AreEqual(Piece.Type.Rook, layout[7][0].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[0][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[1][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[2][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[3][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[4][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[5][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[6][1].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[7][1].type);

            Assert.AreEqual(Piece.Type.Pawn, layout[0][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[1][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[2][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[3][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[4][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[5][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[6][6].type);
            Assert.AreEqual(Piece.Type.Pawn, layout[7][6].type);
            Assert.AreEqual(Piece.Type.Rook, layout[0][7].type);
            Assert.AreEqual(Piece.Type.Knight, layout[1][7].type);
            Assert.AreEqual(Piece.Type.Bishop, layout[2][7].type);
            Assert.AreEqual(Piece.Type.Queen, layout[3][7].type);
            Assert.AreEqual(Piece.Type.King, layout[4][7].type);
            Assert.AreEqual(Piece.Type.Bishop, layout[5][7].type);
            Assert.AreEqual(Piece.Type.Knight, layout[6][7].type);
            Assert.AreEqual(Piece.Type.Rook, layout[7][7].type);
        }

        [TestMethod]
        public void GetLegalMovesForPiece()
        {
            Board testBoard = new();
            Piece? dPawn = testBoard.GetPieceAtSquare(new BitBoard("d2")); // d2
            Assert.IsNotNull(dPawn);
            Assert.AreEqual(dPawn.colour, Piece.Colour.White);
            Assert.AreEqual(dPawn.type, Piece.Type.Pawn);
            List<Move> possibleMoves = testBoard.GetLegalMovesForPiece(dPawn);
            BitBoard expectedToD3 = new("d3"); // d3
            BitBoard expectedToD4 = new("d4"); // d4
            Assert.AreEqual(possibleMoves.Count(), 2);
            Assert.AreEqual(possibleMoves[0].piece, dPawn);
            Assert.IsTrue(possibleMoves[0].to.IsEqual(expectedToD3));
            Assert.AreEqual(possibleMoves[1].piece, dPawn);
            Assert.IsTrue(possibleMoves[1].to.IsEqual(expectedToD4));
        }

        [TestMethod]
        public void GetOccupiedSquares()
        {
            Board testBoard = new();
            testBoard.ResetBoard();
            BitBoard blackPieces = testBoard.GetOccupiedSquares(Piece.Colour.Black);
            BitBoard whitePieces = testBoard.GetOccupiedSquares(Piece.Colour.White);
            Assert.IsTrue(blackPieces.IsEqual(0xFFFF000000000000));
            Assert.IsTrue(whitePieces.IsEqual(0x000000000000FFFF));
        }

        [TestMethod]
        public void Move()
        {
            Board board = new();
            Move d3 = new(board.GetPieceAtSquare(new BitBoard("d2")), new BitBoard("d3"));
            board.MovePiece(d3);
            Piece? pushedDPawn = board.GetPieceAtSquare(new BitBoard("d3"));
            Assert.IsNotNull(pushedDPawn);
            Assert.IsTrue(pushedDPawn == d3.piece);
        }

        [TestMethod]
        public void UnMove()
        {
            Board board = new();
            Assert.AreEqual((uint)0b1111, board.bitmask);
            var d2Square = new BitBoard("d2");
            Move d3 = new(board.GetPieceAtSquare(d2Square), new BitBoard("d3"));
            board.MovePiece(d3);
            Piece? pushedDPawn = board.GetPieceAtSquare(new BitBoard("d3"));
            Assert.IsNotNull(pushedDPawn);
            Assert.IsTrue(pushedDPawn == d3.piece);
            Assert.AreEqual((uint)0b1111, board.bitmask);
            board.bitmask = 0b1011;
            board.UnMove();
            Assert.AreEqual(pushedDPawn.position.board, d2Square.board);
            Assert.AreEqual((uint)0b1111, board.bitmask);
        }

        [TestMethod]
        public void PlayD4()
        {
            Board board = new();
            Move d4 = new(board.GetPieceAtSquare(new BitBoard("d2")), new BitBoard("d4"));
            board.MovePiece(d4);
            Piece? pushedDPawn = board.GetPieceAtSquare(new BitBoard("d4"));
            Assert.IsNotNull(pushedDPawn);
            Assert.IsTrue(pushedDPawn == d4.piece);
        }

        [TestMethod]
        public void PlayD5()
        {
            Board board = new();
            BitBoard d7 = new(1);
            d7.GenShift(51);
            Piece? d7Pawn = board.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            BitBoard d5 = new(1);
            d5.GenShift(35);
            Move pawnToD5 = new(d7Pawn, d5);
            try
            {
                board.MovePiece(pawnToD5);
                throw new Exception("Must Fail");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Not Black's Turn");
            }

            Move d4 = new(board.GetPieceAtSquare(new BitBoard("d2")), new BitBoard("d4"));
            board.MovePiece(d4);

            d7Pawn = board.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            pawnToD5 = new(d7Pawn, d5);
            board.MovePiece(pawnToD5);
            Piece? d5Pawn = board.GetPieceAtSquare(d5);
            Assert.IsNotNull(d5Pawn);
            Assert.IsTrue(d5Pawn == d7Pawn);
        }

        [TestMethod]
        public void QueensGambitAccepted()
        {
            Board board = new();

            Move d4 = new(board.GetPieceAtSquare(new BitBoard("d2")), new BitBoard("d4"));
            board.MovePiece(d4);

            BitBoard d7 = new(1);
            d7.GenShift(51);
            Piece? d7Pawn = board.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            BitBoard d5 = new(1);
            d5.GenShift(35);

            d7Pawn = board.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            Move pawnToD5 = new(d7Pawn, d5);
            board.MovePiece(pawnToD5);
            Piece? d5Pawn = board.GetPieceAtSquare(d5);
            Assert.IsNotNull(d5Pawn);
            Assert.IsTrue(d5Pawn == d7Pawn);

            BitBoard c2 = new(1);
            c2.GenShift(10);
            BitBoard c4 = new(1);
            c4.GenShift(26);
            Piece? c2Pawn = board.GetPieceAtSquare(c2);
            Assert.IsNotNull(c2Pawn);
            Move pawnToC4 = new(c2Pawn, c4);
            board.MovePiece(pawnToC4);

            d5Pawn = board.GetPieceAtSquare(d5);
            Piece? c4Pawn = board.GetPieceAtSquare(c4);
            Assert.IsNotNull(d5Pawn);
            Assert.IsNotNull(c4Pawn);
            Move d5xc4 = new(d5Pawn, c4, c4);
            board.MovePiece(d5xc4);
            Assert.IsTrue(c4Pawn.position.IsEqual(0));
            Assert.IsNotNull(board.LastMove);
            Assert.AreEqual(d5xc4.piece, board.LastMove.piece);
            Assert.IsTrue(d5xc4.to.IsEqual(board.LastMove.to));
        }

        // 1. e4,  Nf6 2. Nc3,  e5 3. Bc4,  Bb4 4. d3,  Kg8 5. Nf3,  d6 6. Kg1,  Bg4 7. h3,  Bxf3 8. Qxf3,  d5 9. Nxd5,  Nxd5 10. Bxd5,  c6 11. Bb3,  Qd4 12. c3,  Qc5 13. axb4,  Qxb4 14. Qg3,  Nd7 15. Bh6,  g6 16. h4,  Re8 17. h5,  Re7 18. axg6,  gxg6 19. f4,  Qb6+ 20. Kh2,  Kh8 
        [TestMethod]
        public void ReproduceStackOverflow()
        {
            Board board = new();
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("e2")), new("e4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g8")), new("f6")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("b1")), new("c3")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("e7")), new("e5")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f1")), new("c4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f8")), new("b4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d2")), new("d3")));
            Move blackCastleShort = new(board.GetPieceAtSquare(new BitBoard("e8")), new("g8"));
            blackCastleShort.castleDirection = Chess.Shared.Move.CastleDirection.Short;
            board.MovePiece(blackCastleShort);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g1")), new("f3")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d7")), new("d6")));
            Move whiteCastleShort = new(board.GetPieceAtSquare(new BitBoard("e1")), new("g1"));
            whiteCastleShort.castleDirection = Chess.Shared.Move.CastleDirection.Short;
            board.MovePiece(whiteCastleShort);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("c8")), new("g4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("h2")), new("h3")));
            Move Bxf3 = new(board.GetPieceAtSquare(new BitBoard("g4")), new("f3"));
            Bxf3.isCapture = true;
            board.MovePiece(Bxf3);
            Move Qxf3 = new(board.GetPieceAtSquare(new BitBoard("d1")), new("f3"));
            Qxf3.isCapture = true;
            board.MovePiece(Qxf3);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d6")), new("d5")));
            Move Nxd5 = new(board.GetPieceAtSquare(new BitBoard("c3")), new("d5"));
            Nxd5.isCapture = true;
            board.MovePiece(Nxd5);
            Move Nxd5_2 = new(board.GetPieceAtSquare(new BitBoard("f6")), new("d5"));
            Nxd5_2.isCapture = true;
            board.MovePiece(Nxd5_2);
            Move Bxd5 = new(board.GetPieceAtSquare(new BitBoard("c4")), new("d5"));
            Bxd5.isCapture = true;
            board.MovePiece(Bxd5);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("c7")), new("c6")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d5")), new("b3")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d8")), new("d4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("c2")), new("c3")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("d4")), new("c5")));
            Move cxb4 = new(board.GetPieceAtSquare(new BitBoard("c3")), new("b4"));
            cxb4.isCapture = true;
            board.MovePiece(cxb4);
            Move Qxb4 = new(board.GetPieceAtSquare(new BitBoard("c5")), new("b4"));
            Qxb4.isCapture = true;
            board.MovePiece(Qxb4);
            //// 14. Qg3,  Nd7 15. Bh6,  g6 16. h4,  Re8 17. h5,  Re7 18. axg6,  gxg6 19. f4,  Qb6+ 20. Kh2,  Kh8
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f3")), new("g3")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("b8")), new("d7")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("c1")), new("h6")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g7")), new("g6")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("h3")), new("h4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f8")), new("e8")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("h4")), new("h5")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("e8")), new("e7")));
            Move hxg6 = new(board.GetPieceAtSquare(new BitBoard("h5")), new("g6"));
            hxg6.isCapture = true;
            board.MovePiece(hxg6);
            Move hxg6_2 = new(board.GetPieceAtSquare(new BitBoard("h7")), new("g6"));
            hxg6_2.isCapture = true;
            board.MovePiece(hxg6_2);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f2")), new("f4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("b4")), new("b6")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g1")), new("h2")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g8")), new("h8")));
            Move Bxf7 = new(board.GetPieceAtSquare(new BitBoard("b2")), new("f7"));
            Bxf7.isCapture = true;
            board.MovePiece(Bxf7);
            Move Qxb2 = new(board.GetPieceAtSquare(new BitBoard("b6")), new("b2"));
            Qxb2.isCapture = true;
            board.MovePiece(Qxb2);
            Move Qxg6 = new(board.GetPieceAtSquare(new BitBoard("g3")), new("g6"));
            Qxg6.isCapture = true;
            board.MovePiece(Qxg6);
            Move exf4 = new(board.GetPieceAtSquare(new BitBoard("e5")), new("f4"));
            exf4.isCapture = true;
            board.MovePiece(exf4);
            Move Rxf4 = new(board.GetPieceAtSquare(new BitBoard("f1")), new("f4"));
            Rxf4.isCapture = true;
            board.MovePiece(Rxf4);
            Move Qxa1 = new(board.GetPieceAtSquare(new BitBoard("b2")), new("a1"));
            Qxa1.isCapture = true;
            board.MovePiece(Qxa1);
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("f4")), new("h4")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("a1")), new("e5")));
            board.MovePiece(new(board.GetPieceAtSquare(new BitBoard("g2")), new("g3")));

            Bot bot = new(board);
            Move move = bot.FindMove(); // CANNOT REPRODUCE??
        }
    }

    [TestClass]
    public class MoveTest
    {
        private static Piece testPiece = new Pawn(Piece.Colour.White, 1<<11);
        [TestMethod]
        public void Move()
        {
            Move move = new(MoveTest.testPiece, new BitBoard("d3"));
            Assert.IsNotNull(move);
            Assert.IsFalse(move.isCapture);
            move.isCapture = true;
            Assert.IsTrue(move.isCapture);
            move.isCapture = false;
            Assert.IsFalse(move.isCapture);
        }
    }

}