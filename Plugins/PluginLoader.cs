using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Waypoint;

namespace Beacon.Plugins;

internal static class PluginLoader {
  public static List<BeaconPlugin> loadPlugins(string pluginPath) {
    var plugins = new List<BeaconPlugin>();
    if (!Directory.Exists(pluginPath)) return plugins;

    var pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
    foreach (var pluginFile in pluginFiles) {
      var pluginAssembly = Assembly.LoadFile(pluginFile);
      foreach (var type in pluginAssembly.GetTypes()) {
        if (!typeof(BeaconPlugin).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface) continue;
        var beaconPlugin = (BeaconPlugin)Activator.CreateInstance(type);
        plugins.Add(beaconPlugin);
        beaconPlugin.onPluginLoaded();
      }
    }

    return plugins;
  }
}