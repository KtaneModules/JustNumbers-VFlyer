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
	bool hasUnicorn = false;

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
		if(rnd.value <= 0.01f)
		{
			hasUnicorn = true;
			for(int i = 0; i < 10; i++)
			{
				buttonTexts[i].color = new Color32(255, 0, 0, 255);
			}
			answer = unicornAnswer;
			Debug.LogFormat("[Just Numbers #{0}] You got a unicorn, submit 000.", moduleId);
			return;
		}
		else
        {
			Debug.LogFormat("[Just Numbers #{0}] The module will only calculate as soon as the 3rd digit is typed. However a solution can be generated at 0 strikes.", moduleId);
			int indicators = bomb.GetIndicators().Count();
			int batteryCount = bomb.GetBatteryCount();
			int temp1 = indicators + batteryCount;
			int digit1 = DR(temp1);
			Debug.LogFormat("[Just Numbers #{0}] The first number should be {1}.", moduleId, temp1);
			Debug.LogFormat("[Just Numbers #{0}] The first digit should be {1}.", moduleId, digit1);
			// Second Digit
			int batteryHolderCount = bomb.GetBatteryHolderCount();
			int FirstDigit = bomb.GetSerialNumberNumbers().First();
			int temp2 = batteryHolderCount + bomb.GetStrikes() + FirstDigit;
			int digit2 = DR(temp2);
			Debug.LogFormat("[Just Numbers #{0}] The second number should be {1}.", moduleId, temp2);
			Debug.LogFormat("[Just Numbers #{0}] The second digit should be {1}.", moduleId, digit2);
			// Third Digit
			int temp3 = digit1 + digit2;
			int digit3 = DR(temp3);
			Debug.LogFormat("[Just Numbers #{0}] The third number should be {1}.", moduleId, temp3);
			Debug.LogFormat("[Just Numbers #{0}] The third digit should be {1}.", moduleId, digit3);
			Debug.LogFormat("[Just Numbers #{0}] The answer should be {1} at {2} strike(s).", moduleId, new int[] { digit1, digit2, digit3 }.Join(""), bomb.GetStrikes());
		}
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
			if (!hasUnicorn)
			{
				Debug.LogFormat("[Just Numbers #{0}] Recalculating the solution at {1} strike(s).", moduleId, bomb.GetStrikes());
				GetAnswer();
			}
			if(UserInput == answer)
			{
				for(int i = 0; i < 10; i++)
				{
					buttonTexts[i].color = new Color32(0, 255, 0, 255);
				}
				Debug.LogFormat("[Just Numbers #{0}] Sequence correct. Module solved.", moduleId);
				moduleSolved = true;
				Audio.PlaySoundAtTransform("solve", Module.transform);
				Module.HandlePass();
			}
			else
			{
				PressCount = 0;
				Debug.LogFormat("[Just Numbers #{0}] You submitted \"{1}\" which is not correct. Strike!", moduleId, UserInput);
				UserInput = "";
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
		answer = "";
		// First Digit
		int indicators = bomb.GetIndicators().Count();
		int batteryCount = bomb.GetBatteryCount();
		int temp1 = indicators + batteryCount;
		int digit1 = DR(temp1);
		answer += digit1.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The first number should be {1}.", moduleId, temp1);
		Debug.LogFormat("[Just Numbers #{0}] The first digit should be {1}.", moduleId, digit1);
		// Second Digit
		int batteryHolderCount = bomb.GetBatteryHolderCount();
		int FirstDigit = bomb.GetSerialNumberNumbers().First();
		int temp2 = batteryHolderCount + bomb.GetStrikes() + FirstDigit;
		int digit2 = DR(temp2);
		answer += digit2.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The second number should be {1}.", moduleId, temp2);
		Debug.LogFormat("[Just Numbers #{0}] The second digit should be {1}.", moduleId, digit2);
		// Third Digit
		int temp3 = digit1 + digit2;
		int digit3 = DR(temp3);
		answer += digit3.ToString();
		Debug.LogFormat("[Just Numbers #{0}] The third number should be {1}.", moduleId, temp3);
		Debug.LogFormat("[Just Numbers #{0}] The third digit should be {1}.", moduleId, digit3);
		Debug.LogFormat("[Just Numbers #{0}] The answer should be {1} at {2} strike(s).", moduleId, answer, bomb.GetStrikes());
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "Submit a 3 digit number on the module with \"!{0} submit ###\"";
    #pragma warning restore 414
	
	string[] Numerals = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length != 2)
			{
				yield return "sendtochaterror Invalid parameter length.";
				yield break;
			}
			
			else if (parameters.Length == 2)
			{
				foreach (char c in parameters[1])
				{
					if (!c.ToString().EqualsAny(Numerals))
					{
						yield return "sendtochaterror The number contains an invalid character.";
						yield break;
					}
				}
				
				if (parameters[1].Length != 3)
				{
					yield return "sendtochaterror The number is longer/shorter than 3 characters.";
					yield break;
				}
				
				foreach (char d in parameters[1])
				{
					buttons[int.Parse(d.ToString())].OnInteract();
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
	}
}
