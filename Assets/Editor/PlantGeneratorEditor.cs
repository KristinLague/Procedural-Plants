using System.Collections.Generic;
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
        });
        rulesDD.value = dropdownChoices[0];
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