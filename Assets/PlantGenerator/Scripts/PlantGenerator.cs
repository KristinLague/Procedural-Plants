using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class PlantGenerator : MonoBehaviour
{
    public Material PlantMaterial;
    
    [Header("Rules")]
    public List<Rules> PersonalizedRules = new List<Rules>();

    [Header("General Plant Settings")]
    [Range(1,10)] public int Iterations;
    [Range(8, 22)] public int Resolutions;
    public ExampleRules SelectedRules;
    public float Angle;
    public float Variance;
    
    [Header("Stem Settings")] 
    public Gradient StemColor;
    public float StemWidth;
    public Vector2 StemLengthMinMax;

    [Header("Leaf Setting:")] 
    public Gradient LeafColor;
    public Vector2 LeafLengthMinMax;
    public float LeafWidth;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private float minHeight;
    private float maxHeight;

    private bool initialised;
    private Rules appliedRules;
    
    //This represents the starting character
    private const string axiom = "X";

    //This keeps track of our rules.
    //Refer to AB -> A and A -> B (Wikipedia Graph)
    private Dictionary<char, string> rules = new Dictionary<char, string>();

    //This is necessary for when we have to save and refer back to a prior position.
    //Imagine a stack like a stack of paper, this is perfect because some of our rules
    //will instruct us to safe the current transform, do a few moves and the return back
    //to the last saved transform.
    private Stack<SavedTransform> transformStack = new Stack<SavedTransform>();

    private Vector3 startPosition;

    //this string keeps our path instructions, it is growing with every iteration
    //We therefore need to keep reference of it
    private string currentString = "";
    private float[] randomRotations;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        Plant currentPlant = GeneratePlant();


        List<CylinderGenerator.CylinderInfo> infos = new List<CylinderGenerator.CylinderInfo>();

        for(int s = 0; s < currentPlant.PlantSegments.Count; s++)
        {
            List<Vector3> pointList = new List<Vector3>();
            List<float> RadiiList = new List<float>();

            for (int e = 0; e < currentPlant.PlantSegments[s].PlantElements.Count; e++)
            {
                if(e == 0)
                {
                    pointList.Add(currentPlant.PlantSegments[s].PlantElements[e].StartPosition);
                    RadiiList.Add(currentPlant.PlantSegments[s].PlantElements[e].Width);
                }
                pointList.Add(currentPlant.PlantSegments[s].PlantElements[e].EndPosition);
                RadiiList.Add(currentPlant.PlantSegments[s].PlantElements[e].Width);
            }

            if(pointList.Count > 1)
            {
                var info =  new CylinderGenerator.CylinderInfo();
                info.Points = pointList.ToArray();
                info.Radii = RadiiList.ToArray();
                infos.Add(info);
            }
   
        }

        meshFilter.mesh = CylinderGenerator.GenerateCylinderMesh(infos.ToArray(), StemColor,minHeight, maxHeight, Resolutions);
        meshRenderer.material = PlantMaterial;
        
    }

    public Plant GeneratePlant()
    {
        minHeight = float.PositiveInfinity;
        maxHeight = float.NegativeInfinity;

        currentString = "";
        //Now to make our endresult appear in three dimensions we need some random angles.
        //To do so We are generating and array of as many random values between -1 and 1 as we like.
        randomRotations = new float[1000];
        for (int i = 0; i < randomRotations.Length; i++)
        {
            randomRotations[i] = Random.Range(-1.0f, 1.0f);
        }

        SwitchRules();

        Plant currentPlant = new Plant();

        //------------PART ONE - GROWING OF THE STRING/PATH--------------------------------
        //We are starting out to setting our currentString that is going to be our
        //"growing path" to our start position (the axiom)
        currentString = axiom;

        //We are using System.Text to make use of a stringbuilder
        StringBuilder stringBuilder = new StringBuilder();

        //We are looping through our iterations as we are repeating the process that many times.
        for (int i = 0; i < Iterations; i++)
        {
            //We are then getting an array consisting of all the chars in our currentString,
            //because we want to loop over them.To check if our current set of rules contains 
            //this char, if it does we want to apply our rules.
            //This is how our stirng will grow.
            char[] currentStringChars = currentString.ToCharArray();
            for (int j = 0; j < currentStringChars.Length; j++)
            {
                stringBuilder.Append(rules.ContainsKey(currentStringChars[j]) ? rules[currentStringChars[j]] : currentStringChars[j].ToString());
            }

            currentString = stringBuilder.ToString();
            stringBuilder = new StringBuilder();
        }

        //------------PART TWO - DRAWING OF THE LINES--------------------------------
        //We are starting out by looping over every character in our currentString
        //to then apply our instructions to each of them and draw the lines approprietly
      
        PlantSegment currentPlantSegment = new PlantSegment();
        Vector3 lastPosition = transform.position;
        currentPlant.PlantSegments.Add(currentPlantSegment);

        for (int k = 0; k < currentString.Length; k++)
        {
            switch (currentString[k])
            {
                case 'F':
                    //Positioning Changes - Moving our Object up/Forwards
                    PlantElement currentPlantElement = new PlantElement();
                    
                    currentPlantElement.StartPosition = transform.position;
                   

                    if (IsLeaf(k))
                    {
                        transform.Translate(transform.up * Random.Range(LeafLengthMinMax.x,LeafLengthMinMax.y));
                    }
                    else
                    {
                        transform.Translate(transform.up * Random.Range(StemLengthMinMax.x,StemLengthMinMax.y));
                    }

                    currentPlantElement.EndPosition = transform.position;
                    currentPlantElement.Width = (IsLeaf(k)) ? LeafWidth /2f : StemWidth;
                    currentPlantSegment.PlantElements.Add(currentPlantElement);

                    //Keeping track of the minimum and maximum height to apply the gradient as color later
                    if (transform.position.y > maxHeight)
                    {
                        maxHeight = transform.position.y;
                    }
                    else if (transform.position.y < minHeight)
                    {
                        minHeight = transform.position.y;
                    }

                    break;

                case 'X':
                    break;

                //For +,-,* and / we want to rotate in differenct directions.
                //We are using our Array of Random Directions here. 
                case '+':
                    transform.Rotate(Vector3.right * Angle * (1 + Variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

                case '-':
                    transform.Rotate(Vector3.left * Angle * (1 + Variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

                case '*':
                    transform.Rotate(Vector3.forward * Angle * (1 + Variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

                case '/':
                    transform.Rotate(Vector3.back * Angle * (1 + Variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

                //Now, for the spikey bracket we are saving the current position and placing it at
                //the top of our Stack. The best way to explain this process is the following:
                //If we are trying to draw a tree on paper, which has multiple end Points, as it has different StemPrefabes and such
                //We will need to lift the pen off the paper every now and then. So what we are doing is we are saving
                //the position were we want to draw a StemPrefab at a later point. And then once we finished the current StemPrefab we were 
                //working on we are going back to that saved position to add our new StemPrefab there.
                case '[':
                    transformStack.Push(new SavedTransform(transform.position, transform.rotation));
                    break;

                //We are getting the last saved transform from our strack and apply it to our gameobject.
                //to then continue from this position.
                case ']':
                    SavedTransform savedTransform = transformStack.Pop();

                    currentPlantSegment = new PlantSegment();
                    currentPlant.PlantSegments.Add(currentPlantSegment);

                    transform.position = savedTransform.Position;
                    transform.rotation = savedTransform.Rotation;
                    break;
            }

        }
        return currentPlant;
    }

    private bool IsLeaf(int k)
    {
        return currentString[k + 1] % currentString.Length == 'X' || currentString[k + 3] % currentString.Length == 'F' &&
                    currentString[k + 4] % currentString.Length == 'X';
    }

    public void SwitchRules()
    {
        rules.Clear();
        switch (SelectedRules)
        {
            case ExampleRules.ONE:
                rules.Add('X', "[-FX][+FX][FX]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.TWO:
                rules.Add('X', "[F-[X+X]+F[+FX]-X]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.THREE:
                rules.Add('X', "[-FX]X[+FX][+F-FX]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.FOUR:
                rules.Add('X', "[FF[+XF-F+FX]--F+F-FX]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.FIVE:
                rules.Add('X', "[FX[+F[-FX]FX][-F-FXFX]]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.SIX:
                rules.Add('X', "[F[+FX][*+FX][/+FX]]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.SEVEN:
                rules.Add('X', "[*+FX]X[+FX][/+F-FX]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.EIGHT:
                rules.Add('X', "[F[-X+F[+FX]][*-X+F[+FX]][/-X+F[+FX]-X]]");
                rules.Add('F', "FF");
                break;

            case ExampleRules.PERSONAL:
                //If we have set up some personalized rules we are using them.
                if (PersonalizedRules.Count > 0)
                {
                    appliedRules = PersonalizedRules[0];
                    TranslateRulesToDictionary();
                }
                else
                {
                    //We are also populating our set of rules with a standard set of rules just in
                    //case that our personalized rules are empty.
                    rules.Add('X', "[-FX][+FX][FX]");
                    rules.Add('F', "FF");
                }
                break;
        }

        TranslateRulesToDictionary();
        
    }

    public void TranslateRulesToDictionary()
    {
        if (PersonalizedRules.Contains(appliedRules))
        {
            //Clearing the old rules from the dictionary
            rules.Clear();
            //Looping through all the rules and adding them to the rules dictionary.
            for (int i = 0; i < appliedRules.Instructions.Count; i++)
            {
                rules.Add(appliedRules.Instructions[i].Name, appliedRules.Instructions[i].Addition);
            }
        }
    }
}

public static class CylinderGenerator
{
    //Generating multiple cylinders that are being connected to each other, depending on their endPositions and their width.
    //These are being returned as a mesh
    public static Mesh GenerateCylinderMesh(CylinderInfo[] cylinderInfos, Gradient gradient ,float minHeight, float maxHeight, int resolution)
    {
        var allVerts = new List<Vector3>();
        var allTris = new List<int>();
        var allcolors = new List<Color>();

        for (int i = 0; i < cylinderInfos.Length; i++)
        {
            var (verts, tris, colors) = GenerateCylinder(cylinderInfos[i], gradient,minHeight, maxHeight, resolution);
            int numCurrentVerts = allVerts.Count;
            allVerts.AddRange(verts);

            for (int j = 0; j < tris.Count; j++)
            {
                allTris.Add(tris[j] + numCurrentVerts);
            }

            allcolors.AddRange(colors);
        }
        
        Mesh mesh = new Mesh();
        
        mesh.SetVertices(allVerts);
        mesh.SetTriangles(allTris, 0, true);
        mesh.SetColors(allcolors);
        mesh.RecalculateNormals();
        
        return mesh;
    }

    static (List<Vector3> vertices, List<int> triangles, List<Color> colors) GenerateCylinder(CylinderInfo cylinderInfo, Gradient gradient,float minHeight, float maxHeight, int resolution)
    {
        int numPoints = cylinderInfo.Points.Length;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();

        Vector3 prevAxisA = Vector3.zero;
        Vector3 prevAxisB = Vector3.zero;
        // Loop over each centre point
        for (int circleIndex = 0; circleIndex < numPoints; circleIndex++)
        {

            Vector3 centre = cylinderInfo.Points[circleIndex];
            float radius = cylinderInfo.Radii[circleIndex];

            // Calculate dir the circle should face
            Vector3 dir = Vector3.zero;

            // For the first circle, this will be pointing towards the next circle
            if (circleIndex == 0)
            {
                dir = (cylinderInfo.Points[circleIndex + 1] - cylinderInfo.Points[circleIndex]).normalized;
            }
            // For the last circle, this will be in the same direction as the previous circle
            else if (circleIndex == numPoints - 1)
            {
                dir = (cylinderInfo.Points[circleIndex] - cylinderInfo.Points[circleIndex - 1]).normalized;
            }
            // For the rest, the circle should be oriented halfway between the previous and next circle
            else
            {
                var nextDir = (cylinderInfo.Points[circleIndex + 1] - cylinderInfo.Points[circleIndex]).normalized;
                var prevDir = (cylinderInfo.Points[circleIndex] - cylinderInfo.Points[circleIndex - 1]).normalized;
                dir = (nextDir + prevDir).normalized;
            }

            Vector3 axisA;
            Vector3 axisB;
            if (circleIndex == 0)
            {
                axisA = Vector3.Cross(dir, Vector3.forward).normalized;
                axisB = Vector3.Cross(axisA, dir).normalized;
            }
            else
            {
                axisA = Vector3.Cross(dir, prevAxisB).normalized;
                axisB = Vector3.Cross(axisA, dir).normalized;
            }
            prevAxisA = axisA;
            prevAxisB = axisB;

            // Construct a circle of vertices around the centre point
            for (int i = 0; i < resolution; i++)
            {
                float angle = 2 * Mathf.PI * i / (float)(resolution);
                Vector3 pos = centre + (axisA * Mathf.Sin(angle) + axisB * Mathf.Cos(angle)) * radius;

                vertices.Add(pos);
                var averageHeight = cylinderInfo.Points[circleIndex].y;

                // Calculate triangle indices to join with the next circle
                if (circleIndex < numPoints - 1)
                {
                    int triStartIndex = circleIndex * resolution;
                    triangles.Add(triStartIndex + i);
                    triangles.Add(triStartIndex + (i + 1) % resolution);
                    triangles.Add(triStartIndex + i + resolution);

                    triangles.Add(triStartIndex + (i + 1) % resolution);
                    triangles.Add(triStartIndex + (i + 1) % resolution + resolution);
                    triangles.Add(triStartIndex + i + resolution);

                }

                colors.Add(EvaluateColor(averageHeight, gradient,minHeight,maxHeight));
               
            }
        }
        return (vertices, triangles, colors);
    }

    static Color EvaluateColor(float averageHeight, Gradient colorGradient, float minHeight, float maxHeight)
    {
        var gradientVal = Mathf.InverseLerp(minHeight, maxHeight, averageHeight);
        return colorGradient.Evaluate(gradientVal); 
    }

    public class CylinderInfo
    {
        public Vector3[] Points;
        public float[] Radii;
    }
}
