public class Cell {
  public CellType Type { get; private set; }
  public CellColor Color { get; private set; }
  public bool InGroup { get { return Group != null; } }
  public int RoundCreated { get; private set; }
  public CellGroup Group { get; set; }
  public Point Position { get; set; }

  public Cell(CellColor c, CellType t, int r) {
    Type = t;
    Color = c;
    RoundCreated = r;
  }

  public override string ToString() {
    return "[Cell] Color: " + Color + ", Type:" + Type + " , Round Created:" + RoundCreated;
  }
}
