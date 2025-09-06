using System.Diagnostics.CodeAnalysis;

public class Spell
{
    required public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Rank { get; set; }
    public int Type { get; set; }
    public int Art { get; set; }
    public string OverdriveIncrement { get; set; }
    public string Overdrive { get; set; }
    public int Action { get; set; }
    public string Requirements { get; set; }

    [SetsRequiredMembers]
    public Spell(int id, string name, string description, int rank, int type, int art,
                 string overdriveIncrement, string overdrive, int action, string requirements)
    {
        Id = id;
        Name = name;
        Description = description;
        Rank = rank;
        Type = type;
        Art = art;
        OverdriveIncrement = overdriveIncrement;
        Overdrive = overdrive;
        Action = action;
        Requirements = requirements;
    }
}
