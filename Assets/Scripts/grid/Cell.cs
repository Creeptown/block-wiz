public class Cell {
  public CellType Type { get; private set; }
  public CellColor Color { get; private set; }
  public bool InGroup { get { return Group != null; } }
  public int RoundCreated { get; private set; }
  public CellGroup Group { get; set; }
  public Point Position { get; set; }

  public Cell(CellSpawn spawn) {
    Type = spawn.Type;
    Color = spawn.Color;
    RoundCreated = spawn.RoundCreated;
  }

  public override string ToString() {
    return "[Cell] Color: " + Color + ", Type:" + Type + " , Position: " + Position +", Round Created:" + RoundCreated;
  }
}
