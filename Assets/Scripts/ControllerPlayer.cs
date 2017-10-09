using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerPlayer : Controller {
	void Start () {
    board = GetComponent<Board>();
    if (board) StartCoroutine("HandleInput");
	}

  IEnumerator HandleInput() {
    while (true) {
      if (board) {
        float direction  = Input.GetAxis("Horizontal");
        bool drop        = Input.GetButton("Drop");
        bool rotateLeft  = Input.GetButtonDown("RotateCounterClockwise");
        bool rotateRight = Input.GetButtonDown("RotateClockwise");

        yield return board.Drop(drop);
        if (direction != 0f) yield return board.MoveHorizontal(direction);
        if (rotateLeft) yield return board.Rotate(90);
        if (rotateRight) yield return board.Rotate(-90);
      }

      yield return 0;
    }
  }
}
