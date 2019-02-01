using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Assets.Assets.Scripts;
using System.Xml;
using System.Linq;
using System.IO;

public class XmlFileWriter : MonoBehaviour
{
    //string name, int playerCoins, string spentTime, DateTime date, bool endWasForced
    public void WriteResultsToXmlFile(Result gameResult)
    {
        string path = Application.dataPath + "/results.xml";

        XmlDocument document = new XmlDocument();
        document.Load(Application.dataPath + "/results.xml");

        XmlElement gameResultElement = document.CreateElement("GameResult");

        XmlElement name = document.CreateElement("Name");
        XmlElement coins = document.CreateElement("Coins");
        XmlElement spentTime = document.CreateElement("SpentTime");
        XmlElement date = document.CreateElement("Date");
        XmlElement wasEndForced = document.CreateElement("WasEndForced");

        XmlElement[] elements = new XmlElement[] { name, coins, spentTime, date, wasEndForced };
        string[] values = new string[] {
            gameResult.name ?? "null",
            gameResult.playerCoins.ToString(),
            gameResult.spentTime.ToString(),
            gameResult.date.ToString(),
            gameResult.endWasForced.ToString().ToLower()
        };
        
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].AppendChild(document.CreateTextNode(values[i]));
        }

        foreach (var i in new[] { name, coins, spentTime, date, wasEndForced })
        {
            gameResultElement.AppendChild(i);
        }

        document.DocumentElement.AppendChild(gameResultElement);
        document.Save(path);
    }
}
