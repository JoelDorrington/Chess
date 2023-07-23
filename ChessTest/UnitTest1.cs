using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Chess.Shared;

namespace ChessTest
{
    [TestClass]
    public class BitBoardTest
    {

        private readonly BitBoard board1 = new(0);
        private readonly BitBoard board2 = new(0);
        private readonly BitBoard board3 = new(1);
        [TestMethod]
        public void IsEqual()
        {
            Assert.IsTrue(board1.IsEqual(board2));
            Assert.IsFalse(board1.IsEqual(board3));
        }

        [TestMethod]
        public void UnionWith()
        {
            BitBoard aPawnPosition = new(1 << 8);
            BitBoard bPawnPosition = new(1 << 9);
            BitBoard result = aPawnPosition.UnionWith(bPawnPosition);
            BitBoard expected = new(0x0300);
            Assert.IsTrue(result.IsEqual(expected));
        }

        [TestMethod]
        public void UnionWith2()
        {
            BitBoard bPawnPosition = new(1 << 9);
            BitBoard cPawnPosition = new(1 << 10);
            BitBoard result = bPawnPosition.UnionWith(cPawnPosition);
            BitBoard expected = new(0x0600);
            Assert.IsTrue(result.IsEqual(expected));
        }

        [TestMethod]
        public void IntersectionOf()
        {
            BitBoard secondRank = new(Board.firstRank << 8);
            BitBoard c2Pawn = new(1 << 10);
            BitBoard result = secondRank.IntersectionOf(c2Pawn);
            Assert.IsFalse(result.IsEqual(0));
            Assert.IsTrue(result.IsEqual(c2Pawn));
        }

        [TestMethod]
        public void IntersectionOf2()
        {
            BitBoard secondRank = new(Board.firstRank << 8);
            BitBoard c3 = new(1 << 18);
            BitBoard result = secondRank.IntersectionOf(c3);
            Assert.IsTrue(result.IsEqual(0));
            Assert.IsFalse(result.IsEqual(c3));
        }

        //[TestMethod]
        //public void rotateLeft()
        //{
        //    BitBoard aPawnPosition = new BitBoard(1 << 8);
        //    BitBoard hPawnPosition = new BitBoard(1 << 15);
        //    aPawnPosition.rotateLeft();
        //    Assert.IsTrue(aPawnPosition.isEqual(hPawnPosition));

        //}
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
            Piece? dPawn = testBoard.GetPieceAtSquare(new BitBoard(1 << 11)); // d2
            Assert.IsNotNull(dPawn);
            Assert.AreEqual(dPawn.colour, Piece.Colour.White);
            Assert.AreEqual(dPawn.type, Piece.Type.Pawn);
            List<Move> possibleMoves = testBoard.GetLegalMovesForPiece(dPawn);
            BitBoard expectedToD3 = new(1 << 19); // d3
            BitBoard expectedToD4 = new(1 << 27); // d4
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
            Board initialBoard = new();
            Move d3 = new(initialBoard.GetPieceAtSquare(new BitBoard(1 << 11)), new BitBoard(1<<19));
            Board move1W = initialBoard.MovePiece(d3);
            Piece? pushedDPawn = move1W.GetPieceAtSquare(new BitBoard(1 << 19));
            Assert.IsNotNull(pushedDPawn);
            Assert.IsFalse(pushedDPawn == d3.piece);
        }

        [TestMethod]
        public void PlayD4()
        {
            Board initialBoard = new();
            Move d4 = new(initialBoard.GetPieceAtSquare(new BitBoard(1 << 11)), new BitBoard(1 << 27));
            Board move1W = initialBoard.MovePiece(d4);
            Piece? pushedDPawn = move1W.GetPieceAtSquare(new BitBoard(1 << 27));
            Assert.IsNotNull(pushedDPawn);
            Assert.IsFalse(pushedDPawn == d4.piece);
        }

        [TestMethod]
        public void PlayD5()
        {
            Board initialBoard = new();
            BitBoard d7 = new(1);
            d7.GenShift(51);
            Piece? d7Pawn = initialBoard.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            BitBoard d5 = new(1);
            d5.GenShift(35);
            Move pawnToD5 = new(d7Pawn, d5);
            Board move1B;
            try
            {
                move1B = initialBoard.MovePiece(pawnToD5);
                throw new Exception("Must Fail");
            } catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Not Black's Turn");
            }

            Move d4 = new(initialBoard.GetPieceAtSquare(new BitBoard(1 << 11)), new BitBoard(1 << 27));
            Board move1W = initialBoard.MovePiece(d4);

            d7Pawn = move1W.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            pawnToD5 = new(d7Pawn, d5);
            move1B = move1W.MovePiece(pawnToD5);
            Piece? d5Pawn = move1B.GetPieceAtSquare(new BitBoard(1 << 35));
            Assert.IsNotNull(d5Pawn);
            Assert.IsFalse(d5Pawn == d7Pawn);
        }

        [TestMethod]
        public void QueensGambitAccepted()
        {
            Board initialBoard = new();
            BitBoard d7 = new(1);
            d7.GenShift(51);
            Piece? d7Pawn = initialBoard.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            BitBoard d5 = new(1);
            d5.GenShift(35);
            Move pawnToD5 = new(d7Pawn, d5);
            Board move1B;
            try
            {
                move1B = initialBoard.MovePiece(pawnToD5);
                throw new Exception("Must Fail");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Not Black's Turn");
            }

            Move d4 = new(initialBoard.GetPieceAtSquare(new BitBoard(1 << 11)), new BitBoard(1 << 27));
            Board move1W = initialBoard.MovePiece(d4);

            d7Pawn = move1W.GetPieceAtSquare(d7);
            Assert.IsNotNull(d7Pawn);
            pawnToD5 = new(d7Pawn, d5);
            move1B = move1W.MovePiece(pawnToD5);
            Piece? d5Pawn = move1B.GetPieceAtSquare(new BitBoard(1 << 35));
            Assert.IsNotNull(d5Pawn);
            Assert.IsFalse(d5Pawn == d7Pawn);

            BitBoard c2 = new(1);
            c2.GenShift(10);
            BitBoard c4 = new(1);
            c4.GenShift(26);
            Piece? c2Pawn = move1B.GetPieceAtSquare(c2);
            Assert.IsNotNull(c2Pawn);
            Move pawnToC4 = new(c2Pawn, c4);
            Board move2W = move1B.MovePiece(pawnToC4);

            d5Pawn = move2W.GetPieceAtSquare(d5);
            Piece? c4Pawn = move2W.GetPieceAtSquare(c4);
            Assert.IsNotNull(d5Pawn);
            Assert.IsNotNull(c4Pawn);
            int index = Array.IndexOf(move2W.pieces, c4Pawn);
            Move d5xc4 = new(d5Pawn, c4);
            Board move2B = move2W.MovePiece(d5xc4);
            Piece capturedPawn = move2B.pieces[index];
            Assert.IsTrue(capturedPawn.position.IsEqual(0));
            Assert.IsNotNull(move2W.movePlayed);
            Assert.AreEqual(d5xc4.piece, move2W.movePlayed.piece);
            Assert.IsTrue(d5xc4.to.IsEqual(move2W.movePlayed.to));
            Assert.IsNull(move2B.movePlayed);
        }
    }

}