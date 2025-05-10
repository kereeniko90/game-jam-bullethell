using UnityEngine;

/// <summary>
/// Utility class for converting between seconds and formatted time strings
/// </summary>
public static class TimeFormatter
{
    /// <summary>
    /// Converts seconds to a formatted time string (HH:MM:SS)
    /// </summary>
    /// <param name="totalSeconds">Total seconds to convert</param>
    /// <param name="includeHours">Whether to include hours in the format (00:00:00) or just minutes and seconds (00:00)</param>
    /// <returns>Formatted time string</returns>
    public static string SecondsToTimeString(float totalSeconds, bool includeHours = true)
    {
        // Handle negative time
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int hours = Mathf.FloorToInt(totalSeconds / 3600);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);

        if (includeHours || hours > 0)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Converts seconds to a formatted time string with milliseconds (HH:MM:SS.MS)
    /// </summary>
    /// <param name="totalSeconds">Total seconds to convert</param>
    /// <param name="includeHours">Whether to include hours in the format</param>
    /// <returns>Formatted time string with milliseconds</returns>
    public static string SecondsToTimeStringWithMilliseconds(float totalSeconds, bool includeHours = false)
    {
        // Handle negative time
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int hours = Mathf.FloorToInt(totalSeconds / 3600);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        int milliseconds = Mathf.FloorToInt((totalSeconds * 1000) % 1000);

        if (includeHours || hours > 0)
        {
            return string.Format("{0:00}:{1:00}:{2:00}.{3:000}", hours, minutes, seconds, milliseconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
        }
    }

    /// <summary>
    /// Converts a time string (HH:MM:SS) to total seconds
    /// </summary>
    /// <param name="timeString">Time string in format "HH:MM:SS" or "MM:SS"</param>
    /// <returns>Total seconds, or -1 if the string format is invalid</returns>
    public static float TimeStringToSeconds(string timeString)
    {
        // Split the string by colons
        string[] timeParts = timeString.Split(':');
        
        // Check if we have a valid format (2 or 3 parts)
        if (timeParts.Length < 2 || timeParts.Length > 3)
        {
            Debug.LogWarning("Invalid time string format: " + timeString);
            return -1;
        }

        try
        {
            if (timeParts.Length == 3) // HH:MM:SS
            {
                int hours = int.Parse(timeParts[0]);
                int minutes = int.Parse(timeParts[1]);
                float seconds = float.Parse(timeParts[2]);
                
                return (hours * 3600) + (minutes * 60) + seconds;
            }
            else // MM:SS
            {
                int minutes = int.Parse(timeParts[0]);
                float seconds = float.Parse(timeParts[1]);
                
                return (minutes * 60) + seconds;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error parsing time string: " + e.Message);
            return -1;
        }
    }

    /// <summary>
    /// Converts a duration in seconds to a human-readable format (e.g., "2h 30m 15s" or "45m 20s")
    /// </summary>
    /// <param name="totalSeconds">Total seconds to convert</param>
    /// <param name="useShortFormat">Whether to use shortened labels (h/m/s) instead of full words</param>
    /// <returns>Human-readable duration string</returns>
    public static string SecondsToDurationString(float totalSeconds, bool useShortFormat = true)
    {
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int hours = Mathf.FloorToInt(totalSeconds / 3600);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);

        string result = "";

        // Only include hours if there are any
        if (hours > 0)
        {
            result += useShortFormat ? $"{hours}h " : $"{hours} hours ";
        }

        // Only include minutes if there are any or if there were hours
        if (minutes > 0 || hours > 0)
        {
            result += useShortFormat ? $"{minutes}m " : $"{minutes} minutes ";
        }

        // Always include seconds
        result += useShortFormat ? $"{seconds}s" : $"{seconds} seconds";

        return result.Trim();
    }
}