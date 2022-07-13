using UnityEngine;

public struct SavedTransform
{
    private Vector3 position;
    private Quaternion rotation;

	public Vector3 Position { get { return position; }  }
	public Quaternion Rotation { get { return rotation; } }

	public SavedTransform(Vector3 position, Quaternion rotation)
	{
        this.position = position;
        this.rotation = rotation;
    }
}
