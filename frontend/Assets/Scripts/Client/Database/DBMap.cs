using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all information about locally stored map graphics
/// </summary>
[Serializable]
public class DBMap {
    public readonly long MapID;
    public readonly string MapName;
    public readonly DateTime ValidBefore;
    public FilePath FP;

    public List<DBMapPOI> dBMapPOIs;

    public DBMap(long mapID, string mapName, DateTime validBefore, string filePath, bool remote = true) {
        MapID = mapID;
        MapName = mapName;
        ValidBefore = validBefore;
        dBMapPOIs = new List<DBMapPOI>();
        FP = new FilePath(remote, filePath);
    }

    public void AddPoi(DBMapPOI poiToAdd) {
        dBMapPOIs.Add(poiToAdd);
    }

    public void SetAllMapPois(List<DBMapPOI> toSet) {
        dBMapPOIs = new List<DBMapPOI>(toSet);
    }
}
