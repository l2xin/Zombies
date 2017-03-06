using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class SignConfig : SingletonInstance<SignConfig>
{
    private Dictionary<int, Dictionary<int, Dictionary<int, DayRewardConf>>> dayRewardConfMapOfYear;

    public void Setup(string obj)
    {
        dayRewardConfMapOfYear = new Dictionary<int, Dictionary<int, Dictionary<int, DayRewardConf>>>();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(obj);
        XmlNode xmlnode = xmlDoc.SelectSingleNode("config");
        xmlnode = xmlnode.SelectSingleNode("signinlist");
        XmlNodeList xmlNodeList = xmlnode.SelectNodes("node");

        foreach (XmlElement item in xmlNodeList)
        {
            int year = 0;
            GeneralUtils.TryParseInt(item.GetAttribute("year"), out year);
            XmlNodeList xmlNodeListOf = item.SelectNodes("item");
            Dictionary<int, Dictionary<int, DayRewardConf>> dayRewardConfMapOfMonth = new Dictionary<int, Dictionary<int, DayRewardConf>>();
            dayRewardConfMapOfYear.Add(year, dayRewardConfMapOfMonth);
            foreach (XmlElement itemOf in xmlNodeListOf)
            {
                int month;
                GeneralUtils.TryParseInt(itemOf.GetAttribute("month"), out month);
                Dictionary<int, DayRewardConf> dayRewardConfMapOfDay = new Dictionary<int, DayRewardConf>();
                dayRewardConfMapOfMonth.Add(month, dayRewardConfMapOfDay);

                string days = itemOf.GetAttribute("days");
                string[] daysStrList = days.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < daysStrList.Length; i++)
                {
                    string[] singleDaysStrList = daysStrList[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    DayRewardConf dayRewardConf = new DayRewardConf();
                    GeneralUtils.TryParseInt(singleDaysStrList[0], out dayRewardConf.day);
                    GeneralUtils.TryParseInt(singleDaysStrList[1], out dayRewardConf.rewardId);
                    GeneralUtils.TryParseInt(singleDaysStrList[2], out dayRewardConf.rewardCount);
                    dayRewardConfMapOfDay.Add(i + 1, dayRewardConf);
                }

                string days7 = itemOf.GetAttribute("days7");
                string days14 = itemOf.GetAttribute("days14");
                string days28 = itemOf.GetAttribute("days28");

                string[] days7StrList = days7.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                DayRewardConf continueDayRewardConf = new DayRewardConf();
                continueDayRewardConf.isDay7 = true;
                GeneralUtils.TryParseInt(days7StrList[0], out continueDayRewardConf.rewardId);
                GeneralUtils.TryParseInt(days7StrList[1], out continueDayRewardConf.rewardCount);
                dayRewardConfMapOfDay.Add((int)ContinueDaysRewardEnum.Day7, continueDayRewardConf);

                string[] days14StrList = days14.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                continueDayRewardConf = new DayRewardConf();
                continueDayRewardConf.isDay14 = true;
                GeneralUtils.TryParseInt(days14StrList[0], out continueDayRewardConf.rewardId);
                GeneralUtils.TryParseInt(days14StrList[1], out continueDayRewardConf.rewardCount);
                dayRewardConfMapOfDay.Add((int)ContinueDaysRewardEnum.Day14, continueDayRewardConf);

                string[] days28StrList = days28.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                continueDayRewardConf = new DayRewardConf();
                continueDayRewardConf.isDay28 = true;
                GeneralUtils.TryParseInt(days28StrList[0], out continueDayRewardConf.rewardId);
                GeneralUtils.TryParseInt(days28StrList[1], out continueDayRewardConf.rewardCount);
                dayRewardConfMapOfDay.Add((int)ContinueDaysRewardEnum.Day28, continueDayRewardConf);
            }
        }
    }

    public DayRewardConf GetDayRewardConf(int year, int month, int day)
    {
        Dictionary<int, Dictionary<int, DayRewardConf>> dayRewardConfMapOfMonth = dayRewardConfMapOfYear[year];
        Dictionary<int, DayRewardConf> dayRewardConfMapOfDay = dayRewardConfMapOfMonth[month];
        return dayRewardConfMapOfDay[day];
    }
}

public class DayRewardConf
{
    public bool isDay7 = false;//32
    public bool isDay14 = false;//33
    public bool isDay28 = false;//34
    public int day;
    public int rewardId;
    public int rewardCount;
}

public enum ContinueDaysRewardEnum
{
    Day7 = 32,
    Day14,
    Day28,
}