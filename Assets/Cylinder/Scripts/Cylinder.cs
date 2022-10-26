using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Cylinder : MonoBehaviour
{
    [Header("Basic Settings:")] 
    public Material CylinderMaterial;
    
    public MeshRenderer Meshrenderer;
    public MeshFilter Meshfilter;
    
    [Range(1,10)]public int Amount;
    [Range(7, 25)] public int Resolution;
    public float Width;
    public float Height;
    
    public List<Transform> Target = new List<Transform>();
    
    
    public void Update()
    {
        if(Meshfilter.sharedMesh == null)
        {
            Meshfilter.sharedMesh = new Mesh();
        }

        Meshfilter.sharedMesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int e = 0; e < Amount; e++)
        {
            if (e == 0)
            {
                //Generating the first circle for the cylinder
                for (int r = 0; r < Resolution; r++)
                {
                    var angle = (r / (float)Resolution) * (Mathf.PI * 2);

                    var startVertPos = Vector3.zero;
                    startVertPos.x = Mathf.Sin(angle) * Width;
                    startVertPos.y = 0f;
                    startVertPos.z = Mathf.Cos(angle) * Width;

                    vertices.Add(startVertPos);
                }
            }

            Vector3 zylinderCentre = Vector3.up * (Height * e);
            // Calculate direction that points from the centre of the top face of zylinder towards the target point
            // You can think of this as the 'normal' of the top face
            Vector3 localUp = (Target[e].position - zylinderCentre).normalized;
            // Calculate a vector that is perpendicular to both the local up and the global forward vector
            Vector3 localRight = Vector3.Cross(localUp, Vector3.forward).normalized;
            // Calculate a vector that is perpendicular to both the local up and local right vector
            Vector3 localForward = Vector3.Cross(localRight, localUp).normalized;

            // Draw the vectors so we can see what's going on :)
            Debug.DrawRay(zylinderCentre, localUp, Color.green);
            Debug.DrawRay(zylinderCentre, localRight, Color.red);
            Debug.DrawRay(zylinderCentre, localForward, Color.cyan);

            //Top circle of the cylinder
            for (int r = 0; r < Resolution; r++)
            {
                var angle = (r / (float)Resolution) * (Mathf.PI * 2);
                float x = Mathf.Sin(angle) * Width;
                float y = Mathf.Cos(angle) * Width;
                // Use the local directions to orient the circle
                var endvertPos = localUp * (Height * (e + 1)) + localRight * x + localForward * y;

                vertices.Add(endvertPos);
            }

            //Setting the triangles
            var startIndex = Resolution * e;
            for (int i = 0; i < Resolution; i++)
            {

                triangles.Add(startIndex + i);
                triangles.Add(startIndex + (i + 1) % Resolution);
                triangles.Add(startIndex + i + Resolution);

                triangles.Add(startIndex + (i + 1) % Resolution);
                triangles.Add(startIndex + (i + 1) % Resolution + Resolution);
                triangles.Add(startIndex + i + Resolution);
            }
        }

        Meshfilter.sharedMesh.vertices = vertices.ToArray();
        Meshfilter.sharedMesh.triangles = triangles.ToArray();

        Meshfilter.sharedMesh.RecalculateBounds();
        Meshfilter.sharedMesh.RecalculateNormals();

        Meshrenderer.material = CylinderMaterial;
    }
    
}
