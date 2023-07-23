namespace Chess.Shared
{

    public class Piece
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

        public Piece Clone()
        {
            return new Piece(this.colour, this.type, this.position);
        }

        public int[] GetCoordinates()
        {
            return this.position.GetCoordinates();
        }
    }
}
