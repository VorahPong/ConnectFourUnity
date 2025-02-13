using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Piece object
    // player 1 = yellow , 2 = red
    public GameObject player1;
    public GameObject player2;

    // Score Tracker
    private static int player1Score = 0;
    public Text player1ScoreText;
    private static int player2Score = 0;
    public Text player2ScoreText;

    // Win cover for piece
    public GameObject winCircle;
    public List<Vector3> winLocation;


    // Piece object ghost
    public GameObject player1Ghost;
    public GameObject player2Ghost;

    // keep track if any piece is in falling motion
    public GameObject fallingPiece;

    // Size of board
    public const int columnSize = 7;
    public const int rowSize = 6;

    // Player's Turn
    bool player1Turn = true;

    // Win Screen show when someone win
    public GameObject winScreen;
    public Text winnerText;

    // sound fx
    public AudioSource soundSource;
    public AudioClip fx_click;
    public AudioClip fx_win;
    public AudioClip fx_tie;


    // Save all locations of pieces that been put
    // 0 = empty, 1 = player1's piece, 2 = player2's piece
    /*
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    0   0   0   0   0   0   0
    */
    int [,] boardState = { 
        {0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0},
        {0, 0, 0, 0, 0, 0, 0},
        };

    // Locations that pieces can spawn
    public GameObject[] spawnLocations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make the ghost piece to not show when start
        player1Ghost.SetActive(false);
        player2Ghost.SetActive(false);

        // Make to not show Winner Screen
        winScreen.SetActive(false);

        // Load Score
        player1ScoreText.text = "Player 1 : " + player1Score.ToString();
        player2ScoreText.text = "Player 2 : " + player2Score.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // spawn the ghost piece hovering above the board when player hovering
    public void HoverColumn(int column) 
    {
        if (boardState[rowSize - 1, column] == 0) // check if that column is not full
        {
            Vector3 offset = new Vector3(0, 1, 0);
            if (fallingPiece != null && player1Turn)
            {
                player1Ghost.SetActive(true);
                player1Ghost.transform.position = spawnLocations[column].transform.position + offset;
            }
            else 
            {
                player2Ghost.SetActive(true);
                player2Ghost.transform.position = spawnLocations[column].transform.position + offset;
            }   
        }
    }

    // wait for piece to stop falling
    IEnumerator WaitForPieceToStop(GameObject piece, int column)
    {
        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to the falling piece!");
            yield break;
        }

        // Wait until the Rigidbody's velocity is effectively zero
        while (rb.linearVelocity.sqrMagnitude > 0.001f) // Use a small threshold for precision
        {
            yield return null; // Wait for the next frame
        }

        Debug.Log("The piece has stopped moving.");

        // Update the board state once the piece has stopped
        UpdateBoardState(column);
    }

    // TakeTurn function will spawn player's piece in the column they click and switch turn
    public void TakeTurn(int column)
    {
        player2Ghost.SetActive(false);
        player1Ghost.SetActive(false);
        if (boardState[rowSize - 1, column] == 0 && fallingPiece != null && fallingPiece.GetComponent<Rigidbody>().linearVelocity == Vector3.zero) // check if that column is not full
        {
            // play sound fx
            soundSource.clip = fx_click;
            soundSource.Play();

            // Spawn game piece
            Quaternion offset_rotation = Quaternion.Euler(-360, 270, 360);
            if (player1Turn) 
            {
                fallingPiece = Instantiate(player1, spawnLocations[column].transform.position, offset_rotation);
                fallingPiece.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0.1f, 0);
                player1Turn = false;
            }
            else {
                fallingPiece = Instantiate(player2, spawnLocations[column].transform.position, offset_rotation);
                fallingPiece.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0.1f, 0);
                player1Turn = true;
            }
            StartCoroutine(WaitForPieceToStop(fallingPiece, column));
        }
    }

    // Print 2D array board for debug purpose
    void PrintBoardState() 
    {
        string num = "\n";
        for (int row = 0; row < rowSize; row++) {
            for (int col = 0; col < columnSize; col++) {
                num += boardState[row, col].ToString() + " ";
            }
            num += "\n";
        }
        Debug.Log(num);
    }

    // Return true if the board is full
    bool IsBoardFull()
    {
        for (int col = 0; col < columnSize; col++) {
            if (boardState[rowSize - 1, col] == 0)
                return false;
        }
        return true;
    }
/*
    CheckWinner()

    x position of piece location should alway remain the same
    y position of piece is the row of array
    z position of piece is column of array
*/
    int CheckWinner(int pos_row, int pos_column, int player) 
    {
        int counter;
    

        // Vertical
        counter = 1;
        winLocation.Add(new Vector3(-0.06f, pos_row, -pos_column));
        for (int i = pos_row - 1; i >= 0; i--) 
        {
            if (boardState[i, pos_column] == player) 
            {
                // winLocation.Add(position - new Vector3(pos_column - 3, i+1, 0));
                winLocation.Add(new Vector3(-0.06f, i, -pos_column));
                counter++;
            }
            else 
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
        }
        winLocation.Clear();

        // Horizontal
        counter = 1;
        winLocation.Add(new Vector3(-0.06f, pos_row, -pos_column));
        // left to right
        for (int i = pos_column + 1; i < columnSize; i++) 
        {
            if (boardState[pos_row, i] == player) 
            {
                winLocation.Add(new Vector3(-0.06f, pos_row, -i));
                counter++;
            }
            else 
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
        }

        // right to left
        for (int i = pos_column - 1; i >= 0; i--) 
        {
            if (boardState[pos_row, i] == player) 
            {
                winLocation.Add(new Vector3(-0.06f, pos_row, -i));
                counter++;
            }
            else 
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
        }
        winLocation.Clear();

        // Positive Diagonal
        winLocation.Add(new Vector3(-0.06f, pos_row, -pos_column));
        counter = 1;
        int col = pos_column + 1;
        int row = pos_row + 1;
        while (row < rowSize && col < columnSize)
        {
            if (boardState[row, col] == player)
            {
                winLocation.Add(new Vector3(-0.06f, row, -col));
                counter++;
            }
            else
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
            col++;
            row++;
        }

        col = pos_column - 1;
        row = pos_row - 1;
        while (row >= 0 && col >= 0)
        {
            if (boardState[row, col] == player)
            {
                winLocation.Add(new Vector3(-0.06f, row, -col));
                counter++;
            }
            else
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
            col--;
            row--;
        }
        winLocation.Clear();

        // Negative Diagonal
        winLocation.Add(new Vector3(-0.06f, pos_row, -pos_column));
        counter = 1;
        col = pos_column + 1;
        row = pos_row - 1;
        while (row >= 0 && col < columnSize)
        {
            if (boardState[row, col] == player)
            {
                winLocation.Add(new Vector3(-0.06f, row, -col));
                counter++;
            }
            else
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
            col++;
            row--;
        }

        col = pos_column - 1;
        row = pos_row + 1;
        while (row < rowSize && col >= 0)
        {
            if (boardState[row, col] == player)
            {
                winLocation.Add(new Vector3(-0.06f, row, -col));
                counter++;
            }
            else
            {
                break;
            }
            if (counter >= 4) 
            {
                return player;
            }
            col--;
            row++;
        }

        winLocation.Clear();
        return 0;
    }

    // Update the 2D board when player drop their game piece
    void UpdateBoardState(int column) 
    {
        for (int row = 0; row < rowSize; row++) {
            if (boardState[row, column] == 0) // if empty
            {
                if (!player1Turn)
                    boardState[row, column] = 1;
                else
                    boardState[row, column] = 2;
                
                PrintBoardState(); // print for debug purpose
                if (IsBoardFull() == true) // check if board is full
                {
                    // play sound fx
                    soundSource.clip = fx_tie;
                    soundSource.Play();
                    winScreen.SetActive(true);
                    winnerText.text = "This round is a draw!";
                }
                else
                {
                    int winner = CheckWinner(row, column, boardState[row, column]); 
                    if (winner != 0) { // check winner function
                        fallingPiece = null;
                        // play sound fx
                        soundSource.clip = fx_win;
                        soundSource.Play();
                        // Update Score
                        if (winner == 1)
                        {
                            player1Score += 1;
                            player1ScoreText.text = "Player 1 : " + player1Score.ToString();
                        }
                        else
                        {
                            player2Score += 1;
                            player2ScoreText.text = "Player 2 : " + player2Score.ToString();
                        }

                        // Show winner
                        Debug.Log("The winner is Player " + winner);
                        winScreen.SetActive(true);
                        winnerText.text = "The winner is Player " + winner;
                        
                        foreach (Vector3 location in winLocation)
                        {
                            Instantiate(winCircle, location, Quaternion.Euler(-360, 270, 360));
                        }
                        winLocation.Clear();
                    }

                }
                return;
            }
        }
    }

    // Reset Score button function
    public void ResetScore()
    {
        player2Score = player1Score = 0;
        player1ScoreText.text = "Player 1 : " + player1Score.ToString();
        player2ScoreText.text = "Player 2 : " + player2Score.ToString();

    }

    // Restart button function
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    // Exit button function
    public void QuitGame()
    {
        Debug.LogWarning("Exiting Game...");
        Application.Quit();
    }
}
