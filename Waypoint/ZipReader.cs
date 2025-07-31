using System.IO.Compression;

namespace Waypoint;

/*
  Middleman class for reading and managing opened zip file handles
  allowing for thread-safe access to zip file contents.
*/
public class ZipReader {
  public readonly string archivePath;
  public readonly ZipArchiveEntry[] entries;
  public readonly bool isEncrypted;
  private readonly ZipArchive zipArchive;

  public ZipReader(string path) {
    this.zipArchive = ZipFile.OpenRead(path);
    this.isEncrypted = false;
    this.archivePath = path;
    this.entries = [.. this.zipArchive.Entries];
  }
}