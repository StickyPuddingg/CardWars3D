#define ASSERTS_ON
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;
using UnityEngine;

public class TFUtils
{
    private const bool CheckKeyAssert = false;

    public const string DEBUG_CHANNEL_IAP = "iap";
    public const string DEBUG_CHANNEL_AI = "ai";
    public const string DEBUG_CHANNEL_SAVE_LOAD = "saveload";
    public const string DEBUG_CHANNEL_CALENDAR = "calendar";
    public const string DEBUG_CHANNEL_SESSION = "session";

    public static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static string DeviceID;
    public static string DeviceName;
    private static string cachedPersistentAssetsPath;
    private static string cachedStreamingAssetsPath;


    private static HashAlgorithm hash;

    public static DateTime lastServerTimeUpdate = new DateTime(0L);
    public static TimeSpan serverTimeDiff = new TimeSpan(0L);

    public static List<string> activeDebugChannels = new List<string>();
    public static bool strictDebugChannelMode = false;
    private static bool debugChannelsInitialized = false;

    public static DateTime ServerTime
    {
        get
        {
            return DateTime.Now + serverTimeDiff;
        }
    }

    public static void Init()
    {
        DeviceID = SystemInfo.deviceUniqueIdentifier;
        DeviceName = SystemInfo.deviceName;

        // Cache the combined path safely from the main thread
        cachedPersistentAssetsPath = Path.Combine(Application.persistentDataPath, "Contents");
        cachedStreamingAssetsPath = Application.streamingAssetsPath;
    }

    public static bool IsServerTimeValid()
    {
        return true;
    }

    public static int EpochTime()
    {
        return EpochTime(DateTime.UtcNow);
    }

    public static int EpochTime(DateTime dt)
    {
        return (int)(dt - EPOCH).TotalSeconds;
    }

    public static DateTime EpochToDateTime(int seconds)
    {
        return EPOCH.AddSeconds(seconds);
    }

    public static string DurationToString(int duration)
    {
        if (duration < 60)
        {
            return string.Format("{0}s", duration);
        }

        int num = duration % 60;
        duration -= num;
        int num2 = duration / 60;

        if (num2 < 60)
        {
            return num == 0 ? string.Format("{0}m", num2) : string.Format("{0}m {1}s", num2, num);
        }

        int num3 = num2 / 60;
        num2 %= 60;

        if (num3 < 24)
        {
            return num2 == 0 ? string.Format("{0}h", num3) : string.Format("{0}h {1}m", num3, num2);
        }

        int num4 = num3 / 24;
        num3 %= 24;

        return num3 == 0 ? string.Format("{0}d", num4) : string.Format("{0}d {1}h", num4, num3);
    }

    public static Dictionary<KeyType, ValueType> CloneDictionary<KeyType, ValueType>(Dictionary<KeyType, ValueType> source)
    {
        Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>(source.Count);
        foreach (KeyValuePair<KeyType, ValueType> kvp in source)
        {
            dictionary[kvp.Key] = kvp.Value;
        }
        return dictionary;
    }

    public static void CloneDictionaryInPlace<KeyType, ValueType>(Dictionary<KeyType, ValueType> source, Dictionary<KeyType, ValueType> dest)
    {
        dest.Clear();
        foreach (KeyValuePair<KeyType, ValueType> item in source)
        {
            dest.Add(item.Key, item.Value);
        }
    }

    public static Dictionary<KeyType, ValueType> ConcatenateDictionaryInPlace<KeyType, ValueType>(Dictionary<KeyType, ValueType> dest, Dictionary<KeyType, ValueType> source)
    {
        foreach (KeyValuePair<KeyType, ValueType> kvp in source)
        {
            if (dest.ContainsKey(kvp.Key))
            {
                throw new ArgumentException("Destination dictionary already contains key " + kvp.Key.ToString());
            }
            dest[kvp.Key] = kvp.Value;
        }
        return dest;
    }

    public static List<To> CloneAndCastList<From, To>(List<From> list) where From : To
    {
        List<To> list2 = new List<To>(list.Count);
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            list2.Add(list[i]);
        }
        return list2;
    }

    private static T AssertCast<T>(Dictionary<string, object> dict, string key)
    {
        return (T)dict[key];
    }

    public static List<T> TryLoadList<T>(Dictionary<string, object> data, string key)
    {
        return !data.ContainsKey(key) ? null : LoadList<T>(data, key);
    }

    public static List<T> LoadList<T>(Dictionary<string, object> data, string key)
    {
        if (data[key] is List<T>)
        {
            return (List<T>)data[key];
        }

        List<object> list = (List<object>)data[key];
        List<T> retval = new List<T>(list.Count);
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            retval.Add((T)Convert.ChangeType(list[i], typeof(T)));
        }
        return retval;
    }

    public static Dictionary<string, object> LoadDict(Dictionary<string, object> data, string key)
    {
        return (Dictionary<string, object>)data[key];
    }

    public static Dictionary<string, object> TryLoadDict(Dictionary<string, object> data, string key)
    {
        object val;
        return data.TryGetValue(key, out val) ? (Dictionary<string, object>)val : null;
    }

    public static string LoadString(Dictionary<string, object> data, string key, string defaultValue)
    {
        string result = TryLoadString(data, key);
        return result != null ? result : defaultValue;
    }

    public static string LoadString(Dictionary<string, object> data, string key)
    {
        return AssertCast<string>(data, key);
    }

    public static string TryLoadString(Dictionary<string, object> data, string key)
    {
        object val;
        return data.TryGetValue(key, out val) ? (string)val : null;
    }

    public static string LoadLocalizedString(Dictionary<string, object> data, string key, string defaultValue)
    {
        return KFFLocalization.Get(LoadString(data, key, defaultValue));
    }

    public static string LoadLocalizedString(Dictionary<string, object> data, string key)
    {
        return KFFLocalization.Get(LoadString(data, key));
    }

    public static string TryLoadLocalizedString(Dictionary<string, object> data, string key)
    {
        return KFFLocalization.Get(TryLoadString(data, key));
    }

    public static string LoadNullableString(Dictionary<string, object> data, string key)
    {
        object val;
        return data.TryGetValue(key, out val) ? (string)val : null;
    }

    public static List<int> LoadRange(Dictionary<string, object> data, string key)
    {
        string text = LoadString(data, key, string.Empty);
        return string.IsNullOrEmpty(text) ? new List<int>() : Range.Interpret(text);
    }

    public static int? LoadNullableInt(Dictionary<string, object> d, string key)
    {
        object obj;
        if (d.TryGetValue(key, out obj) && obj != null)
        {
            return Convert.ToInt32(obj);
        }
        return null;
    }

    public static int? TryLoadNullableInt(Dictionary<string, object> d, string key)
    {
        object val;
        if (d.TryGetValue(key, out val) && val != null)
        {
            try
            {
                if (val is int)
                {
                    return (int)val;
                }
                if (val is long)
                {
                    return (int)(long)val;
                }
                if (val is float)
                {
                    return (int)Math.Floor((float)val + 0.5f);
                }
                if (val is double)
                {
                    return (int)Math.Floor((double)val + 0.5);
                }
                if (val is string)
                {
                    string s = (string)val;
                    if (string.IsNullOrEmpty(s))
                    {
                        return null;
                    }
                    int parsedInt;
                    if (int.TryParse(s, out parsedInt))
                    {
                        return parsedInt;
                    }
                    float parsedFloat;
                    if (float.TryParse(s, out parsedFloat))
                    {
                        return (int)Math.Floor(parsedFloat + 0.5f);
                    }
                }
                return Convert.ToInt32(val);
            }
            catch (Exception)
            {
                TFUtils.DebugLog("TFUtils.TryLoadNullableInt: failed to parse key '" + key + "' value '" + val + "'");
                return null;
            }
        }
        return null;
    }

    public static uint? LoadNullableUInt(Dictionary<string, object> d, string key)
    {
        object obj;
        return d.TryGetValue(key, out obj) && obj != null ? LoadUint(d, key) : (uint?)null;
    }

    public static uint? TryLoadNullableUInt(Dictionary<string, object> d, string key)
    {
        return d.ContainsKey(key) ? LoadNullableUInt(d, key) : null;
    }

    public static object NullableToObject<T>(T? nullable) where T : struct
    {
        return !nullable.HasValue ? null : (object)nullable.Value;
    }

    public static int? TryLoadInt(Dictionary<string, object> data, string key)
    {
        return data.ContainsKey(key) ? LoadIntHelper(data, key) : (int?)null;
    }

    public static bool LoadBoolAsInt(Dictionary<string, object> d, string key)
    {
        return LoadInt(d, key) != 0;
    }

    public static bool LoadBoolAsInt(Dictionary<string, object> d, string key, bool defaultValue)
    {
        return LoadInt(d, key, defaultValue ? 1 : 0) != 0;
    }

    public static bool LoadBool(Dictionary<string, object> d, string key, bool defaultValue)
    {
        object obj;
        if (d.TryGetValue(key, out obj))
        {
            if (obj is int) return ((int)obj) != 0;
            if (obj is string)
            {
                bool result;
                if (bool.TryParse((string)obj, out result)) return result;
            }
        }
        return defaultValue;
    }

    public static bool? TryLoadNullableBool(Dictionary<string, object> d, string key)
    {
        object val;
        bool res;
        return d.TryGetValue(key, out val) && bool.TryParse(val.ToString(), out res) ? res : (bool?)null;
    }

    public static int LoadInt(Dictionary<string, object> d, string key)
    {
        return LoadIntHelper(d, key);
    }

    public static int LoadInt(Dictionary<string, object> d, string key, int defaultValue)
    {
        object obj;
        if (d.TryGetValue(key, out obj) && (!(obj is string) || ((string)obj).Length > 0))
        {
            return (int)Math.Floor(Convert.ToSingle(obj) + 0.5f);
        }
        return defaultValue;
    }

    private static int LoadIntHelper(Dictionary<string, object> d, string key)
    {
        return (int)Math.Floor(Convert.ToSingle(d[key]) + 0.5f);
    }

    public static uint LoadUint(Dictionary<string, object> data, string key)
    {
        return LoadUintHelper(data, key);
    }

    public static uint? TryLoadUint(Dictionary<string, object> data, string key)
    {
        return !data.ContainsKey(key) ? null : (uint?)LoadUintHelper(data, key);
    }

    private static uint LoadUintHelper(Dictionary<string, object> data, string key)
    {
        return Convert.ToUInt32(data[key]);
    }

    public static float? TryLoadNullableFloat(Dictionary<string, object> d, string key)
    {
        object val;
        return d.TryGetValue(key, out val) ? Convert.ToSingle(val) : (float?)null;
    }

    public static float? TryLoadFloat(Dictionary<string, object> data, string key)
    {
        object val;
        return data.TryGetValue(key, out val) ? Convert.ToSingle(val) : (float?)null;
    }

    public static float LoadFloat(Dictionary<string, object> d, string key)
    {
        return Convert.ToSingle(d[key]);
    }

    public static float LoadFloat(Dictionary<string, object> d, string key, float defaultValue)
    {
        object obj;
        if (d.TryGetValue(key, out obj) && (!(obj is string) || ((string)obj).Length > 0))
        {
            return Convert.ToSingle(obj);
        }
        return defaultValue;
    }

    public static void LoadVector3(out Vector3 v3, Dictionary<string, object> d, float defaultValue)
    {
        object x, y, z;
        v3.x = !d.TryGetValue("x", out x) ? defaultValue : Convert.ToSingle(x);
        v3.y = !d.TryGetValue("y", out y) ? defaultValue : Convert.ToSingle(y);
        v3.z = !d.TryGetValue("z", out z) ? defaultValue : Convert.ToSingle(z);
    }

    public static void SaveVector3(Vector3 v3, string name, Dictionary<string, object> d)
    {
        Dictionary<string, object> coords = new Dictionary<string, object>();
        coords.Add("x", v3.x);
        coords.Add("y", v3.y);
        coords.Add("z", v3.z);
        d[name] = coords;
    }

    public static void LoadVector2(out Vector2 v2, Dictionary<string, object> d, float defaultValue)
    {
        object x, y;
        v2.x = !d.TryGetValue("x", out x) ? defaultValue : Convert.ToSingle(x);
        v2.y = !d.TryGetValue("y", out y) ? defaultValue : Convert.ToSingle(y);
    }

    public static void LoadVector3(out Vector3 v3, Dictionary<string, object> d)
    {
        LoadVector3(out v3, d, 0f);
    }

    public static void LoadVector2(out Vector2 v2, Dictionary<string, object> d)
    {
        LoadVector2(out v2, d, 0f);
    }

    public static Vector3 ExpandVector(Vector2 vector)
    {
        return new Vector3(vector.x, vector.y, 0f);
    }

    public static Vector3 ExpandVector(Vector2 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static Vector2 TruncateVector(Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static void TruncateFile(string filePath)
    {
        DeleteFile(filePath);
        using (FileStream fileStream = File.Create(filePath)) { fileStream.Close(); }
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    public static string GetPersistentAssetsPath()
    {
        // Fallback safety check in case Init() hasn't executed yet
        if (string.IsNullOrEmpty(cachedPersistentAssetsPath))
        {
            // Only use the native call if we are currently on the main thread
            cachedPersistentAssetsPath = Path.Combine(Application.persistentDataPath, "Contents");
        }
        return cachedPersistentAssetsPath;
    }
    public static string GetStreamingAssetsPath()
    {
        // Fallback safety check in case Init() hasn't executed yet
        if (string.IsNullOrEmpty(cachedStreamingAssetsPath))
        {
            cachedStreamingAssetsPath = Application.streamingAssetsPath;
        }
        return cachedStreamingAssetsPath;
    }

    public static string GetStreamingAssetsSubfolder(string path)
    {
        return GetStreamingAssetsPath() + Path.DirectorySeparatorChar + path;
    }

    public static string GetStreamingAssetsFileInDirectory(string path, string filename)
    {
        return GetStreamingAssetsFile(path + Path.DirectorySeparatorChar + filename);
    }

    public static string GetStreamingAssetsFile(string fileName)
    {
        string text = GetPersistentAssetsPath() + Path.DirectorySeparatorChar + fileName;
        return File.Exists(text) ? text : GetStreamingAssetsPath() + Path.DirectorySeparatorChar + fileName;
    }

    public static string GetJsonFileContent(string filename)
    {
        return File.ReadAllText(GetStreamingAssetsFile(filename));
    }

    public static string GetJsonLocalContent(string filename)
    {
        return File.ReadAllText(filename);
    }

    public static string[] GetFilesInPath(string path, string searchPattern)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        string streamingAssetsSubfolder = GetStreamingAssetsSubfolder(path);
        string streamingAssetsPath = GetStreamingAssetsPath();

        if (Directory.Exists(streamingAssetsSubfolder))
        {
            string[] files = Directory.GetFiles(streamingAssetsSubfolder, searchPattern, SearchOption.AllDirectories);
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                dictionary[files[i].Substring(streamingAssetsPath.Length)] = files[i];
            }
        }

        string path2 = GetPersistentAssetsPath() + Path.DirectorySeparatorChar + path;
        if (Directory.Exists(path2))
        {
            string[] files2 = Directory.GetFiles(path2, searchPattern, SearchOption.AllDirectories);
            string persistentAssetsPath = GetPersistentAssetsPath();
            int length2 = files2.Length;
            for (int i = 0; i < length2; i++)
            {
                dictionary[files2[i].Substring(persistentAssetsPath.Length)] = files2[i];
            }
        }

        string[] array = new string[dictionary.Count];
        dictionary.Values.CopyTo(array, 0);
        return array;
    }

    [Conditional("UNITY_EDITOR")]
    public static void DebugDict(Dictionary<string, object> d) { }

    [Conditional("UNITY_EDITOR")]
    public static void LogFormat(string format, params object[] args) { }

    private static void SetupDebugChannels()
    {
        strictDebugChannelMode = false;
        string path = GetStreamingAssetsFile("debugLogChannels.txt");
        if (!File.Exists(path)) return;

        using (StreamReader reader = new StreamReader(path))
        {
            string line = reader.ReadLine();
            if (line != null && line.StartsWith("1")) strictDebugChannelMode = true;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length > 0 && !line.StartsWith("//") && !line.StartsWith("#"))
                {
                    EnableDebugChannel(line);
                }
            }
        }
        debugChannelsInitialized = true;
    }

    public static void EnableDebugChannel(string channel)
    {
        if (!activeDebugChannels.Contains(channel)) activeDebugChannels.Add(channel);
    }

    public static void DisableDebugChannel(string channel)
    {
        activeDebugChannels.Remove(channel);
    }

    private static void InitLog()
    {
        if (!debugChannelsInitialized) SetupDebugChannels();
    }

    private static bool ShouldLog(string channel, bool bypassStrictMode)
    {
        if (strictDebugChannelMode && !bypassStrictMode && (channel == null || !activeDebugChannels.Contains(channel)))
        {
            return false;
        }
        return true;
    }

    public static void DebugLog(object message, string channel = null, bool bypassStrictMode = true)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log(message);
#endif
    }

    public static void ErrorLog(object message, string channel = null, bool bypassStrictMode = true)
    {
        UnityEngine.Debug.LogError(message);
    }

    public static void WarnLog(object message, string channel = null, bool bypassStrictMode = true)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("DEBUG")]
    public static void UnexpectedEntry()
    {
        throw new Exception("Unexpected path of code execution!");
    }

    [Conditional("ASSERTS_ON")]
    public static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    public static GameObject FindGameObjectInHierarchy(GameObject root, string name)
    {
        if (root.name == name) return root;

        Transform transform = root.transform;
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject result = FindGameObjectInHierarchy(transform.GetChild(i).gameObject, name);
            if (result != null) return result;
        }
        return null;
    }

    public static GameObject FindParentGameObjectInHierarchy(GameObject root, string name)
    {
        Transform transform = root.transform;
        while (transform.parent != null)
        {
            if (transform.gameObject.name == name) return transform.gameObject;
            transform = transform.parent;
        }
        return null;
    }

    public static byte[] Zip(string str)
    {
        return Zip(Encoding.UTF8.GetBytes(str));
    }

    public static byte[] Zip(byte[] bytedata)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(bytedata, 0, bytedata.Length);
            }
            return memoryStream.ToArray();
        }
    }

    public static byte[] UnzipToBytes(byte[] input)
    {
        using (MemoryStream stream = new MemoryStream(input))
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                byte[] array = new byte[1024];
                int num;
                while ((num = gZipStream.Read(array, 0, array.Length)) > 0)
                {
                    memoryStream.Write(array, 0, num);
                }
            }
            return memoryStream.ToArray();
        }
    }

    public static string Unzip(byte[] input)
    {
        return Encoding.UTF8.GetString(UnzipToBytes(input));
    }

    public static int BoolToInt(bool myBool)
    {
        return myBool ? 1 : 0;
    }

    public static void WriteFile(string filename, string data)
    {
        File.WriteAllText(Path.Combine(Application.persistentDataPath, Path.GetFileName(filename)), data);
    }

    public static string ReadFile(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            throw new FileNotFoundException("ReadFile filename is null or empty.");
        }
        string filePath = Path.Combine(Application.persistentDataPath, Path.GetFileName(filename));
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found: " + filename);
        }
        return File.ReadAllText(filePath);
    }

    public static void PlayMovie(string movie, bool canSkip = true)
    {
        //Handheld.PlayFullScreenMovie(movie, Color.black, (!canSkip) ? FullScreenMovieControlMode.Hidden : FullScreenMovieControlMode.CancelOnInput);
    }

    public static string ComputeDigest(string input)
    {
        if (input == null) input = string.Empty;
        if (hash == null) hash = MD5.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] array = hash.ComputeHash(bytes);

        StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i].ToString("X2"));
        }
        return stringBuilder.ToString();
    }
}