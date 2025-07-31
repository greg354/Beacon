namespace Waypoint;

public interface BeaconPlugin {
  string pluginName { get; set; }
  WorkspaceTest[] workspaceTests { get; set; }
  void onPluginLoaded();
}