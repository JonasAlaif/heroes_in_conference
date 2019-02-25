using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
/// <summary>
/// Is handed a list of DBEvents and will create a calendar GameObject for these events
/// </summary>
public class CalendarLayoutManager : MonoBehaviour {
    [SerializeField] private GameObject currentCalendar;
    [SerializeField] private RectTransform parentPanel;
    [SerializeField] private Sprite botRightBorder, topLeftBorder, eventBorder;
    [SerializeField] private Font textFont;

    #region Debug
    public List<DBEvent> debugCalendar;
    public bool generateCalWith2DaysAndBuils;
    public bool addFollowingEventToCal;
    public string StartTime;
    public string EndTime;

    public bool buildTheCal;
    // 08 Jan 2019 12:00:00 GMT
    #endregion

    void Update() {
        if (addFollowingEventToCal) {
            addFollowingEventToCal = false;
            DBEvent toAdd = new DBEvent(0, DateTime.Parse(StartTime), DateTime.Parse(EndTime), "", "");
            debugCalendar.Add(toAdd);
        }
        if (generateCalWith2DaysAndBuils) {
            generateCalWith2DaysAndBuils = false;
            debugCalendar.Clear();
            DBEvent toAdd = new DBEvent(0, DateTime.Parse("08 Jan 2019 12:00:00 GMT"), DateTime.Parse("08 Jan 2019 13:00:00 GMT"), "", "");
            debugCalendar.Add(toAdd);
            toAdd = new DBEvent(0, DateTime.Parse("09 Jan 2019 12:00:00 GMT"), DateTime.Parse("09 Jan 2019 13:00:00 GMT"), "", "");
            debugCalendar.Add(toAdd);
            BuildNewCalendarGO(debugCalendar);
        }
        if (!buildTheCal)
            return;

        buildTheCal = false;
       
        BuildNewCalendarGO(debugCalendar);
    }

    public void BuildNewCalendarGO(List<DBEvent> allEvents) {
        if (allEvents.Count == 0) {
            Debug.LogError("Trying to display no events!");
            return;
        }
        // Ignores events that go over midnight!
        List<LayoutEvent> sortedCalendar = new List<LayoutEvent>(allEvents.Select(dBEvent => new LayoutEvent(dBEvent)).OrderBy(ev => ev.Start).ThenBy(ev => ev.End).ToList());
        LayoutEvents(sortedCalendar);
        Queue<LayoutEvent> calendarQueue = new Queue<LayoutEvent>(sortedCalendar);

        int calendarTotalDays = (sortedCalendar.Last().End - sortedCalendar.First().Start).Days + 1;

        if (calendarTotalDays > 7) {
            Debug.LogError("Trying to display events within a span of more than 7 days!");
        }
        Debug.Log("Total calendar days: " + calendarTotalDays);

        bool over3Days = calendarTotalDays > 3;
        float titlePanelHeight = 1 - (((over3Days ? 510 : 108) / calendarTotalDays + 16) / parentPanel.rect.size.y);
        int hoursWidth = 60;
        float hourHeight = 60 / parentPanel.rect.size.y*titlePanelHeight;

        RectTransform calendarObject = buildBaseGO();
        calendarObject.gameObject.SetActive(false);

        RectTransform titlesPanel = buildTitlesPanel(calendarObject, titlePanelHeight);
        RectTransform dayTitlesPanel = buildDayTitlesPanel(titlesPanel, hoursWidth);

        RectTransform eventsViewportPanel = buildEventsViewportPanel(calendarObject, titlePanelHeight);
        RectTransform scrollContentPanel = buildScrollContentPanel(eventsViewportPanel, hourHeight);
        RectTransform daysPanel = buildDaysPanel(scrollContentPanel, hoursWidth);
        setupScrolRect(calendarObject.gameObject, scrollContentPanel, eventsViewportPanel);

        DateTime startDate = sortedCalendar.First().Start;
        for (int i = 0; i < calendarTotalDays; i++) {
            buildDayTitle(dayTitlesPanel, i, calendarTotalDays, startDate, over3Days);

            RectTransform dayPanel = buildDayPanel(daysPanel, i, calendarTotalDays);
            for(int j = 0; j < 24; j++) {
                buildHourPanel(dayPanel, j);
            }
            if (calendarQueue.Count == 0)
                continue;
            while(calendarQueue.Peek().Start.Date == startDate.AddDays(i).Date) {
                LayoutEvent le = calendarQueue.Dequeue();
                buildEventPanel(dayPanel, le);
                if (calendarQueue.Count == 0)
                    break;
            }
        }
        for (int j = 1; j < 24; j++) {
            buildHourText(scrollContentPanel, j, hoursWidth);
        }


        if (currentCalendar != null) {
            currentCalendar.SetActive(false);
            DestroyImmediate(currentCalendar);
        }
        calendarObject.gameObject.SetActive(true);
        currentCalendar = calendarObject.gameObject;
    }

    public class LayoutEvent {
        public readonly DBEvent dBEvent;
        public float Left;
        public float Right;
        public LayoutEvent(DBEvent dBEvent) {
            this.dBEvent = dBEvent;
        }

        public DateTime Start { get => dBEvent.StartTime; }
        public DateTime End { get => dBEvent.EndTime; }

        public bool CollidesWith(LayoutEvent other) {
            return (End > other.Start && Start < other.End);
        }
    }

    private float[] getWidthForeachEvent(SortedSet<DBEvent> calendar) {
        // Creates a list of pairs of start and end times for each event
        // Variable name shortened from startEndTimeTicks to sETT for later use
        Tuple<long, long>[] sETT = calendar.Select(evnt => new Tuple<long, long>(evnt.StartTime.Ticks, evnt.EndTime.Ticks)).ToArray();
        int[] allOverlapsAfter = new int[calendar.Count], allOverlapsBefore = new int[calendar.Count];
        float[] widthPercentage = new float[calendar.Count];
        for (int i = 0; i < calendar.Count; i++) {
            for (int j = i + 1; j < calendar.Count; j++) {
                // The calendar is sorted so j cannot be before i
                bool iBeforeJ = sETT[i].Item2 <= sETT[j].Item1;
                if (!iBeforeJ) {
                    allOverlapsAfter[i]++;
                    allOverlapsBefore[j]++;
                } // Sorted so once j is after i no further j will overlap, can go onto next i
                else break;
            }
            float baseWidth = 1f;
            if(allOverlapsBefore[i] == 0) {
                baseWidth /= allOverlapsAfter[i] + 1;
            }
            else {
                for (int k = 0; k < allOverlapsBefore[i]; k++) {
                    baseWidth -= widthPercentage[(i - 1) - k];
                }
            }
            widthPercentage[i] = 1f / allOverlapsAfter[i];

            // TODO: figure this out! Ahaha
            widthPercentage[i] = 1f;
        }
        return widthPercentage;
    }
    
    
    /// Pick the left and right positions of each event, such that there are no overlap.
    /// Step 3 in the algorithm.
    void LayoutEvents(List<LayoutEvent> events) {
        List<List<LayoutEvent>> columns = new List<List<LayoutEvent>>();
        DateTime? lastEventEnding = null;
        foreach (LayoutEvent ev in events) {
            if (ev.Start >= lastEventEnding) {
                PackEvents(columns);
                columns.Clear();
                lastEventEnding = null;
            }
            bool placed = false;
            foreach (List<LayoutEvent> col in columns) {
                if (!col.Last().CollidesWith(ev)) {
                    col.Add(ev);
                    placed = true;
                    break;
                }
            }
            if (!placed) {
                columns.Add(new List<LayoutEvent> { ev });
            }
            if (lastEventEnding == null || ev.End > lastEventEnding.Value) {
                lastEventEnding = ev.End;
            }
        }
        if (columns.Count > 0) {
            PackEvents(columns);
        }
    }

    /// Set the left and right positions for each event in the connected group.
    /// Step 4 in the algorithm.
    void PackEvents(List<List<LayoutEvent>> columns) {
        float numColumns = columns.Count;
        int iColumn = 0;
        foreach (var col in columns) {
            foreach (var ev in col) {
                int colSpan = ExpandEvent(ev, iColumn, columns);
                ev.Left = iColumn / numColumns;
                ev.Right = (iColumn + colSpan) / numColumns;
            }
            iColumn++;
        }
    }

    /// Checks how many columns the event can expand into, without colliding with
    /// other events.
    /// Step 5 in the algorithm.
    int ExpandEvent(LayoutEvent ev, int iColumn, List<List<LayoutEvent>> columns) {
        int colSpan = 1;
        foreach (var col in columns.Skip(iColumn + 1)) {
            foreach (var ev1 in col) {
                if (ev1.CollidesWith(ev)) {
                    return colSpan;
                }
            }
            colSpan++;
        }
        return colSpan;
    }



    #region Build Individual GOs
    /// <summary>
    /// Build the parent GO of this calendar
    /// </summary>
    /// <returns></returns>
    private RectTransform buildBaseGO() {
        RectTransform calendarObject = new GameObject("Calendar").AddComponent<RectTransform>();
        calendarObject.SetParent(parentPanel);
        setRectTransformPos(calendarObject, 0, 0, 1, 1);
        return calendarObject;
    }


    /// <summary>
    /// Build panel for dates
    /// </summary>
    private RectTransform buildTitlesPanel(RectTransform calendarObject, float titlePanelHeight) {
        RectTransform titlesPanel = new GameObject("Titles Panel").AddComponent<RectTransform>();
        titlesPanel.SetParent(calendarObject);
        setRectTransformPos(titlesPanel, 0, titlePanelHeight, 1, 1);
        addBorder(titlesPanel.gameObject, botRightBorder);
        return titlesPanel;
    }
    /// <summary>
    /// Build panel for dates titles
    /// </summary>
    private RectTransform buildDayTitlesPanel(RectTransform titlesPanel, int hoursWidth) {
        RectTransform dayTitlesPanel = new GameObject("Day Titles Panel").AddComponent<RectTransform>();
        dayTitlesPanel.SetParent(titlesPanel);
        setRectTransformPos(dayTitlesPanel, hoursWidth / titlesPanel.rect.size.x, 0, 1, 1);
        addBorder(dayTitlesPanel.gameObject, topLeftBorder);
        return dayTitlesPanel;
    }
    /// <summary>
    /// Build a title for an individual day
    /// </summary>
    /// <param name="dayTitlesPanel"></param>
    /// <param name="day"></param>
    /// <param name="calendarTotalDays"></param>
    /// <param name="startDate"></param>
    /// <param name="over3Days"></param>
    private void buildDayTitle(RectTransform dayTitlesPanel, int day, int calendarTotalDays, DateTime startDate, bool over3Days) {
        RectTransform dayTitlePanel = new GameObject("Day " + day + " Title Panel").AddComponent<RectTransform>();
        dayTitlePanel.SetParent(dayTitlesPanel);
        setRectTransformPos(dayTitlePanel, day / (float)calendarTotalDays, 0,
                                     (day + 1) / (float)calendarTotalDays, 1);
        addBorder(dayTitlePanel.gameObject, topLeftBorder);

        GameObject dayTitleText = new GameObject("Day " + day + " Title Text");
        RectTransform dayTitleTextRT = dayTitleText.AddComponent<RectTransform>();
        dayTitleTextRT.SetParent(dayTitlePanel);
        setRectTransformPos(dayTitleTextRT, 0, 0, 1, 1);
        Text dayTitleTextUI = dayTitleText.AddComponent<Text>();
        dayTitleTextUI.alignment = TextAnchor.MiddleCenter;
        float fSize = (float)96 / calendarTotalDays;
        dayTitleTextUI.fontSize = (int)fSize;
        dayTitleTextUI.font = textFont;
        dayTitleTextUI.color = Color.black;
        if (over3Days) {
            string formatString = @"\<\s\i\z\e\=" + (int)(fSize * 1.5) + @"\>" + "ddd" + @"\<\/\s\i\z\e\>" + Environment.NewLine +
                                  @"\<\s\i\z\e\=" + (int)(fSize * 2) + @"\>" + "d" + @"\<\/\s\i\z\e\>" + Environment.NewLine +
                                  @"\<\s\i\z\e\=" + (int)(fSize * 1.2) + @"\>" + "MMM" + @"\<\/\s\i\z\e\>";
            dayTitleTextUI.text = startDate.AddDays(day).ToString(formatString);
        }
        else
            dayTitleTextUI.text = startDate.AddDays(day).ToString("ddd d MMM");
    }


    /// <summary>
    /// Build viewport panel for events
    /// </summary>
    private RectTransform buildEventsViewportPanel(RectTransform calendarObject, float titlePanelHeight) {
        RectTransform eventsViewportPanel = new GameObject("Events Panel").AddComponent<RectTransform>();
        eventsViewportPanel.SetParent(calendarObject);
        setRectTransformPos(eventsViewportPanel, 0, 0, 1, titlePanelHeight);
        eventsViewportPanel.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        addBorder(eventsViewportPanel.gameObject, topLeftBorder);
        return eventsViewportPanel;
    }
    /// <summary>
    /// Build scrollable panel for events
    /// </summary>
    private RectTransform buildScrollContentPanel(RectTransform eventsViewportPanel, float hourHeight) {
        RectTransform scrollViewportPanel = new GameObject("Scroll Panel").AddComponent<RectTransform>();
        scrollViewportPanel.SetParent(eventsViewportPanel);
        setRectTransformPos(scrollViewportPanel, 0, 1-(hourHeight*24), 1, 1);
        addBorder(scrollViewportPanel.gameObject, topLeftBorder);
        return scrollViewportPanel;
    }
    private void setupScrolRect(GameObject calendarObj, RectTransform scrollContentPanel, RectTransform eventsViewportPanel) {
        ScrollRect sr = calendarObj.AddComponent<ScrollRect>();
        sr.content = scrollContentPanel;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.viewport = eventsViewportPanel;
    }
    /// <summary>
    /// Build panel for dates with events
    /// </summary>
    private RectTransform buildDaysPanel(RectTransform scrollContentPanel, int hoursWidth) {
        RectTransform daysPanel = new GameObject("Days Panel").AddComponent<RectTransform>();
        daysPanel.SetParent(scrollContentPanel);
        setRectTransformPos(daysPanel, hoursWidth / scrollContentPanel.rect.size.x, 0, 1, 1);
        return daysPanel;
    }
    /// <summary>
    /// Build panel for one date with events
    /// </summary>
    private RectTransform buildDayPanel(RectTransform daysPanel, int day, int calendarTotalDays) {
        RectTransform dayPanel = new GameObject("Day " + day + " Panel").AddComponent<RectTransform>();
        dayPanel.SetParent(daysPanel);
        setRectTransformPos(dayPanel, day / (float)calendarTotalDays, 0,
                                (day + 1) / (float)calendarTotalDays, 1);
        return dayPanel;
    }
    /// <summary>
    /// Build hour image
    /// </summary>
    private RectTransform buildHourPanel(RectTransform dayPanel, int hour) {
        RectTransform hourPanel = new GameObject("Hour " + hour + " Panel").AddComponent<RectTransform>();
        hourPanel.SetParent(dayPanel);
        setRectTransformPos(hourPanel, 0, 1 - (hour+1) / 24f,
                                       1, 1 - hour / 24f);
        addBorder(hourPanel.gameObject, topLeftBorder);
        return dayPanel;
    }
    /// <summary>
    /// Build hour text
    /// </summary>
    private RectTransform buildHourText(RectTransform scrollContentPanel, int hour, int hourWidth) {
        RectTransform hourPanel = new GameObject("Hour " + hour + " Panel").AddComponent<RectTransform>();
        hourPanel.SetParent(scrollContentPanel);
        setRectTransformPos(hourPanel, 0, 1 - (hour+0.5f) / 24f,
                                       hourWidth / scrollContentPanel.rect.size.x, 1 - (hour-0.5f) / 24f);

        Text dayTitleTextUI = hourPanel.gameObject.AddComponent<Text>();
        dayTitleTextUI.alignment = TextAnchor.MiddleCenter;
        dayTitleTextUI.fontSize = 13;
        dayTitleTextUI.font = textFont;
        dayTitleTextUI.color = Color.grey;
        dayTitleTextUI.text = hour.ToString().PadLeft(2, '0') + ":00";
        return hourPanel;
    }
    
    /// <summary>
    /// Build event panel
    /// </summary>
    private RectTransform buildEventPanel(RectTransform dayPanel, LayoutEvent theEvent) {
        RectTransform eventPanel = new GameObject("Event " + theEvent.dBEvent.EventName + " Panel").AddComponent<RectTransform>();
        eventPanel.SetParent(dayPanel);
        setRectTransformPos(eventPanel, theEvent.Left, 1 - (theEvent.End.Hour + (theEvent.End.Minute / 60f) - 1) / 24f,
                                      theEvent.Right, 1 - (theEvent.Start.Hour + (theEvent.Start.Minute / 60f) - 1) / 24f);
        /*
        Text dayTitleTextUI = eventPanel.gameObject.AddComponent<Text>();
        dayTitleTextUI.alignment = TextAnchor.MiddleCenter;
        dayTitleTextUI.fontSize = 13;
        dayTitleTextUI.font = textFont;
        dayTitleTextUI.color = Color.grey;
        dayTitleTextUI.text = hour.ToString().PadLeft(2, '0') + ":00";*/
        addBorder(eventPanel.gameObject, eventBorder);
        return eventPanel;
    }
    #endregion

    #region Utility Methods
    private void setRectTransformPos(RectTransform rt, float x0, float y0, float x1, float y1) {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void addBorder(GameObject toAddTo, Sprite theBorder) {
        Image dtBorder = toAddTo.AddComponent<Image>();
        dtBorder.sprite = theBorder;
        dtBorder.type = Image.Type.Sliced;
    }
    #endregion

    private void OnRectTransformDimensionsChange() {
        return;

        if(currentCalendar != null)
            currentCalendar.SetActive(false);
        if(NetworkDatabase.NDB != null)
            BuildNewCalendarGO(NetworkDatabase.NDB.GetCalendar());
    }
}
