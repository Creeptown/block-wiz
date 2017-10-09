using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CellGroups move as a unit and render their cells
public class CellRenderer : MonoBehaviour {
  public enum State {
    Preview,    // Showing as upcoming drop
    Spawning,   // Group is about to become Falling
    Falling,    // Group is moving towards target
    Landed,     // Touching another CellGroup but still able to be moved
    Fixed,      // No longer player controllable
    Combining,  // Combining into larger cell
    Destroying, // In the processed of being destroyed
    Dead,       // Destroyed. Can be removed from game
  }

  public Sprite normalSprite;
  public Sprite bombSprite;

  internal Vector3 targetPosition;
  internal State state;
  internal Cell cell;

  SpriteRenderer spriteRenderer;
  int cellSize;
  int gravity;

  void Awake () {
    // TODO Should be Spawning
    state = State.Falling;
    gameObject.AddComponent<SpriteRenderer>();
  }

  internal void Initialize(Cell cell, int cellSize, int gravity) {
    this.cell = cell;
    this.cellSize = cellSize;
    this.gravity = gravity;
    transform.position = GridToWorldSpace(new Point(0, cell.position.col));
    targetPosition = GridToWorldSpace(cell.position);
    Render();
  }

  internal void Destroy() {
    foreach (Transform t in transform) {
      Destroy(t.gameObject);
    }
    Destroy(gameObject);
  }

  internal void Render() {
    spriteRenderer = GetComponent<SpriteRenderer>();
    switch (cell.type) {
      case Cell.Type.Bomb:
        spriteRenderer.sprite = bombSprite;
        break;
      default:
        spriteRenderer.sprite = normalSprite;
        break;
    }
    switch (cell.color) {
      case Cell.Color.Red:
        spriteRenderer.color = Color.red;
        break;
      case Cell.Color.Green:
        spriteRenderer.color = Color.green;
        break;
      case Cell.Color.Blue:
        spriteRenderer.color = Color.blue;
        break;
      case Cell.Color.Yellow:
        spriteRenderer.color = Color.yellow;
        break;
      default:
        spriteRenderer.color = Color.black;
        break;
    }
    if (!RenderableGroup()) return;
    Debug.Log("RENDERING: " + cell.group + "w: " + cell.group.width + "h:" + cell.group.height);
    spriteRenderer.color = Color.black;
    transform.localScale = new Vector3(cell.group.width, cell.group.height, 0);
  }

  internal void RenderGroup() {
    if (!RenderableGroup()) return;
    transform.position = targetPosition = GroupCenter();
    Debug.Log("Rendering Group at " + targetPosition);
    Render();
  }

  internal void UpdateTarget() {
    if (RenderableGroup()) {
      targetPosition = GroupCenter();
      Debug.Log("Updateing target. pos: " + transform.position + ", target: " + targetPosition);
    } else {
      targetPosition = GridToWorldSpace(cell.position);
    }
}

  Vector3 GroupCenter() {
    var grp = cell.group;
    float x = 0, y = 0;
    for (int i = grp.col; i < (grp.col + grp.width); i++) x += i * cellSize;
    for (int j = grp.row; j > (grp.row - grp.height); j--) y += j * cellSize;
    var ret = new Vector3(x / grp.width, (y / grp.height) * gravity, 0f);
    Debug.Log("GROUP: col:"+grp.col+", row:"+grp.row+", width: "+grp.width+", height: " + grp.height + " pos: " + ret);
    return ret;
  }

  // Coverts a grid position (row, column) to world space coordinate (Vector3)
  Vector3 GridToWorldSpace(Point p) {
    return new Vector3(p.col * cellSize, p.row * cellSize * gravity, 0f);
  }

  // Cells will be grouped for a time until they are cleaned up by the board
  bool RenderableGroup() {
    return cell.group != null && cell.group.Cell == cell;
  }
}
