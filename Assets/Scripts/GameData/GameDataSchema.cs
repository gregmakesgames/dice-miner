using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameDataSchemaRoot
{
    public List<GameDataConfigTypeDef> configTypes = new();
}

[Serializable]
public sealed class GameDataConfigTypeDef
{
    public string name = string.Empty;
    public List<GameDataFieldDef> fields = new();
}

[Serializable]
public sealed class GameDataFieldDef
{
    public string name = string.Empty;
    public GameDataFieldType type = GameDataFieldType.String;
    public string refType = string.Empty;
}

public enum GameDataFieldType
{
    Int,
    Float,
    Bool,
    String,
    Vector2,
    Vector3,
    Color,
    Ref,
    Sprite,
    Mesh,
    Prefab
}
