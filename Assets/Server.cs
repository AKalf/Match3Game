using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server
{

    private static Server serverInstance = null;

    List<GameClient> clients = new List<GameClient>();

    private Server() { }

    public static Server GetServerInstance()
    {
        if (serverInstance == null)
        {
            serverInstance = new Server();
        }
        return serverInstance;
    }

    public void AcceptClient()
    {
        clients.Add(new GameClient(BoardManager.GetNewBoardManager()));
    }

    public void SendMessageToClientNewAnimationsMessages(List<AnimationMessage> messages, GameClient client)
    {
        // TO-DO: Serialize messages to json/binary and actually send them
    }

    public void SendMessageToClientChangeBalanceTo(float newAmount, GameClient client)
    {
        // TO-D0: Send command to client to change the U.I. with the new amount
        DetailsManager.ChangeBalanceTextTo(newAmount);
    }

    public void SendMessageToClientChangeSwapCost(float newSwapCost, GameClient client)
    {
        // TO-D0: Send command to client to change the U.I. with the new swap cost
        DetailsManager.ChangeSwapCostTo(newSwapCost);
    }

}
public class GameClient
{
    BoardManager gameInstance;
    string IP = "";
    int port = 8000;

    public GameClient(BoardManager gameInstance)
    {
        this.gameInstance = gameInstance;
        gameInstance.SetClient(this);
    }
}