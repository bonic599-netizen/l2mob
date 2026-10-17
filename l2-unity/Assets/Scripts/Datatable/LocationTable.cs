using System;
using System.Collections.Generic;
using UnityEngine;

public enum LocationCategory
{
    TownsAndVillages = 1,
    CastlesAndTerritories = 2,
    StartingLocations = 3,
    LocationsGludioGludin = 4,
    LocationsDionFloran = 5,
    LocationsGiran = 6,
    LocationsOrenHunters = 7,
    LocationsAden = 8,
    LocationsGoddard = 9,
    LocationsRune = 10,
    LocationsSchuttgart = 11,
    CatacombsAndNecropolises = 12,
    DungeonsAndSpecialZones = 13,
    IslandsAndHarbors = 14
}

public class LocationPoint
{
    public string Name { get; set; }
    public LocationCategory Category { get; set; }
    public string CategoryTitle { get; set; }
    public Vector3 L2Pos { get; set; }
    public Vector3 UnityPos { get; set; }
    public float Radius { get; set; }
    public bool IsTown { get; set; }

    public LocationPoint(string name, LocationCategory category, string categoryTitle, float l2x, float l2y, float l2z, float radius = 8000f, bool isTown = false)
    {
        Name = name;
        Category = category;
        CategoryTitle = categoryTitle;
        L2Pos = new Vector3(l2x, l2y, l2z);
        UnityPos = new Vector3(l2y / 52.5f, l2z / 52.5f, l2x / 52.5f);
        Radius = radius;
        IsTown = isTown;
    }
}

public class LocationTable
{
    private static LocationTable _instance;
    public static LocationTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LocationTable();
                _instance.Initialize();
            }
            return _instance;
        }
    }

    private readonly List<LocationPoint> _locations = new List<LocationPoint>();
    private readonly Dictionary<LocationCategory, List<LocationPoint>> _byCategory = new Dictionary<LocationCategory, List<LocationPoint>>();

    public void Initialize()
    {
        if (_locations.Count > 0) return;

        AddCategory1_Towns();
        AddCategory2_Castles();
        AddCategory3_Starting();
        AddCategory4_GludioGludin();
        AddCategory5_DionFloran();
        AddCategory6_Giran();
        AddCategory7_OrenHunters();
        AddCategory8_Aden();
        AddCategory9_Goddard();
        AddCategory10_Rune();
        AddCategory11_Schuttgart();
        AddCategory12_Catacombs();
        AddCategory13_Dungeons();
        AddCategory14_Islands();

        foreach (var loc in _locations)
        {
            if (!_byCategory.ContainsKey(loc.Category))
            {
                _byCategory[loc.Category] = new List<LocationPoint>();
            }
            _byCategory[loc.Category].Add(loc);
        }
    }

    private void AddLoc(string name, LocationCategory cat, string catTitle, float x, float y, float z, float radius = 8000f, bool isTown = false)
    {
        _locations.Add(new LocationPoint(name, cat, catTitle, x, y, z, radius, isTown));
    }

    private void AddCategory1_Towns()
    {
        const LocationCategory C = LocationCategory.TownsAndVillages;
        const string T = "Города и деревни";
        AddLoc("Talking Island Village", C, T, -84168f, 244510f, -3720f, 15000f, true);
        AddLoc("Elven Village", C, T, 46927f, 51465f, -2977f, 15000f, true);
        AddLoc("Dark Elf Village", C, T, 9745f, 15606f, -4574f, 15000f, true);
        AddLoc("Dwarven Village", C, T, 115113f, -178212f, -880f, 15000f, true);
        AddLoc("Orc Village", C, T, -44836f, -112524f, -235f, 15000f, true);
        AddLoc("Gludin Village", C, T, -80826f, 149775f, -3040f, 15000f, true);
        AddLoc("Town of Gludio", C, T, -12672f, 122776f, -3116f, 18000f, true);
        AddLoc("Town of Dion", C, T, 18748f, 145437f, -3120f, 18000f, true);
        AddLoc("Floran Village", C, T, 17144f, 170156f, -3496f, 12000f, true);
        AddLoc("Giran Castle Town", C, T, 83400f, 147943f, -3400f, 20000f, true);
        AddLoc("Heine", C, T, 111409f, 219364f, -3545f, 18000f, true);
        AddLoc("Town of Oren", C, T, 82956f, 53162f, -1490f, 18000f, true);
        AddLoc("Town of Aden", C, T, 147450f, 26741f, -2200f, 22000f, true);
        AddLoc("Hunters Village", C, T, 116819f, 76994f, -2714f, 15000f, true);
        AddLoc("Town of Goddard", C, T, 147928f, -55273f, -2728f, 20000f, true);
        AddLoc("Rune Township", C, T, 43799f, -47727f, -792f, 20000f, true);
        AddLoc("Town of Schuttgart", C, T, 87386f, -143246f, -1290f, 20000f, true);
        AddLoc("Ketra Orc Village", C, T, 138000f, -56000f, -3300f, 12000f, true);
        AddLoc("Varka Silenos Village", C, T, 113000f, -45000f, -3300f, 12000f, true);
    }

    private void AddCategory2_Castles()
    {
        const LocationCategory C = LocationCategory.CastlesAndTerritories;
        const string T = "Замки и территории";
        AddLoc("Gludio Castle", C, T, -18135f, 110295f, -2630f, 10000f);
        AddLoc("Dion Castle", C, T, 22150f, 160100f, -3100f, 10000f);
        AddLoc("Giran Castle", C, T, 76500f, 153000f, -2800f, 10000f);
        AddLoc("Oren Castle", C, T, 81800f, 37500f, -2200f, 10000f);
        AddLoc("Aden Castle", C, T, 147450f, 6500f, -1500f, 12000f);
        AddLoc("Innadril Castle", C, T, 115000f, 248000f, -1200f, 10000f);
        AddLoc("Goddard Castle", C, T, 147000f, -42000f, -2100f, 10000f);
        AddLoc("Rune Castle", C, T, 33000f, -50000f, 100f, 10000f);
        AddLoc("Schuttgart Castle", C, T, 77000f, -152000f, -500f, 10000f);
        AddLoc("Gludio Territory", C, T, -35000f, 130000f, -3000f, 40000f);
        AddLoc("Dion Territory", C, T, 20000f, 155000f, -3000f, 40000f);
        AddLoc("Giran Territory", C, T, 90000f, 140000f, -3200f, 40000f);
        AddLoc("Oren Territory", C, T, 80000f, 50000f, -2000f, 40000f);
        AddLoc("Aden Territory", C, T, 150000f, 20000f, -2000f, 45000f);
        AddLoc("Innadril Territory", C, T, 110000f, 220000f, -3300f, 40000f);
        AddLoc("Goddard Territory", C, T, 140000f, -50000f, -2600f, 40000f);
        AddLoc("Rune Territory", C, T, 40000f, -45000f, -1000f, 40000f);
        AddLoc("Schuttgart Territory", C, T, 85000f, -140000f, -1500f, 40000f);
    }

    private void AddCategory3_Starting()
    {
        const LocationCategory C = LocationCategory.StartingLocations;
        const string T = "Стартовые локации";
        AddLoc("Talking Island", C, T, -107000f, 237000f, -3600f, 35000f);
        AddLoc("Talking Island, Eastern Territory", C, T, -90000f, 240000f, -3600f, 20000f);
        AddLoc("Talking Island, Western Territory", C, T, -115000f, 240000f, -3600f, 20000f);
        AddLoc("Elven Forest", C, T, 47000f, 48000f, -3000f, 25000f);
        AddLoc("Dark Forest", C, T, 12000f, 16000f, -4500f, 25000f);
        AddLoc("Immortal Plateau", C, T, -45000f, -115000f, -200f, 30000f);
        AddLoc("Immortal Plateau, Northern Region", C, T, -45000f, -120000f, -200f, 20000f);
        AddLoc("Immortal Plateau, Southern Region", C, T, -45000f, -110000f, -200f, 20000f);
        AddLoc("Dwarven Territory", C, T, 115000f, -180000f, -900f, 30000f);
        AddLoc("Orc Territory", C, T, -44000f, -112000f, -200f, 30000f);
        AddLoc("Dark Elf Territory", C, T, 10000f, 16000f, -4500f, 30000f);
        AddLoc("Valley of Heroes", C, T, 110000f, 75000f, -2700f, 15000f);
    }

    private void AddCategory4_GludioGludin()
    {
        const LocationCategory C = LocationCategory.LocationsGludioGludin;
        const string T = "Локации возле Gludio и Gludin";
        AddLoc("Windmill Hill", C, T, -20000f, 130000f, -3200f);
        AddLoc("Abandoned Camp", C, T, -50000f, 140000f, -3000f);
        AddLoc("Wasteland", C, T, -30000f, 175000f, -4000f, 15000f);
        AddLoc("Ruins of Agony", C, T, -25000f, 115000f, -3500f);
        AddLoc("Ruins of Agony Bend", C, T, -23000f, 118000f, -3500f);
        AddLoc("Ruins of Despair", C, T, -15000f, 140000f, -3600f);
        AddLoc("Execution Grounds", C, T, 50000f, 145000f, -3500f);
        AddLoc("Windawood Manor", C, T, -38000f, 165000f, -3600f);
        AddLoc("Red Rock Ridge", C, T, -32000f, 155000f, -3500f);
        AddLoc("Fellmere Harvesting Grounds", C, T, -85000f, 125000f, -3200f);
        AddLoc("Fellmere Lake", C, T, -85000f, 118000f, -3500f);
        AddLoc("Langk Lizardmen Dwellings", C, T, -65000f, 185000f, -3700f);
        AddLoc("Maille Lizardmen Barracks", C, T, -35000f, 110000f, -3500f);
        AddLoc("Ol Mahum Checkpoint", C, T, -25000f, 150000f, -3500f);
        AddLoc("Orc Barracks", C, T, -85000f, 105000f, -3600f);
        AddLoc("Partisan's Hideaway", C, T, 45000f, 110000f, -3800f);
        AddLoc("Ant Nest", C, T, -15000f, 175000f, -4200f);
        AddLoc("Ant Incubator", C, T, -21000f, 182000f, -5700f);
        AddLoc("Fortress of Resistance", C, T, 52000f, 110000f, -3800f);
        AddLoc("Neutral Zone", C, T, 30000f, 75000f, -3500f);
        AddLoc("Black Rock Hill", C, T, 15000f, 65000f, -4000f);
        AddLoc("Swampland", C, T, 12000f, 30000f, -4000f);
        AddLoc("Evil Hunting Grounds", C, T, 25000f, 20000f, -4200f);
        AddLoc("Spine Mountains", C, T, 60000f, 30000f, -3500f);
        AddLoc("Abandoned Coal Mines", C, T, 115000f, -170000f, -1100f);
        AddLoc("Western Mining Zone", C, T, 110000f, -175000f, -900f);
        AddLoc("Eastern Mining Zone", C, T, 120000f, -175000f, -900f);
        AddLoc("Strip Mine", C, T, 118000f, -182000f, -900f);
        AddLoc("Mining Zone Passage", C, T, 116000f, -177000f, -900f);
    }

    private void AddCategory5_DionFloran()
    {
        const LocationCategory C = LocationCategory.LocationsDionFloran;
        const string T = "Локации возле Dion и Floran";
        AddLoc("Plains of Dion", C, T, 25000f, 150000f, -3400f);
        AddLoc("Dion Hills", C, T, 22000f, 138000f, -3300f);
        AddLoc("Cruma Marshlands", C, T, 15000f, 125000f, -3700f);
        AddLoc("Cruma Tower", C, T, 17700f, 114000f, -11700f);
        AddLoc("Bee Hive", C, T, 35000f, 180000f, -3600f);
        AddLoc("Mandragora Farm", C, T, 15000f, 155000f, -3400f);
        AddLoc("Floran Agricultural Area", C, T, 18000f, 175000f, -3500f);
        AddLoc("Windy Hill", C, T, 40000f, 150000f, -3500f);
        AddLoc("Tanor Canyon", C, T, 65000f, 155000f, -3700f);
        AddLoc("Partisan's Hideaway", C, T, 45000f, 110000f, -3800f);
        AddLoc("Alligator Island", C, T, 110000f, 200000f, -3600f);
        AddLoc("Alligator Beach", C, T, 105000f, 210000f, -3600f);
        AddLoc("Field of Whispers", C, T, 100000f, 225000f, -3600f);
        AddLoc("Field of Silence", C, T, 110000f, 230000f, -3600f);
        AddLoc("Sea of Spores", C, T, 65000f, 25000f, -3600f);
        AddLoc("Ivory Tower", C, T, 85300f, 16200f, -3650f);
        AddLoc("Ivory Tower Crater", C, T, 85300f, 16200f, -3650f);
        AddLoc("Skyshadow Meadow", C, T, 80000f, 65000f, -3400f);
        AddLoc("Sky Wagon Relic", C, T, 105000f, -165000f, -1800f);
        AddLoc("Rainbow Springs Chateau", C, T, 150000f, -125000f, -2200f);
    }

    private void AddCategory6_Giran()
    {
        const LocationCategory C = LocationCategory.LocationsGiran;
        const string T = "Локации возле Giran";
        AddLoc("Gorgon Flower Garden", C, T, 95000f, 175000f, -3600f);
        AddLoc("Death Pass", C, T, 75000f, 125000f, -3700f);
        AddLoc("Dragon Valley", C, T, 108000f, 120000f, -3700f);
        AddLoc("Antharas' Lair", C, T, 131116f, 114383f, -3704f);
        AddLoc("Antharas' Nest", C, T, 175000f, 115000f, -7700f);
        AddLoc("Hardin's Academy", C, T, 105000f, 110000f, -3200f);
        AddLoc("Devil's Isle", C, T, 50000f, 220000f, -3500f);
        AddLoc("Forgotten Temple", C, T, -45000f, 175000f, -4200f);
        AddLoc("Pirate Tunnel", C, T, 52000f, 225000f, -3600f);
        AddLoc("Breka's Stronghold", C, T, 80000f, 120000f, -3600f);
        AddLoc("Dragon Valley Cave", C, T, 125000f, 115000f, -3700f);
        AddLoc("Giran Harbor", C, T, 48000f, 185000f, -3500f);
        AddLoc("Giran Arena", C, T, 73000f, 142000f, -3700f);
        AddLoc("Coliseum", C, T, 150000f, 47000f, -3400f);
        AddLoc("Tour Boat Dock", C, T, 50000f, 187000f, -3500f);
        AddLoc("Narsell Lake", C, T, 105000f, 165000f, -3400f);
    }

    private void AddCategory7_OrenHunters()
    {
        const LocationCategory C = LocationCategory.LocationsOrenHunters;
        const string T = "Локации возле Oren и Hunters Village";
        AddLoc("Sea of Spores", C, T, 65000f, 25000f, -3600f);
        AddLoc("Forest of Mirrors", C, T, 140000f, 60000f, -3600f);
        AddLoc("Enchanted Valley", C, T, 115000f, 45000f, -3600f);
        AddLoc("Valley of Saints", C, T, 78000f, -15000f, -3600f);
        AddLoc("Forest of Evil", C, T, 70000f, 40000f, -3500f);
        AddLoc("Silent Valley", C, T, 150000f, 5000f, -3400f);
        AddLoc("Plains of Glory", C, T, 95000f, 40000f, -3500f);
        AddLoc("War-Torn Plains", C, T, 160000f, 30000f, -3600f);
        AddLoc("Ivory Tower Crater", C, T, 85300f, 16200f, -3650f);
        AddLoc("Tower of Insolence", C, T, 114400f, 13400f, 1000f);
        AddLoc("Cemetery", C, T, 170000f, 20000f, -3300f);
        AddLoc("Ancient Battleground", C, T, 165000f, 15000f, -3500f);
        AddLoc("Pavel Ruins", C, T, 95000f, -115000f, -3400f);
        AddLoc("Giants' Cave", C, T, 185000f, 60000f, -5000f);
        AddLoc("The Giant's Cave", C, T, 185000f, 60000f, -5000f);
        AddLoc("Outlaw Forest", C, T, 125000f, 40000f, -3500f);
        AddLoc("Timak Outpost", C, T, 80000f, 45000f, -3600f);
        AddLoc("Skyshadow Meadow", C, T, 80000f, 65000f, -3400f);
        AddLoc("Anghel Waterfall", C, T, 120000f, 85000f, -3200f);
        AddLoc("Starlight Waterfall", C, T, 125000f, 75000f, -3200f);
        AddLoc("Iris Lake", C, T, 50000f, 55000f, -3200f);
        AddLoc("Frozen Waterfalls", C, T, 115000f, -115000f, -3000f);
        AddLoc("Undine Waterfall", C, T, 42000f, 60000f, -3200f);
        AddLoc("Singing Waterfall", C, T, 55000f, 62000f, -3200f);
        AddLoc("Valley of Heroes", C, T, 110000f, 75000f, -2700f);
    }

    private void AddCategory8_Aden()
    {
        const LocationCategory C = LocationCategory.LocationsAden;
        const string T = "Локации возле Aden";
        AddLoc("Plains of the Lizardmen", C, T, 90000f, 85000f, -3500f);
        AddLoc("Forest of Mirrors", C, T, 140000f, 60000f, -3600f);
        AddLoc("Cemetery", C, T, 170000f, 20000f, -3300f);
        AddLoc("Ancient Battleground", C, T, 165000f, 15000f, -3500f);
        AddLoc("Fields of Massacre", C, T, 185000f, 20000f, -3600f);
        AddLoc("Blazing Swamp", C, T, 145000f, -10000f, -3400f);
        AddLoc("Forsaken Plains", C, T, 175000f, 45000f, -3600f);
        AddLoc("Devastated Castle", C, T, 178000f, -15000f, -2200f);
        AddLoc("Silent Valley", C, T, 150000f, 5000f, -3400f);
        AddLoc("The Giant's Cave", C, T, 185000f, 60000f, -5000f);
        AddLoc("Anghel Waterfall", C, T, 120000f, 85000f, -3200f);
        AddLoc("Wall of Argos", C, T, 168000f, -45000f, -3400f);
        AddLoc("The Forbidden Gateway", C, T, 185000f, 40000f, -3600f);
        AddLoc("The Enchanted Valley", C, T, 115000f, 45000f, -3600f);
        AddLoc("Garden of Beasts", C, T, 145000f, -70000f, -3400f);
        AddLoc("Shrine of Loyalty", C, T, 170000f, -50000f, -3400f);
        AddLoc("The Pilgrim's Necropolis", C, T, -21600f, 77300f, -5170f);
        AddLoc("The Patriot's Necropolis", C, T, -21600f, 77300f, -5170f);
        AddLoc("The Disciple's Necropolis", C, T, 172500f, -17600f, -4900f);
        AddLoc("The Saint's Necropolis", C, T, 111300f, 84300f, -5420f);
        AddLoc("The Martyr's Necropolis", C, T, 111500f, 174000f, -5430f);
        AddLoc("The Worshipper's Necropolis", C, T, 45200f, 123900f, -5410f);
        AddLoc("The Sacrifice's Necropolis", C, T, -41500f, 123000f, -3300f);
        AddLoc("The Apostate's Catacomb", C, T, -20100f, 77400f, -5170f);
        AddLoc("The Heretic's Catacomb", C, T, 39400f, 143500f, -3650f);
        AddLoc("The Branded Catacomb", C, T, 46200f, 170300f, -4970f);
        AddLoc("The Witch's Catacomb", C, T, 14000f, -110000f, -5500f);
        AddLoc("The Forbidden Path Catacomb", C, T, 140000f, 79700f, -5420f);
        AddLoc("The Dark Omens Catacomb", C, T, -19900f, 13700f, -4900f);
    }

    private void AddCategory9_Goddard()
    {
        const LocationCategory C = LocationCategory.LocationsGoddard;
        const string T = "Локации возле Goddard";
        AddLoc("Hot Springs", C, T, 158000f, -122000f, -2800f);
        AddLoc("Frost Lake", C, T, 110000f, -135000f, -3300f);
        AddLoc("Frozen Labyrinth", C, T, 115000f, -125000f, -3100f);
        AddLoc("Ice Merchant Cabin", C, T, 114000f, -126000f, -3000f);
        AddLoc("Frozen Waterfalls", C, T, 115000f, -115000f, -3000f);
        AddLoc("Ketra Orc Outpost", C, T, 142000f, -50000f, -3200f);
        AddLoc("Varka Silenos Barracks", C, T, 118000f, -40000f, -3200f);
        AddLoc("Varka Silenos Outpost", C, T, 125000f, -42000f, -3200f);
        AddLoc("Wall of Argos", C, T, 168000f, -45000f, -3400f);
        AddLoc("Garden of Beasts", C, T, 145000f, -70000f, -3400f);
        AddLoc("Shrine of Loyalty", C, T, 170000f, -50000f, -3400f);
        AddLoc("Imperial Tomb", C, T, 180000f, -85000f, -7200f);
        AddLoc("Four Sepulchers", C, T, 183000f, -78000f, -5500f);
        AddLoc("Forge of the Gods", C, T, 183000f, -15000f, -2700f);
        AddLoc("Hall of Flames", C, T, 185000f, -12000f, -2700f);
        AddLoc("The Last Imperial Tomb", C, T, 186000f, -88000f, -7400f);
        AddLoc("Anachron's Den", C, T, 140000f, -60000f, -3200f);
        AddLoc("Rainbow Springs Chateau", C, T, 150000f, -125000f, -2200f);
        AddLoc("Beast Farm", C, T, 55000f, -85000f, -3300f);
        AddLoc("Wild Beast Reserve", C, T, 60000f, -80000f, -3300f);
        AddLoc("Fields of Massacre", C, T, 185000f, 20000f, -3600f);
        AddLoc("Devastated Castle", C, T, 178000f, -15000f, -2200f);
        AddLoc("Goddard Territory", C, T, 140000f, -50000f, -2600f);
        AddLoc("Ketra Orc Village", C, T, 138000f, -56000f, -3300f);
        AddLoc("Varka Silenos Village", C, T, 113000f, -45000f, -3300f);
    }

    private void AddCategory10_Rune()
    {
        const LocationCategory C = LocationCategory.LocationsRune;
        const string T = "Локации возле Rune";
        AddLoc("Forest of the Dead", C, T, 59000f, -42000f, -3000f);
        AddLoc("Cursed Village", C, T, 58000f, -32000f, -3200f);
        AddLoc("Swamp of Screams", C, T, 87000f, -65000f, -3300f);
        AddLoc("Stakato Nest", C, T, 89000f, -68000f, -3400f);
        AddLoc("Monastery of Silence", C, T, 113000f, -76000f, -100f);
        AddLoc("Pagan Temple", C, T, -16300f, -37000f, -10700f);
        AddLoc("Pilgrim's Temple", C, T, 42000f, -40000f, -1000f);
        AddLoc("Valley of Saints", C, T, 78000f, -15000f, -3600f);
        AddLoc("Wild Beast Reserve", C, T, 60000f, -80000f, -3300f);
        AddLoc("Beast Farm", C, T, 55000f, -85000f, -3300f);
        AddLoc("Rune Harbor", C, T, 38000f, -38000f, -3600f);
        AddLoc("Western Border Outpost", C, T, 48000f, -58000f, -2700f);
        AddLoc("Eastern Border Outpost", C, T, 62000f, -58000f, -2700f);
        AddLoc("Border Outpost", C, T, 55000f, -58000f, -2700f);
        AddLoc("Rune Territory", C, T, 40000f, -45000f, -1000f);
    }

    private void AddCategory11_Schuttgart()
    {
        const LocationCategory C = LocationCategory.LocationsSchuttgart;
        const string T = "Локации возле Schuttgart";
        AddLoc("Frozen Labyrinth", C, T, 115000f, -125000f, -3100f);
        AddLoc("Ice Merchant Cabin", C, T, 114000f, -126000f, -3000f);
        AddLoc("Frost Lake", C, T, 110000f, -135000f, -3300f);
        AddLoc("Frozen Waterfalls", C, T, 115000f, -115000f, -3000f);
        AddLoc("Mithril Mines", C, T, 171000f, -172000f, -3400f);
        AddLoc("Abandoned Coal Mines", C, T, 115000f, -170000f, -1100f);
        AddLoc("Pavel Ruins", C, T, 95000f, -115000f, -3400f);
        AddLoc("Crypts of Disgrace", C, T, 72000f, -120000f, -3500f);
        AddLoc("Den of Evil", C, T, 70000f, -110000f, -3500f);
        AddLoc("Plunderous Plains", C, T, 90000f, -130000f, -3400f);
        AddLoc("Fields of Massacre", C, T, 185000f, 20000f, -3600f);
        AddLoc("Schuttgart Territory", C, T, 85000f, -140000f, -1500f);
    }

    private void AddCategory12_Catacombs()
    {
        const LocationCategory C = LocationCategory.CatacombsAndNecropolises;
        const string T = "Катакомбы и некрополи";
        AddLoc("Catacomb of the Heretic", C, T, 39400f, 143500f, -3650f);
        AddLoc("Catacomb of the Branded", C, T, 46200f, 170300f, -4970f);
        AddLoc("Catacomb of the Apostate", C, T, -20100f, 77400f, -5170f);
        AddLoc("Catacomb of the Witch", C, T, 14000f, -110000f, -5500f);
        AddLoc("Catacomb of Dark Omens", C, T, -19900f, 13700f, -4900f);
        AddLoc("Catacomb of the Forbidden Path", C, T, 140000f, 79700f, -5420f);
        AddLoc("Necropolis of Sacrifice", C, T, -41500f, 123000f, -3300f);
        AddLoc("Necropolis of Worship", C, T, 45200f, 123900f, -5410f);
        AddLoc("Necropolis of Devotion", C, T, -52200f, 79000f, -4740f);
        AddLoc("Necropolis of Martyrdom", C, T, 111500f, 174000f, -5430f);
        AddLoc("Necropolis of Pilgrims", C, T, -21600f, 77300f, -5170f);
        AddLoc("Necropolis of Patriots", C, T, -21600f, 77300f, -5170f);
        AddLoc("Necropolis of Saints", C, T, 111300f, 84300f, -5420f);
        AddLoc("Necropolis of Disciples", C, T, 172500f, -17600f, -4900f);
    }

    private void AddCategory13_Dungeons()
    {
        const LocationCategory C = LocationCategory.DungeonsAndSpecialZones;
        const string T = "Прочие подземелья и спец. зоны";
        AddLoc("Elven Ruins", C, T, 45000f, 52000f, -4100f);
        AddLoc("Elven Fortress", C, T, 49000f, 48000f, -3600f);
        AddLoc("School of Dark Arts", C, T, 15000f, 18000f, -4500f);
        AddLoc("Cave of Trials", C, T, -46000f, -114000f, -200f);
        AddLoc("Mithril Mines", C, T, 171000f, -172000f, -3400f);
        AddLoc("Forgotten Temple", C, T, -45000f, 175000f, -4200f);
        AddLoc("Cruma Tower", C, T, 17700f, 114000f, -11700f);
        AddLoc("Devil's Isle", C, T, 50000f, 220000f, -3500f);
        AddLoc("Antharas' Lair", C, T, 131116f, 114383f, -3704f);
        AddLoc("Tower of Insolence", C, T, 114400f, 13400f, 1000f);
        AddLoc("Imperial Tomb", C, T, 180000f, -85000f, -7200f);
        AddLoc("Four Sepulchers", C, T, 183000f, -78000f, -5500f);
        AddLoc("Forge of the Gods", C, T, 183000f, -15000f, -2700f);
        AddLoc("Pagan Temple", C, T, -16300f, -37000f, -10700f);
        AddLoc("Monastery of Silence", C, T, 113000f, -76000f, -100f);
        AddLoc("Dimensional Rift", C, T, -114500f, -152500f, -6750f);
        AddLoc("Olympiad Stadium", C, T, -78000f, 142000f, -3800f);
        AddLoc("Gludin Arena", C, T, -85000f, 150000f, -3100f);
        AddLoc("Dion Arena", C, T, 22000f, 146000f, -3100f);
        AddLoc("Coliseum", C, T, 150000f, 47000f, -3400f);
        AddLoc("Giran Arena", C, T, 73000f, 142000f, -3700f);
    }

    private void AddCategory14_Islands()
    {
        const LocationCategory C = LocationCategory.IslandsAndHarbors;
        const string T = "Острова и дополнительные территории";
        AddLoc("Primeval Isle", C, T, 10500f, -24000f, -3600f, 25000f);
        AddLoc("Primeval Isle Wharf", C, T, 10000f, -22000f, -3600f);
        AddLoc("Talking Island Harbor", C, T, -96000f, 255000f, -3600f);
        AddLoc("Gludin Harbor", C, T, -90000f, 150000f, -3600f);
        AddLoc("Giran Harbor", C, T, 48000f, 185000f, -3500f);
        AddLoc("Rune Harbor", C, T, 38000f, -38000f, -3600f);
        AddLoc("Tour Boat Dock", C, T, 50000f, 187000f, -3500f);
        AddLoc("Alligator Island", C, T, 110000f, 200000f, -3600f);
        AddLoc("Devil's Isle", C, T, 50000f, 220000f, -3500f);
    }

    public LocationPoint GetNearestLocation(Vector3 unityPos)
    {
        float l2X = unityPos.z * 52.5f;
        float l2Y = unityPos.x * 52.5f;

        LocationPoint best = null;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < _locations.Count; i++)
        {
            LocationPoint loc = _locations[i];
            float dx = loc.L2Pos.x - l2X;
            float dy = loc.L2Pos.y - l2Y;
            float dSq = dx * dx + dy * dy;

            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                best = loc;
            }
        }

        return best;
    }

    public string GetRegionName(Vector3 unityPos)
    {
        float l2X = unityPos.z * 52.5f;
        float l2Y = unityPos.x * 52.5f;

        // Check if inside any known point radius (prioritize towns first)
        for (int i = 0; i < _locations.Count; i++)
        {
            var loc = _locations[i];
            if (!loc.IsTown) continue;
            float dx = loc.L2Pos.x - l2X;
            float dy = loc.L2Pos.y - l2Y;
            if (dx * dx + dy * dy <= loc.Radius * loc.Radius)
            {
                return loc.Name;
            }
        }

        for (int i = 0; i < _locations.Count; i++)
        {
            var loc = _locations[i];
            float dx = loc.L2Pos.x - l2X;
            float dy = loc.L2Pos.y - l2Y;
            if (dx * dx + dy * dy <= loc.Radius * loc.Radius)
            {
                return loc.Name;
            }
        }

        LocationPoint nearest = GetNearestLocation(unityPos);
        if (nearest != null)
        {
            float distMeters = Mathf.Sqrt(Mathf.Pow(nearest.L2Pos.x - l2X, 2) + Mathf.Pow(nearest.L2Pos.y - l2Y, 2)) / 52.5f;
            return $"{nearest.Name} (~{Mathf.RoundToInt(distMeters)}m)";
        }

        return "Aden Continent";
    }

    public List<LocationPoint> GetLocations(LocationCategory category)
    {
        if (_byCategory.TryGetValue(category, out List<LocationPoint> list))
        {
            return list;
        }
        return new List<LocationPoint>();
    }

    public IReadOnlyList<LocationPoint> AllLocations => _locations;
}
