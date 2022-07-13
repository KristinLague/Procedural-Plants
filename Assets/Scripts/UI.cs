using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour 
{
    public Dropdown rulesDropDown;
    public Slider iterationSlider;
    public Text currentIterations;
    public Slider angle;
    public Text angleText;
    public Slider widthSlider;
    public Text widthText;
    public Slider minlengthSlider;
    public Text minlengthText;
    public Slider maxlengthSlider;
    public Text maxlengthText;
    public Slider varianceSlider;
    public Text varianceText;
	public Slider minllengthSlider;
    public Text minllengthText;
    public Slider maxllengthSlider;
    public Text maxllengthText;
	


    private void Start()
	{
        SetUpIterationSlider();
        AddRulesToDropDown();
        SetUpValues();
    }

	private void SetUpIterationSlider()
	{
        iterationSlider.minValue = 1;
        iterationSlider.maxValue = 7;
        iterationSlider.value = LSystem.Instance.iterations;
		currentIterations.text = LSystem.Instance.iterations.ToString();
        iterationSlider.onValueChanged.AddListener(delegate 
		{
            IterationsChanged(iterationSlider.value);
        });
    }

	private void AddRulesToDropDown()
	{
        rulesDropDown.ClearOptions();
        List<string> rulesOptions = new List<string>();
        for (int i = 0; i < LSystem.Instance.personalizedRules.Count; i++)
		{
            rulesOptions.Add(LSystem.Instance.personalizedRules[i].name);
        }
        rulesDropDown.AddOptions(rulesOptions);
		rulesDropDown.value = LSystem.Instance.personalizedRules.IndexOf(LSystem.Instance.currentlyAppliedRules);
        rulesDropDown.onValueChanged.AddListener(delegate
		{
            RulesChanged(rulesDropDown.value);
        });
    }

	private void SetUpValues()
	{
        angle.minValue = 0f;
        angle.maxValue = 360f;
        angle.value = LSystem.Instance.angle;
        angleText.text = LSystem.Instance.angle.ToString() + "°";
        angle.onValueChanged.AddListener(delegate
        {
            AngleChange(angle.value);
        });

        widthSlider.minValue = 0.1f;
        widthSlider.maxValue = 10f;
        widthSlider.value = LSystem.Instance.width;
        widthText.text = LSystem.Instance.width.ToString();
        widthSlider.onValueChanged.AddListener(delegate
        {
            WidthChange(widthSlider.value);
        });

        minlengthSlider.minValue = 0.1f;
        minlengthSlider.maxValue = 25f;
        minlengthSlider.value = LSystem.Instance.minlength;
        minlengthText.text = LSystem.Instance.minlength.ToString();
        minlengthSlider.onValueChanged.AddListener(delegate
        {
            ChangeMinLength(minlengthSlider.value);
        });

		maxlengthSlider.minValue = 0.1f;
        maxlengthSlider.maxValue = 25f;
        maxlengthSlider.value = LSystem.Instance.maxLength;
        maxlengthText.text = LSystem.Instance.maxLength.ToString();
        maxlengthSlider.onValueChanged.AddListener(delegate
        {
            ChangeMaxLength(maxlengthSlider.value);
        });

		minllengthSlider.minValue = 0.1f;
        minllengthSlider.maxValue = 25f;
        minllengthSlider.value = LSystem.Instance.minLeafLength;
        minllengthText.text = LSystem.Instance.minLeafLength.ToString();
        minllengthSlider.onValueChanged.AddListener(delegate
        {
            ChangeMinLlength(minllengthSlider.value);
        });

		maxllengthSlider.minValue = 0.1f;
        maxllengthSlider.maxValue = 25f;
        maxllengthSlider.value = LSystem.Instance.maxLeafLength;
        maxllengthText.text = LSystem.Instance.maxLeafLength.ToString();
        maxllengthSlider.onValueChanged.AddListener(delegate
        {
            ChangeMaxLlength(maxllengthSlider.value);
        });

        varianceSlider.minValue = 0.1f;
        varianceSlider.maxValue = 100f;
        varianceSlider.value = LSystem.Instance.variance;
        varianceText.text = LSystem.Instance.variance.ToString() + "%";
        varianceSlider.onValueChanged.AddListener(delegate
        {
            ChangeVariance(varianceSlider.value);
        });
    }

	private void RulesChanged(int value)
	{
        LSystem.Instance.SwitchRules(LSystem.Instance.personalizedRules[value]);
    }

	private void IterationsChanged(float value)
	{
        LSystem.Instance.iterations = Mathf.RoundToInt(value);
        currentIterations.text = LSystem.Instance.iterations.ToString();
        LSystem.Instance.Generate();
    }

	private void AngleChange(float value)
	{
        LSystem.Instance.angle = value;
		angleText.text = LSystem.Instance.angle.ToString() + "°";
        LSystem.Instance.Generate();
    }

	private void WidthChange(float value)
	{
        LSystem.Instance.width = value;
        widthText.text = LSystem.Instance.width.ToString();
        LSystem.Instance.Generate();
    }

	private void ChangeMinLength(float value)
	{
		if(value > maxlengthSlider.value)
		{
            minlengthSlider.value = maxlengthSlider.value - 0.1f;
            value = minlengthSlider.value;
        }
        LSystem.Instance.minlength = value;
        minlengthText.text = LSystem.Instance.minlength.ToString();
        LSystem.Instance.Generate();
    }

	private void ChangeMaxLength(float value)
	{
		if(value < minlengthSlider.value)
		{
            maxlengthSlider.value = minlengthSlider.value + 0.1f;
            value = maxlengthSlider.value;
        }
        LSystem.Instance.maxLength = value;
        minlengthText.text = LSystem.Instance.maxLength.ToString();
        LSystem.Instance.Generate();
    }

	private void ChangeMinLlength(float value)
	{
		if(value > maxllengthSlider.value)
		{
            minllengthSlider.value = maxllengthSlider.value - 0.1f;
            value = minllengthSlider.value;
        }
        LSystem.Instance.minLeafLength = value;
        minllengthText.text = LSystem.Instance.minLeafLength.ToString();
        LSystem.Instance.Generate();
    }

	private void ChangeMaxLlength(float value)
	{
		if(value < minllengthSlider.value)
		{
            maxllengthSlider.value = minllengthSlider.value + 0.1f;
            value = maxllengthSlider.value;
        }
        LSystem.Instance.maxLeafLength = value;
        minllengthText.text = LSystem.Instance.maxLeafLength.ToString();
        LSystem.Instance.Generate();
    }

	private void ChangeVariance(float value)
	{
        LSystem.Instance.variance = value;
		varianceText.text = LSystem.Instance.variance.ToString() + "%";
        LSystem.Instance.Generate();
    }
}
