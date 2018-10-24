using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{

  public List<GameObject> currentMatches = new List<GameObject>();

  private Board board;

  // Use this for initialization
  void Start()
  {
    board = FindObjectOfType<Board>();
  }

  public void FindAllMatches()
  {
    StartCoroutine(FindAllMatchesCo());
  }

  private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3) 
  {
    List<GameObject> currentDots = new List<GameObject>();

    if (dot1.isRowBomb) {
      currentDots.Union(GetRowPieces(dot1.row));
    }

    if (dot2.isRowBomb) {
      currentDots.Union(GetRowPieces(dot2.row));
    }

    if (dot3.isRowBomb) {
      currentDots.Union(GetRowPieces(dot3.row));
    }

    return currentDots;
  }

  private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
  {
    List<GameObject> currentDots = new List<GameObject>();

    if (dot1.isColumnBomb) {
      currentDots.Union(GetColumnPieces(dot1.column));
    }

    if (dot2.isColumnBomb) {
      currentDots.Union(GetColumnPieces(dot2.column));
    }

    if (dot3.isColumnBomb) {
      currentDots.Union(GetColumnPieces(dot3.column));
    }

    return currentDots;
  }

  private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
  {
    List<GameObject> currentDots = new List<GameObject>();

    if (dot1.isAdjacentBomb) {
      currentDots.Union(GetAdjacentPieces(dot1.column, dot1.row));
    }

    if (dot2.isAdjacentBomb) {
      currentDots.Union(GetAdjacentPieces(dot2.column, dot2.row));
    }

    if (dot3.isAdjacentBomb) {
      currentDots.Union(GetAdjacentPieces(dot3.column, dot3.row));
    }

    return currentDots;
  }

  private void AddToListAndMatch(GameObject dot)
  {
    if (!currentMatches.Contains(dot)) {
      currentMatches.Add(dot);
    }

    dot.GetComponent<Dot>().isMatched = true;
  }

  private void GetNearbyDots(GameObject dot1, GameObject dot2, GameObject dot3)
  {

    AddToListAndMatch(dot1);
    AddToListAndMatch(dot2);
    AddToListAndMatch(dot3);
  }

  private IEnumerator FindAllMatchesCo()
  {
    yield return new WaitForSeconds(.2f);

    for (int column = 0; column < board.width; column++) {
      for (int row = 0; row < board.height; row++) {
        GameObject currentDot = board.allDots[column, row];

        if (currentDot != null) {
          Dot currentDotScript = currentDot.GetComponent<Dot>();

          if (column > 0 && column < board.width - 1) {
            GameObject leftDot = board.allDots[column - 1, row];
            GameObject rightDot = board.allDots[column + 1, row];

            if (leftDot != null && rightDot != null) {
              Dot leftDotScript = leftDot.GetComponent<Dot>();
              Dot rightDotScript = rightDot.GetComponent<Dot>();

              if (leftDot != null && rightDot != null) {
                if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) {
                  currentMatches.Union(IsRowBomb(leftDotScript, currentDotScript, rightDotScript));
                  currentMatches.Union(IsColumnBomb(leftDotScript, currentDotScript, rightDotScript));
                  currentMatches.Union(IsAdjacentBomb(leftDotScript, currentDotScript, rightDotScript));
                  GetNearbyDots(leftDot, currentDot, rightDot);
                }
              }
            }
          }

          if (row > 0 && row < board.height - 1) {
            GameObject upDot = board.allDots[column, row + 1];
            GameObject downDot = board.allDots[column, row - 1];

            if (upDot != null && downDot != null) {
              Dot upDotScript = upDot.GetComponent<Dot>();
              Dot downDotScript = downDot.GetComponent<Dot>();

              if (upDot != null && downDot != null) {
                if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag) {
                  currentMatches.Union(IsColumnBomb(upDotScript, currentDotScript, downDotScript));
                  currentMatches.Union(IsRowBomb(upDotScript, currentDotScript, downDotScript));
                  currentMatches.Union(IsAdjacentBomb(upDotScript, currentDotScript, downDotScript));
                  GetNearbyDots(upDot, currentDot, downDot);
                }
              }
            }
          }
        }
      }
    }
  }

  public void MatchAllDotsOfColor(string color)
  {
    for (int column = 0; column < board.width; column++) {
      for (int row = 0; row < board.height; row++) {
        // Check that the dot exists
        if (board.allDots[column, row] != null) {
          GameObject dot = board.allDots[column, row];
          // Check the tag of the dot
          if (dot.tag == color) {
            dot.GetComponent<Dot>().isMatched = true;
          }
        }
      }
    }
  }

  List<GameObject> GetAdjacentPieces(int column, int row)
  {
    List<GameObject> dots = new List<GameObject>();

    for (int adjacentColumn = column - 1; adjacentColumn <= column + 1; adjacentColumn++) {
      for (int adjacentRow = row - 1; adjacentRow <= row + 1; adjacentRow++) {
        // Check if the piece is inside the board
        if (adjacentColumn >= 0 && adjacentColumn < board.width && adjacentRow >= 0 && adjacentRow < board.height) {
          GameObject adjacentDot = board.allDots[adjacentColumn, adjacentRow];
          dots.Add(adjacentDot);
          adjacentDot.GetComponent<Dot>().isMatched = true;
        }
      }
    }

    return dots;
  }

  List<GameObject> GetColumnPieces(int column)
  {
    List<GameObject> dots = new List<GameObject>();

    for (int row = 0; row < board.height; row++) {
      if (board.allDots[column, row] != null) {
        dots.Add(board.allDots[column, row]);
        board.allDots[column, row].GetComponent<Dot>().isMatched = true;
      }
    }

    return dots;
  }

  List<GameObject> GetRowPieces(int row)
  {
    List<GameObject> dots = new List<GameObject>();

    for (int column = 0; column < board.width; column++) {
      if (board.allDots[column, row] != null) {
        dots.Add(board.allDots[column, row]);
        board.allDots[column, row].GetComponent<Dot>().isMatched = true;
      }
    }

    return dots;
  }

  public void CheckBombs()
  {
    if (board.currentDot != null) {
      if (board.currentDot.isMatched) {
        CreateBomb(board.currentDot);
      }

      if (board.currentDot.otherDot != null) {
        Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
        if (otherDot.isMatched) {
          CreateBomb(otherDot);
        }
      }
    }
  }

  private void CreateBomb(Dot dot)
  {
    dot.isMatched = false;
    int bombType = Random.Range(0, 100);

    Debug.Log(dot);

    if (board.currentDot.swipeAngle != 0) {
      if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)) {
        bombType = 0;
      } else {
        bombType = 99;
      }
    }

    if (bombType < 50) {
      dot.CreateRowBomb();
    } else {
      dot.CreateColumnBomb();
    }
  }
}
