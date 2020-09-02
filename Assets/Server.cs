using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour {

    List<GameClient> clients = new List<GameClient>();

    public void AcceptClient() {
        clients.Add(new GameClient(BoardManager.GetNewBoardManager()));
    }

    public static void SendAnimationsToClient(List<AnimationMessage> messages) {
        // TO-DO: Serialize messages to json/binary and actually send them
    }
}
public class GameClient {
    BoardManager gameInstance;
    string IP = "";
    int port = 8000;

    public GameClient(BoardManager gameInstance) {
        this.gameInstance = gameInstance;
    }
}