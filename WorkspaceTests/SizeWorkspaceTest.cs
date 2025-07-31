using System.Collections.Generic;
using System.Linq;
using System.Web;
using Waypoint;

namespace Beacon.WorkspaceTests;

[TestName("Size")]
[TestDescription(
  "Check if any files in the workspace exceed the size limit, compares individual compression ratios of files.")]
internal class SizeWorkspaceTest : WorkspaceTest {
  private const double maxWorkspaceSizeBytes = 64 * 1024 * 1024;
  private const double defaultCompressionRatio = 0.7;

  private readonly Dictionary<string, double> mimeTypeCompressionRatios = new() {
    { "text/*", 0.3 },
    { "image/*", 0.95 },
    { "audio/*", 0.95 },
    { "video/*", 0.99 },
    { "application/zip", 1.0 },
    { "application/vnd.openxmlformats-officedocument.*", 0.5 },
    { "application/octet-stream", 0.95 }
  };

  protected override bool validate(TestContext context) {
    var workspaceSize = context.zipArchive.entries.Aggregate(0f, (acc, entry) => {
      var mimeType = MimeMapping.GetMimeMapping(entry.Name);
      var compressionRatio = this.getCompressionRatio(mimeType);
      var estimatedCompressedSize = entry.Length * compressionRatio;
      const double threshold = maxWorkspaceSizeBytes / 2;
      if (estimatedCompressedSize > threshold)
        this.addWarning($"File {entry.Name} is unusually large at length {entry.Length / 1e+6}MB.");
      return acc += entry.Length;
    });

    return workspaceSize <= maxWorkspaceSizeBytes;
  }

  /*
   * gets compression ratios from the dictionary with added wildcard support.
   */
  private double getCompressionRatio(string mimeType) {
    var match = this.mimeTypeCompressionRatios.FirstOrDefault(ratio =>
      ratio.Key == mimeType || (ratio.Key.EndsWith("/*") && mimeType.StartsWith(ratio.Key.TrimEnd('*'))) ||
      (ratio.Key.EndsWith("*") && mimeType.EndsWith(ratio.Key.TrimEnd('*'))));
    return match.Key != null ? match.Value : defaultCompressionRatio;
  }
}