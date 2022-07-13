using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystem : MonoBehaviour 
{
    public float growSpeed=20;
    public float leafGrowSpeed = 2;

    //Changeable variables to modify outcome
    public List<Rules> personalizedRules = new List<Rules>();
    public int iterations;
    public float angle;
    public float width;
    public float minLeafLength;
    public float maxLeafLength;
    public float minlength;
    public float maxLength;
    public float variance;
    public bool hasTreeChanged;
    public GameObject tree;
    public Rules currentlyAppliedRules;
    
	//Prefabs
    public GameObject treeParent;
    public GameObject branch;
    public GameObject leaf;

	private static LSystem instance;
	public static LSystem Instance {get { return instance; } }

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
    List<Coroutine> animationRoutines;

    //The string keeps our path instructions, it is growing with every iteration. 
    //We therefore need to keep track of it
    private string currentString = "";
    private float[] randomRotations;

	private void Awake()
	{
        instance = this;

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

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
            StartGrowthAnimation();
        }
	}

	void StartGrowthAnimation() {
        CancelAnimation();
        animationRoutines = new List<Coroutine>();
        var a = StartCoroutine(AnimateGrowth());
        animationRoutines.Add(a);
    }

	IEnumerator AnimateGrowth() {
        var lines = allLines;
        foreach (var l in lines)
            l.gameObject.SetActive(false);

        float leafWaitTime = 0;

        foreach (var treeElement in lines) {
			if (treeElement.isLeaf) {
                leafWaitTime += Random.Range(.5f, 2);
                var a = StartCoroutine(AnimateLeaf(treeElement.lineRenderer, leafWaitTime));
                animationRoutines.Add(a);
                leafWaitTime += 1 / leafGrowSpeed;
                continue;
            }
			else {
                leafWaitTime = 0;
            }
            float t = 0;
            Vector3 target = treeElement.lineRenderer.GetPosition(1);
            Vector3 start = treeElement.lineRenderer.GetPosition(0);
            treeElement.lineRenderer.SetPosition(1, start);
			treeElement.lineRenderer.gameObject.SetActive(true);

			while(t<1) {
                t += Time.deltaTime * growSpeed;
                treeElement.lineRenderer.SetPosition(1, Vector3.Lerp(start, target, t));
                yield return null;
            }
        }
    }

    IEnumerator AnimateLeaf(LineRenderer leaf, float waitTime) {
        yield return new WaitForSeconds(waitTime);
        float t = 0;
		Vector3 target = leaf.GetPosition(1);
		Vector3 start = leaf.GetPosition(0);
		leaf.SetPosition(1, start);
		leaf.gameObject.SetActive(true);

		while(t<1) {
			t += Time.deltaTime * leafGrowSpeed;
			leaf.SetPosition(1, Vector3.Lerp(start, target, t));
			yield return null;
		}
	}

	void CancelAnimation() {
		if (animationRoutines != null) {
			foreach (var c in animationRoutines) {
                StopCoroutine(c);
            }
		}
	}


	public void Generate()
	{
        CancelAnimation();
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
                    initialPosition = transform.position;
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
						transform.Translate(Vector3.up * 2f * Random.Range(minLeafLength,maxLeafLength));
					} else
					{
						transform.Translate(Vector3.up * 2f * Random.Range(minlength, maxLength));
					}
                    currentTreeElement.lineRenderer.SetPosition(1, transform.position);
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
					transform.Rotate(Vector3.back * angle * (1 + variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

				case '-':
					transform.Rotate(Vector3.forward * angle * (1 + variance / 100 * randomRotations[k % randomRotations.Length]));
                    break;

				case '*':
                    transform.Rotate(Vector3.up * 120f * (1f + variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

				case '/':
					transform.Rotate(Vector3.down * 120f * (1f + variance / 100f * randomRotations[k % randomRotations.Length]));
                    break;

				//Now, for the spikey bracket we are saving the current position and placing it at
				//the top of our Stack. The best way to explain this process is the following:
				//If we are trying to draw a tree on paper, which has multiple end points, as it has different branches and such
				//We will need to lift the pen off the paper every now and then. So what we are doing is we are saving
				//the position were we want to draw a branch at a later point. And then once we finished the current branch we were 
				//working on we are going back to that saved position to add our new branch there.
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

			//Calculating the bounds to make sure that our newly generated tree can be displayed
            boundsMinMaxX.x = Mathf.Min(transform.position.x, boundsMinMaxX.x);
			boundsMinMaxX.y = Mathf.Max(transform.position.x, boundsMinMaxX.y);
			boundsMinMaxY.x = Mathf.Min(transform.position.y, boundsMinMaxY.x);
			boundsMinMaxY.y = Mathf.Max(transform.position.y, boundsMinMaxY.y);
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
            for (int i = 0; i < currentlyAppliedRules.rules.Count; i++)
			{
                rules.Add(currentlyAppliedRules.rules[i].Name, currentlyAppliedRules.rules[i].addition);
            }
        }
	}

	void OnDrawGizmos() {
        Gizmos.DrawWireCube(new Vector3(boundsMinMaxX.x + boundsMinMaxX.y, boundsMinMaxY.x + boundsMinMaxY.y) * .5f, new Vector3(boundsMinMaxX.y - boundsMinMaxX.x, boundsMinMaxY.y - boundsMinMaxY.x));
    }
}