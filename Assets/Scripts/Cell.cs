using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
  public enum Type {
    Normal,
    Bomb,
    Counter,
    Diamond
  }

  public enum Color {
    All,
    Red,
    Green,
    Blue,
    Yellow
  }

  public Type type;
  public Color color;

  private int RoundCreated = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

  public bool CanSpawn() {
    return true;
  }
}
