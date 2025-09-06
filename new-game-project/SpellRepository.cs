using Microsoft.Data.Sqlite;

public static class SpellRepository
{
    public static DbError AddSpell(Spell newSpell)
    {
        using (SqliteConnection connection = DbUtility.openConnection())
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string query = @"
                    INSERT INTO spells
                    (name, description, rank, type, art, overdrive_increment, overdrive, action, requirements)
                    VALUES
                    (@Name, @Description, @Rank, @Type, @Art, @OverdriveIncrement, @Overdrive, @Action, @Requirements)";
                    
                    var cmd = new SqliteCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Name", newSpell.Name);
                    cmd.Parameters.AddWithValue("@Description", newSpell.Description);
                    cmd.Parameters.AddWithValue("@Rank", newSpell.Rank);
                    cmd.Parameters.AddWithValue("@Type", newSpell.Type);
                    cmd.Parameters.AddWithValue("@Art", newSpell.Art);
                    cmd.Parameters.AddWithValue("@OverdriveIncrement", newSpell.OverdriveIncrement);
                    cmd.Parameters.AddWithValue("@Overdrive", newSpell.Overdrive);
                    cmd.Parameters.AddWithValue("@Action", newSpell.Action);
                    cmd.Parameters.AddWithValue("@Requirements", newSpell.Requirements);
                    cmd.ExecuteNonQuery();

                    long spellId = (long)new SqliteCommand("SELECT last_insert_rowid();", connection, transaction).ExecuteScalar();
                    transaction.Commit();
                    return DbUtility.getError(0, spellId.ToString());
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    return DbUtility.getError(1, ex.Message);
                }
            }
        }
    }

    public static DbError UpdateSpell(Spell updatedSpell)
    {
        using (SqliteConnection connection = DbUtility.openConnection())
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string query = @"
                    UPDATE spells
                    SET name=@Name, description=@Description, rank=@Rank, type=@Type, art=@Art,
                        overdrive_increment=@OverdriveIncrement, overdrive=@Overdrive,
                        action=@Action, requirements=@Requirements
                    WHERE id=@Id";

                    var cmd = new SqliteCommand(query, connection, transaction);
                    cmd.Parameters.AddWithValue("@Name", updatedSpell.Name);
                    cmd.Parameters.AddWithValue("@Description", updatedSpell.Description);
                    cmd.Parameters.AddWithValue("@Rank", updatedSpell.Rank);
                    cmd.Parameters.AddWithValue("@Type", updatedSpell.Type);
                    cmd.Parameters.AddWithValue("@Art", updatedSpell.Art);
                    cmd.Parameters.AddWithValue("@OverdriveIncrement", updatedSpell.OverdriveIncrement);
                    cmd.Parameters.AddWithValue("@Overdrive", updatedSpell.Overdrive);
                    cmd.Parameters.AddWithValue("@Action", updatedSpell.Action);
                    cmd.Parameters.AddWithValue("@Requirements", updatedSpell.Requirements);
                    cmd.Parameters.AddWithValue("@Id", updatedSpell.Id);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    return DbUtility.getError(0, "");
                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    return DbUtility.getError(1, ex.Message);
                }
            }
        }
    }
}
