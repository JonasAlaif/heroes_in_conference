using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.IO;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class Client {
    // TODO: check exact wording of error message
    const string errorText = "error", okText = "ok";
    const string getMapsApi = "api/maps", getEventsApi = "api/events", getAchievementsApi = "api/achievements", getGroupsApi = "api/groups", getOauthApi = "api/oauth/", 
        getAchieved =  "api/user/achieved/{0}?", getAllMyInterested = "api/user/interested?",   getInterested  = "api/user/interested/{0}?", getUninterested = "api/user/uninterested/{0}?",
        getQueryUser = "api/users/query/{0}?",  getProfileUser = "api/users/{0}/profile?", getCurrentUser = "api/user/?", getMapInfo = "api/markers/{0}";
    const string staticServerCheckAddress = "8.8.8.8";

    private string serverAddress = @"https://heroesinconference.tk/";
    /// <summary>
    /// The server that the client will be talking to
    /// </summary>
    public string ServerAddress { get => (serverAddress + (serverAddress.EndsWith("/") ? "" : "/")); set => serverAddress = value; }

    /// <summary>
    /// Expecting folder name with no \ or / at the end!</param>
    /// </summary>
    private string downloadPath;

    public Client(string downloadPath, Action whatToDoOnNetworkError) {
        this.downloadPath = downloadPath;
        this.whatToDoOnNetworkError = whatToDoOnNetworkError;
    }
    public bool LoadSessionToken(string loadPath) {
        if (!File.Exists(loadPath))
            return false;

        using (StreamReader streamReader = new StreamReader(loadPath)) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            SessionToken st;
            try {
                st = (SessionToken)binaryFormatter.Deserialize(streamReader.BaseStream);
                // TODO: catch error if not DB object
            }
            catch (SerializationException ex) {
                throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
            }
            sessionToken = st;
            if (!sessionToken.IsValid())
                return false;
            return true;
        }
    }
    public bool SaveSessionToken(string savePath) {
        if (sessionToken == null)
            return false;
        using (Stream stream = File.Open(savePath, FileMode.Create)) {
            try {
                new BinaryFormatter().Serialize(stream, sessionToken);
            }
            catch (SerializationException ex) {
                throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
            }
        }
        return true;
    }


    #region Login
    /// <summary>
    /// Holds the session token string and information about its validity
    /// </summary>
    [Serializable]
    private class SessionToken {
        private bool isLoggedIn;
        public readonly string Token;
        private DateTime expiryTime;

        public SessionToken(string sessionToken) {
            Token = sessionToken;
            expiryTime = DateTime.Now.AddHours(4);
        }

        public void LoggedIn() {
            isLoggedIn = true;
        }

        public bool IsValid() {
            if (!isLoggedIn)
                return false;
            if (expiryTime == DateTime.MinValue)
                return false;
            if (DateTime.Now > expiryTime)
                return false;
            return true;
        }

        public void Logout() {
            expiryTime = DateTime.MinValue;
        }
    }
    private SessionToken sessionToken;
    public bool SignedIn { get {
            if (sessionToken == null)
                return false;
            return sessionToken.IsValid();
        }
        set {
            if (value && sessionToken != null)
                sessionToken.LoggedIn();
        }
    }

    public bool TryLogin(out string loginAddr) {
        if(sessionToken != null)
            if(sessionToken.IsValid()) {
                loginAddr = null;
                return true;
            }
        if (!checkConnectivity()) {
            loginAddr = null;
            return false;
        }

        dynamic jsonResponse = GetJsonDynamic(getOauthApi);
        if (jsonResponse == null) {
            loginAddr = null;
            return false;
        }

        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            loginAddr = null;
            return false;
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            sessionToken = new SessionToken((string)jsonResponse.payload);
            loginAddr = serverAddress + getOauthApi + "?session=" + sessionToken.Token;
            return true;
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
    }

    public void LoginSucess() {
        sessionToken.LoggedIn();
    }

    private bool checkConnectivity() {
        return true;
        System.Net.NetworkInformation.Ping netMon = new System.Net.NetworkInformation.Ping();
        try {
            PingReply response = netMon.Send(staticServerCheckAddress, 500);
            if (response != null) {
                return response.Status == IPStatus.Success;
            }
        } catch (Exception e) {
            Debug.LogError(e.ToString());
            return false;
        }

        return false;
    }
    
    
    public bool checkAuthenticated() {
        dynamic jsonResponse = GetJsonDynamic(getCurrentUser);
        if (jsonResponse == null)
            return false;

        if (okText.CompareTo((string)jsonResponse.status) == 0)
            return true;
        else {
            return false;
        }
    }
    
    public void Logout() {
        if (sessionToken != null)
            sessionToken.Logout();
    }
    #endregion

    #region Networking
    private string Get(string uri) {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri + (uri.EndsWith("?") ? "session=" + sessionToken.Token : ""));
        //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        Debug.Log(uri + (uri.EndsWith("?") ? "session=" + sessionToken.Token : ""));
        try {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        } catch (Exception e) {
            Debug.LogError(e);
            return null;
        }
    }
    private dynamic GetJsonDynamic(string request) {
        string json, uriRequest = ServerAddress + request;
        json = Get(uriRequest);
        // TODO: this is debug, remove later!
        //json = File.ReadAllText(@"DebugFiles\TestJson.txt");
        Debug.Log(json);
        if (json == null)
            return null;
        else
            return JsonConvert.DeserializeObject(json);
    }

    /// <summary>
    /// Download file from server at remote path and store it locally at the download path, if fails then simply return remote path
    /// </summary>
    /// <param name="remoteFilePath">URL to the file</param>
    /// <param name="downloadFilePath">Path (incl. file name) to local store</param>
    /// <returns>If download succeeded then path of local file, otherwise path to remote file</returns>
    private FilePath downloadFile(string remoteFilePath, string downloadFilePath) {
        if (File.Exists(downloadFilePath))
            return new FilePath(false, downloadFilePath);

        byte[] imageData;
        // TODO: Certificate validation
        ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
        using (WebClient client = new WebClient()) {
            try {
                imageData = client.DownloadData(remoteFilePath);
            }
            catch (Exception e) {
                return new FilePath(true, remoteFilePath);
            }
        }
        string dirPath = Path.GetDirectoryName(downloadFilePath);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using (BinaryWriter bw = new BinaryWriter(new FileStream(downloadFilePath, FileMode.Create))) {
            bw.Write(imageData);
        }
        return new FilePath(false, downloadFilePath);
    }
    #endregion

    #region Events
    /// <summary>
    /// Sends a get request to server to fetch all the events
    /// </summary>    
    /// <returns>List of events we got from the server, might be null if network error!</returns>
    public List<DBEvent> GetEvents() {
        dynamic jsonResponse = GetJsonDynamic(getEventsApi);
        if (jsonResponse == null)
            return null;

        List<DBEvent> events = new List<DBEvent>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " events from server");
            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic eventI = jsonResponse.payload[i];
                long id = (long)eventI.id;
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds((long)eventI.start.seconds);
                DateTime end = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds((long)eventI.end.seconds);
                //Debug.Log("Event " + i + " id is " + id + " start is " + start + " end is " + end);
                DBEvent @event = new DBEvent(id, start, end, (string)eventI.name, (string)eventI.desc);
                events.Add(@event);
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return events;
    }

    public bool SetInterest(long eventID, bool isInterested = true) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(isInterested ? getInterested : getUninterested, eventID));
        // TODO: parse response
        return true;
    }

    public List<long> GetEventsInterested() {
        dynamic jsonResponse = GetJsonDynamic(getCurrentUser);
        if (jsonResponse == null)
            return null;

        List<long> gotEventsInterested = new List<long>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            for (int i = 0; i < jsonResponse.payload.interested.Count; i++) {
                dynamic eveI = jsonResponse.payload.interested[i];
                gotEventsInterested.Add((long)eveI.id);
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return gotEventsInterested;
    }
    #endregion

    #region Maps
    /// <summary>
    /// Sends a get request to server to fetch all the maps and then downloads all the map graphics
    /// </summary>
    /// <returns>List of maps we got from the server, might be null if network error!</returns>
    public List<DBMap> GetMaps() {
        dynamic jsonResponse = GetJsonDynamic(getMapsApi);
        if (jsonResponse == null)
            return null;

        List<DBMap> maps = new List<DBMap>();
        if(errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        } else if(okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " maps from server");
            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic mapI = jsonResponse.payload[i];
                DBMap map = new DBMap((long)mapI.id, (string)mapI.name, DateTime.Now.AddDays(1), serverAddress + (string)mapI.image);
                Debug.Log("Remote fp is " + map.FP.Path);
                TryDownloadMap(map);
                maps.Add(map);
            }
        } else {
            throw new Exception("Unknow status message recieved");
        }
        return maps;
    }

    public List<DBMapPOI> GetMapPOIs(long mapId) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getMapInfo, mapId));
        if (jsonResponse == null)
            return null;

        List<DBMapPOI> POIs = new List<DBMapPOI>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " points of interest from server");
            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic poiI = jsonResponse.payload[i];
                DBMapPOI poi = new DBMapPOI((long)poiI.id, (string)poiI.name, (string)poiI.desc, (int)poiI.x, (int)poiI.y);
                POIs.Add(poi);
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return POIs;
    }

    public bool TryDownloadMap(DBMap map) {
        FilePath mapFP = map.FP;
        if (mapFP == null)
            return false;
        if (!mapFP.IsRemote) return true;
        FilePath fp = downloadFile(map.FP.Path, Path.Combine(downloadPath, map.MapID + "." + map.FP.Path.Split('.').Last()));
        if (!fp.IsRemote) {
            map.FP = fp;
            return true;
        } else
            return false;
    }
    #endregion

    #region Achievements
    /// <summary>
    /// Sends a get request to server to fetch all the maps and then downloads all the map graphics
    /// </summary>
    /// <returns>List of maps we got from the server, might be null if network error!</returns>
    public List<DBAchievement> GetAchievements() {
        dynamic jsonResponse = GetJsonDynamic(getAchievementsApi);
        if (jsonResponse == null)
            return null;

        List<DBAchievement> achievements = new List<DBAchievement>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        } else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " achievements from server");

            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic achI = jsonResponse.payload[i];
                DBAchievement achievement = new DBAchievement((long)achI.id, (string)achI.name, (string)achI.desc, false);
                achievements.Add(achievement);
            }
        } else {
            throw new Exception("Unknow status message recieved");
        }
        return achievements;
    }

    public List<long> GetAllAchievedAchievements() {
        dynamic jsonResponse = GetJsonDynamic(getCurrentUser);
        if (jsonResponse == null)
            return null;

        List<long> gotAchs = new List<long>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            for (int i = 0; i < jsonResponse.payload.achievements.Count; i++) {
                dynamic achI = jsonResponse.payload.achievements[i];
                gotAchs.Add((long)achI.id);
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return gotAchs;
    }

    public bool CompleteAchievement(long achievementID) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getAchieved, achievementID));

        //bool success = jsonAck.Success;
        return true;
    }
    #endregion

    #region Users
    public bool QueryUsers(string query, out long[] ids, out string[] names) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getQueryUser, query));
        ids = null;
        names = null;
        if (jsonResponse == null)
            return false;

        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
            return false;
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " users from server");
            if (jsonResponse.payload.Count == 0)
                return true;

            names = new string[jsonResponse.payload.Count];
            ids = new long[jsonResponse.payload.Count];
            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic userI = jsonResponse.payload[i];
                ids[i] = (long)userI.id;
                names[i] = (string)userI.name;
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return true;
    }

    public DBPlayer GetUserProfile(long userID) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getProfileUser, userID));

        DBPlayer playerGot = null;
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            playerGot = new DBPlayer((long)jsonResponse.payload.id, (string)jsonResponse.payload.name);
            playerGot.FP = new FilePath(true, serverAddress + "profiles/" + playerGot.PlayerID + ".jpeg");
            TryDownloadPlayerProfile(playerGot);
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return playerGot;
    }

    public bool TryDownloadPlayerProfile(DBPlayer player) {
        if (!player.FP.IsRemote) return true;
        FilePath fp = downloadFile(player.FP.Path, Path.Combine(downloadPath, + player.PlayerID + "." + player.FP.Path.Split('.').Last()));
        if (!fp.IsRemote) {
            player.FP = fp;
            return true;
        }
        else
            return false;
    }
    #endregion

    #region Content Groups
    public Dictionary<string, bool> GetContentGroups() {
        dynamic jsonResponse = GetJsonDynamic(getGroupsApi);
        if (jsonResponse == null)
            return null;

        Dictionary<string, bool> cGroups = new Dictionary<string, bool>();
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {
            handleError();
        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            //Debug.Log("Got " + jsonResponse.payload.Count + " content groups from server");
            for (int i = 0; i < jsonResponse.payload.Count; i++) {
                dynamic cgI = jsonResponse.payload[i];
                cGroups[(string)cgI.name] = (bool)cgI.enabled;
            }
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return cGroups;
    }
    #endregion

    Action whatToDoOnNetworkError;
    private void handleError() {
        whatToDoOnNetworkError();
    }
}
