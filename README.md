# Introduction

**DbScriptReader** is an attempt at writing a custom Roslyn source generator.
The main purpose of this generator is to extend classes that wrap some `IDbConnection`
with methods that would read external script files and execute them via `Dapper` extensions.

# Problem

When migrating a data analysis project from PostgreSQL to ClickHouse we observed a lack
of some features, i.e. user defined table valued functions. Furthermore currenlty ClickHouse
does [not allow](https://github.com/ClickHouse/ClickHouse/issues/61608) multiqueries over its API.

# Solution

To mitigate the problems above we would place each SQL query in a
separate file and write something like the following methods:
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

    private IEnumrable<DataModel1> QueryData1(string table)
    {
        var text = File.ReadAllText("Scripts/Data1.sql");
        var query = string.Format(text, name);
        return Query<DataModel1>(query);
    }

    private IEnumrable<DataModel2> QueryData2(string table)
    {
        var text = File.ReadAllText("Scripts/Data2.sql");
        var query = string.Format(text, name);
        return Query<DataModel2>(query);
    }

    // Final method that would combine all the data.
    public IEnumerable<ReportModel> GetReport()
    {
        var tables = CreateTempTables();
        var data1 = QueryData1(tables[0]);
        var data2 = QueryData2(tables[1]);
        DropTempTables(tables);
        // ...
    }
}
```

# Source Generator

Implemented source generator would simplify adding these methods as following:
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

    // Final method that would combine all the data.
    public IEnumerable<ReportModel> GetReport()
    {
        var tables = QueryCreateTempTables<string>().ToArray();
        var data1 = QueryData1(tables[0]);
        var data2 = QueryData2(tables[1]);
        _ = ExecuteDropTempTables(tables);
        // ...
    }
}
```
Note that `Query*` and `Execute*` methods are auto-generated.\
See [template](DbScriptReader/Files/Template.txt) for a full list of such methods.

Unfortunately this source generator didn't get a production
version so I decided to implement it as a pet project.
