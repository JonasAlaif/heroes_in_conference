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
    
    private FilePath fp;
    public FilePath FP { get => fp; set => fp = value; }

    public DBMap(long mapID, string mapName, DateTime validBefore) {
        MapID = mapID;
        MapName = mapName;
        ValidBefore = validBefore;
    }

    // TODO: move this class outside of DBMap, since it isnt unique to that
    [Serializable]
    public class FilePath {
        public readonly bool IsRemote;
        public readonly string Path;

        public FilePath(bool isRemote, string path) {
            IsRemote = isRemote;
            Path = path;
        }
    }
}
