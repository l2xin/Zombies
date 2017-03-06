using System.Collections.Generic;
using System.Xml;

public class DirtyWordConfig
{
    private static Dictionary<string, string> _dictionary;
    static public void Parse(XmlNode node)
    {
        _dictionary = new Dictionary<string, string>();
        string words = (node as XmlElement).GetAttribute("name");
        string[] wordArr = words.Split('|');
        foreach (string word in wordArr)
        {
            if(!string.IsNullOrEmpty(word))
            {
                string temp = word.ToLower();
                if (!_dictionary.ContainsKey(temp))
                {
                    _dictionary.Add(temp, temp);
                }
            }
        }
    }

    public bool CheckIsDirtyWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }
        word = word.Trim('|');
        word = word.Replace("|", "");
        if (!string.IsNullOrEmpty(word))
        {
            word = word.ToLower();
            if (_dictionary.ContainsKey(word))
                return true;
            foreach(string s in _dictionary.Values)
            {
                if(word.Contains(s))
                {
                    return true;
                }
            }
        }
        return false;
    }
}