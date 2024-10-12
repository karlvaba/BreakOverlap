namespace breakoverlap;

/*
    Main class the holds time data and handles calculation logic.
*/
public class Timeline(TimelineConfig config)
{
    public TimelineConfig Config { get; set; } = config;
    public List<TimeMarker> TimeMarkers { get; set; } = []; //Holds time markers (start and end times) of driver breaks

    /*
        Reads break times from file, converts them to time markers.
        Markers are ordered first by value (smaller times first), then by type (start markers before end).
        Time complexity of O(nLog(n))
        Such ordering required for the calculation algorithm.
    */
    public List<int> ReadFromFile(string filename) {
        if (!File.Exists(filename)) {
            throw new ArgumentException("Could not find file with specified path. Continuing with empty list of break times.");
        }
        List<int> errorLines = [];
        
        //Reads lines from file and converts them to time markers.
        TimeMarkers = [.. File.ReadLines(filename)
            .SelectMany((line, index) => 
                {
                    try {
                        return TimeStringToMarkers(line, index);
                    } catch (ArgumentException e){
                        if (Config.FileAllOrNothing) { //Reads no lines if a single line in the file cannot be parsed, if the config option = true
                            throw new ArgumentException(e.Message + "File line: " + index);
                        } else { //Continues to read other lines if all or nothing = false
                            errorLines.Add(index+1);
                            return [];
                        }
                    }
                
            })
            .OrderBy(marker => marker.Value)
            .ThenBy(marker => marker.Type)]; 

        return errorLines;   
    }

    /*
        Adds a single break time as time markers.
        Finds indices for each of the markers and inserts them. 
        Each operation (find and insert) ~O(n) time complexity
    */
    public void AddBreakTime(string breakTime) {
        TimeMarker[] markers = TimeStringToMarkers(breakTime, null);

        int index = TimeMarkers.FindIndex(m => m.Value >= markers[0].Value);
        if (index < 0) {
            TimeMarkers.Add(markers[0]);
        } else {
            TimeMarkers.Insert(index, markers[0]);
        } 
        index = TimeMarkers.FindIndex(m => m.Value > markers[1].Value);

        if (index < 0) {
            TimeMarkers.Add(markers[1]);
        } else {
            TimeMarkers.Insert(index, markers[1]);
        }
    }

    /*
        Tries to convert the input string (<HH:mm><HH:mm>, e.g 10:0011:15 to time markers)
    */
    public TimeMarker[] TimeStringToMarkers(string timeString, int? lineNumber) {
        TimeOnly start = new();
        TimeOnly end = new();
        if (ValidateTimeString(timeString, out start, out end)) {
            return [new TimeMarker(MarkerType.Start, start), new TimeMarker(MarkerType.End, end)];
        } else {
            throw new ArgumentException("Time string is invalid. Provided time string: " + timeString);
        }
    }

    /*
        Validates the time string. Three conditions for the timestring:
        1) Must be of a specified length
        2) Both halves of the string (start and end time) must be of the format HH:mm
        3) Start time must be <= end time ( < than, if configuration does not allow equal star and end times)
    */
    public bool ValidateTimeString(string timeString, out TimeOnly start, out TimeOnly end) {
        if (timeString.Length == Config.TimeStringLength) {
            if (TimeOnly.TryParseExact(timeString[..(Config.TimeStringLength/2)], Config.TimeFormat, out start) 
                && TimeOnly.TryParseExact(timeString[(Config.TimeStringLength/2)..], Config.TimeFormat, out end)) {
                    return Config.StartCanEqualEnd ? start <= end : start < end;
            }
        }

        start = new();
        end = new();
        return false;
    }

    /*
        Calculates the time range in which most drivers are on break.
        Uses a simple approach of sweeping over the time markers from start to finish.
        If a start marker is encountered, a new "open range" has been found, increasing the count of open ranges. 
        If an end marker is encountered, an open range is closed, decreasing the number of open ranges.

        At any iteration, the number of open ranges equals the number of overlapped times.

        If a range is closed, and the number of overlapped times was greater than before, then updates the most overlapped times.
        If two time ranges has the same number of overlaps, keeps the one with the longer timespan.

        When all time markers are iterated through, returns the most overlapped interval.   

        (More easily explained with a diagram, check the Readme.md :) )

        The markers must be ordered by tiem value (ascending) and marker type (start markers before end markers if values are equal)
    */
    public (TimeRange, int) MostCommonTimeRange() {

        int openRangeCount = 0;
        int mostOverlaps = 0;
        TimeRange mostCommonRange = new();
        TimeOnly commonRangeStart = new();        
    
        foreach (TimeMarker marker in TimeMarkers) {
            if (marker.Type == MarkerType.Start) {
                commonRangeStart = marker.Value;
                openRangeCount++;
            } else {
                if (mostOverlaps < openRangeCount) {
                    mostCommonRange.Start = commonRangeStart;
                    mostCommonRange.End = marker.Value;
                    mostOverlaps = openRangeCount;
                } else if (mostOverlaps == openRangeCount) {
                    TimeRange tmp = new(commonRangeStart, marker.Value);
                    mostCommonRange = tmp.SpanMinutes() > mostCommonRange.SpanMinutes() ? tmp : mostCommonRange;
                }

                openRangeCount--;
            }
        }
        return (mostCommonRange, mostOverlaps);

    }

}

/*
    Possible configuration options. 
    Current implementation uses the default values. 
*/
public struct TimelineConfig {
    public string TimeFormat { get; set; } //format requirement for star and end times
    public int TimeStringLength { get; set; } //required for parsing inputs
    public bool StartCanEqualEnd { get; set; } //Wether start time can be equal to an end time or not
    public bool FileAllOrNothing { get; set; } //Wether to discard the file if a single line cannot be read.

    public TimelineConfig() {
        TimeFormat = "HH:mm";
        TimeStringLength = 10;
        StartCanEqualEnd = true;
        FileAllOrNothing = false;
    }

    public TimelineConfig(string timeFormat, int timeStringLength, bool startCanEqualEnd, bool fileAllOrNothing) {
        TimeFormat = timeFormat;
        TimeStringLength = timeStringLength;
        StartCanEqualEnd = startCanEqualEnd;
        FileAllOrNothing = fileAllOrNothing;
    }
}

/*
    Simple data class to denote time markers. 
    Markers can mark either a start or an end of a (break) time range.
*/
public class TimeMarker(MarkerType type, TimeOnly value)
{
    public MarkerType Type { get; set; } = type;
    public TimeOnly Value { get; set; } = value;


    public override string ToString() {
        return this.Value.ToString("HH:mm") + " - " + this.Type.ToString();
    }
}
public enum MarkerType {
        Start = 1,
        End = 2
}


public class TimeRange
{
    public TimeOnly Start { get; set; } 
    public TimeOnly End { get; set; }

    public TimeRange() {
        this.Start = TimeOnly.ParseExact("00:00", "HH:mm");
        this.End = TimeOnly.ParseExact("00:00", "HH:mm");
    }

    public TimeRange(TimeOnly start, TimeOnly end) {
        this.Start = start;
        this.End = end;
    }

    public double SpanMinutes() {
        return (End - Start).TotalMinutes;
    }
    

    public override string ToString() {
        return this.Start.ToString("HH:mm") + " - " + this.End.ToString("HH:mm");
    }
}
