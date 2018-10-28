using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
  wait,
  move
}

public enum TileType
{
  Default,
  Breakable,
  Blank
}

[System.Serializable]
public class Tile
{
  public int x;
  public int y;
  public TileType tileType;
}

public class Board : MonoBehaviour
{

  public GameState currentState = GameState.move;

  [Header("Grid")]
  public int width;
  public int height;
  public int offset = 15;

  [Header("Game objects")]
  public GameObject tilePrefab;
  public GameObject[] dots;
  public GameObject[,] allDots;
  public GameObject destroyEffect;
  public GameObject breakableTilePrefab;
  public Dot currentDot;
  public Tile[] boardLayout;

  private bool[,] blankSpaces;
  private FindMatches findMatches;
  private BackgroundTile[,] breakableTiles;

  void Start()
  {
    blankSpaces = new bool[width, height];
    allDots = new GameObject[width, height];
    findMatches = FindObjectOfType<FindMatches>();
    breakableTiles = new BackgroundTile[width, height];

    Setup();
  }

  public TileType GetTileType(int column, int row)
  {
    for (int i = 0; i < boardLayout.Length; i++) {
      Tile tile = boardLayout[i];

      if (tile.x == column && tile.y == row) {
        return tile.tileType;
      }
    }

    return TileType.Default;
  }

  public bool IsValidDotPosition(int column, int row)
  {
    TileType tileAtLocation = GetTileType(column, row);

    if (tileAtLocation != TileType.Blank) {
      return true;
    }

    return false;
  }

  public void GenerateBreakableTiles()
  {
    for (int i = 0; i < boardLayout.Length; i++) {
      Tile tile = boardLayout[i];

      if (tile.tileType == TileType.Breakable) {
        Vector2 position = new Vector2(tile.x, tile.y);
        GameObject breakableTileObject = Instantiate(breakableTilePrefab, position, Quaternion.identity);
        breakableTiles[tile.x, tile.y] = breakableTileObject.GetComponent<BackgroundTile>();
      }
    }
  }

  public void DestroyMatches()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] != null) {
          DestroyMatchesAt(column, row);
        }
      }
    }

    findMatches.currentMatches.Clear();
    StartCoroutine(DecreaseRowCo());
  }

  private void Setup()
  {
    GenerateBreakableTiles();

    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        // Check that we can render a dot in this spot.
        if (IsValidDotPosition(column, row)) {
          Vector2 tempPosition = new Vector2(column, row + offset);
          GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;

          backgroundTile.transform.parent = this.transform;
          backgroundTile.name = "Tile ( " + column + ", " + row + " )";

          int dotToUse = Random.Range(0, dots.Length);
          int iterations = 0;

          while (MatchesAt(column, row, dots[dotToUse]) && iterations < 100) {
            dotToUse = Random.Range(0, dots.Length);
            iterations++;
          }

          iterations = 0;

          GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);

          dot.GetComponent<Dot>().row = row;
          dot.GetComponent<Dot>().column = column;

          dot.transform.parent = this.transform;
          dot.name = "Dot ( " + column + ", " + row + " )";

          allDots[column, row] = dot;
        }
      }
    }
  }

  private bool MatchesAt(int column, int row, GameObject piece)
  {
    if (column > 1 && row > 1) {
      if (allDots[column - 1, row] != null && allDots[column - 2, row] != null) {
        if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
          return true;
        }
      }

      if (allDots[column, row - 1] != null && allDots[column, row - 2] != null) {
        if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
          return true;
        }
      }

    } else if (column <= 1 || row <= 1) {
      if (row > 1) {
        if (allDots[column, row - 1] != null && allDots[column, row - 2] != null) {
          if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2]) {
            return true;
          }
        }

        if (column > 1) {
          if (allDots[column - 1, row] != null && allDots[column - 2, row] != null) {
            if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row]) {
              return true;
            }
          }
        }
      }
    }

    return false;
  }

  private bool ColumnOrRow()
  {
    int numberHorizontal = 0;
    int numberVertical = 0;

    Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();

    if (firstPiece != null) {
      foreach (GameObject currentPiece in findMatches.currentMatches) {
        Dot dotScript = currentPiece.GetComponent<Dot>();

        if (dotScript.row == firstPiece.row) {
          numberHorizontal++;
        }

        if (dotScript.column == firstPiece.column) {
          numberVertical++;
        }
      }
    }

    return numberVertical == 5 || numberHorizontal == 5;
  }

  private void ShouldMakeBomb()
  {
    int matchesCount = findMatches.currentMatches.Count;

    if (matchesCount == 4 || matchesCount == 7) {
      findMatches.CheckBombs();
    }

    if (matchesCount == 5 || matchesCount == 8) {
      if (ColumnOrRow()) {
        // Make a color bomb
        if (currentDot != null) {
          if (currentDot.isMatched) {
            if (!currentDot.isColorBomb) {
              currentDot.isMatched = false;
              currentDot.CreateColorBomb();
            }
          } else {
            if (currentDot.otherDot != null) {
              Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
              if (otherDot.isMatched) {
                if (!otherDot.isColorBomb) {
                  otherDot.isMatched = false;
                  otherDot.CreateColorBomb();
                }
              }
            }
          }
        }

      } else {
        // Make an adjacent bomb
        if (currentDot != null) {
          if (currentDot.isMatched) {
            if (!currentDot.isAdjacentBomb) {
              currentDot.isMatched = false;
              currentDot.CreateAdjacentBomb();
            }
          } else {
            if (currentDot.otherDot != null) {
              Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
              if (otherDot.isMatched) {
                if (!otherDot.isAdjacentBomb) {
                  otherDot.isMatched = false;
                  otherDot.CreateAdjacentBomb();
                }
              }
            }
          }
        }
      }
    }
  }

  private void DestroyMatchesAt(int column, int row)
  {
    if (allDots[column, row].GetComponent<Dot>().isMatched) {
      if (findMatches.currentMatches.Count >= 4) {
        ShouldMakeBomb();
      }

      if (breakableTiles[column, row] != null) {
        // If there is a brwakable tile here, give it 1 damage.
        breakableTiles[column, row].TakeDamage(1);

        if (breakableTiles[column, row].hitPoints <= 0) {
          breakableTiles[column, row] = null;
        }
      }

      GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
      Destroy(particle, .5f);

      Destroy(allDots[column, row]);
      allDots[column, row] = null;
    }
  }

  private void RefillBoard()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] == null && IsValidDotPosition(column, row)) {
          Vector2 tempPosition = new Vector2(column, row + offset);
          int dotToUse = Random.Range(0, dots.Length);
          GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
          allDots[column, row] = piece;

          piece.GetComponent<Dot>().row = row;
          piece.GetComponent<Dot>().column = column;
        }
      }
    }
  }

  private bool MatchesOnBoard()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] != null) {
          if (allDots[column, row].GetComponent<Dot>().isMatched) {
            return true;
          }
        }
      }
    }

    return false;
  }

  private void SwitchPieces(int column, int row, Vector2 direction)
  {
    // Take the first piece and save it in a variable
    GameObject tempPiece = allDots[column + (int)direction.x, row + (int)direction.y] as GameObject;
    // Switch the first dot into the second position
    allDots[column + (int)direction.x, row + (int)direction.y] = allDots[column, row];
    allDots[column, row] = tempPiece;
  }

  private bool CheckForMatches()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] != null) {
          var dot = allDots[column, row];
          // Make sure that 1 and 2 to the right are in the board
          if (column < width - 2) {
            // Make sure the dots to the right and 2 to the right exist
            if (allDots[column + 1, row] != null && allDots[column + 2, row] != null) {
              var rightDot = allDots[column + 1, row];
              var right2Dot = allDots[column + 2, row];

              if (rightDot.tag == dot.tag && right2Dot.tag == dot.tag) {
                return true;
              }
            }
          }

          // Make sure that 1 and 2 up are in the board
          if (row < height - 2) {
            // make sure that the dots 1 and 2 up exist
            if (allDots[column, row + 1] != null && allDots[column, row + 2] != null) {
              var upDot = allDots[column, row + 1];
              var up2Dot = allDots[column, row + 2];

              if (upDot.tag == dot.tag && up2Dot.tag == dot.tag) {
                return true;
              }
            }
          }
        }
      }
    }

    return false;
  }

  private bool SwitchAndCheckMatch(int column, int row, Vector2 direction)
  {
    SwitchPieces(column, row, direction);

    if (CheckForMatches()) {
      SwitchPieces(column, row, direction);
      return true;
    }

    SwitchPieces(column, row, direction);
    return false;
  }

  private bool IsDeadLocked()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] != null) {
          if (column < width - 1) {
            if (SwitchAndCheckMatch(column, row, Vector2.right)) {
              return false;
            }
          }

          if (row < height - 1) {
            if (SwitchAndCheckMatch(column, row, Vector2.up)) {
              return false;
            }
          }
        }
      }
    }

    return true;
  }

  private void ShuffleBoard()
  {
    List<GameObject> newBoard = new List<GameObject>();

    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] != null) {
          newBoard.Add(allDots[column, row]);
        }
      }
    }

    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (IsValidDotPosition(column, row)) {
          int pieceToUse = Random.Range(0, newBoard.Count);
          
          Dot piece = newBoard[pieceToUse].GetComponent<Dot>();

          piece.column = column;
          piece.row = row;

          allDots[column, row] = newBoard[pieceToUse];
          newBoard.Remove(newBoard[pieceToUse]);
        }
      }
    }

    // Check if it's still deadlocked
    if (IsDeadLocked()) {
      ShuffleBoard();
    } else {
      // Find matches that may have resulted from the shuffle
      // TODO: Fix argumentoutofrange exception when a match happens here.
      findMatches.FindAllMatches();
    }
  }

  private IEnumerator DecreaseRowCo()
  {
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        // If we can render a dot here and it isn't empty
        if (IsValidDotPosition(column, row) && allDots[column, row] == null) {
          for (int rowAbove = row + 1; rowAbove < height; rowAbove++) {
            if (allDots[column, rowAbove] != null) {
              allDots[column, rowAbove].GetComponent<Dot>().row = row;
              allDots[column, rowAbove] = null;
              break;
            }
          }
        }
      }
    }

    yield return new WaitForSeconds(.4f);
    StartCoroutine(FillBoardCo());
  }

  //private IEnumerator DecreaseRowCo()
  //{
  //  int nullCount = 0;

  //  for (int column = 0; column < width; column++) {
  //    for (int row = 0; row < height; row++) {
  //      if (allDots[column, row] == null) {
  //        nullCount++;
  //      } else if (nullCount > 0) {
  //        allDots[column, row].GetComponent<Dot>().row -= nullCount;
  //        allDots[column, row] = null;
  //      }
  //    }

  //    nullCount = 0;
  //  }

  //  yield return new WaitForSeconds(.25f);

  //  StartCoroutine(FillBoardCo());
  //}

  private IEnumerator FillBoardCo()
  {
    RefillBoard();
    yield return new WaitForSeconds(.25f);

    while (MatchesOnBoard()) {
      yield return new WaitForSeconds(.25f);
      DestroyMatches();
    }

    findMatches.currentMatches.Clear();
    currentDot = null;
    yield return new WaitForSeconds(.25f);

    if (IsDeadLocked()) {
      Debug.Log("Deadlocked!");
      ShuffleBoard();
    }

    currentState = GameState.move;
  }
}
