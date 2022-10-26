using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystem : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject StemPrefab;
    public GameObject LeafPrefab;
    public Material PlantMaterial;

    [Header("Rules")]
    public List<Rules> PersonalizedRules = new List<Rules>();

    [Header("General Plant Settings")]
    [Range(1,10)] public int Iterations;
    public ExampleRules SelectedRules;
    public float Angle;
    public float Variance;
    
    [Header("Stem Settings")] 
    public Color StemColor;
    public float StemWidth;
    public Vector2 StemLengthMinMax;

    [Header("Leaf Setting:")] 
    public Gradient LeafColor;
    public Vector2 LeafLengthMinMax;
    public Vector2 LeafWidthMinMax;

    [HideInInspector] public GameObject Plant;

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
        Initiate();
    }

    public void Initiate()
    {
        currentString = "";
        //Now to make our endresult appear in three dimensions we need some random angles.
        //To do so We are generating and array of as many random values between -1 and 1 as we like.
        randomRotations = new float[1000];
        for (int i = 0; i < randomRotations.Length; i++)
        {
            randomRotations[i] = Random.Range(-1.0f, 1.0f);
        }

        SwitchRules();

        //Lastly we need to call our Generate() function to generate a tree.
        Generate();
    }
    
    public void Generate()
    {
        if(Plant != null)
        {
            DestroyImmediate(Plant);
        }
        
        Plant = new GameObject("new Plant");

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
        for (int k = 0; k < currentString.Length; k++)
        {
            switch (currentString[k])
            {
                case 'F':
                    //Positioning Changes - Moving our Object up/Forwards
                    startPosition = transform.position;
                    bool isLeaf = false;

                    GameObject currentElement;
                    //Since every F - so therefore every move forward is represented in a new Tree Element we need to decide
                    //if this new Element should be a Leaf or a StemPrefab.
                    if (IsLeaf(k))
                    {
                        currentElement = Instantiate(LeafPrefab);
                        isLeaf = true;
                    }
                    else
                    {
                        currentElement = Instantiate(StemPrefab);
                    }

                    currentElement.transform.SetParent(Plant.transform);

                    //Getting the currentElement's linerenderer
                    LineRenderer currentLineRenderer = currentElement.GetComponent<LineRenderer>();

                    //Setting the LineRenderers start and endpoint aswell as its thiccness and color
                    currentLineRenderer.SetPosition(0, startPosition);
                    
                    
                    if (isLeaf)
                    {
                        transform.Translate(transform.up * Random.Range(LeafLengthMinMax.x,LeafLengthMinMax.y));
                    }
                    else
                    {
                        transform.Translate(transform.up * Random.Range(StemLengthMinMax.x,StemLengthMinMax.y));
                    }

                    float width;

                    currentLineRenderer.SetPosition(1, transform.position);
                 

                    if (isLeaf)
                    {
                        
                        width = Random.Range(LeafWidthMinMax.x,LeafWidthMinMax.y);
                        currentLineRenderer.startWidth = width * 2f;
                        currentLineRenderer.endWidth = width / 4f;
                        currentLineRenderer.colorGradient = LeafColor;
                    }
                    else
                    {
                        currentLineRenderer.startWidth = StemWidth;
                        currentLineRenderer.endWidth = StemWidth;
                        currentLineRenderer.startColor = StemColor;
                        currentLineRenderer.endColor = StemColor;
                    }

                    currentLineRenderer.sharedMaterial = PlantMaterial;
                    break;

                case 'X':
                    break;

                //For +,-,* and / we want to rotate in different directions.
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
                //If we are trying to draw a tree on paper, which has multiple end points, as it has different StemPrefabes and such
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

                    transform.position = savedTransform.Position;
                    transform.rotation = savedTransform.Rotation;
                    break;
            }

        }

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
        Generate();
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

public enum ExampleRules
{
    ONE,
    TWO,
    THREE,
    FOUR,
    FIVE,
    SIX,
    SEVEN,
    EIGHT,
    PERSONAL
}
