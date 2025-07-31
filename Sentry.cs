using System;

internal static class Sentry {
  public static void info(string message) {
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($@"[INFO] {message}");
    Console.ResetColor();
  }

  public static void error(string message) {
    Console.BackgroundColor = ConsoleColor.Red;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.WriteLine($@"[ERROR] {message}");
    Console.ResetColor();
  }

  public static void warn(string message) {
    Console.BackgroundColor = ConsoleColor.Yellow;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.WriteLine($@"[WARN] {message}");
    Console.ResetColor();
  }

  public static void debug(string message) {
    // if (Environment.GetEnvironmentVariable("DEBUG") == null) return;
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($@"[DEBUG] {message}");
    Console.ResetColor();
  }
}