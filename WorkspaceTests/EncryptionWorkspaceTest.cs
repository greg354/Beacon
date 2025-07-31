using System.IO;
using System.Linq;
using Waypoint;

namespace Beacon.WorkspaceTests;

[TestName("Encryption")]
[TestDescription("Check if any files in the workspace are encrypted.")]
internal class EncryptionWorkspaceTest : WorkspaceTest {
  protected override bool validate(TestContext context) {
    var encryptedFiles = context.zipArchive.entries.Where(entry => {
      var encrypted = (entry.ExternalAttributes & (int)FileAttributes.Encrypted) == (int)FileAttributes.Encrypted;
      if (encrypted) this.addWarning($"File {entry.FullName} is encrypted.");
      return encrypted;
    }).ToList();

    return encryptedFiles.Any() == false;
  }
}