using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Linq;
using TMPro;
using System;

public class XmlFileReader : MonoBehaviour
{
    public TextMeshProUGUI uiText;

    void Start()
    {
        ParseXmlFile();
    }

    public void ParseXmlFile()
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(Application.dataPath + "/results.xml");
        string xmlPathPattern = "//GameResults/GameResult";

        XmlNode[] nodes =
            xmlDocument.SelectNodes(xmlPathPattern).Cast<XmlNode>()
            .OrderByDescending(
             t => Convert.ToDateTime(
                 t.ChildNodes.Cast<XmlNode>()
                .Where(t_ch => t_ch.Name == "Date")
                .First()
                .InnerText
                )
            ).ToArray();

        string result = "";
        var count = 1;
        foreach (XmlNode node in nodes)
        {
            string name = null;
            string coins = null;
            string dateTime = null;
            string spentTime = null;
            string forcedEnd = null;

            foreach (XmlNode i in node.ChildNodes)
            {
                if (i.Name == "Name")
                    name = i.InnerText;
                else if (i.Name == "Coins")
                    coins = i.InnerText;
                else if (i.Name == "Date")
                    dateTime = i.InnerText;
                else if (i.Name == "SpentTime")
                    spentTime = i.InnerText;
                else if (i.Name == "WasEndForced")
                    forcedEnd = i.InnerText;
            }

            result +=
                string.Format("GameResult #{5}:\nName: {0}\nCoins: {1}\nDate: " +
                "{2}\nTime spent in game: {3}\n Game ending reason: {4}",
                name, coins, dateTime, spentTime, forcedEnd == "true" ? "End forced by player" : "Died by enemy", count);
            result += "\n\n";

            count++;
        }
        uiText.text = result;
    }

}
