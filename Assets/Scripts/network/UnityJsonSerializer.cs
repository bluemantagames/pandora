using RestSharp;
using RestSharp.Serialization;
using UnityEngine;

public class UnityJsonSerializer : IRestSerializer
{
    private string UnitySerialize(object obj)
    {
        Debug.Log("Serializing using Unity serializer...");
        return JsonUtility.ToJson(obj);
    }

    private T UnityDeserialize<T>(string json)
    {
        Debug.Log($"Deserializing using Unity serializer... {json}");
        return JsonUtility.FromJson<T>(json);
    }

    public string Serialize(object obj) => UnitySerialize(obj);

    public string Serialize(Parameter bodyParameter) => Serialize(bodyParameter.Value);

    public T Deserialize<T>(IRestResponse response) => UnityDeserialize<T>(response.Content);

    public string[] SupportedContentTypes { get; } =
    {
        "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
    };

    public string ContentType { get; set; } = "application/json";

    public DataFormat DataFormat { get; } = DataFormat.Json;
}