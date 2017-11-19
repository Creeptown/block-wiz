using UnityEngine;

public class CellRenderer : MonoBehaviour {
  public enum CellRendererState {
    Preview,    // Showing as upcoming drop
    Spawning,   // Group is about to become Falling
    Falling,    // Group is moving towards target
    Landed,     // Touching another CellGroup but still able to be moved
    Fixed,      // No longer player controllable
    Combining,  // Combining into larger cell
    Destroying, // In the processed of being destroyed
    Dead,       // Destroyed. Can be removed from game
  }

  [Tooltip("Normal Sprite")]
  public Sprite normalSprite;
  [Tooltip("Bomb Sprite")]
  public Sprite bombSprite;
  public Color red;
  public Color blue;
  public Color green;
  public Color yellow;

  public Vector3 TargetPosition { get; private set; }
  public CellRendererState State { get; private set; }
  public Cell Cell { get; private set; }

  float zOffset = -0.1f;
  SpriteRenderer spriteRenderer;
  Board board;

  void Awake () {
    // TODO Should be Spawning
    State = CellRendererState.Falling;
    gameObject.AddComponent<SpriteRenderer>();
  }

  // Set the target position and render the cell based on its properties
  internal void Initialize(Cell cell, Board board) {
    this.Cell = cell;
    this.board = board;
    TargetPosition = board.GridToWorldSpace(cell.Position);
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
    switch (Cell.Type) {
      case CellType.Bomb:
        spriteRenderer.sprite = bombSprite;
        break;
      default:
        spriteRenderer.sprite = normalSprite;
        break;
    }
    switch (Cell.Color) {
      case CellColor.Red:
        spriteRenderer.color = red;
        break;
      case CellColor.Green:
        spriteRenderer.color = green;
        break;
      case CellColor.Blue:
        spriteRenderer.color = blue;
        break;
      case CellColor.Yellow:
        spriteRenderer.color = yellow;
        break;
      default:
        spriteRenderer.color = Color.black;
        break;
    }

    if (RenderableGroup()) {
      transform.localScale = new Vector3(Cell.Group.Width, Cell.Group.Height, 0);
    }
  }

  internal void RenderGroup() {
    if (!RenderableGroup()) return;
    transform.position = TargetPosition = GroupCenter();
    //Debug.Log("Rendering Group at " + TargetPosition);
    Render();
  }

  internal void UpdateTarget() {
    if (RenderableGroup()) {
      TargetPosition = GroupCenter();
      //Debug.Log("Updateing target. pos: " + transform.position + ", target: " + TargetPosition);
    } else {
      TargetPosition = board.GridToWorldSpace(Cell.Position);
    }
}

  // TODO This is now wrong
  Vector3 GroupCenter() {
    var grp = Cell.Group;
    float x = 0, y = 0;
    for (int i = grp.Column; i < (grp.Column + grp.Width); i++) x += i * board.cellSize;
    for (int j = grp.Row; j > (grp.Row - grp.Height); j--) y += j * board.cellSize;
    var ret = new Vector3(x / grp.Width, (y / grp.Height) * (int)board.gravity, zOffset);
    //Debug.Log("GROUP: col:"+grp.Column+", row:"+grp.Row+", width: "+grp.Width+", height: " + grp.Height + " pos: " + ret);
    return ret;
  }

  // Cells will be grouped for a time until they are cleaned up by the board
  bool RenderableGroup() {
    return Cell.Group != null && Cell.Group.Cell == Cell;
  }
}
