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
    readonly string unicornAnswer = "000";
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
			Debug.LogFormat("[Just Numbers #{0}] You got a rare condition that has occured, submit 000 to disarm the module instead.", moduleId);
			return;
		}
		else
        {
			Debug.LogFormat("[Just Numbers #{0}] The module will only calculate as soon as the 3rd digit is typed. However a solution can be generated at 0 strikes.", moduleId);
			GetAnswer();
		}
	}

	void ButtonPress(KMSelectable button)
	{
		Audio.PlaySoundAtTransform("press", button.transform);
		button.AddInteractionPunch(0.5f);
		if(moduleSolved)
		{
			return;
		}
		UserInput += button.GetComponentInChildren<TextMesh>().text;
		PressCount++;
		if(PressCount >= 3)
		{
			if (!hasUnicorn)
			{
				var strikeCnt = bomb.GetStrikes();
				if (strikes != strikeCnt)
				{
					strikes = strikeCnt;
					Debug.LogFormat("[Just Numbers #{0}] Recalculating the solution at {1} strike(s).", moduleId, strikes);
					GetAnswer();
				}
			}
			if(UserInput == answer)
			{
				for(int i = 0; i < 10; i++)
				{
					buttonTexts[i].color = new Color32(0, 255, 0, 255);
				}
				Debug.LogFormat("[Just Numbers #{0}] You submitted the correct sequence of numbers. Module solved.", moduleId);
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
		Debug.LogFormat("[Just Numbers #{0}] After adding the number of indicators and batteries, the result should be {1}.", moduleId, temp1);
		Debug.LogFormat("[Just Numbers #{0}] The first digit to input should be {1}.", moduleId, digit1);
		// Second Digit
		int batteryHolderCount = bomb.GetBatteryHolderCount();
		int FirstDigit = bomb.GetSerialNumberNumbers().FirstOrDefault();
		int temp2 = batteryHolderCount + strikes + FirstDigit;
		int digit2 = DR(temp2);
		answer += digit2.ToString();
		Debug.LogFormat("[Just Numbers #{0}] After adding the number of battery holders, strikes, and the first serial number digit, the result should be {1}.", moduleId, temp2);
		Debug.LogFormat("[Just Numbers #{0}] The second digit to input should be {1}.", moduleId, digit2);
		// Third Digit
		int temp3 = digit1 + digit2;
		int digit3 = DR(temp3);
		answer += digit3.ToString();
		Debug.LogFormat("[Just Numbers #{0}] After adding the first and second digits, the result should be {1}.", moduleId, temp3);
		Debug.LogFormat("[Just Numbers #{0}] The third digit should be {1}.", moduleId, digit3);
		Debug.LogFormat("[Just Numbers #{0}] The answer should be {1} at {2} strike(s).", moduleId, answer, strikes);
	}
	
	//twitch plays handling begins here
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = "Submit a 3 digit number on the module with \"!{0} submit ###\"";
    #pragma warning restore 414
	
	//string[] Numerals = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		var cmdSubmit = Regex.Match(command, @"^\s*submit\s+(\d+(\s\d+)*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		//string[] parameters = command.Split(' ');
		if (cmdSubmit.Success)
		{
			var allSelectables = new List<KMSelectable>();
			var possibleDigits = cmdSubmit.Value.Substring(6).Replace(" ","");
			var possibleSelectables = new Dictionary<char, KMSelectable>
			{
				{'0', buttons[0] },
				{'1', buttons[1] },
				{'2', buttons[2] },
				{'3', buttons[3] },
				{'4', buttons[4] },
				{'5', buttons[5] },
				{'6', buttons[6] },
				{'7', buttons[7] },
				{'8', buttons[8] },
				{'9', buttons[9] },
			};
			foreach (char a in possibleDigits)
            {
				if (possibleSelectables.ContainsKey(a))
					allSelectables.Add(possibleSelectables[a]);
				else
                {
					yield return string.Format("sendtochaterror The given character '{0}' is not a digit", a);
					yield break;
				}

            }

			if (allSelectables.Count != 3)
            {
				yield return string.Format("sendtochaterror You provided {0} digit(s) when I expected exactly 3 digits.", allSelectables.Count);
				yield break;
            }
			yield return null;
			foreach (var d in allSelectables)
			{
				d.OnInteract();
				yield return new WaitForSeconds(0.2f);
			}
		}
	}
}
