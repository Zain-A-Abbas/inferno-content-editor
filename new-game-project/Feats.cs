using Godot;
using System;
using Microsoft.Data.Sqlite;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

public partial class Feats : MarginContainer
{
    private HFlowContainer classesFlowContainer;
    private Button newFeatButton;
    private Button saveFeatButton;
    private Button loadFeatButton;
    private LineEdit featNameEdit;
    private SpinBox levelSpinBox;
    private OptionButton featActionSelect;
    private SpinBox staminaSpinBox;
    private TextEdit featDescription;
    private Label featId;


    private ScrollContainer featSearchScroll;
    private VBoxContainer featSearchVbox;
    private LineEdit featRequirementsEdit;

    [Export]
    private string databaseLocation;

    private bool searchingFeat = false;

    // If -1, new feat. Otherwise, editing.
    long currentFeatId = -1;

    public override void _Ready()
    {
        classesFlowContainer = GetNode<HFlowContainer>("%ClassesFlowContainer");
        newFeatButton = GetNode<Button>("%NewFeatButton");
        saveFeatButton = GetNode<Button>("%SaveFeatButton");
        loadFeatButton = GetNode<Button>("%LoadFeatButton");
        featNameEdit = GetNode<LineEdit>("%FeatNameEdit");
        levelSpinBox = GetNode<SpinBox>("%LevelSpinBox");
        featActionSelect = GetNode<OptionButton>("%FeatActionSelect");
        staminaSpinBox = GetNode<SpinBox>("%StaminaSpinBox");
        featDescription = GetNode<TextEdit>("%FeatDescription");
        featId = GetNode<Label>("%FeatID");
        featSearchScroll = GetNode<ScrollContainer>("%FeatSearchScroll");
        featSearchVbox = GetNode<VBoxContainer>("%FeatSearchVbox");
        featRequirementsEdit = GetNode<LineEdit>("%FeatRequirementsEdit");

        newFeatButton.Pressed += () => pressedNewButton();
        saveFeatButton.Pressed += () => pressedSaveButton();
        loadFeatButton.Pressed += () => pressedLoadButton();
        featNameEdit.TextChanged += (sender) => featNameEditChanged("");

        loadDatabase();
    }

    private SqliteConnection openConnection()
    {
        string connectionStr = "Data Source=" + databaseLocation;
        SqliteConnection connection = new SqliteConnection(connectionStr);
        connection.Open();
        return connection;
    }

    private void loadDatabase()
    {
        SqliteConnection connection = openConnection();

        foreach (Node child in classesFlowContainer.GetChildren())
        {
            child.QueueFree();
        }

        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, name FROM classes";
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ClassFeatCheckbox newCheckbox = new ClassFeatCheckbox(reader.GetInt32(0), reader.GetString(1));
                    classesFlowContainer.AddChild(newCheckbox);
                }
            }
        }

        connection.Close();

    }

    private void pressedNewButton()
    {
        featNameEdit.Clear();
        featActionSelect.Select(0);
        staminaSpinBox.Value = 0;

        foreach (ClassFeatCheckbox checkbox in getClassCheckboxes())
        {
            checkbox.ButtonPressed = false;
        }

        featDescription.Clear();
        featRequirementsEdit.Clear();
        currentFeatId = -1;
        featId.Text = "NEW";

    }

    private void pressedSaveButton()
    {
        #region Check Valid Feat

        bool invalidFeat = false;
        string invalidMessage = "";

        if (featNameEdit.Text.Replace(" ", "") == "")
        {
            invalidFeat = true;
            invalidMessage = "Empty name.";
        }
        if (featDescription.Text.Replace(" ", "").Replace("\n", "") == "")
        {
            invalidFeat = true;
            invalidMessage = "Empty description.";
        }

        if (invalidFeat)
        {
            warningMessage(invalidMessage);
            return;
        }

        #endregion

        DbError error;

        if (currentFeatId == -1)
        {
            Feat newFeat = new Feat(-1, featNameEdit.Text, featDescription.Text, (int)levelSpinBox.Value, (int)staminaSpinBox.Value, featRequirementsEdit.Text, featActionSelect.Selected);
            DbError featInsert = FeatRepository.addFeat(newFeat, getSelectedClasses());
            error = featInsert;

        }
        else
        {
            Feat updatedFeat = new Feat((int)currentFeatId, featNameEdit.Text, featDescription.Text, (int)levelSpinBox.Value, (int)staminaSpinBox.Value, featRequirementsEdit.Text, featActionSelect.Selected);
            DbError featUpdate = FeatRepository.updateFeat(updatedFeat, getSelectedClasses());
            error = featUpdate;
        }


        if (error.code == 0)
        {
            // Success; get the new/updated feat's ID
            featId.Text = error.message;
        }
        else
        {
            warningMessage(error.message);
        }

    }


    private List<int> getSelectedClasses()
    {
        List<int> classes = [];

        foreach (ClassFeatCheckbox checkbox in getClassCheckboxes())
        {
            if (checkbox.ButtonPressed)
            {            
                classes.Add(checkbox.getClassId());
            }
        }


        return classes;
    }

    private void warningMessage(string warningText)
    {
        AcceptDialog invalidDialog = new AcceptDialog();
        invalidDialog.ForceNative = true;
        invalidDialog.DialogText = warningText;
        invalidDialog.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
        AddChild(invalidDialog);
        invalidDialog.Confirmed += () => invalidDialog.QueueFree();
        invalidDialog.Canceled += () => invalidDialog.QueueFree();
        invalidDialog.Show();
    }

    private void pressedLoadButton()
    {

        switchFeatSearchMode();

    }

    private void switchFeatSearchMode()
    {
        searchingFeat = !searchingFeat;

        levelSpinBox.Editable = !searchingFeat;
        featActionSelect.Disabled = searchingFeat;
        staminaSpinBox.Editable = !searchingFeat;
        featDescription.Visible = !searchingFeat;
        featSearchScroll.Visible = searchingFeat;

        featNameEdit.Text = "";
    }

    private void featNameEditChanged(string newText)
    {
        if (!searchingFeat) { return; }
        searchForFeats();
    }

    private void searchForFeats()
    {
        foreach (Node child in featSearchVbox.GetChildren()) { child.QueueFree(); }

        if (featNameEdit.Text == "") { return; }

        using (SqliteConnection connection = openConnection())
        {
            string featSearchQuery = "SELECT id, name FROM feats WHERE feats.name LIKE @NameSearch";
            using (var searchCommand = new SqliteCommand(featSearchQuery, connection))
            {
                searchCommand.Parameters.AddWithValue("@NameSearch", "%" + featNameEdit.Text + "%");
                using (SqliteDataReader reader = searchCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Button featButton = new Button();
                        featButton.SetMeta("buttonFeatId", reader.GetInt32(0));
                        featButton.Alignment = HorizontalAlignment.Left;
                        featButton.Text = $"{reader.GetInt32(0)} | {reader.GetString(1)}";
                        featSearchVbox.AddChild(featButton);
                        featButton.Pressed += () => selectExistingFeat(featButton);
                    }
                }
            }
            connection.Close();
        }
    }

    private void selectExistingFeat(Button buttonFeat)
    {
        currentFeatId = (int)buttonFeat.GetMeta("buttonFeatId");
        switchFeatSearchMode();

        using (SqliteConnection connection = openConnection())
        {
            string featSearchQuery = "SELECT id, name, description, level, stamina, actions FROM feats WHERE feats.id = @LoadedFeatId";
            using (var searchCommand = new SqliteCommand(featSearchQuery, connection))
            {
                searchCommand.Parameters.AddWithValue("@LoadedFeatId", currentFeatId);
                using (SqliteDataReader reader = searchCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        featId.Text = reader.GetInt32(0).ToString();
                        featNameEdit.Text = reader.GetString(1);
                        levelSpinBox.Value = reader.GetInt32(3);
                        staminaSpinBox.Value = reader.GetInt32(4);
                        featActionSelect.Select(reader.GetInt32(5));
                        featDescription.Text = reader.GetString(2);
                        featRequirementsEdit.Text = reader.GetString(6);

                        foreach (ClassFeatCheckbox checkbox in getClassCheckboxes())
                        {
                            checkbox.ButtonPressed = false;
                        }

                        string featClassSearchQuery = "SELECT feat_id, class_id FROM feat_classes WHERE feat_classes.feat_id = @LoadedFeatId";
                        using (var classSearchCommand = new SqliteCommand(featClassSearchQuery, connection))
                        {
                            classSearchCommand.Parameters.AddWithValue("@LoadedFeatId", currentFeatId);
                            using (SqliteDataReader classReader = classSearchCommand.ExecuteReader())
                            {
                                while (classReader.Read())
                                {
                                    foreach (ClassFeatCheckbox checkbox in getClassCheckboxes())
                                    {
                                        if (checkbox.getClassId() == classReader.GetInt32(1))
                                        {
                                            checkbox.ButtonPressed = true;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }

            connection.Close();
        }

    }

    private List<ClassFeatCheckbox> getClassCheckboxes()
    {
        List<ClassFeatCheckbox> checkboxes = [];
        foreach (Node child in classesFlowContainer.GetChildren())
        {
            if (child is ClassFeatCheckbox checkbox)
            {
                checkboxes.Add(checkbox);
            }
        }
        return checkboxes;
    }

}
