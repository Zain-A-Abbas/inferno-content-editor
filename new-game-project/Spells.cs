using Godot;
using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public partial class Spells : MarginContainer
{
    private Button newSpellButton;
    private Button saveSpellButton;
    private Button loadSpellButton;
    private LineEdit spellNameEdit;
    private SpinBox rankSpinBox;
    private OptionButton spellTypeSelect;
    private CheckButton spellArtSelect;
    private TextEdit spellDescription;
    private LineEdit overdriveIncrementEdit;
    private LineEdit overdriveEdit;
    private OptionButton spellActionSelect;
    private LineEdit requirementsEdit;
    private Label spellId;

    private ScrollContainer spellSearchScroll;
    private VBoxContainer spellSearchVbox;

    [Export]
    private string databaseLocation;

    private bool searchingSpell = false;
    long currentSpellId = -1;

    public override void _Ready()
    {
        newSpellButton = GetNode<Button>("%NewSpellButton");
        saveSpellButton = GetNode<Button>("%SaveSpellButton");
        loadSpellButton = GetNode<Button>("%LoadSpellButton");
        spellNameEdit = GetNode<LineEdit>("%SpellNameEdit");
        rankSpinBox = GetNode<SpinBox>("%RankSpinBox");
        spellTypeSelect = GetNode<OptionButton>("%SpellTypeSelect");
        spellArtSelect = GetNode<CheckButton>("%ArtCheckButton");
        spellDescription = GetNode<TextEdit>("%SpellDescription");
        overdriveIncrementEdit = GetNode<LineEdit>("%OverdriveIncrementEdit");
        overdriveEdit = GetNode<LineEdit>("%OverdriveEdit");
        spellActionSelect = GetNode<OptionButton>("%SpellActionSelect");
        requirementsEdit = GetNode<LineEdit>("%RequirementsEdit");
        spellId = GetNode<Label>("%SpellID");
        spellSearchScroll = GetNode<ScrollContainer>("%SpellSearchScroll");
        spellSearchVbox = GetNode<VBoxContainer>("%SpellSearchVbox");

        newSpellButton.Pressed += () => PressedNewButton();
        saveSpellButton.Pressed += () => PressedSaveButton();
        loadSpellButton.Pressed += () => SwitchSpellSearchMode();
        spellNameEdit.TextChanged += (_) => { if (searchingSpell) SearchForSpells(); };
    }

    private SqliteConnection OpenConnection()
    {
        string connectionStr = "Data Source=" + databaseLocation;
        SqliteConnection connection = new SqliteConnection(connectionStr);
        connection.Open();
        return connection;
    }

    private void PressedNewButton()
    {
        spellNameEdit.Clear();
        spellDescription.Clear();
        overdriveIncrementEdit.Clear();
        overdriveEdit.Clear();
        rankSpinBox.Value = 0;
        spellTypeSelect.Select(0);
        spellArtSelect.ButtonPressed = false;
        spellActionSelect.Select(0);
        requirementsEdit.Clear();
        currentSpellId = -1;
        spellId.Text = "NEW";
    }

    private void PressedSaveButton()
    {
        if (string.IsNullOrWhiteSpace(spellNameEdit.Text))
        {
            WarningMessage("Spell name cannot be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(spellDescription.Text))
        {
            WarningMessage("Spell description cannot be empty.");
            return;
        }

        Spell spellData = new Spell(
            (int)currentSpellId,
            spellNameEdit.Text,
            spellDescription.Text,
            (int)rankSpinBox.Value,
            spellTypeSelect.Selected,
            Convert.ToInt32(spellArtSelect.ButtonPressed),
            overdriveIncrementEdit.Text,
            overdriveEdit.Text,
            spellActionSelect.Selected,
            requirementsEdit.Text
        );

        DbError error = (currentSpellId == -1)
            ? SpellRepository.AddSpell(spellData)
            : SpellRepository.UpdateSpell(spellData);

        if (error.code == 0)
            spellId.Text = error.message;
        else
            WarningMessage(error.message);
    }

    private void SwitchSpellSearchMode()
    {
        searchingSpell = !searchingSpell;
        spellSearchScroll.Visible = searchingSpell;
        spellDescription.Visible = !searchingSpell;
        spellNameEdit.Text = "";
    }

    private void SearchForSpells()
    {
        foreach (Node child in spellSearchVbox.GetChildren()) { child.QueueFree(); }

        if (string.IsNullOrWhiteSpace(spellNameEdit.Text)) return;

        using (SqliteConnection connection = OpenConnection())
        {
            string query = "SELECT id, name FROM spells WHERE spells.name LIKE @NameSearch";
            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@NameSearch", "%" + spellNameEdit.Text + "%");
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Button spellButton = new Button();
                        spellButton.SetMeta("spellId", reader.GetInt32(0));
                        spellButton.Alignment = HorizontalAlignment.Left;
                        spellButton.Text = $"{reader.GetInt32(0)} | {reader.GetString(1)}";
                        spellSearchVbox.AddChild(spellButton);
                        spellButton.Pressed += () => SelectExistingSpell(spellButton);
                    }
                }
            }
        }
    }

    private void SelectExistingSpell(Button spellButton)
    {
        currentSpellId = (int)spellButton.GetMeta("spellId");
        SwitchSpellSearchMode();

        using (SqliteConnection connection = OpenConnection())
        {
            string query = @"
            SELECT id, name, description, rank, type, art, overdrive_increment, overdrive, action, requirements
            FROM spells WHERE id = @SpellId";
            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@SpellId", currentSpellId);
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        spellId.Text = reader.GetInt32(0).ToString();
                        spellNameEdit.Text = reader.GetString(1);
                        spellDescription.Text = reader.GetString(2);
                        rankSpinBox.Value = reader.GetInt32(3);
                        spellTypeSelect.Select(reader.GetInt32(4));
                        spellArtSelect.ButtonPressed = reader.GetInt32(5) == 1;
                        overdriveIncrementEdit.Text = reader.GetString(6);
                        overdriveEdit.Text = reader.GetString(7);
                        spellActionSelect.Select(reader.GetInt32(8));
                        requirementsEdit.Text = reader.GetString(9);
                    }
                }
            }
        }
    }

    private void WarningMessage(string text)
    {
        AcceptDialog dialog = new AcceptDialog();
        dialog.ForceNative = true;
        dialog.DialogText = text;
        dialog.InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
        AddChild(dialog);
        dialog.Confirmed += () => dialog.QueueFree();
        dialog.Canceled += () => dialog.QueueFree();
        dialog.Show();
    }
}
