using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class NetworkDatabase : MonoBehaviour {
    const string saveFileName = "localDb.save";

    private static NetworkDatabase ndb;
    public static NetworkDatabase NDB { get => ndb; private set => ndb = value; }
    private Database localDb;
    private Client client;

    void Awake() {
        if (NDB != null) {
            Debug.LogError("NetworkDatabase should be a singleton, deleting myself!");
            Destroy(this);
            return;
        }

        NDB = this;

        /*
        if (PlayerPrefs.HasKey("DB_File_Path")) {
            localDb = Database.LoadDatabase(PlayerPrefs.GetString("DB_File_Path"));
        } else {
            localDb = new Database();
        }
        */
        string loadFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        localDb = Database.LoadDatabase(loadFilePath);

        client = new Client(@"DebugFiles");
        Debug.Log("Database will be saved at " + Application.persistentDataPath);
        //client = new Client(Application.persistentDataPath + "\test.jpg");
        // TODO: Authenticate with client

        //localDb.SetAllMaps(client.GetMaps());
    }

    /// <summary>
    /// Called localy to tell the server that an achievement was won, also updates local db
    /// </summary>
    /// <param name="achievementID">ID of the achievement</param>
    /// <param name="wonAchievement">If I won it. Redundant since achievements cannot be unachieved once on the server</param>
    public void SetAchievement(long achievementID, bool wonAchievement = true) {
        localDb.SetAchievement(achievementID, wonAchievement);
        if(wonAchievement)
            client.CompleteAchievement(achievementID);
    }

    /// <summary>
    /// Sets if the user is interested in a given event, updates both local database and also contacts the server
    /// </summary>
    /// <param name="eventID">ID of the event</param>
    /// <param name="isInterested">What the users interest should be updated to (default true = am interested)</param>
    public void SetInterest(long eventID, bool isInterested = true) {
        localDb.SetInterest(eventID, isInterested);
        client.SetInterest(eventID, isInterested);
    }

    /// <summary>
    /// Adds the specified amount of items to the local database. If the item doens exist yet, just create a new one with count amount
    /// Items dont get synced with the server!
    /// </summary>
    /// <param name="itemID">Items unique id</param>
    /// <param name="count">The amount to add, default to 1</param>
    public void AddItem(long itemID, int count = 1) {
        localDb.AddItem(itemID, count);
    }

    /// <summary>
    /// Gets the number of items in the local database
    /// </summary>
    /// <param name="itemID">Items unique id</param>
    /// <returns>Number of items in the db, 0 if it doent exist</returns>
    public int GetNumberOfItem(long itemID) {
        return localDb.GetNumberOfItem(itemID);
    }

    /// <summary>
    /// Gets list of events from the local database (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of events from local db</returns>
    public List<DBEvent> GetCalendar() {
        return localDb.GetCalendar();
    }

    /// <summary>
    /// Gets list of maps from the local database (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of maps from local db</returns>
    public List<DBMap> GetMaps() {
        return localDb.GetMaps();
    }

    /// <summary>
    /// Contact the server to try and download a map, returns true if map is already local
    /// </summary>
    /// <param name="map">The Map object we want to download - has the remote path stored within the obj</param>
    /// <returns>True if the map is saved localy, false if still on remote server (ie failed)</returns>
    public bool TryDownloadMap(DBMap map) {
        return client.TryDownloadMap(map);
    }

    private void OnApplicationQuit() {
        string saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        localDb.SaveDatabase(saveFilePath);
        // TODO: catch errors here!
    }
}