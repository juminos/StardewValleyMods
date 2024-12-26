using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrenshipRings.ConstantsAndEnums;

/// <summary>
/// Static class that contains common error messages.
/// </summary>
public static class ErrorMessageConsts
{
    /// <summary>
    /// The error message string used for a crash that happens while applying harmony patches.
    /// </summary>
    public const string HARMONYCRASH = "Mod crashed while applying harmony patches:\n\n{0}";

    /// <summary>
    /// The message string used to suppress StyleCop ordering warnings for records.
    /// </summary>
    public const string STYLECOPRECORDS = "Records break StyleCop :(.";
}
