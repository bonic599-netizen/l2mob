using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SkillNameTable
{
    private static SkillNameTable _instance;
    private static Dictionary<int, Dictionary<int, SkillNameData>> _names;
    public Dictionary<int, Dictionary<int, SkillNameData>> Names { get { return _names; } }
    public static SkillNameTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SkillNameTable();
            }

            return _instance;
        }
    }

    public void Initialize()
    {
        _names = new Dictionary<int, Dictionary<int, SkillNameData>>();
        ParseSkillNameGrps();
    }

    public void ClearTable()
    {
        _names.Clear();
        _names = null;
        _instance = null;
    }

    public SkillNameData GetName(int id, int level)
    {
        if (_names == null) _names = new Dictionary<int, Dictionary<int, SkillNameData>>();

        if (_names.TryGetValue(id, out Dictionary<int, SkillNameData> skillLevel) && skillLevel != null)
        {
            if (skillLevel.TryGetValue(level, out SkillNameData skillName) && skillName != null)
            {
                return skillName;
            }
            foreach (var kvp in skillLevel)
            {
                if (kvp.Value != null) return kvp.Value;
            }
        }

        SkillNameData fallback = new SkillNameData
        {
            Id = id,
            Level = level > 0 ? level : 1,
            SubLevel = 0,
            Name = $"Skill #{id}",
            Desc = $"Hellsing Skill #{id} (Lv. {level})",
            DescParams = Array.Empty<string>()
        };
        TryAdd(fallback);
        return fallback;
    }

    public Dictionary<int, SkillNameData> GetNames(int id)
    {
        if (_names == null) _names = new Dictionary<int, Dictionary<int, SkillNameData>>();
        if (!_names.TryGetValue(id, out Dictionary<int, SkillNameData> skillLevel) || skillLevel == null || skillLevel.Count == 0)
        {
            GetName(id, 1);
            _names.TryGetValue(id, out skillLevel);
        }

        return skillLevel;
    }

    // public string GetDescription(int id)
    // {
    //     return GetDescription(id, 1);
    // }

    // public string GetDescription(int id, int level)
    // {
    //     _names.TryGetValue(id, out Dictionary<int, SkillNameData> skillLevel);
    //     if (skillLevel == null) return null;

    //     bool found = skillLevel.TryGetValue(level, out SkillNameData skillName);
    //     if (found)
    //     {
    //         return skillName.Desc;
    //     }

    //     string desc = string.Empty;
    //     foreach (KeyValuePair<int, SkillNameData> lvl in skillLevel)
    //     {
    //         if (!string.IsNullOrEmpty(lvl.Value.Desc))
    //         {
    //             desc = lvl.Value.Desc;
    //             break;
    //         }
    //     }
    //     return desc;
    // }

    private void ParseSkillNameGrps()
    {

        string dataPath = Path.Combine(Application.streamingAssetsPath, "Data/Meta/SkillName_Classic-eu.txt");

        if (!File.Exists(dataPath))
        {
            Debug.LogWarning("File not found: " + dataPath);
            return;
        }

        using (StreamReader reader = new StreamReader(dataPath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                SkillNameData nameData = new SkillNameData();

                string[] keyvals = line.Split('\t');

                for (int i = 0; i < keyvals.Length; i++)
                {
                    if (!keyvals[i].Contains("="))
                    {
                        continue;
                    }

                    string[] keyval = keyvals[i].Split("=");
                    string key = keyval[0];
                    string value = keyval[1];

                    switch (key)
                    {
                        case "skill_id":
                            nameData.Id = int.Parse(value);
                            break;
                        case "skill_level":
                            nameData.Level = int.Parse(value);
                            break;
                        case "skill_sublevel":
                            nameData.SubLevel = int.Parse(value);
                            break;
                        case "name":
                            nameData.Name = DatUtils.CleanupString(value);
                            break;
                        case "desc":
                            nameData.Desc = DatUtils.CleanupString(value);
                            break;
                        case "prev_skill_id":
                            nameData.PrevSkillId = int.Parse(value);
                            break;
                        case "desc_param":
                            nameData.DescParams = DatUtils.CleanupString(value).Split(';');
                            break;
                    }
                }

                if (!SkillTable.Instance.ShouldLoadSkill(nameData.Id))
                {
                    continue;
                }

                TryAdd(nameData);
            }

            Debug.Log($"Successfully imported {_names.Count} actionName(s)");
        }
    }

    private void TryAdd(SkillNameData skillName)
    {

        if (_names.ContainsKey(skillName.Id) == true)
        {
            Dictionary<int, SkillNameData> dataGrp = _names[skillName.Id];
            dataGrp.TryAdd(skillName.Level, skillName);
        }
        else
        {
            Dictionary<int, SkillNameData> dataGrp = CreateDict();
            AddDict(dataGrp, skillName);
            _names.TryAdd(skillName.Id, dataGrp);
        }
    }

    private Dictionary<int, SkillNameData> CreateDict()
    {
        return new Dictionary<int, SkillNameData>();
    }

    private void AddDict(Dictionary<int, SkillNameData> dataGrp, SkillNameData skillName)
    {
        dataGrp.TryAdd(skillName.Level, skillName);
    }
}
