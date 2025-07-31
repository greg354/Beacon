using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Waypoint;

namespace Beacon.WorkspaceTests;

[TestName("JavaScript")]
[TestDescription("Evaluate javascript files for common malicious code practices.")]
internal class JavaScriptWorkspaceTest : WorkspaceTest {
  protected override bool validate(TestContext context) {
    var allFilesValid = true;
    foreach (var entry in context.zipArchive.entries) {
      if (!Path.GetExtension(entry.FullName).EndsWith(".js")) continue;
      using var reader = new StreamReader(entry.Open());
      var jsCode = reader.ReadToEnd();
      if (Regex.IsMatch(jsCode, @"\beval\s*\(")) {
        this.addWarning($"File '{entry.FullName}' contains usage of eval(), which is potentially unsafe.");
        allFilesValid = false;
      }

      if (Regex.IsMatch(jsCode, @"document\.write\s*\(")) {
        this.addWarning($"File '{entry.FullName}' contains usage of document.write(), which can be unsafe.");
        allFilesValid = false;
      }

      if (Regex.IsMatch(jsCode, @"on\w+\s*=\s*[""'].+[""']")) {
        this.addWarning(
          $"File '{entry.FullName}' contains inline event handlers, which can be a vector for XSS attacks.");
        allFilesValid = false;
      }

      if (Regex.IsMatch(jsCode, @"(['""]).+\1\.split\('\'\)\.reverse\(\)\.join\('\'\)")) {
        this.addWarning(
          $"File '{entry.FullName}' contains suspicious string manipulation. Possible obfuscation attempt.");
        allFilesValid = false;
      }

      if (Regex.IsMatch(jsCode, @"for\s*\([^;]*;[^;]*;[^;]*\)\s*\{\s*\}")) {
        this.addWarning($"File '{entry.FullName}' contains a potential infinite loop.");
        allFilesValid = false;
      }

      if (Regex.IsMatch(jsCode, @"\.innerHTML\s*=")) {
        this.addWarning(
          $"File '{entry.FullName}' uses innerHTML, which can be a vector for XSS if not properly sanitized.");
        allFilesValid = false;
      }

      var semicolonCount = Regex.Matches(jsCode, ";").Count;
      if (semicolonCount > jsCode.Length / 10) {
        this.addWarning(
          $"File '{entry.FullName}' contains an unusually high number of semicolons. Possible obfuscation.");
        allFilesValid = false;
      }

      var openBrackets = jsCode.Count(c => c == '{');
      var closeBrackets = jsCode.Count(c => c == '}');
      var openParens = jsCode.Count(c => c == '(');
      var closeParens = jsCode.Count(c => c == ')');
      if (openBrackets != closeBrackets || openParens != closeParens) {
        this.addWarning(
          $"File '{entry.FullName}' contains mismatched brackets or parentheses. Code may be malformed.");
        allFilesValid = false;
      }
    }

    return allFilesValid;
  }
}