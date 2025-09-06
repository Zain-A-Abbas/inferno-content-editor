using Microsoft.Data.Sqlite;

public struct DbError
{
    public int code;
    public string message;
}

public static class DbUtility
{

    const string DATABASE_LOCATION = "C:/Users/Zain/Documents/Inferno TTRPG/database/inferno.db";

    public static SqliteConnection openConnection()
    {
        string connectionStr = "Data Source=" + DATABASE_LOCATION;
        SqliteConnection connection = new SqliteConnection(connectionStr);
        connection.Open();
        return connection;

    }
    public static DbError getError(int code, string message)
    {
        DbError newError = new DbError();
        newError.code = code;
        newError.message = message;
        return newError;
    }
}