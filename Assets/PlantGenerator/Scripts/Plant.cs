using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant
{
    public List<PlantSegment> PlantSegments;

    public Plant()
    {
        PlantSegments = new List<PlantSegment>();
    }
}

public class PlantSegment
{
    public List<PlantElement> PlantElements;

    public PlantSegment()
    {
        PlantElements = new List<PlantElement>();
    }
}

public class PlantElement
{
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public float Width;
}
