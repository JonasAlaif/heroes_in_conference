using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all information about locally stored Achievement
/// </summary>
[Serializable]
public class DBMapPOI {
    public long MapPoiID { get; }
    public string MapPoiName { get; }
    public string MapPoiDescription { get; }
    public int X { get; }
    public int Y { get; }

    public DBMapPOI(long mapPoiID, string mapPoiName, string mapPoiDescription, int x, int y) {
        MapPoiID = mapPoiID;
        MapPoiName = mapPoiName;
        MapPoiDescription = mapPoiDescription;
        X = x;
        Y = y;
    }
}
