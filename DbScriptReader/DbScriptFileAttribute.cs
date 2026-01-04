namespace DbScriptReader
{
    /// <summary>
    /// This attribute marks methods that should be extended with additional overloads.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DbScriptFileAttribute : Attribute
    {
        public readonly string Path;

        /// <summary>
        /// Create new instance of attribute with specific path.
        /// </summary>
        /// <param name="path">Relative path to a script file.</param>
        public DbScriptFileAttribute(string path)
        {
            Path = path;
        }
    }
}
