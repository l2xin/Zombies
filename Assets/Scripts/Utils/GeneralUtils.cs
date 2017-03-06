using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// nashnie
/// </summary>
public static class GeneralUtils
{
    public static string ToMd5(string str)
    {
        byte[] result = Encoding.UTF8.GetBytes(str);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] output = md5.ComputeHash(result);
        return (BitConverter.ToString(output).Replace("-", "")).ToLower();
    }

    public static string GetMd5Hash(string input)
    {
        MD5 md5 = MD5.Create();
        string pwd = "";
        byte[] s = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        for (int i = 0; i < s.Length; i++)
        {
            pwd = pwd + s[i].ToString("x2");
        }

        return pwd;
    }

    public static bool CheckNormalString(ref string Name, string defualttext = null)
    {
        if (string.IsNullOrEmpty(Name))
        {
            if (defualttext == null)
            {
                return false;
            }
            else
            {
                Name = defualttext;
                return true;
            }
        }

        Name = Trim(Name, false);
        if (!string.IsNullOrEmpty(Name))
        {
            if (ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(Name))
            {
                //PopMessageUIItem.Show(LanguageManager.GetLanguage("1809"));
                PopManager.ShowSimpleItem("请不要输入敏感词汇哦！");
                return false;
            }
            return true;
        }
        return false;
    }

    public static bool CheckName(ref string Name, bool needcolor = true, bool Cn2 = true)
    {
        if (string.IsNullOrEmpty(Name))
            return false;

        Name = Trim(Name, needcolor);
        if (!string.IsNullOrEmpty(Name))
        {
            if (ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(Name))
            {
                PopManager.ShowSimpleItem("请不要在昵称中输入敏感的词汇哦！");
                return false;
            }
            if (Cn2)
            {
                Name = SupString(Name, 14);
            }
            else
            {
                Name = Name.Substring(0, Mathf.Min(14, Name.Length));
            }
            return true;
        }

        return false;
    }

    public static string Trim(string s, bool needcolor = true)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        s = s.Trim(' ', '\n', (char)0x200a, (char)0x200b, '\u2009', '\u3000');
        s = s.Replace("\n", string.Empty);
        s = s.Replace("　", string.Empty);
        //s = needcolor ? TrimNameEscColor(s) : TrimAll(s);
        return s;
    }

    public static string TrimAll(string s)
    {
        //return NGUIText.StripSymbols(s);
        return "";
    }

    public static int GetStringCharLength(string s)
    {
        if (string.IsNullOrEmpty(s))
            return 0;
        char[] charlist = s.ToCharArray();
        int length = 0;
        foreach (char c in charlist)
        {
            if ((int)c > 255)
            {
                length += 2;
            }
            else
            {
                length += 1;
            }
        }
        return length;
    }

    public static string SupString(string input, int count)
    {
        if (input == null)
            return null;
        List<char> charlist = new List<char>();
        int length = 0;
        foreach (char c in input)
        {
            if ((int)c > 255)
            {
                length += 2;
            }
            else
            {
                length += 1;
            }
            if (length <= count)
                charlist.Add(c);
        }
        return new string(charlist.ToArray());
    }

    public static bool CheckEmail(string email)
    {
        if (email.Contains("@"))
        {
            int at = email.IndexOf('@');
            if (at > 0)
            {
                if (email.Contains("."))
                {
                    int dot = email.IndexOf('.');
                    if (dot - at > 1)
                    {
                        if (email.Split('.').Length == 2)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public static bool CheckAccount(string acc)
    {
        int asc = 0;
        foreach (char c in acc)
        {
            asc = c;
            if (!((asc > 47 && asc < 58) || (asc > 64 && asc < 91) || (asc > 96 && asc < 123)))
            {
                if (asc != 95)
                {
                    if (asc < 255)
                        return false;
                }
            }
        }
        return true;
    }

    public static bool CheckAccountOnlyNumberandEnglish(string acc)
    {
        int asc = 0;
        foreach (char c in acc)
        {
            asc = c;
            if (((asc > 47 && asc < 58) || (asc > 64 && asc < 91) || (asc > 96 && asc < 123)))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CheckAccountWithoutSpace(string acc)
    {
        int asc = 0;
        foreach (char c in acc)
        {
            asc = c;
            if (asc == 32)
            {
                return false;
            }
        }
        return true;
    }

    public static bool CheckPassword(string playername, string pass)
    {
        if (string.IsNullOrEmpty(pass))
        {
            PopManager.ShowSimpleItem("密码不能为空");
            return false;
        }
        if (playername.Equals(pass))
        {
            //PopMessageUIItem.Show(LanguageManager.GetLanguage(1942));
            PopManager.ShowSimpleItem("密码不能与用户名相同");
            return false;
        }
        if (pass.Length < 8 || pass.Length > 14)
        {
            //PopMessageUIItem.Show(LanguageManager.GetLanguage(295));
            PopManager.ShowSimpleItem("密码长度必须介于8~14个字符之间");
            return false;
        }

        int asc = 0;
        bool havenum = false;
        bool haveletter = false;
        foreach (char c in pass)
        {
            asc = c;
            if (!(asc <= 126 && asc >= 33))
            {
                //PopMessageUIItem.Show(LanguageManager.GetLanguage(1940));
                PopManager.ShowSimpleItem("密码只能包含英文字母（区分大小写）、数字字符以及标点符号");
                return false;
            }
            if (asc > 47 && asc < 58)
            {
                havenum = true;
            }
            if ((asc > 64 && asc < 91) || (asc > 96 && asc < 123))
            {
                haveletter = true;
            }
        }

        if ((!havenum) || (!haveletter))
        {
            //PopMessageUIItem.Show(LanguageManager.GetLanguage(1941));
            PopManager.ShowSimpleItem("密码至少包含1个英文字母和1个数字字符");
            return false;
        }

        return true;
    }

    public static bool CheckInputPlayerName(string playername)
    {
        playername = Trim(playername);
        if (string.IsNullOrEmpty(playername))
        {
            //PopMessageUIItem.Show(LanguageManager.GetLanguage("182"));
            PopManager.ShowSimpleItem("输入不能为空哦!");
            return false;
        }
        if (ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(playername))
        {
            //PopMessageUIItem.Show(LanguageManager.GetLanguage("181"));
            PopManager.ShowSimpleItem("请不要在昵称中输入敏感的词汇哦！");
            return false;
        }
        if(playername.Length > 14)
        {
            PopManager.ShowSimpleItem("用户名长度应小于14个字符");
            return false;
        }
        return true;
    }

    public static void AddRenderQueue(GameObject go)
    {
        Renderer[] rlist = go.GetComponentsInChildren<Renderer>(true);
        if (rlist != null)
        {
            foreach (Renderer r in rlist)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m != null)
                        m.renderQueue += 1000;
                }
            }
        }
    }

    public static void SetRenderQueue(GameObject go, int render = 4000)
    {
        Renderer[] rlist = go.GetComponentsInChildren<Renderer>(true);
        if (rlist != null)
        {
            foreach (Renderer r in rlist)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    if (m != null)
                        m.renderQueue = render;
                }
            }
        }
    }

    public static void AddRenderQueueMaterial(GameObject go)
    {
        Renderer[] rlist = go.GetComponentsInChildren<Renderer>(true);
        if (rlist != null)
        {
            foreach (Renderer r in rlist)
            {
                foreach (Material m in r.materials)
                {
                    if (m != null)
                        m.renderQueue += 1000;
                }
            }
        }
    }

    public static void RecursiveAndSetLayer(GameObject gameObject, int layer)
    {
        if (gameObject != null)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                RecursiveAndSetLayer(child.gameObject, layer);
            }
        }
    }

    public static Sprite GetCountryFlag(int countryId)
    {
        if(countryId > 0)
        {
            if(ConfigManager.Instance.countryFlagConfMap.ContainsKey(countryId) == false)
            {
                Debug.Log("countryId.error..." + countryId);
            }
            string flagName = ConfigManager.Instance.countryFlagConfMap[countryId];
            string flagPath = "Flag/" + flagName;
            return Resources.Load<GameObject>(flagPath).GetComponent<SpriteRenderer>().sprite;
        }
        return null;
    }

    public static Sprite GetHeadIcon(int index)
    {
        string iconPath = "HeadIcon/" + index;
        GameObject headIcon = Resources.Load<GameObject>(iconPath);
        if (headIcon != null)
        {
            return headIcon.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            headIcon = Resources.Load<GameObject>("HeadIcon/1");
            if (headIcon != null)
            {
                return headIcon.GetComponent<SpriteRenderer>().sprite;
            }
        }
        return null;
    }

    public static void ScaleParticleSystem(GameObject gameObj, float scale)
    {
        var hasParticleObj = false;
        var particles = gameObj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem p in particles)
        {
            hasParticleObj = true;
            p.startSize *= scale;
            p.startSpeed *= scale;
            p.startRotation *= scale;
            p.transform.localScale *= scale;
        }
        if (hasParticleObj)
        {
            gameObj.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
	#region -> Hry 基于原始物体缩放 <-
	public static void ScaleParticleSystem(GameObject BaseGO, GameObject TargetGO, float scale)
	{
		var hasParticleObj = false;
		ParticleSystem [] BaseParticles = BaseGO.GetComponentsInChildren<ParticleSystem>(true);
		ParticleSystem [] TargetParticales = TargetGO.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0;i<BaseParticles.Length;i++)
		{
			ParticleSystem basePS = BaseParticles [i];
			ParticleSystem targetPS = TargetParticales[i];
			hasParticleObj = true;
			float BaseStartSize = basePS.startSize;
			float BasestartSpeed = basePS.startSpeed;
			float BasestartRotation = basePS.startRotation;
			Vector3 BaselocalScale = basePS.transform.localScale;

			targetPS.startSize = BaseStartSize * scale;
			targetPS.startSpeed = BasestartSpeed * scale;
			targetPS.startRotation = BasestartRotation * scale;
			targetPS.transform.localScale = BaselocalScale * scale;
		}
		if (hasParticleObj)
		{
			TargetGO.transform.localScale = new Vector3(scale, scale, 1);
		}
	}
	#endregion
    public static string GeneralVersion(int versionNum)
    {
        int firstVer = versionNum / 100;
        int secondVer = (versionNum - firstVer * 100) / 10;
        int thirdVer = versionNum - firstVer * 100 - secondVer * 10;
        return firstVer + "." + secondVer + "." + thirdVer;
    }

    public static void SetHeroShaderGhost(Material[] materials, bool isOpen)
    {
        for (int j = 0; j < materials.Length; j++)
        {
            Material material = materials[j];
            int param = isOpen == true ? 1 : 0;
            material.SetInt("_Ghost", param);
        }
    }

    public static void SetHeroShaderGhost(List<Material[]> rendererList, bool isOpen)
    {
        for (int i = 0; i < rendererList.Count; i++)
        {
            Material[] materials = rendererList[i];
            SetHeroShaderGhost(materials, isOpen);
        }
    }


    public static Vector2 AngleSpeed(Vector2 v1, Vector2 v2)
    {
        float radians = (float)Math.Atan2(v2.y - v1.y, v2.x - v1.x);
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
    
    public const float DEVELOPMENT_BUILD_TEXT_WIDTH = 115f;

    public static bool AreArraysEqual<T>(T[] arr1, T[] arr2)
    {
        if (arr1 != arr2)
        {
            if (arr1 == null)
            {
                return false;
            }
            if (arr2 == null)
            {
                return false;
            }
            if (arr1.Length != arr2.Length)
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static bool AreBytesEqual(byte[] bytes1, byte[] bytes2)
    {
        return AreArraysEqual<byte>(bytes1, bytes2);
    }

    public static bool CallbackIsValid(Delegate callback)
    {
        bool flag = true;
        if (callback == null)
        {
            return false;
        }
        if (!callback.Method.IsStatic)
        {
            flag = IsObjectAlive(callback.Target);
            if (!flag)
            {
                Debug.Log("Target for callback " + callback.Method.Name + " is null.");
            }
        }
        return flag;
    }

    private static object CloneClass(object obj, System.Type objType)
    {
        object obj2 = CreateNewType(objType);
        foreach (FieldInfo info in objType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            info.SetValue(obj2, CloneValue(info.GetValue(obj), info.FieldType));
        }
        return obj2;
    }

    private static object CloneValue(object src, System.Type type)
    {
        if (((src == null) || (type == typeof(string))) || !type.IsClass)
        {
            return src;
        }
        if (!type.IsGenericType)
        {
            return CloneClass(src, type);
        }
        if (src is IDictionary)
        {
            IDictionary dictionary = src as IDictionary;
            IDictionary dictionary2 = CreateNewType(type) as IDictionary;
            System.Type type2 = type.GetGenericArguments()[0];
            System.Type type3 = type.GetGenericArguments()[1];
            IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    dictionary2.Add(CloneValue(current.Key, type2), CloneValue(current.Value, type3));
                }
            }
            finally
            {
                IDisposable disposable = enumerator as IDisposable;
                if (disposable == null)
                {
                }
                disposable.Dispose();
            }
            return dictionary2;
        }
        if (!(src is IList))
        {
            return src;
        }
        IList list = src as IList;
        IList list2 = CreateNewType(type) as IList;
        System.Type type4 = type.GetGenericArguments()[0];
        IEnumerator enumerator2 = list.GetEnumerator();
        try
        {
            while (enumerator2.MoveNext())
            {
                object obj2 = enumerator2.Current;
                list2.Add(CloneValue(obj2, type4));
            }
        }
        finally
        {
            IDisposable disposable2 = enumerator2 as IDisposable;
            if (disposable2 == null)
            {
            }
            disposable2.Dispose();
        }
        return list2;
    }

    public static T[] Combine<T>(T[] arr1, T[] arr2)
    {
        T[] destinationArray = new T[arr1.Length + arr2.Length];
        Array.Copy(arr1, 0, destinationArray, 0, arr1.Length);
        Array.Copy(arr1, 0, destinationArray, arr1.Length, arr2.Length);
        return destinationArray;
    }

    public static bool Contains(this string str, string val, StringComparison comparison)
    {
        return (str.IndexOf(val, comparison) >= 0);
    }

    private static object CreateNewType(System.Type type)
    {
        object obj2 = Activator.CreateInstance(type);
        if (obj2 == null)
        {
            throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", type.Name));
        }
        return obj2;
    }

    public static T DeepClone<T>(T obj)
    {
        return (T) CloneValue(obj, obj.GetType());
    }

    public static void DeepReset<T>(T obj)
    {
        System.Type type = typeof(T);
        T local = Activator.CreateInstance<T>();
        if (local == null)
        {
            throw new SystemException(string.Format("Unable to instantiate type {0} with default constructor.", type.Name));
        }
        foreach (FieldInfo info in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            info.SetValue(obj, info.GetValue(local));
        }
    }

    public static void ExitApplication()
    {
        Application.Quit();
    }

    public static bool ForceBool(string strVal)
    {
        string str = strVal.ToLowerInvariant().Trim();
        if ((!(str == "on") && !(str == "1")) && !(str == "true"))
        {
            return false;
        }
        return true;
    }

    public static float ForceFloat(string str)
    {
        float val = 0f;
        TryParseFloat(str, out val);
        return val;
    }

    public static int ForceInt(string str)
    {
        int val = 0;
        TryParseInt(str, out val);
        return val;
    }

    public static long ForceLong(string str)
    {
        long val = 0L;
        TryParseLong(str, out val);
        return val;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
    {
        if (enumerable != null)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    func(current);
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> func)
    {
        if (enumerable != null)
        {
            int num = 0;
            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    func(current, num);
                    num++;
                }
            }
            finally
            {
                if (enumerator == null)
                {
                }
                enumerator.Dispose();
            }
        }
    }

    public static void ForEachReassign<T>(this T[] array, Func<T, T> func)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i]);
            }
        }
    }

    public static void ForEachReassign<T>(this T[] array, Func<T, int, T> func)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = func(array[i], i);
            }
        }
    }

    public static bool IsDevelopmentBuildTextVisible()
    {
        return Debug.isDebugBuild;
    }

    public static bool IsEditorPlaying()
    {
        return false;
    }

    public static bool IsObjectAlive(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (!(obj is UnityEngine.Object))
        {
            return true;
        }
        UnityEngine.Object obj2 = (UnityEngine.Object) obj;
        return (bool) obj2;
    }

    public static bool IsOverriddenMethod(MethodInfo childMethod, MethodInfo ancestorMethod)
    {
        if (childMethod == null)
        {
            return false;
        }
        if (ancestorMethod == null)
        {
            return false;
        }
        if (childMethod.Equals(ancestorMethod))
        {
            return false;
        }
        MethodInfo baseDefinition = childMethod.GetBaseDefinition();
        while (!baseDefinition.Equals(childMethod) && !baseDefinition.Equals(ancestorMethod))
        {
            MethodInfo info2 = baseDefinition;
            baseDefinition = baseDefinition.GetBaseDefinition();
            if (baseDefinition.Equals(info2))
            {
                return false;
            }
        }
        return baseDefinition.Equals(ancestorMethod);
    }

    public static void ListMove<T>(IList<T> list, int srcIndex, int dstIndex)
    {
        if (srcIndex != dstIndex)
        {
            T item = list[srcIndex];
            list.RemoveAt(srcIndex);
            if (dstIndex > srcIndex)
            {
                dstIndex--;
            }
            list.Insert(dstIndex, item);
        }
    }

    public static void ListSwap<T>(IList<T> list, int indexA, int indexB)
    {
        T local = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = local;
    }

    public static bool RandomBool()
    {
        return (UnityEngine.Random.Range(0, 2) == 0);
    }

    public static T[] Slice<T>(this T[] arr)
    {
        return arr.Slice<T>(0, arr.Length);
    }

    public static T[] Slice<T>(this T[] arr, int start)
    {
        return arr.Slice<T>(start, arr.Length);
    }

    public static T[] Slice<T>(this T[] arr, int start, int end)
    {
        int length = arr.Length;
        if (start < 0)
        {
            start = length + start;
        }
        if (end < 0)
        {
            end = length + end;
        }
        int num2 = end - start;
        if (num2 <= 0)
        {
            return new T[0];
        }
        int num3 = length - start;
        if (num2 > num3)
        {
            num2 = num3;
        }
        T[] destinationArray = new T[num2];
        Array.Copy(arr, start, destinationArray, 0, num2);
        return destinationArray;
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T local = a;
        a = b;
        b = local;
    }

    public static bool TryParseBool(string strVal, out bool boolVal)
    {
        switch (strVal.ToLowerInvariant().Trim())
        {
            case "off":
            case "0":
            case "false":
                boolVal = false;
                return true;

            case "on":
            case "1":
            case "true":
                boolVal = true;
                return true;
        }
        boolVal = false;
        return false;
    }

    public static bool TryParseFloat(string str, out float val)
    {
        return float.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static bool TryParseInt(string str, out int val)
    {
        return int.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static bool TryParseLong(string str, out long val)
    {
        return long.TryParse(str, NumberStyles.Any, null, out val);
    }

    public static int UnsignedMod(int x, int y)
    {
        int num = x % y;
        if (num < 0)
        {
            num += y;
        }
        return num;
    }
}

