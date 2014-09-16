using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

[System.Serializable]
public class RaceSettings {
	public int laps = 2;
	public int stage = 0;
	public bool allowSuperSanic = true;
	public int aiBallCount = 7;
	public int aiStupidness = 30;
	public List<int> aiCharacters = new List<int>(new int[] {1,2,3,4,5,6,7,8});

	public RaceSettings() {//New with default settings
	}

	public RaceSettings(RaceSettings original) {//Copy from other settings
		laps = original.laps;
		stage = original.stage;
		allowSuperSanic = original.allowSuperSanic;
		aiBallCount = original.aiBallCount;
		aiStupidness = original.aiStupidness;
		aiCharacters = new List<int>(original.aiCharacters);
	}

	/// <summary>
	/// Returns a string that represents the race settings.
	/// </summary>
	/// <returns>A string that represents the current object.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString ()
	{
		return laps + ";"
				+ stage + ";"
				+ allowSuperSanic + ";"
				+ aiBallCount + ";"
				+ aiStupidness + ";"
				+ GetAICharacterString();
	}

	/// <summary>
	/// Parses from string returned by GetString.
	/// </summary>
	/// <returns>The from string.</returns>
	/// <param name="str">String.</param>
	public static RaceSettings ParseFromString(string str) {
		try {
			RaceSettings output = new RaceSettings();
			string[] args = str.Split(';');

			output.laps = int.Parse(args[0]);
			output.stage = int.Parse(args[1]);
			output.allowSuperSanic = bool.Parse(args[2]);
			output.aiBallCount = int.Parse(args[3]);
			output.aiStupidness = int.Parse(args[4]);
			output.aiCharacters = ParseAICharactersFromString(args[5]);

			return output;
		} catch (System.Exception ex) {
			Debug.LogError("Failed to parse race settings: " + ex);
		}
		return null;
	}
	
	string GetAICharacterString() {
		if (aiBallCount == 0) return "";
		StringBuilder output = new StringBuilder();
		for (int i=0;i<aiBallCount;i++) {
			if (i < aiCharacters.Count)
				output.Append(aiCharacters[i] + ",");
			else
				output.Append("1,");
		}
		output.Length = output.Length-1;//Remove last comma
		return output.ToString();
	}

	static List<int> ParseAICharactersFromString(string s) {
		List<int> output = new List<int>();
		if (s == "") return output;
		string[] chars = s.Split(',');
		for (int i=0;i<chars.Length;i++) {
			output.Add(int.Parse(chars[i]));
		}
		return output;
	}

	public override bool Equals (object other)//Checks racesettings against each other
	{
		RaceSettings rs = other as RaceSettings;//Cast to racesettings

		if (rs != null) {
			if (laps == rs.laps
			    && stage == rs.stage
			    && allowSuperSanic == rs.allowSuperSanic
			    && aiBallCount == rs.aiBallCount
			    && aiStupidness == rs.aiStupidness
			    && aiCharacters.SequenceEqual<int>(rs.aiCharacters)) {
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode ()
	{
		return base.GetHashCode ();//Don't want to mess with this right now
	}
}