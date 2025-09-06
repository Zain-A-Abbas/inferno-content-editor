using Godot;
using System;

public partial class ClassFeatCheckbox : CheckBox
{
    private int classId;

    public ClassFeatCheckbox(int id, string name)
    {
        Text = name;
        classId = id;
    }

    public int getClassId()
    {
        return classId;
    }
}
