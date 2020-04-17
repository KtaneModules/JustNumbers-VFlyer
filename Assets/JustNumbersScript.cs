using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using rnd = UnityEngine.Random;
using UnityEngine;
using KModkit;

public class JustNumbersScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo bomb;
	public KMBombModule Module;
	public KMSelectable[] buttons;
	public TextMesh[] buttonTexts;
	string unicornAnswer = "000";
	string UserInput = "";
	string answer = "";
	private int PressCount = 0;
	int strikes = 0;
	int tempStrikes = 0;

	// Log stuff
	static int moduleIdCounter = 1;
	int moduleId;
	bool moduleSolved = false;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		foreach (KMSelectable button in buttons)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
		}
	}

	void Start()
	{
		int Unicorn = rnd.Range(0, 100);
		if(Unicorn == 78)
		{
			for(int i = 0; i < 10; i++)
			{
				buttonTexts[i].color = new Color32(255, 0, 0, 255);
			}
			answer = unicornAnswer;
			Debug.LogFormat("[Just Numbers #{0}] You got a unicorn, submit 000.", moduleId);
			return;
		}
		StartCoroutine(StrikesUpdate());
		GetAnswer();
	}

	IEnumerator StrikesUpdate()
	{
		while(!moduleSolved)
		{
			yield return new WaitForSeconds(0.1f);
			strikes = bomb.GetStrikes();
			if(tempStrikes == strikes)
			{
				continue;
			}
			tempStrikes = bomb.GetStrikes();
			GetAnswer();
			yield return null;
		}
		yield return null;
	}

	void ButtonPress(KMSelectable button)
	{
		Audio.PlaySoundAtTransform("press", button.transform);
		button.AddInteractionPunch();
		if(moduleSolved)
		{
			return;
		}
		UserInput += button.GetComponentInChildren<TextMesh>().text;
		PressCount++;
		if(PressCount == 3)
		{
			if(UserInput == answer)
			{
				for(int i = 0; i < 10; i++)
				{
					buttonTexts[i].color = new Color32(0, 255, 0, 255);
				}
				Debug.LogFormat("[Just Numbers #{0}] Sequence correct. Module solved.", moduleId);
				Audio.PlaySoundAtTransform("solve", Module.transform);
				Module.HandlePass();
			}
			else
			{
				answer = "";
				PressCount = 0;
				Debug.LogFormat("[Just Numbers #{0}] Incorrect sequence : {1}, Strike", moduleId, UserInput);
				UserInput = "";
				Audio.PlaySoundAtTransform("strike", Module.transform);
				Module.HandleStrike();
			}
		}
	}

	static int DR(int n)
    {
        int root = 0;
        while (n > 0 || root > 9)
        {
            if (n == 0)
			{
                n = root;
                root = 0;
            }
            root += n % 10;
            n /= 10; 
        }
        return root;
    }

	void GetAnswer()
	{
		// First Digit
		int indicators = bomb.GetIndicators().Count();
		int batteryCount = bomb.GetBatteryCount();
		int temp1 = indicators + batteryCount;
		int digit1 = DR(temp1);
		answer += digit1.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The first number is {1}.", moduleId, temp1);
		Debug.LogFormat("[Just Numbers #{0}] The first digit is {1}.", moduleId, digit1);
		// Second Digit
		int batteryHolderCount = bomb.GetBatteryHolderCount();
		int FirstDigit = bomb.GetSerialNumberNumbers().First();
		int temp2 = batteryHolderCount + strikes + FirstDigit;
		int digit2 = DR(temp2);
		answer += digit2.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The second number is {1}.", moduleId, temp2);
		Debug.LogFormat("[Just Numbers #{0}] The second digit is {1}.", moduleId, digit2);
		// Third Digit
		int temp3 = digit1 + digit2;
		int digit3 = DR(temp3);
		answer += digit3.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The third number is {1}.", moduleId, temp3);
		Debug.LogFormat("[Just Numbers #{0}] The third digit is {1}.", moduleId, digit3);
		Debug.LogFormat("[Just Numbers #{0}] The answer is {1}.", moduleId, answer);
	}
}
