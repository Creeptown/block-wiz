using System.Collections.Generic;

public class CellGroupEqualityComparer : IEqualityComparer<CellGroup>  {
  public bool Equals(CellGroup a, CellGroup b) {
    return a.Column == b.Column && a.Row == b.Row;
  }
  public int GetHashCode(CellGroup a) {
    return a.GetHashCode();
  }
}
