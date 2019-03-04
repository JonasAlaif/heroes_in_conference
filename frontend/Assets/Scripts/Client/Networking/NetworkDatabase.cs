using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class NetworkDatabase : MonoBehaviour {
    private string sessionTokenSavePath, localDbSavePath;

    private static NetworkDatabase ndb;
    public static NetworkDatabase NDB { get => ndb; private set => ndb = value; }
    private Database localDb;
    private Client client;

    private void Awake() {
        if (NDB != null) {
            // Called when scene is reloaded
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        NDB = this;


        Debug.Log("Delete all files in this folder to reset save state: " + Application.persistentDataPath);
        localDbSavePath = Path.Combine(Application.persistentDataPath, "localDb.save");
        sessionTokenSavePath = Path.Combine(Application.persistentDataPath, "session.token");

        localDb = Database.LoadDatabase(localDbSavePath);

        client = new Client(Application.persistentDataPath, () => LoginScript.logout = true);

        OnLoggedIn += DownloadAllAchievements;

        if (client.LoadSessionToken(sessionTokenSavePath))
            StartCoroutine("loginWait");

        cgDownloader = new Thread(delegate () {
            DownloadAllContentGroups();
        });
        cgDownloader.Start();
    }
    Thread cgDownloader;

    private string serverLoc = null;
    private void Update() {
        if(serverLoc != null) {
            Debug.Log("Opening browser at " + serverLoc);
            Application.OpenURL(serverLoc);
            serverLoc = null;
        }
    }

    private IEnumerator loginWait() {
        yield return new WaitForSeconds(0.2f);
        TryLogin();
    }

    public delegate bool TodoOnSuccessLogin();

    public TodoOnSuccessLogin OnLoggedIn = null;
    public bool TryLogin() {
        //Debug.Log("Trying login!");
        string facebookLoginAddr;
        if (client.TryLogin(out facebookLoginAddr)) {
            if (facebookLoginAddr != null) {
                serverLoc = facebookLoginAddr;
            }
            new Thread(delegate () {
                waitForFBsignin();
            }).Start();
            return true;
        }
        Debug.LogError("Failed login!");
        return false;
    }
    
    private void waitForFBsignin() {
        int i = 0;
        while (!client.checkAuthenticated()) {
            Thread.Sleep(500);
            if(i > 50) {
                return;
            }
            i++;
        }
        if (OnLoggedIn != null) {
            LoadingPanel.LP.StartLoading();
            foreach (TodoOnSuccessLogin task in OnLoggedIn.GetInvocationList()) {
                if (task()) {
                    OnLoggedIn -= task;
                }
                LoadingPanel.LP.AddLoadingDot();
            }
            LoadingPanel.LP.FinishLoading();
        }
        client.SignedIn = true;
    }

    public bool IsLoggedIn() {
        if (client == null)
            return false;
        return client.SignedIn;
    }

    public void Logout() {
        client.Logout();
    }


    /// <summary>
    /// Called localy to tell the server that an achievement was won, also updates local db
    /// </summary>
    /// <param name="achievementID">ID of the achievement</param>
    /// <param name="wonAchievement">If I won it. Redundant since achievements cannot be unachieved once on the server</param>
    public void SetAchievement(long achievementID, bool wonAchievement = true) {
        localDb.SetAchievement(achievementID, wonAchievement);
        if (wonAchievement) {
            TodoOnSuccessLogin trySendSetAchievement = () => client.CompleteAchievement(achievementID);
            if (!trySendSetAchievement())
                OnLoggedIn += new TodoOnSuccessLogin(trySendSetAchievement);
        }
    }

    /// <summary>
    /// Sets if the user is interested in a given event, updates both local database and also contacts the server
    /// </summary>
    /// <param name="eventID">ID of the event</param>
    /// <param name="isInterested">What the users interest should be updated to (default true = am interested)</param>
    public void SetInterest(long eventID, bool isInterested = true) {
        localDb.SetInterest(eventID, isInterested);
        TodoOnSuccessLogin trySendSetInterest = () => client.SetInterest(eventID, isInterested);
        if (!trySendSetInterest())
            OnLoggedIn += new TodoOnSuccessLogin(trySendSetInterest);
    }
    public bool ToggleInterest(long eventID) {
        bool newInterest = localDb.ToggleInterest(eventID);
        TodoOnSuccessLogin trySendSetInterest = () => client.SetInterest(eventID, newInterest);
        if (!trySendSetInterest())
            OnLoggedIn += new TodoOnSuccessLogin(trySendSetInterest);
        return newInterest;
    }

    #region Items
    /// <summary>
    /// Adds the specified amount of items to the local database. If the item doens exist yet, just create a new one with count amount
    /// Items dont get synced with the server!
    /// </summary>
    /// <param name="itemID">Items unique id</param>
    /// <param name="count">The amount to add, default to 1</param>
    public void AddItem(long itemID, int count = 1)
    {
        localDb.AddItem(itemID, count);
    }

    /// <summary>
    /// Gets the number of items in the local database
    /// </summary>
    /// <param name="itemID">Items unique id</param>
    /// <returns>Number of items in the db, 0 if it doent exist</returns>
    public int GetNumberOfItem(long itemID)
    {
        return localDb.GetNumberOfItem(itemID);
    }
    #endregion

    #region Get from local db
    /// <summary>
    /// Gets list of events from the local database (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of events from local db</returns>
    public List<DBEvent> GetCalendar() {
        List<DBEvent> calendar = localDb.GetCalendar();
        return calendar;
    }

    /// <summary>
    /// Gets list of maps from the local database (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of maps from local db</returns>
    public List<DBMap> GetMaps() {
        List<DBMap> maps = localDb.GetMaps();
        return maps;
    }

    /// <summary>
    /// Gets list of achievements from the local database (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of maps from local db</returns>
    public Dictionary<long, DBAchievement> GetAchievements() {
        Dictionary<long, DBAchievement> achievements = localDb.GetAllAchievements();
        return achievements;
    }

    /// <summary>
    /// Gets list of all won achievements (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>List of all won achievements</returns>
    public List<DBAchievement> GetAllWonAchievements() {
        return localDb.GetAllWonAchievements();
    }

    /// <summary>
    /// Gets and achievement from local db by name (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>The achievement ID</returns>
    public long GetAchievementIdByName(string name) {
        return localDb.GetAchievementIdByName(name);
    }

    /// <summary>
    /// Gets and achievement from local db by name (DOESNT CONTACT THE SERVER)
    /// </summary>
    /// <returns>The achievement Object</returns>
    public DBAchievement GetAchievementObjByName(string name) {
        return localDb.GetAchievementObjByName(name);
    }

    public bool GetContentGroupActiveByName(string name) {
        return localDb.GetContentGroupActiveByName(name);
    }
    #endregion


    #region Contact server to get info
    public UnityEvent EventsDownloaded;
    public bool DownloadAllEvents() {
        List<DBEvent> allEvents = client.GetEvents();
        if (allEvents == null) {
            OnLoggedIn += new TodoOnSuccessLogin(() => DownloadAllEvents());
            CreateNoNetworkNotification(() => TryLogin());
            return false;
        }
        else {
            localDb.SetAllEvents(allEvents);
            List<long> interested = client.GetEventsInterested();
            if(interested != null)
                foreach (long intEve in interested) {
                    localDb.SetInterest(intEve);
                    //Debug.Log("Interested in eve " + intEve);
                }
            EventsDownloaded.Invoke();
            return true;
        }
    }

    public UnityEvent MapsDownloaded;
    public bool DownloadAllMaps() {
        List<DBMap> allMaps = client.GetMaps();
        if (allMaps == null) {
            OnLoggedIn += new TodoOnSuccessLogin(() => DownloadAllMaps());
            CreateNoNetworkNotification(() => TryLogin());
            return false;
        }
        else {
            localDb.SetAllMaps(allMaps);
            MapsDownloaded.Invoke();
            return true;
        }
    }
    /// <summary>
    /// Contact the server to try and download a map, returns true if map is already local
    /// </summary>
    /// <param name="map">The Map object we want to download - has the remote path stored within the obj</param>
    /// <returns>True if the map is saved localy, false if still on remote server (ie failed)</returns>
    public bool TryDownloadMap(DBMap map) {
        return client.TryDownloadMap(map);
    }

    /// <summary>
    /// Contact the server to try and download the players profile, returns true if map is already local
    /// </summary>
    /// <param name="player">The Player object we want to download for - has the remote path stored within the obj</param>
    /// <returns>True if the pic is saved localy, false if still on remote server (ie failed)</returns>
    public bool TryDownloadPlayerProfile(DBPlayer player) {
        return client.TryDownloadPlayerProfile(player);
    }

    public UnityEvent AchievementsDownloaded;
    private bool DownloadAllAchievements() {
        List<DBAchievement> allAchievements = client.GetAchievements();
        if (allAchievements == null) {
            OnLoggedIn += new TodoOnSuccessLogin(() => DownloadAllAchievements());
            CreateNoNetworkNotification(() => TryLogin());
            return false;
        }
        else {
            localDb.SetAllAchievements(allAchievements);
            AchievementsDownloaded.Invoke();
            return true;
        }
    }

    public void UpdateAllWonAchievementsServer() {
        List<long> gotAchievements = client.GetAllAchievedAchievements();
        if(gotAchievements != null)
            localDb.SetAllWonAchievements(gotAchievements);
    }

    public UnityEvent ContentGroupsDownloaded;
    private void DownloadAllContentGroups() {
        while (true) {
            Thread.Sleep(5000);
            Dictionary<string, bool> allContentGroups = client.GetContentGroups();
            if (allContentGroups != null) {
                localDb.SetAllContentGroups(allContentGroups);
                ContentGroupsDownloaded.Invoke();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="player">Null if not foun</param>
    /// <returns>False if network error</returns>
    public bool SearchForPlayerOnServer(string searchTerm, out long[] ids, out string[] names) {
        if (!client.QueryUsers(searchTerm, out ids, out names))
            return false;
        return true;
    }
    
    public DBMap GetMapPOI(long mapId) {
        DBMap mapToGet = localDb.GetMapById(mapId);
        if (mapToGet == null)
            return null;
        List<DBMapPOI> pois = client.GetMapPOIs(mapId);
        if (pois == null)
            return null;
        mapToGet.SetAllMapPois(pois.OrderByDescending(poi => poi.Y).ToList());
        return mapToGet;
    }
    #endregion

    public DBPlayer GetPlayerById(long pId) {
        DBPlayer player = localDb.TryFindPlayerById(pId);
        if (player != null)
            return player;
        return client.GetUserProfile(pId);
    }


    [Space]
    [SerializeField] RectTransform canvas;
    [SerializeField] Sprite backgroundSprite, buttonBackgroundSprite;
    [SerializeField] private Font textFont;
    private void CreateNoNetworkNotification(Func<bool> ToApplyOnReconnect) {
        NetworkErrorPopup.toBuild = new NetworkErrorPopup.Popup(ToApplyOnReconnect, backgroundSprite, buttonBackgroundSprite, canvas, textFont);
    }

    private void OnApplicationPause(bool pause) {
        if (!pause)
            return;
        //Debug.Log("Saving in OnApplicationPause");
        localDb.SaveDatabase(localDbSavePath);
        client.SaveSessionToken(sessionTokenSavePath);
    }
    private void OnApplicationQuit() {
        cgDownloader.Abort();
        //Debug.Log("Saving in OnApplicationQuit");
        localDb.SaveDatabase(localDbSavePath);
        client.SaveSessionToken(sessionTokenSavePath);
        // TODO: catch errors here!
    }
}