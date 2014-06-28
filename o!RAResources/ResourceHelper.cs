using System.IO;
using System.Reflection;

namespace oRAResources
{
    public class ResourceHelper
    {
        /// <summary>
        /// Provides the stream of an internal resource.
        /// Example: en.png -> assembly.en.png
        /// </summary>
        /// <param name="resourceName">The name of the resource</param>
        /// <returns>The resource stream</returns>
        public static Stream GetResourceStream(string resourceName)
        {
            string internalName = GetResourceName(resourceName);
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(internalName);
        }

        private static string GetResourceName(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetName().Name + ".Resources." + resourceName.Replace(" ", "_")
                                                                                 .Replace("\\", ".")
                                                                                 .Replace("/", ".");
        }

        public static string[] GetResourceNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
    }
}
