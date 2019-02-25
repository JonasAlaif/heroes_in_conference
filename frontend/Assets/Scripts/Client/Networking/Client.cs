using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Client {
    const string errorText = "error", okText = "ok";
    const string getMapsApi = "api/maps", getEventsApi = "api/events", getAchievementsApi = "api/achievements", getGroupsApi = "api/groups", getOauthApi = "api/oauth/", 
        getAchieved = "api/{0}/achieved/{1}", getInterested = "api/{0}/interested/{1}", getUninterested = "api/{0}/uninterested/{1}", getQueryUser = "api/users/query/{0}", getProfileUser = "api/users/profile/{0}";

    /// <summary>
    /// The server that the client will be talking to
    /// </summary>
    public string ServerAddress { get => (serverAddress + (serverAddress.EndsWith("/") ? "" : "/")); set => serverAddress = value; }
    private string serverAddress = @"https://b77423f9.ngrok.io/";

    /// <summary>
    /// The session token that we get when authenticated with the server
    /// </summary>
    private string sessionToken;
    /// <summary>
    /// Expecting folder name with no \ or / at the end!</param>
    /// </summary>
    private string downloadPath;

    public Client(string downloadPath) {
        this.downloadPath = downloadPath;
        dynamic tokenGet = GetJsonDynamic(getOauthApi, false);
        if (errorText.CompareTo((string)tokenGet.status) == 0) {

        }
        else if (okText.CompareTo((string)tokenGet.status) == 0) {
#if !UNITY_EDITOR
            Application.OpenURL(@"http://abbaksdkfhiuhejktest.com/" + getOauthApi + "?token=" + (string)tokenGet.payload.name);
#endif
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
    }

    #region Networking
    private string Get(string uri, bool includeSessionToken = true) {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri + (includeSessionToken?"?" + sessionToken:""));
        //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream)) {
            return reader.ReadToEnd();
        }
    }
    private dynamic GetJsonDynamic(string request, bool includeSessionToken = true) {
        string json, uriRequest = ServerAddress + request;
        //json = Get(uriRequest, includeSessionToken);
            // TODO: this is debug, remove later!
            json = File.ReadAllText(@"DebugFiles\TestJson.txt");
        return JsonConvert.DeserializeObject(json);
        //return JObject.Parse(json);
    }

    /// <summary>
    /// Download file from server at remote path and store it locally at the download path, if fails then simply return remote path
    /// </summary>
    /// <param name="remoteFilePath">URL to the file</param>
    /// <param name="downloadFilePath">Path (incl. file name) to local store</param>
    /// <returns>If download succeeded then path of local file, otherwise path to remote file</returns>
    private DBMap.FilePath downloadFile(string remoteFilePath, string downloadFilePath) {
        byte[] imageData;
        // TODO: Certificate validation
        ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) => { return true; };
        using (WebClient client = new WebClient()) {
            try {
                imageData = client.DownloadData(remoteFilePath);
            }
            catch (Exception e) {
                Debug.Log(e);
                return new DBMap.FilePath(true, remoteFilePath);
            }
        }
        string dirPath = Path.GetDirectoryName(downloadFilePath);
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using (BinaryWriter bw = new BinaryWriter(new FileStream(downloadFilePath, FileMode.Create))) {
            bw.Write(imageData);
        }
        return new DBMap.FilePath(false, downloadFilePath);
    }
    #endregion

    #region Events
    /// <summary>
    /// Sends a get request to server to fetch all the events
    /// </summary>    
    /// <returns>List of events we got from the server</returns>
    public List<DBEvent> GetEvents() {
        dynamic jsonResponse = GetJsonDynamic(getEventsApi);

        List<DBEvent> events = new List<DBEvent>();
        // TODO: check exact wording of error message
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {

        }
        else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            DBEvent @event = new DBEvent((long)jsonResponse.payload.id, DateTime.Parse((string)jsonResponse.payload.start), DateTime.Parse((string)jsonResponse.payload.end),
                                         (string)jsonResponse.payload.name, (string)jsonResponse.payload.description);
            events.Add(@event);
        }
        else {
            throw new Exception("Unknow status message recieved");
        }
        return events;
    }

    public bool SetInterest(long eventID, bool isInterested = true) {
        // TODO: get user
        dynamic jsonResponse = GetJsonDynamic(string.Format(isInterested ? getInterested : getUninterested, "usr", eventID));
        //bool success = jsonAck.Success;
        return true;
    }
    #endregion

    #region Maps
    /// <summary>
    /// Sends a get request to server to fetch all the maps and then downloads all the map graphics
    /// </summary>
    /// <returns>List of maps we got from the server</returns>
    public List<DBMap> GetMaps() {
        dynamic jsonResponse = GetJsonDynamic(getMapsApi);

        List<DBMap> maps = new List<DBMap>();
        // TODO: check exact wording of error message
        if(errorText.CompareTo((string)jsonResponse.status) == 0) {

        } else if(okText.CompareTo((string)jsonResponse.status) == 0) {
            DBMap map = new DBMap((long)jsonResponse.payload.id, (string)jsonResponse.payload.name, DateTime.Now.AddDays(1));
            map.FP = new DBMap.FilePath(true, (string)jsonResponse.payload.image);
            TryDownloadMap(map);
            maps.Add(map);
        } else {
            throw new Exception("Unknow status message recieved");
        }
        return maps;
    }

    public bool TryDownloadMap(DBMap map) {
        if (!map.FP.IsRemote) return true;
        DBMap.FilePath fp = downloadFile(map.FP.Path, downloadPath + @"\" + map.MapID + "." + map.FP.Path.Split('.').Last());
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
    /// <returns>List of maps we got from the server</returns>
    public List<DBAchievement> GetAchievements() {
        dynamic jsonResponse = GetJsonDynamic(getAchievementsApi);

        List<DBAchievement> achievements = new List<DBAchievement>();
        // TODO: check exact wording of error message
        if (errorText.CompareTo((string)jsonResponse.status) == 0) {

        } else if (okText.CompareTo((string)jsonResponse.status) == 0) {
            DBAchievement achievement = new DBAchievement((long)jsonResponse.payload.id, (string)jsonResponse.payload.name, (string)jsonResponse.payload.description, (bool)jsonResponse.payload.won);
            achievements.Add(achievement);
        } else {
            throw new Exception("Unknow status message recieved");
        }
        return achievements;
    }

    public bool CompleteAchievement(long achievementID) {
        // TODO: get user
        dynamic jsonResponse = GetJsonDynamic(string.Format(getAchieved, "usr", achievementID));

        //bool success = jsonAck.Success;
        return true;
    }
    #endregion

    #region Users
    public bool QueryUser(long userID) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getQueryUser, "usr"));

        // TODO: Parse response
        return true;
    }

    public DBPlayer GetUserProfile(long userID) {
        dynamic jsonResponse = GetJsonDynamic(string.Format(getProfileUser, "usr"));

        // TODO: Parse response
        return null;
    }
    #endregion
}
