using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ColorInfo
{
    public Color color;
    public int chance;
}
public struct UnitColorInfo
{
    public Color hairColor;
    public Color skinColor;
    public Color[] rarityColors;
    public bool hasRarity;

    public UnitColorInfo(Color hairColor, Color skinColor, Color[] rarityColors, bool hasRarity)
    {
        this.hairColor = hairColor;
        this.skinColor = skinColor;
        this.rarityColors = rarityColors;
        this.hasRarity = hasRarity;
    }
}
public class BaseUtils : MonoBehaviour
{
    public ScriptableCurve[] curves;
    public static readonly Dictionary<CurveType, ScriptableCurve> curveDict = new Dictionary<CurveType, ScriptableCurve>();
    public ScriptableUnit[] units;
    public static readonly Dictionary<UnitType, ScriptableUnit> unitDict = new Dictionary<UnitType, ScriptableUnit>();
    public ScriptableTile[] tiles;
    public static readonly Dictionary<TileType, ScriptableTile> tileDict = new Dictionary<TileType, ScriptableTile>();
    public bool _offlineMode;
    public static bool offlineMode;
    public static bool showingWarn;
    public static bool onTavern;

    public Material _normalUI;
    public Material _outlineUI;
    public Material _lightoutUI;
    public Material _highlightUI;
    public Material _highLineUI;
    public Material _grayscaleUI;
    public Material _graylightUI;
    public Material _yellowMaterial;
    public Material _yellowMaterialUI;
    public Material _purpleMaterial;
    public Material _purpleMaterialUI;
    public Material _yellowWaterMaterial;
    public Material _purpleWaterMaterial;
    public static Material normalUI;
    public static Material outlineUI;
    public static Material highlightUI;
    public static Material highLineUI;
    public static Material grayscaleUI;
    public static Material graylightUI;
    public static Material yellowMaterial;
    public static Material yellowMaterialUI;
    public static Material purpleMaterial;
    public static Material purpleMaterialUI;
    public static Material yellowWaterMaterial;
    public static Material purpleWaterMaterial;
    public static WarningController warningController;
    public WarningController _warningController;
    public Sprite[] _rankEdges;
    public static Sprite[] rankEdges;
    public Sprite[] _rankSlots;
    public static Sprite[] rankSlots;
    public Sprite[] _tierSlots;
    public static Sprite[] tierSlots;
    public ColorInfo[] _hairColorInfos;
    public static ColorInfo[] hairColorInfos;
    public ColorInfo[] _skinColorInfos;
    public static ColorInfo[] skinColorInfos;
    public ColorInfo[] _rarityOneColorInfos;
    public static ColorInfo[] rarityOneColorInfos;
    public ColorInfo[] _rarityTwoColorInfos;
    public static ColorInfo[] rarityTwoColorInfos;
    public ColorInfo[] _rarityThreeColorInfos;
    public static ColorInfo[] rarityThreeColorInfos;
    public ColorInfo[] _rarityFourColorInfos;
    public static ColorInfo[] rarityFourColorInfos;
    public ColorInfo[] _rarityFiveColorInfos;
    public static ColorInfo[] rarityFiveColorInfos;
    public Color _poisonColor;
    public static Color poisonColor;
    public Color _atkColor;
    public static Color atkColor;
    public Color _speedColor;
    public static Color speedColor;
    public Color _slowColor;
    public static Color slowColor;
    public Color _healingColor;
    public static Color healingColor;
    public Color _damageColor;
    public static Color damageColor;
    public Color _grayColor;
    public static Color grayColor;
    public Color _sellingColor;
    public static Color sellingColor;
    public Sprite _emptyUnit;
    public static Sprite emptyUnit;
    public Camera _thisCam;
    public Transform _mainCanvas;
    public Transform _mainGame;
    public static Camera thisCam;
    public static Transform mainCanvas;
    public static Transform mainGame;
    public char[] letterChars;
    public Sprite[] smallLetters;
    public Sprite[] mediumLetters;
    public static GameObject loadingScreenObj;
    public GameObject _loadingScreenObj;
    public static GameObject textPrefab;
    public GameObject _textPrefab;
    public static readonly Queue<TextController> textPool = new Queue<TextController>();
    public static readonly List<TextController> activeTexts = new List<TextController>();

    public GameObject[] effects;
    public static readonly Dictionary<EffectType, GameObject> effectDict = new Dictionary<EffectType, GameObject>();
    public static readonly Dictionary<EffectType, Queue<EffectController>> effectPool = new Dictionary<EffectType, Queue<EffectController>>();
    public static readonly List<EffectController> activeEffects = new List<EffectController>();

    public static readonly Dictionary<char, Sprite> smallLetterDict = new Dictionary<char, Sprite>();
    public static readonly Dictionary<char, Queue<Image>> smallLetterPool = new Dictionary<char, Queue<Image>>();
    public static readonly Dictionary<char, Sprite> mediumLetterDict = new Dictionary<char, Sprite>();
    public static readonly Dictionary<char, Queue<Image>> mediumLetterPool = new Dictionary<char, Queue<Image>>();
    public static string battleKey = "";

    private static readonly List<string> maleNames = new List<string>()
    {
        "Abe","Abraham","Albert","Ambrose","Amos","Ansel","Archie","Aron","Arthur","Artie","Atticus","August","Barron","Beau","Benedict","Bennett","Bernard","Blaine",
        "Blevins","Brady","Carlo","Cassady","Clarence","Clellon","Clifford","Cole","Cornelius","Cyrus","Damion","Dean","Daniel","Edward","Earl","Edison","Edmund","Erwin","Edwin","Elijah",
        "Elmer","Elrod","Elon","Emmett","Emile","Ernest","Frank","Franklin","Francis","Gael","George","Gerald","Gert","Gracy","Gunther","Gus","Harold","Harmon","Harvey",
        "Hayden","Hector","Henry","Herbert","Holden","Howard","Hugh","Ian","Jasper","Jarrett","Jedediah","Joel","Julien","Kenneth","Lacey","Langston","Lee","Louis",
        "Lucas","Marshall","Mickey","Milton","Morgan","Neal","Nelson","Neville","Norman","Orville","Oscar","Otis","Peyton","Pierce","Presley","Preston","Ralph","Randall",
        "Rawlins","Raymond","Reed","Reginald","Richard","Rodney","Rollo","Roy","Rutherford","Sal","Shadrack","Sherman","Spencer","Stanley","Sterling","Theodore","Tobias",
        "Tobin","Waldo","Whitman","Wilber","Willie","Winston","Windsor","Wright","Albert","Alfred","Archie","Barney","Bernar","Bobby","Bruce","Calvin","Chester","Clarence","Clifford",
        "Clyde","Darryl","Dennis","Don","Dwight","Edgar","Emmet","Eugene","Felix","Franklin","Gary","Garret","Gilbert","Glenn","Grant","Gregory","Hal","Harry","Harvey","Henry",
        "Herman","Horace","Ike","Ira","Irving","Ivan","Jefferson","Jerome","Jerry","Johnny","Keith","Laurie","Leland","Leonard","Lloyd","Luther","Melvin","Morris","Moses",
        "Murray","Newton","Niles","Norman","Oswald","Percy","Phil","Quincy","Ralph","Rueben","Rex","Rhett","Roland","Seymour","Silas","Terrence","Tracy","Truman",
        "Ulysses","Vernon","Victor","Vincent","Wallace","Walter","Wayne","Wendell","Wilfred","Wiley","Wilson","Wilbur","Woodrow","Zacharias","Zeke",
    };
    private static readonly List<string> femaleNames = new List<string>()
    {
        "Abra","Addison","Adeline","Adelaide","Agatha","Agnes","Alejandra","Alice","Alma","Amara","Anita","Bea","Bernadette","Beatrix","Bertie","Bessie","Birdie",
        "Blythe","Bonnie","Calliope","Camille","Carole","Colette","Connie","Celia","Clara","Dahlia","Daisy","Darcy","Dessie","Dodie","Dora","Doris","Dorothy","Edith",
        "Enid","Etta","Eleanor","Elaine","Ellis","Emily","Emma","Ethel","Eugenie","Evelyn","Faye","Florence","Gabrielle","Galatea","Genevieve","Georgia","Gertie","Greta",
        "Harriet","Hattie","Hazel","Hilda","Irene","Imogen","Inez","Jane","Joan","Josephine","Joyce","Katherine","Laura","Lucille","Lucinda","Luisa","Lorraine","Lydia",
        "Lacy","Liza","Margaret","Martha","Marjorie","Mathilde","Maxine","Miriam","Minnie","Mollie","Nora","Octavia","Olive","Opal","Patricia","Pearl","Penelope","Polly",
        "Pollyanna","Posey","Rolla","Rosemary","Ruth","Sadie","Sandra","Scarlet","Selma","Seraphina","Shirley","Shoshana","Sussanah","Sylvia","Trudy","Una","Valentina",
        "Vera","Virginia","Viola","Violet","Willa","Wren"
    };
    public static readonly KeyCode[] modifierKeys = new KeyCode[44]
    {
        KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl,
        KeyCode.AltGr, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.Tab, KeyCode.CapsLock,
        KeyCode.Numlock, KeyCode.Print, KeyCode.End, KeyCode.PageDown, KeyCode.PageUp,
        KeyCode.Pause, KeyCode.Insert, KeyCode.ScrollLock, KeyCode.Break, KeyCode.DownArrow,
        KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.UpArrow,KeyCode.F1,KeyCode.F2,KeyCode.F3,
        KeyCode.F4,KeyCode.F5,KeyCode.F6,KeyCode.F7,KeyCode.F8,KeyCode.F9,KeyCode.F10,KeyCode.F11,
        KeyCode.F12,KeyCode.LeftWindows,KeyCode.RightWindows,KeyCode.SysReq, KeyCode.Mouse0, KeyCode.Mouse1,
        KeyCode.Mouse2,KeyCode.Mouse3,KeyCode.Mouse4,KeyCode.Mouse5,KeyCode.Mouse6
    };
    private static readonly List<UnitType> tier1Units = new List<UnitType>();
    private static readonly List<UnitType> tier2Units = new List<UnitType>();
    private static readonly List<UnitType> tier3Units = new List<UnitType>();
    private static readonly List<UnitType> tier4Units = new List<UnitType>();
    private void Awake()
    {
        Setup();
    }
    public void Setup()
    {
        Physics.autoSimulation = false;
        offlineMode = _offlineMode;
        thisCam = _thisCam;
        mainCanvas = _mainCanvas;
        mainGame = _mainGame;
        normalUI = _normalUI;
        outlineUI = _outlineUI;
        highlightUI = _highlightUI;
        highLineUI = _highLineUI;
        grayscaleUI = _grayscaleUI;
        graylightUI = _graylightUI;
        purpleMaterial = _purpleMaterial;
        purpleMaterialUI = _purpleMaterialUI;
        yellowMaterial = _yellowMaterial;
        yellowMaterialUI = _yellowMaterialUI;
        yellowWaterMaterial = _yellowWaterMaterial;
        purpleWaterMaterial = _purpleWaterMaterial;
        rankSlots = _rankSlots;
        tierSlots = _tierSlots;
        rankEdges = _rankEdges;
        hairColorInfos = _hairColorInfos;
        skinColorInfos = _skinColorInfos;
        rarityOneColorInfos = _rarityOneColorInfos;
        rarityTwoColorInfos = _rarityTwoColorInfos;
        rarityThreeColorInfos = _rarityThreeColorInfos;
        rarityFourColorInfos = _rarityFourColorInfos;
        rarityFiveColorInfos = _rarityFiveColorInfos;
        warningController = _warningController;
        loadingScreenObj = _loadingScreenObj;
        emptyUnit = _emptyUnit;
        textPrefab = _textPrefab;
        slowColor = _slowColor;
        speedColor = _speedColor;
        atkColor = _atkColor;
        poisonColor = _poisonColor;
        healingColor = _healingColor;
        damageColor = _damageColor;
        grayColor = _grayColor;
        sellingColor = _sellingColor;
        SetDicts();
        SetLetterDicts();
    }
    private void SetDicts()
    {
        curveDict.Clear();
        unitDict.Clear();
        tileDict.Clear();
        for (int i = 0; i < curves.Length; i++)
        {
            curveDict.Add(curves[i].curveType, curves[i]);
        }
        for (int i = 0; i < units.Length; i++)
        {
            unitDict.Add(units[i].unitType, units[i]);
            if (!units[i].summon)
            {
                switch (units[i].unitTier)
                {
                    case 1:
                        tier1Units.Add(units[i].unitType);
                        break;
                    case 2:
                        tier2Units.Add(units[i].unitType);
                        break;
                    case 3:
                        tier3Units.Add(units[i].unitType);
                        break;
                    case 4:
                        tier4Units.Add(units[i].unitType);
                        break;
                }
            }
        }
        for (int i = 0; i < tiles.Length; i++)
        {
            tileDict.Add(tiles[i].tileType, tiles[i]);
        }
        effectDict.Clear();
        effectPool.Clear();
        for (int i = 0; i < effects.Length; i++)
        {
            System.Enum.TryParse(effects[i].name, out EffectType effectType);
            effectDict.Add(effectType, effects[i]);
            effectPool.Add(effectType, new Queue<EffectController>());
        }
    }
    public static void InstantiateText(Transform fromTransform, string gotoString, int damage, Color color, bool followTransform)
    {
        TextController textController;
        if (textPool.Count > 0)
        {
            textController = textPool.Dequeue();
        }
        else
        {
            GameObject textObj = Instantiate(textPrefab, mainCanvas.GetChild(1));
            textController = textObj.GetComponent<TextController>();
        }
        textController.Setup(fromTransform, gotoString, color, damage, followTransform);
    }
    public static void StopAllTexts()
    {
        for (int i = 0; i < activeTexts.Count; i++)
        {
            activeTexts[i].StopAllCoroutines();
            activeTexts[i].Dispose();
        }
    }
    public static void ShowLoading()
    {
        loadingScreenObj.SetActive(true);
    }
    public static void HideLoading()
    {
        loadingScreenObj.SetActive(false);
    }
    public static void ShowWarningMessage(string title, string[] message, Image unitImage, Image edgeImage, Action OnAcceptCallback)
    {
        warningController.Setup(title, message, edgeImage, unitImage, OnAcceptCallback);
    }
    public static void ShowWarningMessage(string title, string[] message, Image unitImage, Image edgeImage)
    {
        warningController.Setup(title, message, edgeImage, unitImage);
    }
    public static void ShowWarningMessage(string title, string[] message, UnitInfo unitInfo)
    {
        warningController.Setup(title, message, unitInfo);
    }
    public static void ShowWarningMessage(string title, string[] message, Action OnAcceptCallback, bool removeNo = false)
    {
        warningController.Setup(title, message, OnAcceptCallback, removeNo);
    }
    public static void ShowWarningMessage(string title, string[] message)
    {
        warningController.Setup(title, message);
    }
    public static void InstantiateEffect(EffectType effectType, Vector3 fromPosition, Vector3 goPosition, bool fromGame, float scaleModifier = 1, float sideModifier = 1)
    {
        EffectController effectController;
        if (effectPool[effectType].Count > 0)
        {
            effectController = effectPool[effectType].Dequeue();
        }
        else
        {
            GameObject effectObj = Instantiate(effectDict[effectType], fromGame ? mainGame : mainCanvas);
            effectController = effectObj.GetComponent<EffectController>();
        }
        effectController.Setup(effectType, fromPosition, goPosition, scaleModifier, sideModifier);
    }
    public static void InstantiateEffect(EffectType effectType, Vector3 goPosition, bool fromGame, float scaleModifier = 1, float sideModifier = 1)
    {
        EffectController effectController;
        if (effectPool[effectType].Count > 0)
        {
            effectController = effectPool[effectType].Dequeue();
        }
        else
        {
            GameObject effectObj = Instantiate(effectDict[effectType], fromGame ? mainGame : mainCanvas);
            effectController = effectObj.GetComponent<EffectController>();
        }
        effectController.Setup(effectType, goPosition, scaleModifier, sideModifier);
    }
    public static void StopAllEffects()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            activeEffects[i].Dispose();
        }
    }
    public void SetLetterDicts()
    {
        smallLetterDict.Clear();
        smallLetterPool.Clear();
        mediumLetterDict.Clear();
        mediumLetterPool.Clear();
        for (int i = 0; i < letterChars.Length; i++)
        {
            smallLetterDict.Add(letterChars[i], smallLetters[i]);
            smallLetterPool.Add(letterChars[i], new Queue<Image>());
            mediumLetterDict.Add(letterChars[i], mediumLetters[i]);
            mediumLetterPool.Add(letterChars[i], new Queue<Image>());
        }
    }
    public static bool RandomBool()
    {
        return UnityEngine.Random.Range(0, 100) > 50;
    }
    public static int RandomSign()
    {
        if (RandomBool())
        {
            return -1;
        }
        return 1;
    }
    public static int RandomInt(int rangeX, int rangeY)
    {
        return UnityEngine.Random.Range(rangeX, rangeY);
    }
    public static int RandomInt(Vector2Int range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
    }
    public static float RandomFloat(float fromFloat, float gotoFloat)
    {
        return RandomInt(Mathf.RoundToInt(fromFloat * 100), Mathf.RoundToInt(gotoFloat * 100)) / 100f;
    }
    public static UnitInfo GenerateRandomUnit(int price = 0, bool enemyID = false)
    {
        UnitType randomUnit;
        int randomInt = RandomInt(0, 100);
        if (randomInt <= 5)
        {
            randomUnit = tier4Units[RandomInt(0, tier4Units.Count)];
        }
        else if (randomInt <= 10)
        {
            randomUnit = tier3Units[RandomInt(0, tier3Units.Count)];
        }
        else if (randomInt <= 35)
        {
            randomUnit = tier2Units[RandomInt(0, tier2Units.Count)];
        }
        else
        {
            randomUnit = tier1Units[RandomInt(0, tier1Units.Count)];
        }
        int unitID = enemyID ? RandomInt(100000, 900000) : RandomInt(0, 100000);
        ScriptableUnit scriptableUnit = unitDict[randomUnit];
        int speed = scriptableUnit.speed + RandomInt(-5, 5);
        int damage = scriptableUnit.damage + RandomInt(-10, 10);
        int health = scriptableUnit.maxHealth + RandomInt(-25, 25);
        return new UnitInfo(randomUnit, GetUnitColorInfo(unitID), unitID, speed, damage, health, price);
    }
    public static UnitColorInfo GetUnitColorInfo(int unitID)
    {
        UnityEngine.Random.InitState(unitID);
        Color hairColor = new Color();
        int randomHairChance = RandomInt(0, 100);
        for (int i = 0; i < hairColorInfos.Length; i++)
        {
            if (randomHairChance <= hairColorInfos[i].chance)
            {
                hairColor = hairColorInfos[i].color;
                break;
            }
        }
        Color skinColor = new Color();
        int randomSkinChance = RandomInt(0, 100);
        for (int i = 0; i < skinColorInfos.Length; i++)
        {
            if (randomSkinChance <= skinColorInfos[i].chance)
            {
                skinColor = skinColorInfos[i].color;
                break;
            }
        }
        bool hasRarity = false;
        Color[] rarityColors = new Color[4];
        int randomRarityChance = RandomInt(0, 100);
        if (randomRarityChance <= rarityFiveColorInfos[0].chance)
        {
            for (int i = 0; i < rarityFiveColorInfos.Length; i++)
            {
                rarityColors[i] = rarityFiveColorInfos[i].color;
            }
            hasRarity = true;
        }
        else if (randomRarityChance <= rarityFourColorInfos[0].chance)
        {
            for (int i = 0; i < rarityFourColorInfos.Length; i++)
            {
                rarityColors[i] = rarityFourColorInfos[i].color;
            }
            hasRarity = true;
        }
        else if (randomRarityChance <= rarityThreeColorInfos[0].chance)
        {
            for (int i = 0; i < rarityThreeColorInfos.Length; i++)
            {
                rarityColors[i] = rarityThreeColorInfos[i].color;
            }
            hasRarity = true;
        }
        else if (randomRarityChance <= rarityTwoColorInfos[0].chance)
        {
            for (int i = 0; i < rarityTwoColorInfos.Length; i++)
            {
                rarityColors[i] = rarityTwoColorInfos[i].color;
            }
            hasRarity = true;
        }
        else
        {
            for (int i = 0; i < rarityOneColorInfos.Length; i++)
            {
                rarityColors[i] = rarityOneColorInfos[i].color;
            }
        }
        return new UnitColorInfo(hairColor, skinColor, rarityColors, hasRarity);
    }
    public static string GetUnitName(UnitInfo unitInfo)
    {
        UnityEngine.Random.InitState(unitInfo.unitID);
        string randomName;
        if (unitInfo.unitType == UnitType.Chemist || unitInfo.unitType == UnitType.Elementalist)
        {
            randomName = femaleNames[RandomInt(0, femaleNames.Count)];
        }
        else
        {
            randomName = maleNames[RandomInt(0, maleNames.Count)];
        }
        return randomName + ", the " + unitInfo.unitType;
    }
    public static int RankToIndex(int rank)
    {
        return Mathf.Clamp((rank - 800) / 200, 0, 14);
    }
    public static string RankToName(int rankIndex)
    {
        switch (rankIndex)
        {
            default:
                //800
                return "bronze 1";
            case 1:
                //1000
                return "bronze 2";
            case 2:
                //1200
                return "bronze 3";
            case 3:
                //1400
                return "silver 1";
            case 4:
                //1600
                return "silver 2";
            case 5:
                //1800
                return "silver 3";
            case 6:
                //1900
                return "gold 1";
            case 7:
                //2000
                return "gold 2";
            case 8:
                //2200
                return "gold 3";
            case 9:
                //2400
                return "platinum 1";
            case 10:
                //2600
                return "platinum 2";
            case 11:
                //2800
                return "platinum 3";
            case 12:
                //3000
                return "diamond 1";
            case 13:
                //3200
                return "diamond 2";
            case 14:
                //3400
                return "diamond 3";
        }
    }
}
public static class Extensions
{
    public static string ToNumberBigChar(this int number)
    {
        string finalString = "";
        foreach (char character in number.ToString())
        {
            switch (character)
            {
                case '0':
                    finalString += "@";
                    break;
                case '1':
                    finalString += "%";
                    break;
                case '2':
                    finalString += "¨";
                    break;
                case '3':
                    finalString += "¬";
                    break;
                case '4':
                    finalString += "£";
                    break;
                case '5':
                    finalString += "§";
                    break;
                case '6':
                    finalString += "³";
                    break;
                case '7':
                    finalString += "²";
                    break;
                case '8':
                    finalString += "º";
                    break;
                case '9':
                    finalString += "¹";
                    break;
            }
        }

        return finalString;
    }
    public static readonly KeyCode[] acceptKeycodes = new KeyCode[26]
    {
                KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
                KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
                KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.X, KeyCode.W, KeyCode.Y, KeyCode.Z
    };
    public static char ToCorrectChar(this KeyCode keyCode)
    {
        if (Array.IndexOf(acceptKeycodes, keyCode) != -1)
        {
            return (char)keyCode;
        }
        bool upperCheck = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        switch (keyCode)
        {
            case KeyCode.Alpha0:
            case KeyCode.Keypad0:
                return upperCheck ? ')' : '0';
            case KeyCode.Alpha1:
            case KeyCode.Keypad1:
                return upperCheck ? '!' : '1';
            case KeyCode.Alpha2:
            case KeyCode.Keypad2:
                return upperCheck ? '@' : '2';
            case KeyCode.Alpha3:
            case KeyCode.Keypad3:
                return upperCheck ? '#' : '3';
            case KeyCode.Alpha4:
            case KeyCode.Keypad4:
                return upperCheck ? '$' : '4';
            case KeyCode.Alpha5:
            case KeyCode.Keypad5:
                return upperCheck ? '%' : '5';
            case KeyCode.Alpha6:
            case KeyCode.Keypad6:
                return upperCheck ? '¨' : '6';
            case KeyCode.Alpha7:
            case KeyCode.Keypad7:
                return upperCheck ? '&' : '7';
            case KeyCode.Alpha8:
            case KeyCode.Keypad8:
                return upperCheck ? '*' : '8';
            case KeyCode.Alpha9:
            case KeyCode.Keypad9:
                return upperCheck ? '(' : '9';
            case KeyCode.KeypadPeriod:
                return upperCheck ? '>' : '.';
            case KeyCode.KeypadDivide:
                return upperCheck ? '?' : '/';
            case KeyCode.KeypadMultiply:
                return upperCheck ? '8' : '*';
            case KeyCode.KeypadMinus:
                return upperCheck ? '_' : '-';
            case KeyCode.KeypadPlus:
                return upperCheck ? '=' : '+';
            case KeyCode.KeypadEquals:
                return upperCheck ? '+' : '=';
            case KeyCode.Exclaim:
                return '!';
            case KeyCode.DoubleQuote:
                return '"';
            case KeyCode.Hash:
                return '#';
            case KeyCode.Dollar:
                return '$';
            case KeyCode.Percent:
                return '%';
            case KeyCode.Ampersand:
                return '&';
            case KeyCode.Quote:
                return '\'';
            case KeyCode.LeftParen:
                return '(';
            case KeyCode.RightParen:
                return ')';
            case KeyCode.Asterisk:
                return '*';
            case KeyCode.Plus:
                return '+';
            case KeyCode.Comma:
                return upperCheck ? '<' : ',';
            case KeyCode.Minus:
                return upperCheck ? '_' : '-';
            case KeyCode.Period:
                return upperCheck ? '>' : '.';
            case KeyCode.Slash:
                return upperCheck ? '?' : '/';
            case KeyCode.Colon:
                return upperCheck ? ':' : ';';
            case KeyCode.Semicolon:
                return upperCheck ? ':' : ';';
            case KeyCode.Less:
                return upperCheck ? ',' : '<';
            case KeyCode.Equals:
                return '=';
            case KeyCode.Greater:
                return '>';
            case KeyCode.Question:
                return '?';
            case KeyCode.At:
                return '@';
            case KeyCode.LeftBracket:
                return upperCheck ? '{' : '[';
            case KeyCode.Backslash:
                return '\\';
            case KeyCode.RightBracket:
                return upperCheck ? '}' : ']';
            case KeyCode.Caret:
                return '^';
            case KeyCode.Underscore:
                return '_';
            case KeyCode.BackQuote:
                return '`';
            case KeyCode.LeftCurlyBracket:
                return '{';
            case KeyCode.Pipe:
                return '|';
            case KeyCode.RightCurlyBracket:
                return '}';
            case KeyCode.Tilde:
                return '~';
            case KeyCode.Space:
                return ' ';
            default:
                return '?';
        }
    }
    public static Vector3 ToScale(this Vector3 scale)
    {
        return new Vector3(scale.x.ToScale(), scale.y.ToScale(), scale.z.ToScale());
    }
    public static float ToScale(this float value)
    {
        return value * BaseUtils.mainCanvas.parent.localScale.x;
    }
    public static float Evaluate(this float value, CurveType curveType)
    {
        return BaseUtils.curveDict[curveType].animationCurve.Evaluate(value);
    }
}
