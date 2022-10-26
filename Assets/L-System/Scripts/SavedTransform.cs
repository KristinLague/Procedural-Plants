using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SavedTransform
{
    public Vector3 Position;
    public Quaternion Rotation;

    public SavedTransform(Vector3 Position, Quaternion Rotation)
    {
        this.Position = Position;
        this.Rotation = Rotation;
    }
}
