﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour {
  public enum State {
    Normal,
  }
  public Board board;
}