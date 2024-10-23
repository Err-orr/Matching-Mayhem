using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

// Enum to represent the current state of the game
public enum GameState {
    wait, // The game is waiting for a player action
    move  // The game is ready for player movement
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move; // Current state of the game
    public int width; // Width of the board in terms of columns
    public int height; // Height of the board in terms of rows
    public int offSet; // Offset for positioning tiles on the board
    public GameObject tilePrefab; // Prefab for creating the background tiles
    private BackgroundTile[,] allTiles; // 2D array to hold all background tiles
    public GameObject[] dots; // Array of available dot prefabs
    public GameObject[,] allDots; // 2D array to hold all the dots currently on the board
    private FindMatches findMatches; // Reference to the FindMatches script for detecting matches
    public GameObject destroyEffect; // Effect to display when a dot is destroyed
    public Dot currentDot; // Reference to the currently selected dot

    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>(); // Retrieve the FindMatches component from the scene
        allTiles = new BackgroundTile[width, height]; // Initialize the 2D array for background tiles
        allDots = new GameObject[width, height]; // Initialize the 2D array for dots
        SetUp(); // Set up the board with tiles and randomly placed dots
    }

    // Set up the board by instantiating tiles and randomly placing dots
    private void SetUp() {
        for (int i = 0; i < width; i++) { // Loop through each column
            for (int j = 0; j < height; j++) { // Loop through each row
                // Calculate the position for the tile based on its index
                UnityEngine.Vector2 tempPosition = new(i, j + offSet); 
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, UnityEngine.Quaternion.identity); // Create a new tile
                backgroundTile.transform.parent = this.transform; // Set the parent of the tile to the board for hierarchy organization
                backgroundTile.name = "( " + i + ", " + j + " )"; // Name the tile for debugging purposes
                
                int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab from the array
                int maxIterations = 0; // Counter to prevent infinite loops when checking for adjacent matches

                // Ensure the randomly selected dot does not match existing adjacent dots
                while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) {
                    dotToUse = Random.Range(0, dots.Length); // Select a new dot if it matches
                    maxIterations++;
                }
                maxIterations = 0; // Reset counter for potential future use

                // Instantiate the selected dot and set its position on the board
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity);
                dot.GetComponent<Dot>().row = j; // Set the row for the dot
                dot.GetComponent<Dot>().column = i; // Set the column for the dot
                dot.transform.parent = this.transform; // Set the parent of the dot to the board for hierarchy organization
                dot.name = "( " + i + ", " + j + " )"; // Name the dot for debugging purposes
                allDots[i, j] = dot; // Store the newly created dot in the array
            }
        }
    }

    // Check if the current position has matching dots
    private bool MatchesAt(int column, int row, GameObject piece) {
        // Check for horizontal and vertical matches
        if (column > 1 && row > 1) {
            // Check for a horizontal match with the two dots to the left
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                return true; // Found a horizontal match
            }
            // Check for a vertical match with the two dots above
            if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                return true; // Found a vertical match
            }
        } else if (column <= 1 || row <= 1) {
            // Handle edge cases where the dot is near the edge of the board
            if (row > 1) {
                // Check for a vertical match with the two dots above
                if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                    return true; // Found a vertical match
                }
            }
            if (column > 1) {
                // Check for a horizontal match with the two dots to the left
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                    return true; // Found a horizontal match
                }
            }
        }
        return false; // No match found at the specified position
    }

    // Determine if the current matches are in a single row or column
    private bool ColumnOrRow() {
        int numberHorizontal = 0; // Counter for horizontal matches
        int numberVertical = 0; // Counter for vertical matches
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>(); // Get the first matched dot

        if (firstPiece != null) {
            // Iterate through all matched pieces to count their orientation
            foreach (GameObject currentPiece in findMatches.currentMatches) {
                Dot dot = currentPiece.GetComponent<Dot>();
                // Increment horizontal count if dots are in the same row
                if (dot.row == firstPiece.row) {
                    numberHorizontal++;
                }
                // Increment vertical count if dots are in the same column
                if (dot.column == firstPiece.column) {
                    numberVertical++;
                }
            }
        }
        // Return true if there are five pieces in a row or column
        return numberVertical == 5 || numberHorizontal == 5;
    }

    // Check if any bombs need to be created based on current matches
    private void CheckToMakeBombs() {
        // Handle bomb creation for specific match counts
        if (findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7) {
            findMatches.CheckBombs(); // Check for any bomb effects based on matches
        }
        
        // Check for potential color bomb creation based on the number of matches
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8) {
            if (ColumnOrRow()) { // If matches are in a line
                if (currentDot != null) {
                    // Create a color bomb if the current dot is matched and not already a color bomb
                    if (currentDot.isMatched) {
                        if (!currentDot.isColorBomb) {
                            currentDot.isMatched = false; // Reset matched status
                            currentDot.MakeColorBomb(); // Create a color bomb
                        }
                    } else { // Check the other dot in case it is matched
                        if (currentDot.otherDot != null) {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched) {
                                if (!otherDot.isColorBomb) {
                                    otherDot.isMatched = false; // Reset matched status
                                    otherDot.MakeColorBomb(); // Create a color bomb
                                }
                            }
                        }
                    }
                }
            } else { // If matches are not in a line, check for adjacent bomb creation
                if (currentDot != null) {
                    // Create an adjacent bomb if the current dot is matched and not already an adjacent bomb
                    if (currentDot.isMatched) {
                        if (!currentDot.isAdjacentBomb) {
                            currentDot.isMatched = false; // Reset matched status
                            currentDot.MakeAdjacentBomb(); // Create an adjacent bomb
                        }
                    } else { // Check the other dot for adjacent bomb creation
                        if (currentDot.otherDot != null) {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched) {
                                if (!otherDot.isAdjacentBomb) {
                                    otherDot.isMatched = false; // Reset matched status
                                    otherDot.MakeAdjacentBomb(); // Create an adjacent bomb
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Destroy matched dots at the specified column and row
    private void DestroyMatchesAt(int column, int row) {
        // Check if the dot at the given position is marked as matched
        if (allDots[column, row].GetComponent<Dot>().isMatched) {
            // Check for special match conditions (e.g., bombs)
            if (findMatches.currentMatches.Count >= 4) {
                CheckToMakeBombs(); // Handle bomb creation if applicable
            }
            // Instantiate a destruction effect at the matched dot's position
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, UnityEngine.Quaternion.identity);
            Destroy(particle, .5f); // Destroy the effect after 0.5 seconds
            Destroy(allDots[column, row]); // Remove the matched dot from the board
            allDots[column, row] = null; // Set the array position to null for cleanup
        }
    }

    // Loop through the entire board and destroy all matched dots
    public void DestroyMatches() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] != null) {
                    DestroyMatchesAt(i, j); // Check and destroy matches at each position
                }
            }
        }
        findMatches.currentMatches.Clear(); // Clear the list of current matches
        StartCoroutine(DecreaseRowCo()); // Start coroutine to handle row adjustments after destruction
    }

    // Coroutine to handle adjusting rows when dots are destroyed
    private IEnumerator DecreaseRowCo() {
        int nullCount = 0; // Count of null dots in a column
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] == null) {
                    nullCount++; // Increment count of null positions
                } else if (nullCount > 0) {
                    // Move existing dots down by the number of nulls above them
                    allDots[i, j].GetComponent<Dot>().row -= nullCount; // Update the row position of the dot
                    allDots[i, j] = null; // Set the current position to null
                }
            }
            nullCount = 0; // Reset null count for the next column
        }
        yield return new WaitForSeconds(.4f); // Wait for 0.4 seconds before refilling the board
        StartCoroutine(FillBoardCo()); // Start coroutine to fill the board with new dots
    }

    // Refill the board with new dots in null positions
    private void RefillBoard() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] == null) { // Check for empty positions
                    UnityEngine.Vector2 tempPosition = new UnityEngine.Vector2(i, j + offSet); // Set the new position for the dot
                    int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, UnityEngine.Quaternion.identity); // Create new dot
                    allDots[i, j] = piece; // Store the new dot in the array
                    piece.GetComponentInParent<Dot>().row = j; // Set the dot's row
                    piece.GetComponentInParent<Dot>().column = i; // Set the dot's column
                }
            }
        }
    }

    // Check if there are any matches currently present on the board
    private bool MatchesOnBoard() {
        for (int i = 0; i < width; i++) { // Iterate through each column
            for (int j = 0; j < height; j++) { // Iterate through each row
                if (allDots[i, j] != null) { // Check for a valid dot
                    if (allDots[i, j].GetComponent<Dot>().isMatched) {
                        return true; // A match has been found
                    }
                }
            }
        }
        return false; // No matches found on the board
    }

    // Coroutine to fill the board with new dots after destroying matches
    private IEnumerator FillBoardCo() {
        RefillBoard(); // Refill the board with new dots
        yield return new WaitForSeconds(.2f); // Wait for 0.2 seconds before checking for matches
        // Continuously check for new matches on the board
        while (MatchesOnBoard()) { 
            yield return new WaitForSeconds(.2f); // Wait before checking for matches again
            DestroyMatches(); // Destroy any new matches found
        }
        findMatches.currentMatches.Clear(); // Clear the matches list after refilling
        yield return new WaitForSeconds(.2f); // Wait before allowing player moves again
        currentState = GameState.move; // Set the state to allow player moves
    }
}