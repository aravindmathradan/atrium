using System;
using UnityEngine;

public static class TimeLogger {
  private static DateTime before;
  private static DateTime after;

  public static void TimerStart() {
    before = DateTime.Now;
  }

  public static void TimerEnd() {
    after = DateTime.Now;
    TimeSpan duration = after.Subtract(before);
    Debug.Log("Duration in seconds: " + duration.Seconds);
  }
}