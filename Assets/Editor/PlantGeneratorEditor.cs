using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class PlantGeneratorEditor : EditorWindow
{
    private List<Rules> personalizedRules = new List<Rules>();
    private int iterations = 5;
    private float angle = 30f;
    private float width = 1f;
    private float minLeafLength = 1.5f;
    private float maxLeafLength = 5f;
    private float minlength = 0.5f;
    private float maxLength = 1.5f;
    private float variance = 10f;
    
    private bool hasTreeChanged;
    private GameObject tree;
    private Rules currentlyAppliedRules;
    
    //This represents our starting character.
    private const string axiom = "X";

    //This keeps tracks of our rules.
    //Refer to AB -> A and A -> B (Maybe show Wikipedia Graphic)
    private Dictionary<char, string> rules = new Dictionary<char, string>();
    //This is necessary for when we have to save and refer back to a prior position.
    //Imagine a stack like a stack of paper, this is perfect for us because some of our Rules 
    //will instruct us to safe the current Transform, do a few moves and then return to the last
    //saved Transform.
    private Stack<SavedTransform> transformStack = new Stack<SavedTransform>();

    private Vector3 initialPosition;

    Vector2 boundsMinMaxX;
    Vector2 boundsMinMaxY;
    List<TreeElement> allLines;

    //The string keeps our path instructions, it is growing with every iteration. 
    //We therefore need to keep track of it
    private string currentString = "";
    private float[] randomRotations;
    
    //Prefabs
    private GameObject treeParent;
    private GameObject branch;
    private GameObject leaf;

    private GameObject transformKeeper;
    
    [MenuItem("L-System/PlantGeneratorEditor")]
    public static void ShowExample()
    {
        PlantGeneratorEditor wnd = GetWindow<PlantGeneratorEditor>();
        wnd.titleContent = new ("PlantGeneratorEditor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PlantGeneratorEditor.uxml");
        VisualElement editorUI     = visualTree.Instantiate();
        root.Add(editorUI);
        
		treeParent = GameObject.Find("treeParent");
		branch = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Branch.Prefab");
		leaf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Leaf.Prefab");

		transformKeeper = new GameObject();
        
        Slider angleSD              = root.Q<Slider>("angleSD");
        Slider widthSD              = root.Q<Slider>("widthSD");
        Slider varianceSD           = root.Q<Slider>("varianceSD");
        SliderInt iterationSD       = root.Q<SliderInt>("iterationsSD");
        MinMaxSlider leafLengthMMSD = root.Q<MinMaxSlider>("leafLengthMMSD");
        MinMaxSlider lengthMMSD     = root.Q<MinMaxSlider>("lengthMMSD");
        DropdownField rulesDD       = root.Q<DropdownField>("rulesDD");
        
        angleSD.SetValueWithoutNotify(angle);
        angleSD.RegisterValueChangedCallback(evt => angle = angleSD.value);
        angleSD.lowValue = 5f;
        angleSD.highValue = 90f;
        
        widthSD.SetValueWithoutNotify(width);
        widthSD.RegisterValueChangedCallback(evt => width = widthSD.value);
        widthSD.lowValue = 0.1f;
        widthSD.highValue = 5f;
        
        varianceSD.SetValueWithoutNotify(variance);
        varianceSD.RegisterValueChangedCallback(evt => variance = varianceSD.value);
        varianceSD.lowValue = 1f;
        varianceSD.highValue = 30f;
        
        iterationSD.SetValueWithoutNotify(iterations);
        iterationSD.RegisterValueChangedCallback(evt => iterations = iterationSD.value);
        iterationSD.lowValue = 1;
        iterationSD.highValue = 10;
        
        leafLengthMMSD.SetValueWithoutNotify(new Vector2(minLeafLength,maxLeafLength));
        leafLengthMMSD.RegisterValueChangedCallback(evt =>
        {
            minLeafLength = leafLengthMMSD.value.x;
            maxLeafLength = leafLengthMMSD.value.y;
        });
        leafLengthMMSD.lowLimit = 0.1f;
        leafLengthMMSD.highLimit = 5f;
        
        lengthMMSD.SetValueWithoutNotify(new Vector2(minlength,maxLength));
        lengthMMSD.RegisterValueChangedCallback(evt =>
        {
            minlength = lengthMMSD.value.x;
            maxLength = lengthMMSD.value.y;
        });
        lengthMMSD.lowLimit = 0.1f;
        lengthMMSD.highLimit = 5f;

        List<string> dropdownChoices = new List<string>();
        personalizedRules = new List<Rules>(GetAllInstances<Rules>());
        foreach (var entry in personalizedRules)
        {
            dropdownChoices.Add(entry.RuleName);
        }

        rulesDD.choices = dropdownChoices;
        rulesDD.RegisterValueChangedCallback(evt =>
        {
            currentlyAppliedRules = personalizedRules.Find(x => x.RuleName == evt.newValue);
            Redraw();
        });
        rulesDD.value = dropdownChoices[0];
    }

    private void Redraw()
    {
	    //Now to make our endresult appear in three dimensions we need some random angles.
	    //To do so We are generating and array of as many random values between -1 and 1 as we like.
	    randomRotations = new float[1000];
	    for (int i = 0; i < randomRotations.Length; i++)
	    {
		    randomRotations[i] = Random.Range(-1.0f, 1.0f);
	    }

	    //If we have set up some personalized rules we are using them.
	    if(personalizedRules.Count > 0)
	    {
		    currentlyAppliedRules = personalizedRules[0];
		    TranslateRulesToDictionary();
	    } 
	    else 
	    {
		    //We are also populating our set of rules with a standard set of rules just in
		    //case that our personalized rules are empty.
		    rules.Add('X', "[-FX][+FX][FX]");
		    rules.Add('F', "FF");
	    }

	    //Lastly we need to call our Generate() function to generate a tree.
	    Generate();
    }

    public void Generate()
	{
		allLines = new List<TreeElement>();
        boundsMinMaxX = new Vector2(float.MaxValue, float.MinValue);
        boundsMinMaxY = new Vector2(float.MaxValue, float.MinValue);

        Destroy(tree);
        tree = Instantiate(treeParent);

        //------------PART ONE - GROWING OF THE STRING/PATH--------------------------------
        //We are starting out to setting our currentString that is going to be our
        //"growing path" to our start position (the axiom)
        currentString = axiom;

        //We are using System.Text to make use of a stringbuilder
        StringBuilder stringBuilder = new StringBuilder();

		//We are looping through our iterations as we are repeating the process that many times.
        for (int i = 0; i < iterations; i++)
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
                    initialPosition = transformKeeper.transform.position;
                    bool isLeaf = false;

                    GameObject currentElement;
                    //Since every F - so therefore every move forward is represented in a new Tree Element we need to decide
                    //if this new Element should be a Leaf or a Branch.
                    if (currentString[k + 1] % currentString.Length == 'X' || currentString[k + 3] % currentString.Length == 'F' &&
                    currentString[k + 4] % currentString.Length == 'X')
                    {
                        currentElement = Instantiate(leaf);
                        isLeaf = true;
                    }
                    else
                    {
                        currentElement = Instantiate(branch);
                    }

                    currentElement.transform.SetParent(tree.transform);

                    //Getting the currentTreeElement so that we can access the linerenderer
                    TreeElement currentTreeElement = currentElement.GetComponent<TreeElement>();

					//Setting the LineRenderers start and endpoint aswell as its thiccness and color
                    currentTreeElement.lineRenderer.SetPosition(0, initialPosition);

					if(isLeaf)
					{
						transformKeeper.transform.Translate(Vector3.up * 2f * Random.Range(minLeafLength,maxLeafLength));
					} else
					{
						transformKeeper.transform.Translate(Vector3.up * 2f * Random.Range(minlength, maxLength));
					}
                    currentTreeElement.lineRenderer.SetPosition(1, transformKeeper.transform.position);
					if(isLeaf)
					{
						currentTreeElement.lineRenderer.startWidth = width * 2f;
                        currentTreeElement.lineRenderer.endWidth = width / 4f;
                        currentTreeElement.isLeaf = true;
                    }
					else
					{
						currentTreeElement.lineRenderer.startWidth = width;
                    	currentTreeElement.lineRenderer.endWidth = width;
					}
                    
                    currentTreeElement.lineRenderer.sharedMaterial = currentTreeElement.lineMaterial;
                    allLines.Add(currentTreeElement);
                    break;

				case 'X':
                    break;
	
				//For +,-,* and / we want to rotate in differenct directions.
				//We are using our Array of Random Directions here. 
				case '+':
					transformKeeper.transform.Rotate(Vector3.back * angle * (1 + variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

				case '-':
					transformKeeper.transform.Rotate(Vector3.forward * angle * (1 + variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

				case '*':
                    transformKeeper.transform.Rotate(Vector3.up * 120f * (1f + variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

				case '/':
					transformKeeper.transform.Rotate(Vector3.down * 120f * (1f + variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

				//Now, for the spikey bracket we are saving the current position and placing it at
				//the top of our Stack. The best way to explain this process is the following:
				//If we are trying to draw a tree on paper, which has multiple end points, as it has different branches and such
				//We will need to lift the pen off the paper every now and then. So what we are doing is we are saving
				//the position were we want to draw a branch at a later point. And then once we finished the current branch we were 
				//working on we are going back to that saved position to add our new branch there.
                case '[':
                    transformStack.Push(new SavedTransform(transformKeeper.transform.position, transformKeeper.transform.rotation));
                    break;

				//We are getting the last saved transform from our strack and apply it to our gameobject.
				//to then continue from this position.
				case ']':
                    SavedTransform savedTransform = transformStack.Pop();

                    transformKeeper.transform.position = savedTransform.Position;
                    transformKeeper.transform.rotation = savedTransform.Rotation;
                    break;
            }

			//Calculating the bounds to make sure that our newly generated tree can be displayed
            boundsMinMaxX.x = Mathf.Min(transformKeeper.transform.position.x, boundsMinMaxX.x);
			boundsMinMaxX.y = Mathf.Max(transformKeeper.transform.position.x, boundsMinMaxX.y);
			boundsMinMaxY.x = Mathf.Min(transformKeeper.transform.position.y, boundsMinMaxY.x);
			boundsMinMaxY.y = Mathf.Max(transformKeeper.transform.position.y, boundsMinMaxY.y);
        }

		//Applying the calculated bounds minimum and maximum in comparison to the rect to our camera for scaling the viewport to our tree.
		float aspect = (float)Screen.width / Screen.height;
        Vector3 treeCentre = new Vector3(boundsMinMaxX.x + boundsMinMaxX.y, boundsMinMaxY.x + boundsMinMaxY.y) * .5f;
        float treeHeight = boundsMinMaxY.y - boundsMinMaxY.x;
		float treeWidth = boundsMinMaxX.y - boundsMinMaxX.x;
        float treeSize = Mathf.Max(treeHeight, treeWidth*aspect);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = treeSize * .5f + 1.5f;
        
        Camera.main.transform.position = treeCentre + Vector3.back + Vector3.left * Camera.main.orthographicSize * aspect * .5f;

    }

	public void SwitchRules(Rules rule)
	{
        currentlyAppliedRules = rule;
        TranslateRulesToDictionary();
        Generate();
    }
    
    public void TranslateRulesToDictionary()
    {
        if(personalizedRules.Contains(currentlyAppliedRules))
        {
            //Clearing the old rules from the dictionary
            rules.Clear();
            //Looping through all the rules and adding them to the rules dictionary.
            for (int i = 0; i < currentlyAppliedRules.AppliedRules.Count; i++)
            {
                rules.Add(currentlyAppliedRules.AppliedRules[i].Name, currentlyAppliedRules.AppliedRules[i].addition);
            }
        }
    }
    
    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);  
        T[] a = new T[guids.Length];
        for(int i = 0; i < guids.Length; i++)         
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }
 
        return a;
    }
}