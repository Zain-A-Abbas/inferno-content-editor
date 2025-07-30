using Godot;
using System;
using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;

public partial class Orders : MarginContainer
{
    [Export]
    private string databaseLocation;

    private SqliteConnection connection;

    public override void _Ready()
    {
        loadDatabase();
    }

    private void loadDatabase()
    {
        string connectionStr = "Data Source=" + databaseLocation;
        connection = new SqliteConnection(connectionStr);
        connection.Open();

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = "SELECT classes.name FROM classes JOIN orders ON orders.id = classes.id";
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    GD.Print(reader.GetString(0));
                }
            }
        }
        
    }

}
