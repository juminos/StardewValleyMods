using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenshipRings.MigrationManager;

/// <summary>
/// Data class that holds information for the migrator.
/// </summary>
public class MigrationDataClass
{
    /// <summary>
    /// Gets or sets a map from the save name to the last vesrion used.
    /// </summary>
    public Dictionary<string, string> VersionMap { get; set; } = new();
}
