using System;
using System.Linq;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class Database {
    private long uid = 1;

    public Database() {
        init();
    }
    private void init() {
        if(maps == null)
            maps = new Dictionary<long, DBMap>();
        if (events == null)
            events = new Dictionary<long, DBEvent>();
        if (achievements == null)
            achievements = new Dictionary<long, DBAchievement>();

        if (wonAchievements == null)
            wonAchievements = new HashSet<long>();
        if (contentGroups == null)
            contentGroups = new Dictionary<string, bool>();
        if (players == null)
            players = new Dictionary<long, DBPlayer>();
    }
    public static Database LoadDatabase(string path) {
        if (!File.Exists(path))
            return new Database();

        using (StreamReader streamReader = new StreamReader(path)) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Database db;
            try {
                db = (Database)binaryFormatter.Deserialize(streamReader.BaseStream);
                // TODO: catch error if not DB object
            } catch (SerializationException ex) { 
                throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
            }

            // Need to init to deal with loading older versions of the database that may not have all lists/hashsets etc initialised
            db.init();
            return db;
        }
    }
    public bool SaveDatabase(string path) {
        using (Stream stream = File.Open(path, FileMode.Create)) {
            try {
                new BinaryFormatter().Serialize(stream, this);
            } catch (SerializationException ex) {
                throw new SerializationException(((object)ex).ToString() + "\n" + ex.Source);
            }
        }
        return true;
    }

    #region Achievements
    private HashSet<long> wonAchievements;
    private Dictionary<long, DBAchievement> achievements;

    public void SetAllAchievements(List<DBAchievement> allAchievements) {
        achievements = new Dictionary<long, DBAchievement>();
        foreach (DBAchievement achievement in allAchievements) {
            achievements[achievement.AchievementID] = achievement;
        }
    }

    public Dictionary<long, DBAchievement> GetAllAchievements() {
        return new Dictionary<long, DBAchievement> (achievements);
    }

    public void SetAchievement(long achievementID, bool wonAchievement = true) {
        if (!achievements.ContainsKey(achievementID))
            return;

        if (wonAchievement)
            wonAchievements.Add(achievementID);
        else
            wonAchievements.Remove(achievementID);
    }

    public void AddAchievement(long achievementID, string achievementName, string achievementDescription) {
        if (achievements.ContainsKey(achievementID))
            return;

        achievements.Add(achievementID, new DBAchievement(achievementID, achievementName, achievementDescription));
    }

    public long GetAchievementIdByName(string name) {
        // TODO: catch error when First not found
        DBAchievement ach = achievements.Values.FirstOrDefault(a => a.AchievementName == name);
        if (ach == null)
            return -1;
        else
            return ach.AchievementID;
    }

    public bool GetAchievementWonByName(string name) {
        return wonAchievements.Contains(GetAchievementIdByName(name));
    }

    public bool GetAchievementWonById(long achId) {
        return wonAchievements.Contains(achId);
    }

    public DBAchievement GetAchievementByName(string name) {
        return achievements.Values.FirstOrDefault(a => a.AchievementName == name);
    }

    public DBAchievement GetAchievementObjByName(string name) {
        return achievements.Values.FirstOrDefault(a => a.AchievementName == name);
    }

    public List<DBAchievement> GetAllWonAchievements() {
        return wonAchievements.Select(id => achievements[id]).ToList();
    }

    public void SetAllWonAchievements(List<long> wonAchs) {
        wonAchievements = new HashSet<long>(wonAchs);
    }

    #endregion

    #region Events
    private Dictionary<long, DBEvent> events;

    public void SetAllEvents(List<DBEvent> allEvents) {
        events = new Dictionary<long, DBEvent>();
        foreach (DBEvent @event in allEvents) {
            events[@event.EventID] = @event;
        }
    }

    public void SetGoingEvent(long eventID, bool isGoing = true) {
        events[eventID].UserGoing = isGoing;
    }
    
    public List<DBEvent> GetCalendar() {
        return new List<DBEvent>(events.Values);
    }

    public void SetInterest(long eventID, bool isInterested = true) {
        if (!events.ContainsKey(eventID))
            return;

        events[eventID].UserGoing = isInterested;
    }

    public bool ToggleInterest(long eventID) {
        if (!events.ContainsKey(eventID))
            return false;

        events[eventID].UserGoing = !events[eventID].UserGoing;
        return events[eventID].UserGoing;
    }
    #endregion

    #region Maps
    private Dictionary<long, DBMap> maps;
    public List<DBMap> GetMaps() {
        return new List<DBMap>(maps.Values);
    }
    public void SetAllMaps(List<DBMap> allMaps) {
        maps = new Dictionary<long, DBMap>();
        foreach (DBMap map in allMaps) {
            maps[map.MapID] = map;
        }
    }

    public DBMap GetMapById(long id) {
        return maps[id];
    }
    #endregion

    #region Items
    private Dictionary<long, int> itemCount = new Dictionary<long, int>();

    public void AddItem(long itemID, int count = 1) {
        if (!itemCount.ContainsKey(itemID))
            itemCount.Add(itemID, count);
        else
            itemCount[itemID] += count;
    }

    public int GetNumberOfItem(long itemID) {
        int ans = 0;
        itemCount.TryGetValue(itemID, out ans);
        return ans;
    }
    #endregion

    #region Content Groups
    private Dictionary<string, bool> contentGroups;

    public void SetAllContentGroups(Dictionary<string, bool> newCG) {
        contentGroups = new Dictionary<string, bool>();
        if (newCG == null)
            return;
        foreach(var cg in newCG) {
            contentGroups[cg.Key] = cg.Value;
        }
    }

    public bool GetContentGroupActiveByName(string name) {
        if (!contentGroups.ContainsKey(name))
            return false;
        return contentGroups[name];
    }
    #endregion

    #region Players
    private Dictionary<long, DBPlayer> players;

    public void SetPlayer(DBPlayer player) {
        players[player.PlayerID] = player;
    }

    public DBPlayer TryFindPlayerById(long playerId) {
        if (!players.ContainsKey(playerId))
            return null;
        return players[playerId];
    }
    #endregion
}