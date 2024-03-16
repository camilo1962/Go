using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point9 : MonoBehaviour
{
    public int x;
    public int y;
    public Stone9 stone9;
    public List<Point9> connections = new List<Point9>();
    public List<Point9> group = new List<Point9>();
    public List<Point9> dupGroup = new List<Point9>();
    public List<Point9> reached = new List<Point9>();
    public List<Point9> neighbors = new List<Point9>();




}
