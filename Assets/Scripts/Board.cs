using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
  wait,
  move
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
  public Dot currentDot;

  private BackgroundTile[,] allTiles;
  private FindMatches findMatches;

  void Start()
  {
    allTiles = new BackgroundTile[width, height];
    allDots = new GameObject[width, height];
    findMatches = FindObjectOfType<FindMatches>();

    Setup();
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
    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
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

  private bool MatchesAt(int column, int row, GameObject piece)
  {
    if (column > 1 && row > 1) {
      if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
        return true;
      }

      if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
        return true;
      }

    } else if (column <= 1 || row <= 1) {

      if (row > 1) {
        if (allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2]) {
          return true;
        }
      }

      if (column > 1) {
        if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row]) {
          return true;
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
        if (allDots[column, row] == null) {
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

  private IEnumerator DecreaseRowCo()
  {
    int nullCount = 0;

    for (int column = 0; column < width; column++) {
      for (int row = 0; row < height; row++) {
        if (allDots[column, row] == null) {
          nullCount++;
        } else if (nullCount > 0) {
          allDots[column, row].GetComponent<Dot>().row -= nullCount;
          allDots[column, row] = null;
        }
      }

      nullCount = 0;
    }

    yield return new WaitForSeconds(.25f);

    StartCoroutine(FillBoardCo());
  }

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
    currentState = GameState.move;
  }
}
