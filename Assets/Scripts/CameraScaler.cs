using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
  public float aspectRatio = 0.625f;
  public float padding = 2;
  public float zOffset;
  private Board board;

  void Start()
  {
    board = FindObjectOfType<Board>();

    if (board != null) {
      RepositionCamera(board.width - 1, board.height - 1);
    }
  }

  void RepositionCamera(float x, float y)
  {
    Vector3 newPosition = new Vector3(x / 2, y / 2, zOffset);
    transform.position = newPosition;

    if (board.width >= board.height) {
      Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
    } else {
      Camera.main.orthographicSize = board.height / 2 + padding;
    }
  }

  void Update()
  {

  }
}
