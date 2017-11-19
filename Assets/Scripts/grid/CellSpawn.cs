public struct CellSpawn {
  public CellType Type { get; private set; }
  public CellColor Color { get; private set; }
  public int RoundCreated { get; private set; }

  public CellSpawn(CellColor color, CellType type, int round) {
    Type = type;
    Color = color;
    RoundCreated = round;
  }
}

