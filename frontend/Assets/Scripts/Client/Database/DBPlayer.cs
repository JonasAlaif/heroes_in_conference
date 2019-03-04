using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all information about locally stored Player
/// </summary>
[Serializable]
public class DBPlayer {
    public readonly long PlayerID;
    public readonly string PlayerName;

    private FilePath fp;

    public DBPlayer(long playerID, string playerName) {
        PlayerID = playerID;
        PlayerName = playerName;
    }

    public FilePath FP { get => fp; set => fp = value; }
}
