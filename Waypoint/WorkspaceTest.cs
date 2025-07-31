using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Waypoint;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TestNameAttribute(string name) : Attribute {
  public string name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TestDescriptionAttribute(string description) : Attribute {
  public string description { get; } = description;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class AppliedWorkspaceAttribute(WorkspaceType workspaceType) : Attribute {
  public WorkspaceType workspaceType { get; } = workspaceType;
}

public struct TestContext {
  public WorkspaceType workspaceType { get; set; }
  public ZipReader zipArchive { get; set; }
}

public enum WorkspaceType {
  Javascript,
  CSharp
}

public abstract class WorkspaceTest {
  private readonly List<string> warnings = [];
  public bool enabled { get; private set; }

  public void enable() => this.enabled = true;
  public void disable() => this.enabled = false;

  public (bool passed, IReadOnlyList<string> warnings) validateAndWarn(TestContext context) {
    var passed = this.validate(context);
    var result = (passed, this.warnings.ToList());
    this.warnings.Clear();
    return result;
  }

  protected abstract bool validate(TestContext context);
  protected void addWarning(string warning) => this.warnings.Add(warning);

  protected IEnumerable<T> getFieldsForWorkspace<T>(TestContext context, params T[] fields) {
    // would have to look into it more, but reflection doesn't work the same with generic types, i.e. the attributes
    // aren't attached as expected, so we have to get all fields in the class then filter based on that
    var classFields = this.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                               BindingFlags.Instance);
    return fields.Where(field => {
      var fieldInfo = classFields.FirstOrDefault(f => {
        var value = f.IsStatic ? f.GetValue(null) : f.GetValue(this);
        return value != null && value.Equals(field);
      });

      var attribute = fieldInfo.GetCustomAttribute<AppliedWorkspaceAttribute>();
      return attribute != null && attribute.workspaceType == context.workspaceType;
    });
  }
}