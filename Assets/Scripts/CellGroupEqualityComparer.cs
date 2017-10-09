using System.Collections.Generic;

public class CellGroupEqualityComparer : IEqualityComparer<CellGroup>  {
  public bool Equals(CellGroup a, CellGroup b) {
    return a.col == b.col && a.row == b.row;
  }
  public int GetHashCode(CellGroup a) {
    return a.GetHashCode();
  }
}
