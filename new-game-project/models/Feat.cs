using System.Diagnostics.CodeAnalysis;
using Godot;

public class Feat
{

    public enum ActionType
    {
        NoAction = 0,
        OneAction = 1,
        TwoActions = 2,
        TacticalAction = 3,
        FreeAction = 4,
        Reaction = 5,
        Varies = 6
    }

    required public int  Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public int Stamina { get; set; }
    public string Requirements { get; set; }
    public ActionType Action { get; set; }

    [SetsRequiredMembers]
    public Feat(int id, string name, string description, int level, int stamina, string requirements, int action)
    {
        Id = id;
        Name = name;
        Description = description;
        Level = level;
        Stamina = stamina;
        Requirements = requirements;
        Action = (ActionType)action;
    }
    
}