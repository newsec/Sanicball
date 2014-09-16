using UnityEngine;
using System.Collections;

public class LobbyTitle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (!Network.isServer) {
			NetworkView nw = GameObject.FindObjectOfType<NetworkView>();
			if (nw != null) {
				nw.RPC("RequestLobbyTitle",RPCMode.Server,Network.player);
			} else {
				Debug.LogWarning("Lobby title couldn't find a networkview to use for requesting game name.");
			}
		} else {
			Server serv = GameObject.FindObjectOfType<Server>();
			guiText.text = serv.settings.gameName;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
