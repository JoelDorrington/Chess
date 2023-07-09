using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using Chess.Shared;

namespace ChessTest
{
    [TestClass]
    public class BitBoardTest
    {

        private static BitBoard board1 = new BitBoard(0);
        private static BitBoard board2 = new BitBoard(0);
        private static BitBoard board3 = new BitBoard(1);
        [TestMethod]
        public void IsEqual()
        {
            Assert.IsTrue(board1.isEqual(board2));
            Assert.IsFalse(board1.isEqual(board3));
        }

        [TestMethod]
        public void unionWith()
        {
            BitBoard empty = new BitBoard(0x0000);
            BitBoard aPawnPosition = new BitBoard(0x0100);
            BitBoard bPawnPosition = new BitBoard(0x0200);
            empty.unionWith(aPawnPosition);
            empty.unionWith(bPawnPosition);
            BitBoard expected = new BitBoard(0x0300);
            Assert.IsTrue(empty.isEqual(expected));
        }

        [TestMethod]
        public void unionWith2()
        {
            BitBoard empty = new BitBoard(0x0000);
            BitBoard aPawnPosition = new BitBoard(0x0200);
            BitBoard bPawnPosition = new BitBoard(0x0400);
            empty.unionWith(aPawnPosition);
            empty.unionWith(bPawnPosition);
            BitBoard expected = new BitBoard(0x0600);
            Assert.IsTrue(empty.isEqual(expected));
        }
    }

    [TestClass]
    public class PieceTest
    {

        [TestMethod]
        public void Constructor()
        {
            Piece pawn1 = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x0100);
            Piece pawn2 = new Piece(Piece.Colour.White, Piece.Type.Pawn, 0x0200);
            Assert.IsFalse(pawn1.position.isEqual(pawn2.position));
        }
    }

    [TestClass]
    public class BoardTest
    {
        private Board testBoard = new Board(null);
        private BitBoard fullBoard = new BitBoard(0xFFFF00000000FFFF);
        [TestMethod]
        public void resetBoard()
        {
            testBoard.resetBoard();
            BitBoard occupiedSquares = testBoard.getOccupiedSquares();
            Assert.IsTrue(occupiedSquares.isEqual(fullBoard));
            Piece? aRookBlack = testBoard.getPieceAtSquare(new BitBoard(0x100000000000000));
            Piece? aPawnWhite = testBoard.getPieceAtSquare(new BitBoard(0x100));
            Assert.IsNotNull(aPawnWhite);
            Assert.AreEqual(aPawnWhite.colour, Piece.Colour.White);
            Assert.AreEqual(aPawnWhite.type, Piece.Type.Pawn);
            Assert.IsNotNull(aRookBlack);
            Assert.AreEqual(aRookBlack.colour, Piece.Colour.Black);
            Assert.AreEqual(aRookBlack.type, Piece.Type.Rook);

        }
    }

}