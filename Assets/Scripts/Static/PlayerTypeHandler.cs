using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PlayerTypeHandler {

	public static bool isInitialized = false;
	static Dictionary<PlayerType,string> flags = new Dictionary<PlayerType,string>();

	static Dictionary<string, PlayerType> specialUsers = new Dictionary<string, PlayerType>();

	/* FLAGS
	 * k = kick
	 */

	public static void Init() {
		if (isInitialized) return;
		flags.Add(PlayerType.Anon,"");
		flags.Add(PlayerType.Normal,"");
		flags.Add(PlayerType.Donator,"");
		flags.Add(PlayerType.Moderator,"k");
		flags.Add(PlayerType.Developer,"k");
		isInitialized = true;
	}

	public static void SetSpecialUsers(Dictionary<string, PlayerType> list) {
		specialUsers = list;
	}

	public static bool HasFlag(PlayerType type, char flag) {
		return flags[type].Contains(flag.ToString());
	}

	public static PlayerType GetPlayerType (string username) {
		PlayerType result = PlayerType.Normal;

		if (specialUsers.TryGetValue(username,out result)) {
			return result;
		}

		return PlayerType.Normal;
	}

	public static Color GetPlayerColor(PlayerType type) {
		switch(type) {
		case PlayerType.Anon:
			return new Color(0.88f,0.88f,0.88f);
		case PlayerType.Normal:
			return Color.white;
		case PlayerType.Developer:
			return new Color(0.6f,0.7f,1);
		case PlayerType.Moderator:
			return new Color(0.2f,0.8f,0.2f);
		case PlayerType.Donator:
			return new Color(1,0.8f,0.4f);
		default:
			return Color.white;
		}
	}
}

public enum PlayerType {
	Anon		= 0,
	Normal		= 1,
	Donator		= 2,
	Moderator	= 3,
	Developer	= 4,
}
