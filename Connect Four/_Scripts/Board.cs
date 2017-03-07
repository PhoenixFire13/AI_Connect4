using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board {

    /// <summary>
    /// The numbers on the board represent an empty piece, player 1 piece, or player 2 piece.
    /// </summary>
    public enum Piece
    {
        Empty = 0,
        Player1 = 1,
        Player2 = 2
    }

    /// <summary>
    /// Access the two dimensional array once the board has been instantiated.
    /// </summary>
    public int[,] NumBoard
    {
        get
        {
            return numBoard;
        }
    }

    /// <summary>
    /// Access the number of rows.
    /// </summary>
    public int Rows
    {
        get
        {
            return numRows;
        }
    }

    /// <summary>
    /// Access the number of columns.
    /// </summary>
    public int Cols
    {
        get
        {
            return numCols;
        }
    }

    private int numRows;
    private int numCols;
    private int[,] numBoard;

    /// <summary>
    /// Constructor for the number representation of the board.
    /// </summary>
    /// <param name="rows">The number of rows in the board.</param>
    /// <param name="cols">The number of columns in the board.</param>
    public Board (int rows, int cols)
    {
        numRows = rows;
        numCols = cols;

        //Create the two dimensional board and fill it with empty pieces.
        numBoard = new int[cols, rows];
        for (int i = 0; i < cols; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                numBoard[i, j] = (int)Piece.Empty;
            }
        }
    }

    /// <summary>
    /// Checks if the board has an empty cell.
    /// </summary>
    /// <returns>True if there is an empty cell, false otherwise.</returns>
    public bool containsEmptyCell()
    {
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if (numBoard[i, j] == (int)Piece.Empty)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if there is an empty cell in a row.
    /// </summary>
    /// <param name="col">The column to be checked for an empty cell.</param>
    /// <returns>True if there is an empty cell in the column, false otherwise.</returns>
    public bool containsEmptyCell(int col)
    {
        return numBoard[col, 0] == (int)Piece.Empty;
    }

    /// <summary>
    /// Gets the row number of the empty cell in the column.
    /// </summary>
    /// <param name="col">The column to be checked.</param>
    /// <returns>The row number of the empty cell. If there is no empty cell, then return -1.</returns>
    public int getEmptyCell(int col)
    {
        if (containsEmptyCell(col))
        {
            for (int i = 0; i < numRows; i++)
            {
                if (numBoard[col, i] == (int)Piece.Empty)
                    continue;
                else
                    return i - 1;
            }
            return numRows - 1;
        }

        return -1;
    }

    /// <summary>
    /// Gets all of the possible moves on the board.
    /// </summary>
    /// <returns>List of the column of all possible moves.</returns>
    public List<int> getPossibleMoves()
    {
        List<int> moves = new List<int>();
        for (int i = 0; i < numCols; i++)
        {
            if (numBoard[i, 0] == (int)Piece.Empty)
            {
                moves.Add(i);
            }
        }

        return moves;
    }

    /// <summary>
    /// Sets a cell on the board to a piece.
    /// </summary>
    /// <param name="col">The column number.</param>
    /// <param name="row">The row number.</param>
    /// <param name="piece">The piece to set the cell to.</param>
    public void setCell(int col, int row, Piece piece)
    {
        numBoard[col, row] = (int)piece;
    }

    /// <summary>
    /// Gets the piece at a cell on the board.
    /// </summary>
    /// <param name="col">The column number.</param>
    /// <param name="row">The row number.</param>
    /// <returns>The piece on the board at the cell.</returns>
    public int getCell(int col, int row)
    {
        return numBoard[col, row];
    }

    /// <summary>
    /// Gets the number of single pieces on the board.
    /// </summary>
    /// <param name="p">The piece to be counted.</param>
    /// <returns>The number of single pieces.</returns>
    public int countSingles(Piece p)
    {
        int count = 0;
        foreach(int i in numBoard)
        {
            if ((int)p == i)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Gets the number of two in a row on the board.
    /// </summary>
    /// <param name="p">The piece to be counted.</param>
    /// <returns>The number of two in a row.</returns>
    public int countDoubles(Piece p)
    {
        int count = 0;

        //Check for two in a row horizontally
        for (int i = 0; i < numCols - 1; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j])
                    count++;
            }
        }

        //Checks for two in a row vertically
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 1; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i, j + 1])
                    count++;
            }
        }

        //Checks for two in a row diagonally right
        for (int i = 0; i < numCols - 1; i++)
        {
            for (int j = 0; j < numRows - 1; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j + 1])
                    count++;
            }
        }

        //Checks for two in a row diagonally left
        for (int i = 1; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 1; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i - 1, j + 1])
                    count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the number of three in a row on the board.
    /// </summary>
    /// <param name="p">The piece to be counted.</param>
    /// <returns>The number of three in a row.</returns>
    public int countTripples(Piece p)
    {
        int count = 0;

        //Check for three in a row horizontally
        for (int i = 0; i < numCols - 2; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j] && (int)p == numBoard[i + 2, j])
                    count++;
            }
        }

        //Check for three in a row vertically
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 2; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i, j + 1] && (int)p == numBoard[i, j + 2])
                    count++;
            }
        }

        //Check for three in a row diagonally right
        for (int i = 0; i < numCols - 2; i++)
        {
            for (int j = 0; j < numRows - 2; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j + 1] && (int)p == numBoard[i + 2, j + 2])
                    count++;
            }
        }

        //Checks for three in a row diagonally left
        for (int i = 2; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 2; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i - 1, j + 1] && (int)p == numBoard[i - 2, j + 2])
                    count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the number of four in a row on the board.
    /// </summary>
    /// <param name="p">The piece to be counted.</param>
    /// <returns>The number of four in a row.</returns>
    public int countQuadruples(Piece p)
    {
        int count = 0;

        //Check for four in a row horizontally
        for (int i = 0; i < numCols - 3; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j] && (int)p == numBoard[i + 2, j] && (int)p == numBoard[i + 3, j])
                    count++;
            }
        }

        //Check for four in a row vertically
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 3; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i, j + 1] && (int)p == numBoard[i, j + 2] && (int)p == numBoard[i, j + 3])
                    count++;
            }
        }

        //Check for four in a row diagonally right
        for (int i = 0; i < numCols - 3; i++)
        {
            for (int j = 0; j < numRows - 3; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i + 1, j + 1] && (int)p == numBoard[i + 2, j + 2] && (int)p == numBoard[i + 3, j + 3])
                    count++;
            }
        }

        //Checks for three in a row diagonally left
        for (int i = 3; i < numCols; i++)
        {
            for (int j = 0; j < numRows - 3; j++)
            {
                if ((int)p == numBoard[i, j] && (int)p == numBoard[i - 1, j + 1] && (int)p == numBoard[i - 2, j + 2] && (int)p == numBoard[i - 3, j + 3])
                    count++;
            }
        }

        return count;
    }
}
