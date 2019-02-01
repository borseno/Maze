using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Assets.Assets.Scripts
{
    // WriteResultsToXml(playerName, playerCoins, dateTime, bool forcedEnd);
    public class Result
    {
        public readonly string name;
        public readonly int playerCoins;
        public readonly string spentTime;
        public readonly DateTime date;
        public readonly bool endWasForced;

        public Result(string name, int playerCoins, string spentTime, DateTime date, bool endWasForced)
        {
            this.name = name;
            this.playerCoins = playerCoins;
            this.spentTime = spentTime;
            this.date = date;
            this.endWasForced = endWasForced;
        }
    }
}
