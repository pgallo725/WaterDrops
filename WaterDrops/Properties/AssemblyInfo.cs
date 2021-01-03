using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Le informazioni generali relative a un assembly sono controllate dal seguente 
// set di attributi. Modificare i valori di questi attributi per modificare le informazioni
// associate a un assembly.
[assembly: AssemblyTitle("WaterDrops")]
[assembly: AssemblyDescription("January 3rd, 2021")]   // Use this to specify release date
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Paolo Gallo")]
[assembly: AssemblyProduct("WaterDrops")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Le informazioni sulla versione di un assembly sono costituite dai seguenti quattro valori:
//
//      Versione principale
//      Versione secondaria 
//      Numero di build
//      Revisione
//
// È possibile specificare tutti i valori oppure impostare valori predefiniti per i numeri relativi alla revisione e alla build 
// usando l'asterisco '*' come illustrato di seguito:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.3.0")]
[assembly: AssemblyFileVersion("1.0.3.0")]
[assembly: ComVisible(false)]


public static class AssemblyInfo
{
    /// <summary>
    /// Utility method for retrieving assembly attributes
    /// </summary>
    /// <typeparam name="T">The type of the attribute that has to be returned.</typeparam>
    /// <param name="assembly">The assembly from which the attribute has to be retrieved.</param>
    /// <returns>The requested assembly attribute value (or null)</returns>
    public static T GetAttribute<T>(Assembly assembly)
        where T : Attribute
    {
        // Get attributes of the required type
        object[] attributes = assembly.GetCustomAttributes(typeof(T), true);

        // If we didn't get anything, return null
        if ((attributes == null) || (attributes.Length == 0))
            return null;

        // Convert the first attribute value
        // into the desired type and return it
        return (T)attributes[0];
    }
}