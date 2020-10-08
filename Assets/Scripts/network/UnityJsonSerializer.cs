using RestSharp;
using RestSharp.Serialization;
using UnityEngine;

public class UnityJsonSerializer : IRestSerializer
{
    private string UnitySerialize(object obj)
    {
        var serialized = JsonUtility.ToJson(obj);
        Logger.Debug($"Serializing using Unity serializer: {serialized}");

        return serialized;
    }

    private T UnityDeserialize<T>(string json)
    {
        Logger.Debug($"Deserializing using Unity serializer: {json}");
        return JsonUtility.FromJson<T>(json);
    }

    public string Serialize(object obj) => UnitySerialize(obj);

    public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T Deserialize<T>(IRestResponse response) => UnityDeserialize<T>(response.Content);

    public T Deserialize<T>(string encoded) => UnityDeserialize<T>(encoded);

    public string[] SupportedContentTypes { get; } =
    {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat { get; } = DataFormat.Json;
}