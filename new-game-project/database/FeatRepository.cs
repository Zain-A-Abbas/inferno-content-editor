using System.Collections.Generic;
using System.Linq;
using Godot;
using Microsoft.Data.Sqlite;

public static class FeatRepository
{
    public static DbError addFeat(Feat newFeat, List<int> selectedClasses)
    {
        using (SqliteConnection connection = DbUtility.openConnection())
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string newFeatQuery = @"
                    INSERT INTO feats 
                    (name, description, level, stamina, requirements, actions)
                    VALUES
                    (@Name, @Description, @Level, @Stamina, @Requirements, @Actions)
                    ";

                    var insertCommand = new SqliteCommand(newFeatQuery, connection, transaction);
                    insertCommand.Parameters.AddWithValue("@Name", newFeat.Name);
                    insertCommand.Parameters.AddWithValue("@Description", newFeat.Description);
                    insertCommand.Parameters.AddWithValue("@Level", newFeat.Level);
                    insertCommand.Parameters.AddWithValue("@Stamina", newFeat.Stamina);
                    insertCommand.Parameters.AddWithValue("@Requirements", newFeat.Requirements);
                    insertCommand.Parameters.AddWithValue("@Actions", newFeat.Action);
                    insertCommand.ExecuteNonQuery();

                    long dbFeatId = -1;
                    var idCommand = new SqliteCommand("SELECT last_insert_rowid();", connection, transaction);
                    dbFeatId = (long)idCommand.ExecuteScalar();

                    var classInsertCommand = new SqliteCommand(@"
                    INSERT INTO feat_classes
                    (feat_id, class_id)
                    VALUES
                    (@FeatId, @ClassId)
                    ", connection, transaction);

                    var featIdParam = classInsertCommand.Parameters.Add("@FeatId", SqliteType.Integer);
                    var classIdParam = classInsertCommand.Parameters.Add("@ClassId", SqliteType.Integer);

                    foreach (int classId in selectedClasses)
                    {
                        featIdParam.Value = dbFeatId;
                        classIdParam.Value = classId;
                        classInsertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    return DbUtility.getError(0, dbFeatId.ToString());

                }
                catch (SqliteException ex)
                {
                    transaction.Rollback();
                    return DbUtility.getError(1, ex.Message);
                }

            }

        }
    }

    public static DbError updateFeat(Feat updatedFeat, List<int> selectedClasses)
    {
        using (SqliteConnection connection = DbUtility.openConnection())
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string updateFeatQuery = "UPDATE feats SET name = @Name, description = @Description, level = @Level, stamina = @Stamina, requirements = @Requirements, actions = @Actions WHERE id = @UpdateFeatId";
                    var updateCommand = new SqliteCommand(updateFeatQuery, connection, transaction);
                    updateCommand.Parameters.AddWithValue("@Name", updatedFeat.Name);
                    updateCommand.Parameters.AddWithValue("@Description", updatedFeat.Description);
                    updateCommand.Parameters.AddWithValue("@Level", updatedFeat.Level);
                    updateCommand.Parameters.AddWithValue("@Stamina", updatedFeat.Stamina);
                    updateCommand.Parameters.AddWithValue("@Requirements", updatedFeat.Requirements);
                    updateCommand.Parameters.AddWithValue("@Actions", updatedFeat.Action);
                    updateCommand.Parameters.AddWithValue("@UpdateFeatId", updatedFeat.Id);

                    updateCommand.ExecuteNonQuery();
                    
                    if (selectedClasses.Any())
                    {
                        string deleteOldLinks = @"
                        DELETE FROM feat_classes
                        WHERE feat_id = @FeatId
                        AND class_id NOT IN (" + string.Join(",", selectedClasses) + @");
                        ";
                        using (var deleteCommand = new SqliteCommand(deleteOldLinks, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@FeatId", updatedFeat.Id);
                            deleteCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string deleteAll = "DELETE FROM feat_classes WHERE feat_id = @FeatId";
                        using (var deleteCommand = new SqliteCommand(deleteAll, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@FeatId", updatedFeat.Id);
                            deleteCommand.ExecuteNonQuery();
                        }
                    }

                    string insertIfNotExists = @"
                    INSERT INTO feat_classes (feat_id, class_id)
                    SELECT @FeatId, @ClassId
                    WHERE NOT EXISTS (
                        SELECT 1 FROM feat_classes
                        WHERE feat_id = @FeatId AND class_id = @ClassId
                    );
                    ";

                    using (var insertCommand = new SqliteCommand(insertIfNotExists, connection, transaction))
                    {
                        var featIdParam = insertCommand.Parameters.Add("@FeatId", SqliteType.Integer);
                        var classIdParam = insertCommand.Parameters.Add("@ClassId", SqliteType.Integer);

                        foreach (int classId in selectedClasses)
                        {
                            featIdParam.Value = updatedFeat.Id;
                            classIdParam.Value = classId;
                            insertCommand.ExecuteNonQuery();
                        }
                    }


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