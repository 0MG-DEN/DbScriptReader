# Introduction

**DbScriptReader** is a custom Roslyn source generator. Its primary purpose is to extend classes that wrap an `IDbConnection` with methods that read external SQL script files and execute them using [Dapper](https://github.com/DapperLib/Dapper) extensions.

# Problem

When migrating a data analysis project from PostgreSQL to ClickHouse, we observed a lack of certain features, such as user-defined table-valued functions. Furthermore, ClickHouse currently does not allow multi-statement queries over its standard API. See [this](https://github.com/ClickHouse/ClickHouse/issues/61608) issue.

# Solution

To address this, we would place each SQL query in a separate file. This led to writing boilerplate code like the following:
```csharp
public abstract class DbConnectionBase
{
    protected IDbConnection Connection;

    protected int Execute(string query)
        => Connection.Execute(query);

    protected IEnumerable<T> Query<T>(string query)
        => Connection.Query<T>(query);
}

public class DbConnectionClickHouse : DbConnectionBase
{
    private string[] CreateTempTables()
    {
        var query = File.ReadAllText("Scripts/CreateTempTables.sql");
        return Query<string>(query).ToArray();
    }

    private void DropTempTables(string[] tables)
    {
        var text = File.ReadAllText("Scripts/DropTempTables.sql");
        var names = string.Join(", ", tables);
        var query = string.Format(text, names);
        _ = Execute(query);
    }

    private IEnumerable<DataModel1> QueryData1(string table)
    {
        var text = File.ReadAllText("Scripts/Data1.sql");
        var query = string.Format(text, table);
        return Query<DataModel1>(query);
    }

    private IEnumerable<DataModel2> QueryData2(string table)
    {
        var text = File.ReadAllText("Scripts/Data2.sql");
        var query = string.Format(text, table);
        return Query<DataModel2>(query);
    }

    // Final method that combines all the data.
    public IEnumerable<ReportModel> GetReport()
    {
        var tables = CreateTempTables();
        var data1 = QueryData1(tables[0]);
        var data2 = QueryData2(tables[1]);
        DropTempTables(tables);
        // Additional logic.
    }
}
```

# Source Generator

The implemented source generator significantly simplifies adding these methods. You only need to declare partial methods with an attribute pointing to the SQL file:
```csharp
public class DbConnectionClickHouse : DbConnectionBase, IDbScriptReader
{
    [DbScriptFile("Scripts/CreateTempTables.sql")]
    private partial string CreateTempTables();

    [DbScriptFile("Scripts/DropTempTables.sql")]
    private partial string DropTempTables(string[] tables);

    [DbScriptFile("Scripts/Data1.sql")]
    private partial string Data1(string table);

    [DbScriptFile("Scripts/Data2.sql")]
    private partial string Data2(string table);

    public string? GetDirectory()
        => default;

    public (IDbConnection Connection, bool Dispose) GetConnection()
        => (Connection, false);

    // Final method that combines all the data.
    public IEnumerable<ReportModel> GetReport()
    {
        var tables = QueryCreateTempTables<string>().ToArray();
        var data1 = QueryData1(tables[0]);
        var data2 = QueryData2(tables[1]);
        _ = ExecuteDropTempTables(tables);
        // Additional logic.
    }
}
```
Note that the actual `Query*` and `Execute*` methods (like `QueryCreateTempTables`, `QueryData1`, and `ExecuteDropTempTables`) are auto-generated. See the [template](DbScriptReader/Files/Template.txt) for a full list of such methods.

This source generator did not reach a production version in its original context, so I decided to implement it as a personal project.
