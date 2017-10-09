public class Cell {
  public enum Type {
    Normal,
    Bomb,
    Counter,
    Diamond
  }

  public enum Color {
    Red,
    Green,
    Blue,
    Yellow
  }

  internal Type type;
  internal Color color;
  internal CellGroup group;
  internal Point position;
  internal int roundCreated;
  internal bool InGroup { get { return group != null; } }

  public Cell(Color c, Type t, int r) {
    type = t;
    color = c;
    roundCreated = r;
  }

  public override string ToString() {
    return "[Cell] Color: " + color + ", Type:" + type + " , Round Created:" + roundCreated;
  }
}
