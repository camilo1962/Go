using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point13 : MonoBehaviour
{
    public int x;
    public int y;
    public Stone13 stone13;
    public List<Point13> connections = new List<Point13>();
    public List<Point13> group = new List<Point13>();
    public List<Point13> dupGroup = new List<Point13>();
    public List<Point13> reached = new List<Point13>();
    public List<Point13> neighbors = new List<Point13>();




}
