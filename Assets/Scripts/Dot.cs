using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
  none,
  right,
  left,
  up,
  down
}

public class Dot : MonoBehaviour
{

  [Header("Board variables")]
  public int column;
  public int row;
  public int previousColumn;
  public int previousRow;
  public int targetX;
  public int targetY;

  [Header("Swipe variables")]
  public float swipeAngle = 0;
  public float swipeResist = .5f;

  [Header("Powerup variables")]
  public bool isColumnBomb;
  public bool isRowBomb;
  public bool isColorBomb;
  public bool isAdjacentBomb;
  public GameObject rowArrow;
  public GameObject columnArrow;
  public GameObject colorBomb;
  public GameObject adjacentBomb;

  [Header("Matches")]
  public bool isMatched = false;
  public GameObject otherDot;

  private Board board;
  private FindMatches findMatches;
  private Vector2 firstTouchPosition;
  private Vector2 finalTouchPosition;
  private Vector2 tempPosition;

  // Use this for initialization
  void Start()
  {
    board = FindObjectOfType<Board>();
    findMatches = FindObjectOfType<FindMatches>();

    isColumnBomb = false;
    isRowBomb = false;
    isColorBomb = false;
    isAdjacentBomb = false;

    // targetX = (int) transform.position.x;
    // targetY = (int) transform.position.y;

    // row = targetY;
    // column = targetX;
  }

  void Update()
  {
    targetX = column;
    targetY = row;

    if (Mathf.Abs(targetX - transform.position.x) > 0.1) {
      // Move towards the target
      tempPosition = new Vector2(targetX, transform.position.y);
      transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);

      if (board.allDots[column, row] != this.gameObject) {
        board.allDots[column, row] = this.gameObject;
      }

      findMatches.FindAllMatches();
    } else {
      // Directly set the position
      tempPosition = new Vector2(targetX, transform.position.y);
      transform.position = tempPosition;
    }

    if (Mathf.Abs(targetY - transform.position.y) > 0.1) {
      // Move towards the target
      tempPosition = new Vector2(transform.position.x, targetY);
      transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);

      if (board.allDots[column, row] != this.gameObject) {
        board.allDots[column, row] = this.gameObject;
      }

      findMatches.FindAllMatches();
    } else {
      // Directly set the position
      tempPosition = new Vector2(transform.position.x, targetY);
      transform.position = tempPosition;
    }
  }

  private void OnMouseOver()
  {
    if (Input.GetMouseButtonDown(1)) {
      isAdjacentBomb = true;
      GameObject adjacentMarker = Instantiate(adjacentBomb, transform.position, Quaternion.identity);
      adjacentMarker.transform.parent = this.transform;
    }
  }

  public IEnumerator CheckMoveCo()
  {
    if (isColorBomb) {
      // This piece is the color bomb
      findMatches.MatchAllDotsOfColor(otherDot.tag);
      isMatched = true;
    } else if (otherDot.GetComponent<Dot>().isColorBomb) {
      // The other piece is the color bomb
      findMatches.MatchAllDotsOfColor(this.gameObject.tag);
      otherDot.GetComponent<Dot>().isMatched = true;
    }

    yield return new WaitForSeconds(.25f);

    if (otherDot != null) {
      if (!isMatched && !otherDot.GetComponent<Dot>().isMatched) {
        otherDot.GetComponent<Dot>().row = row;
        otherDot.GetComponent<Dot>().column = column;

        row = previousRow;
        column = previousColumn;

        yield return new WaitForSeconds(.3f);

        board.currentDot = null;
        board.currentState = GameState.move;
      } else {
        board.DestroyMatches();
      }
    }
  }

  private void OnMouseDown()
  {
    if (board.currentState == GameState.move) {
      firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
  }

  private void OnMouseUp()
  {
    if (board.currentState == GameState.move) {
      finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      CalculateAngle();
    }
  }

  void CalculateAngle()
  {
    if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist) {
      board.currentState = GameState.wait;
      swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
      MovePieces();
      board.currentDot = this;
    } else {
      board.currentState = GameState.move;
    }
  }

  void AssignNextPosition(int assignCol = 0, int assignRow = 0)
  {
    otherDot = board.allDots[column + assignCol, row + assignRow];

    previousRow = row;
    previousColumn = column;

    Dot otherDotScript = otherDot.GetComponent<Dot>();

    otherDotScript.row += assignRow * -1;
    otherDotScript.column += assignCol * -1;

    row += assignRow;
    column += assignCol;

    StartCoroutine(CheckMoveCo());
  }

  void MovePieces()
  {
    if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1) {
      // Right swipe, move one col to the right
      AssignNextPosition(1, 0); // (col, row)
    } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1) {
      // Up swipe, move one row up
      AssignNextPosition(0, 1);
    } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0) {
      // Left swipe, move one col to the left
      AssignNextPosition(-1, 0);
    } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0) {
      // Down swipe, move one row down
      AssignNextPosition(0, -1);
    } else {
      board.currentState = GameState.move;
    }
  }

  public void CreateRowBomb()
  {
    isRowBomb = true;
    GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
    arrow.transform.parent = this.transform;
  }

  public void CreateColumnBomb()
  {
    isColumnBomb = true;
    GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
    arrow.transform.parent = this.transform;
  }

  public void CreateColorBomb()
  {
    isColorBomb = true;
    GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
    color.transform.parent = this.transform;
  }

  public void CreateAdjacentBomb()
  {
    isAdjacentBomb = true;
    GameObject adjacent = Instantiate(adjacentBomb, transform.position, Quaternion.identity);
    adjacent.transform.parent = this.transform;
  }
}
