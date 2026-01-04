using System.Data;

namespace DbScriptReader
{
    /// <summary>
    /// Implementing class can use <see cref="DbScriptFileAttribute"/> on its partial methods.
    /// </summary>
    public interface IDbScriptReader
    {
        /// <summary>
        /// Base directory to resolve scripts paths from.
        /// </summary>
        /// <returns>
        /// Path to scripts root directory or null if
        /// scripts are placed alongside the assembly.
        /// </returns>
        string? GetDirectory();

        /// <summary>
        /// Returns current database connection to run queries against.
        /// </summary>
        /// <returns>
        /// Current database connection and flag indicating
        /// whether to dispose of this connection instance.
        /// </returns>
        (IDbConnection Connection, bool Dispose) GetConnection();
    }
}
