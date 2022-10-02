using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class XMLGenerator : MonoBehaviour
{
    public static string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }
    public static byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }
    public static string SerializeObject(object pObject, Type classType)
    {
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(classType);
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        string XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }
    public static object DeserializeObject(string pXmlizedString, Type classType)
    {
        XmlSerializer xs = new XmlSerializer(classType);
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        //XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xs.Deserialize(memoryStream);
    }
    public static void CreateXML(string rawData, string fileName, string fileLocation = "")
    {
        StreamWriter writer;
        if (fileLocation == "")
        {
            fileLocation = Application.dataPath + "/XMLs/";
        }
        FileInfo t = new FileInfo(fileLocation + "\\" + fileName);
        if (!t.Exists)
        {
            writer = t.CreateText();
        }
        else
        {
            t.Delete();
            writer = t.CreateText();
        }
        writer.Write(rawData);
        writer.Close();
    }
    public static string LoadXML(string fileName, string fileLocation = "")
    {
        if (fileLocation == "")
        {
            fileLocation = Application.dataPath + "/XMLs/";
        }
        if (!File.Exists(fileLocation + "\\" + fileName))
        {
            return "";
        }
        StreamReader r = File.OpenText(fileLocation + "\\" + fileName);
        string _info = r.ReadToEnd();
        r.Close();
        return _info;
    }
    public void GenerateXMLs()
    {
        BaseUtils baseUtils = FindObjectOfType<BaseUtils>();
        UnitValues[] unitValues = new UnitValues[baseUtils.units.Length];
        for (int i = 0; i < baseUtils.units.Length; i++)
        {
            unitValues[i].unitType = baseUtils.units[i].unitType;
            unitValues[i].supports = baseUtils.units[i].supports;
            unitValues[i].dontRun = baseUtils.units[i].dontRun;
            unitValues[i].attackRange = baseUtils.units[i].attackRange;
            unitValues[i].maxHealth = baseUtils.units[i].maxHealth;
            unitValues[i].damage = baseUtils.units[i].damage;
            unitValues[i].speed = baseUtils.units[i].speed;
            unitValues[i].areaOfEffect = baseUtils.units[i].areaOfEffect;
            unitValues[i].attackSpeed = baseUtils.units[i].attackSpeed;
            unitValues[i].unitTier = baseUtils.units[i].unitTier;
            unitValues[i].summon = baseUtils.units[i].summon;
        }
        string rawUnitData = SerializeObject(unitValues, typeof(UnitValues[]));
        CreateXML(rawUnitData, "Units.xml");

        string rawTileData = SerializeObject(baseUtils.tiles, typeof(ScriptableTile[]));
        CreateXML(rawTileData, "Tiles.xml");
    }
}
public struct UnitValues
{
    public UnitType unitType;
    public bool supports;
    public bool dontRun;
    public bool summon;
    public int attackRange;
    public int maxHealth;
    public int damage;
    public int speed;
    public int areaOfEffect;
    public int unitTier;
    public int attackSpeed;
}
#if UNITY_EDITOR
[CustomEditor(typeof(XMLGenerator))]
public class XMLGeneratorUI : Editor
{
    public override void OnInspectorGUI()
    {
        XMLGenerator generator = (XMLGenerator)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate XMLs"))
        {
            EditorUtility.SetDirty(generator);
            generator.GenerateXMLs();
            EditorUtility.ClearDirty(generator);
        }
    }
}
#endif
