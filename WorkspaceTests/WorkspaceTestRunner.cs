using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Waypoint;

namespace Beacon.WorkspaceTests;

internal class TestNotFoundException(string name) : Exception($"Test {name} is not registered with test runner.") {
}

internal readonly struct TestResult(bool passed, IReadOnlyList<string> warnings) {
  public readonly bool passed = passed;
  public readonly IReadOnlyList<string> warnings = warnings;
}

internal class WorkspaceTestRunner {
  private readonly List<WorkspaceTest> tests = [];
  private ZipReader? workspace;
  private WorkspaceType workspaceType;
  public event EventHandler<TestResult>? testComplete;

  public List<WorkspaceTest> getTests() => this.tests;
  public void setWorkspace(ZipReader zipReader) => this.workspace = zipReader;
  public void setWorkspaceType(WorkspaceType workspaceType) => this.workspaceType = workspaceType;
  public void registerTest(WorkspaceTest workspaceTest) => this.tests.Add(workspaceTest);

  public void enableTest(Type testType) {
    if (!typeof(WorkspaceTest).IsAssignableFrom(testType))
      throw new ArgumentException($"Type '{testType.Name}' is not a subclass of WorkspaceTest");

    foreach (var test in this.tests.Where(test => test.GetType() == testType)) {
      test.enable();
      return;
    }

    throw new TestNotFoundException(testType.Name);
  }

  public void disableTest(Type testType) {
    if (!typeof(WorkspaceTest).IsAssignableFrom(testType))
      throw new ArgumentException($"Type '{testType.Name}' is not a subclass of WorkspaceTest");

    foreach (var test in this.tests.Where(test => test.GetType() == testType)) {
      test.disable();
      return;
    }

    throw new TestNotFoundException(testType.Name);
  }


  public void runTests() {
    var context = new TestContext {
      zipArchive = this.workspace!,
      workspaceType = this.workspaceType
    };

    foreach (var test in this.tests) {
      if (!test.enabled) continue;
      var testType = test.GetType();
      ThreadPool.QueueUserWorkItem(_ => {
        Sentry.debug($"Running test {test.GetType().Name} in thread {Environment.CurrentManagedThreadId}");
        try {
          var result = test.validateAndWarn(context);
          this.onTestComplete(testType, new TestResult(result.passed, result.warnings));
        } catch (Exception exception) {
          this.onTestComplete(testType,
            new TestResult(false, ["An unexpected error occurred while running the test."]));
          Sentry.error($"Test failed with an error: {exception.Message}\n{exception.StackTrace}");
        }
      });
    }
  }

  private void onTestComplete(object sender, TestResult result) => testComplete?.Invoke(sender, result);
}