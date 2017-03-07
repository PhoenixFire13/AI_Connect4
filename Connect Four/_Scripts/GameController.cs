using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace ConnectFour
{
	public class GameController : MonoBehaviour 
	{
        enum Point
        {
            One = 1,
            Two = 10,
            Three = 100,
            Four = 1000
        }

		[Range(3, 8)]
		public int numRows = 6;
		[Range(3, 8)]
		public int numColumns = 7;
        [Range(4, 10)]
        public int depth = 4;

		[Tooltip("How many pieces have to be connected to win.")]
		public int numPiecesToWin = 4;

		[Tooltip("Allow diagonally connected Pieces?")]
		public bool allowDiagonally = true;
		
		public float dropTime = 4f;

		// Gameobjects 
		public GameObject pieceRed;
		public GameObject pieceBlue;
		public GameObject pieceField;

		public GameObject winningText;
		public string playerWonText = "You Won!";
		public string playerLoseText = "You Lose!";
		public string drawText = "Draw!";

		public GameObject btnPlayAgain;
		bool btnPlayAgainTouching = false;
		Color btnPlayAgainOrigColor;
		Color btnPlayAgainHoverColor = new Color(255, 143,4);

		GameObject gameObjectField;

		// temporary gameobject, holds the piece at mouse position until the mouse has clicked
		GameObject gameObjectTurn;

        /// <summary>
        /// The Game field.
        /// 0 = Empty
        /// 1 = Blue
        /// 2 = Red
        /// </summary>
        Board board;

		bool isPlayersTurn = true;
		bool isLoading = true;
		bool isDropping = false; 
		bool mouseButtonPressed = false;

		bool gameOver = false;
		bool isCheckingForWinner = false;

        int bestMove;

		// Use this for initialization
		void Start () 
		{
			int max = Mathf.Max (numRows, numColumns);

			if(numPiecesToWin > max)
				numPiecesToWin = max;

			CreateField ();

			isPlayersTurn = System.Convert.ToBoolean(Random.Range (0, 1));

			btnPlayAgainOrigColor = btnPlayAgain.GetComponent<Renderer>().material.color;

		}

		/// <summary>
		/// Creates the field.
		/// </summary>
		void CreateField()
		{
			winningText.SetActive(false);
			btnPlayAgain.SetActive(false);

			isLoading = true;

			gameObjectField = GameObject.Find ("Field");
			if(gameObjectField != null)
			{
				DestroyImmediate(gameObjectField);
			}
			gameObjectField = new GameObject("Field");

            // create an empty field and instantiate the cells
            board = new Board(numRows, numColumns);
			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					GameObject g = Instantiate(pieceField, new Vector3(x, y * -1, -1), Quaternion.identity) as GameObject;
					g.transform.parent = gameObjectField.transform;
				}
			}

			isLoading = false;
			gameOver = false;

			// center camera
			Camera.main.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f), Camera.main.transform.position.z);

			winningText.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) + 1, winningText.transform.position.z);

			btnPlayAgain.transform.position = new Vector3(
				(numColumns-1) / 2.0f, -((numRows-1) / 2.0f) - 1, btnPlayAgain.transform.position.z);
		}

        void Update()
        {
            if (isLoading)
                return;

            if (isCheckingForWinner)
                return;

            if (gameOver)
            {
                winningText.SetActive(true);
                btnPlayAgain.SetActive(true);

                UpdatePlayAgainButton();

                return;
            }

            if (isPlayersTurn)
            {
                if (gameObjectTurn == null)
                {
                    gameObjectTurn = SpawnPiece();
                }
                else
                {
                    // update the objects position
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    gameObjectTurn.transform.position = new Vector3(
                        Mathf.Clamp(pos.x, 0, numColumns - 1),
                        gameObjectField.transform.position.y + 1, 0);

                    // click the left mouse button to drop the piece into the selected column
                    if (Input.GetMouseButtonDown(0) && !mouseButtonPressed && !isDropping)
                    {
                        mouseButtonPressed = true;

                        StartCoroutine(dropPiece(gameObjectTurn));
                    }
                    else
                    {
                        mouseButtonPressed = false;
                    }
                }
            }
            else
            {
                if (gameObjectTurn == null)
                {
                    gameObjectTurn = SpawnPiece();
                }
                else
                {
                    if (!isDropping)
                        StartCoroutine(dropPiece(gameObjectTurn));
                }
            }
        }

        /// <summary>
        /// Spawns a piece at mouse position above the first row
        /// </summary>
        /// <returns>The piece.</returns>
        GameObject SpawnPiece()
		{
			Vector3 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			if(!isPlayersTurn)
			{
                float score = ComputerMove(Mathf.NegativeInfinity, Mathf.Infinity, depth, 0, Board.Piece.Player2, board);
           		spawnPos = new Vector3(bestMove, 0, 0);
			}

			GameObject g = Instantiate(
					isPlayersTurn ? pieceBlue : pieceRed, // is players turn = spawn blue, else spawn red
					new Vector3(
					Mathf.Clamp(spawnPos.x, 0, numColumns-1), 
					gameObjectField.transform.position.y + 1, 0), // spawn it above the first row
					Quaternion.identity) as GameObject;

			return g;
		}

        // --------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// The AI managing method that uses Alphabeta Minimax. Will set the bestMove field.
        /// </summary>
        /// <param name="alpha">Lower bound of the optimal score.</param>
        /// <param name="beta">Upper bound of the optimal score.</param>
        /// <param name="maxDepth">The maximum number of levels deep in the recursion.</param>
        /// <param name="currentDepth">The current number of the level in the recursion. Set to 0 initially to start.</param>
        /// <param name="player">The bestMove in respect to the player.</param>
        /// <param name="tempBoard">The current board state.</param>
        /// <returns>End result of the recursion of the bestScore, ie. the best score possible.</returns>
        private float ComputerMove (float alpha, float beta, int maxDepth, int currentDepth, Board.Piece player, Board tempBoard)
        {
            // base case
            if (board.countQuadruples(player) > 0 || currentDepth == maxDepth)
            {
                //Debug.Log("Board Score : " + EvaluateScore(tempBoard, player));
                return EvaluateScore(tempBoard, player);
            }

            // set initial values
            float bestScore;
            float tempScore;
            int row;
            Board.Piece other = (player == Board.Piece.Player1 ? Board.Piece.Player2 : Board.Piece.Player1);

            //If this is true then it means that it is the maximizing player's turn.
            if (currentDepth % 2 == 0)
            {
                //bestScore has to be overwritten with a larger number, therefore it starts at smallest possible.
                bestScore = Mathf.NegativeInfinity;
                foreach (int move in board.getPossibleMoves())
                {
                    tempScore = bestScore;

                    row = tempBoard.getEmptyCell(move);
                    alpha = Mathf.Max(alpha, bestScore);
                    tempBoard.setCell(move, row, player);

                    bestScore = Mathf.Max(bestScore, ComputerMove(-beta, -alpha, maxDepth, currentDepth + 1, player, tempBoard));
                    if (currentDepth == 0)
                        bestMove = tempScore == bestScore ? bestMove : move;

                    tempBoard.setCell(move, row, Board.Piece.Empty);

                    if (beta <= alpha)
                        break;
                }
            }

            //The opponent's turn or the minimizing player's turn.
            else
            {
                //bestScore has to be overwritten with a smaller number, therefore it starts at the largest number possible.
                bestScore = Mathf.Infinity;
                foreach (int move in board.getPossibleMoves())
                {
                    tempScore = bestScore;

                    row = tempBoard.getEmptyCell(move);
                    tempBoard.setCell(move, row, other);

                    bestScore = Mathf.Min(bestScore, ComputerMove(-beta, -alpha, maxDepth, currentDepth + 1, player, tempBoard));
                    if (currentDepth == 0)
                        bestMove = bestMove = tempScore == bestScore ? bestMove : move;

                    beta = Mathf.Min(beta, bestScore);

                    tempBoard.setCell(move, row, Board.Piece.Empty);
                    if (beta <= alpha)
                        break;
                }
            }

            Debug.Log("Best Move : " + bestMove);
            //Debug.Log("Best Score : " + bestScore);
            return bestScore;
        }

        /// <summary>
        /// Evaluates the board based on how many pieces of a kind there are in a row. The more a player has in a row, the greater their score will be.
        /// </summary>
        /// <param name="b">The board that will be evaluated for a score.</param>
        /// <param name="p">The piece on the board in which the score is taken in respect to.</param>
        /// <returns>The total score of the board in respect to the piece.</returns>
        private float EvaluateScore(Board b, Board.Piece p)
        {
            float score = 0;

            //The opposing piece is called other. If the parameter p is Player 1, then other is Player 2. If p is Player 2, then other is player 1.
            Board.Piece other = (p == Board.Piece.Player1 ? Board.Piece.Player2 : Board.Piece.Player1);

            //The score from each type of pairing. The count of p is positive and the opponent is negative.
            score += (b.countSingles(p) - b.countSingles(other)) * (int)Point.One;
            score += (b.countDoubles(p) - b.countDoubles(other)) * (int)Point.Two;
            score += (b.countTriples(p) - b.countTriples(other)) * (int)Point.Three;
            score += (b.countQuadruples(p) - b.countQuadruples(other)) * (int)Point.Four;

            return score;
        }
        // --------------------------------------------------------------------------------------------------------------

        void UpdatePlayAgainButton()
        {
            RaycastHit hit;
            //ray shooting out of the camera from where the mouse is
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.collider.name == btnPlayAgain.name)
            {
                btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainHoverColor;
                //check if the left mouse has been pressed down this frame
                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && btnPlayAgainTouching == false)
                {
                    btnPlayAgainTouching = true;

                    //CreateField();
                    SceneManager.LoadSceneAsync(0);
                }
            }
            else
            {
                btnPlayAgain.GetComponent<Renderer>().material.color = btnPlayAgainOrigColor;
            }

            if (Input.touchCount == 0)
            {
                btnPlayAgainTouching = false;
            }
        }

		/// <summary>
		/// This method searches for a empty cell and lets 
		/// the object fall down into this cell
		/// </summary>
		/// <param name="gObject">Game Object.</param>
		IEnumerator dropPiece(GameObject gObject)
		{
			isDropping = true;

			Vector3 startPosition = gObject.transform.position;
			Vector3 endPosition = new Vector3();

			// round to a grid cell
			int x = Mathf.RoundToInt(startPosition.x);
			startPosition = new Vector3(x, startPosition.y, startPosition.z);

			// is there a free cell in the selected column?
			bool foundFreeCell = false;

            if (board.containsEmptyCell(x))
            {
                foundFreeCell = true;

                int row = board.getEmptyCell(x);
                board.setCell(x, row, isPlayersTurn ? Board.Piece.Player1 : Board.Piece.Player2);
                endPosition = new Vector3(x, row * -1, startPosition.z);
            }
            
			if(foundFreeCell)
			{
				// Instantiate a new Piece, disable the temporary
				GameObject g = Instantiate (gObject) as GameObject;
				gameObjectTurn.GetComponent<Renderer>().enabled = false;

				float distance = Vector3.Distance(startPosition, endPosition);

				float t = 0;
				while(t < 1)
				{
					t += Time.deltaTime * dropTime * ((numRows - distance) + 1);

					g.transform.position = Vector3.Lerp (startPosition, endPosition, t);
					yield return null;
				}

				g.transform.parent = gameObjectField.transform;

				// remove the temporary gameobject
				DestroyImmediate(gameObjectTurn);

				// run coroutine to check if someone has won
				StartCoroutine(Won());

				// wait until winning check is done
				while(isCheckingForWinner)
					yield return null;

				isPlayersTurn = !isPlayersTurn;
			}

			isDropping = false;

            //Debug.Log("Singles Player 1: " + board.countSingles(Board.Piece.Player1) + "\nDoubles Player 1: " + board.countDoubles(Board.Piece.Player1) + "\nTripples Player 1: " + board.countTriples(Board.Piece.Player1) + "\nQuadruples Player 1 :" + board.countQuadruples(Board.Piece.Player1));
            //Debug.Log("Singles Player 2: " + board.countSingles(Board.Piece.Player2) + "\nDoubles Player 2: " + board.countDoubles(Board.Piece.Player2) + "\nTripples Player 2: " + board.countTriples(Board.Piece.Player2) + "\nQuadruples Player 2 :" + board.countQuadruples(Board.Piece.Player2));

            yield return 0;
		}
      
        // --------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check for Winner
        /// </summary>
        IEnumerator Won()
		{
			isCheckingForWinner = true;

			for(int x = 0; x < numColumns; x++)
			{
				for(int y = 0; y < numRows; y++)
				{
					// Get the Laymask to Raycast against, if its Players turn only include
					// Layermask Blue otherwise Layermask Red
					int layermask = isPlayersTurn ? (1 << 8) : (1 << 9);

					// If its Players turn ignore red as Starting piece and wise versa
					if(board.getCell(x, y) != (isPlayersTurn ? (int)Board.Piece.Player1 : (int)Board.Piece.Player2))
					{
						continue;
					}

					// shoot a ray of length 'numPiecesToWin - 1' to the right to test horizontally
					RaycastHit[] hitsHorz = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.right, 
						numPiecesToWin - 1, 
						layermask);

					// return true (won) if enough hits
					if(hitsHorz.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// shoot a ray up to test vertically
					RaycastHit[] hitsVert = Physics.RaycastAll(
						new Vector3(x, y * -1, 0), 
						Vector3.up, 
						numPiecesToWin - 1, 
						layermask);
					
					if(hitsVert.Length == numPiecesToWin - 1)
					{
						gameOver = true;
						break;
					}

					// test diagonally
					if(allowDiagonally)
					{
						// calculate the length of the ray to shoot diagonally
						float length = Vector2.Distance(new Vector2(0, 0), new Vector2(numPiecesToWin - 1, numPiecesToWin - 1));

						RaycastHit[] hitsDiaLeft = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(-1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaLeft.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}

						RaycastHit[] hitsDiaRight = Physics.RaycastAll(
							new Vector3(x, y * -1, 0), 
							new Vector3(1 , 1), 
							length, 
							layermask);
						
						if(hitsDiaRight.Length == numPiecesToWin - 1)
						{
							gameOver = true;
							break;
						}
					}

					yield return null;
				}

				yield return null;
			}

			// if Game Over update the winning text to show who has won
			if(gameOver == true)
			{
				winningText.GetComponent<TextMesh>().text = isPlayersTurn ? playerWonText : playerLoseText;
			}
			else 
			{
				// check if there are any empty cells left, if not set game over and update text to show a draw
				if(!board.containsEmptyCell())
				{
					gameOver = true;
					winningText.GetComponent<TextMesh>().text = drawText;
				}
			}

			isCheckingForWinner = false;

			yield return 0;
		}
        // --------------------------------------------------------------------------------------------------------------
	}
}
