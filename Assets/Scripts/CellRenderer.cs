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

  public Vector3 TargetPosition { get; private set; }
  public CellRendererState State { get; private set; }
  public Cell Cell { get; private set; }

  SpriteRenderer spriteRenderer;
  int cellSize;
  int gravity;

  void Awake () {
    // TODO Should be Spawning
    State = CellRendererState.Falling;
    gameObject.AddComponent<SpriteRenderer>();
  }

  // Set the target position and render the cell based on its properties
  internal void Initialize(Cell cell, int cellSize, int gravity) {
    this.Cell = cell;
    this.cellSize = cellSize;
    this.gravity = gravity;
    TargetPosition = GridToWorldSpace(cell.Position);
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
        spriteRenderer.color = Color.red;
        break;
      case CellColor.Green:
        spriteRenderer.color = Color.green;
        break;
      case CellColor.Blue:
        spriteRenderer.color = Color.blue;
        break;
      case CellColor.Yellow:
        spriteRenderer.color = Color.yellow;
        break;
      default:
        spriteRenderer.color = Color.black;
        break;
    }
    if (!RenderableGroup()) return;
    Debug.Log("RENDERING: " + Cell.Group + "w: " + Cell.Group.Width + "h:" + Cell.Group.Height);
    spriteRenderer.color = Color.black;
    transform.localScale = new Vector3(Cell.Group.Width, Cell.Group.Height, 0);
  }

  internal void RenderGroup() {
    if (!RenderableGroup()) return;
    transform.position = TargetPosition = GroupCenter();
    Debug.Log("Rendering Group at " + TargetPosition);
    Render();
  }

  internal void UpdateTarget() {
    if (RenderableGroup()) {
      TargetPosition = GroupCenter();
      Debug.Log("Updateing target. pos: " + transform.position + ", target: " + TargetPosition);
    } else {
      TargetPosition = GridToWorldSpace(Cell.Position);
    }
}

  Vector3 GroupCenter() {
    var grp = Cell.Group;
    float x = 0, y = 0;
    for (int i = grp.Column; i < (grp.Column + grp.Width); i++) x += i * cellSize;
    for (int j = grp.Row; j > (grp.Row - grp.Height); j--) y += j * cellSize;
    var ret = new Vector3(x / grp.Width, (y / grp.Height) * gravity, 0f);
    Debug.Log("GROUP: col:"+grp.Column+", row:"+grp.Row+", width: "+grp.Width+", height: " + grp.Height + " pos: " + ret);
    return ret;
  }

  // Coverts a grid position (row, column) to world space coordinate (Vector3)
  Vector3 GridToWorldSpace(Point p) {
    return new Vector3(p.Col * cellSize, p.Row * cellSize * gravity, 0f);
  }

  // Cells will be grouped for a time until they are cleaned up by the board
  bool RenderableGroup() {
    return Cell.Group != null && Cell.Group.Cell == Cell;
  }
}
