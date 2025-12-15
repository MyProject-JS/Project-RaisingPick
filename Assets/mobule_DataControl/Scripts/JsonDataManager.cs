using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// JSON 직렬화/역직렬화를 사용하여 데이터를 파일로 저장하고 불러오는 정적 클래스입니다.
public static class JsonDataManager
{
    public static string SaveFolder { get; set; }
    private static JsonSerializerSettings _settings;

    static JsonDataManager()
    {
        string rootPath;
#if UNITY_EDITOR
        // 에디터 환경에서는 Assets 폴더를 기준으로 경로를 설정합니다.
        rootPath = Application.dataPath;
#elif UNITY_STANDALONE
        // 독립 실행형 빌드에서는 빌드 폴더(.exe 파일이 있는 곳)를 기준으로 경로를 설정합니다.
        rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
#else
        // 모바일 등 기타 플랫폼에서는 영구적인 데이터 경로를 사용합니다.
        rootPath = Application.persistentDataPath;
#endif
        // 세이브 파일을 저장할 폴더 이름을 지정합니다.
        string saveDirectoryName = "Saves";
        SaveFolder = Path.Combine(rootPath, saveDirectoryName);

        // 세이브 폴더가 존재하지 않으면 새로 생성합니다.
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        // JSON 직렬화 설정을 초기화합니다.
        _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new Vector2Converter(),
                new Vector3Converter(),
                new QuaternionConverter(),
                new ColorConverter(),
            }
        };
    }

    public static void SaveToJson<T>(T data, string fileName = null)
    {
        string path = Path.Combine(SaveFolder, fileName ?? typeof(T).Name + ".json");
        string json = JsonConvert.SerializeObject(data, _settings);
        File.WriteAllText(path, json);
    }

    public static T LoadFromJson<T>(string fileName = null) where T : new()
    {
        string path = Path.Combine(SaveFolder, fileName ?? typeof(T).Name + ".json");
        if (!File.Exists(path)) return new T();
        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }
}

//--- Unity 고유 타입(Vector3, Quaternion 등)을 위한 커스텀 JsonConverter ---//

public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Vector3);

    public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector3(
            jo["x"].Value<float>(),
            jo["y"].Value<float>(),
            jo["z"].Value<float>()
        );
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector3 v = (Vector3)value;
        JObject jo = new JObject
        {
            ["x"] = v.x,
            ["y"] = v.y,
            ["z"] = v.z
        };
        jo.WriteTo(writer);
    }
}

public class QuaternionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Quaternion);

    public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Quaternion(
            jo["x"].Value<float>(),
            jo["y"].Value<float>(),
            jo["z"].Value<float>(),
            jo["w"].Value<float>()
        );
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Quaternion q = (Quaternion)value;
        JObject jo = new JObject
        {
            ["x"] = q.x,
            ["y"] = q.y,
            ["z"] = q.z,
            ["w"] = q.w
        };
        jo.WriteTo(writer);
    }
}

public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Vector2);

    public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Vector2(jo["x"].Value<float>(), jo["y"].Value<float>());
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2 v = (Vector2)value;
        JObject jo = new JObject { ["x"] = v.x, ["y"] = v.y };
        jo.WriteTo(writer);
    }
}

public class ColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Color);

    public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Color(
            jo["r"].Value<float>(),
            jo["g"].Value<float>(),
            jo["b"].Value<float>(),
            jo["a"].Value<float>()
        );
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Color c = (Color)value;
        JObject jo = new JObject
        {
            ["r"] = c.r,
            ["g"] = c.g,
            ["b"] = c.b,
            ["a"] = c.a
        };
        jo.WriteTo(writer);
    }
}