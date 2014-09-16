using UnityEngine;
using System.Collections;

/// <summary>
/// Stores info for when trying to connect to a server.
/// </summary>
public class ConnectionStatus {

	public string message;
	public bool isConnecting = false;

	public ConnectionStatus() {

	}

	public ConnectionStatus(string initialMessage) {
		message = initialMessage;
	}

}
