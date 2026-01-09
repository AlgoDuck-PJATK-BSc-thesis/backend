using Microsoft.Data.Sqlite;

namespace AlgoDuck.Tests.Integration.TestHost;

public static class TestDb
{
    public static SqliteConnection CreateOpenSqliteInMemory()
    {
        var connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
        connection.Open();
        return connection;
    }
}