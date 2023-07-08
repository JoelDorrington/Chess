using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

namespace ChessTest
{
    [TestClass]
    public class BitBoardTest
    {
        private const string Expected = "Hello World!";
        [TestMethod]
        public void Exists()
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                Chess.Shared.BitBoard bitboard = new Chess.Shared.BitBoard("1");

                Assert.AreEqual(bitboard.Id, "1");
            }
        }
    }

}