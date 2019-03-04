using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all information about locally stored Achievement
/// </summary>
[Serializable]
public class DBAchievement {
    public readonly long AchievementID;
    public readonly string AchievementName;
    public readonly string AchievementDescription;

    public DBAchievement(long achievementID, string achievementName, string achievementDescription) {
        AchievementID = achievementID;
        AchievementName = achievementName;
        AchievementDescription = achievementDescription;
    }
}
