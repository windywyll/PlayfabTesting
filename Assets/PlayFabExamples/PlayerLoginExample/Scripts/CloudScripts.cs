using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Collections.Generic;

public class CloudScripts : MonoBehaviour {

    public static void AddVerificationsData(string hash)
    {
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddVerificationsData", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { playerID = "4F68C3924E7C355",
                                    Data = new Dictionary<string, string>()
                                            {
                                              {"Hash", hash},
                                              {"Verified", "false"},
                                            }
            }, // The parameter provided to your function
            RevisionSelection = CloudScriptRevisionOption.Latest,
            GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
        };
        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudAddingDatas, OnErrorShared);
    }

    // C# (Unity3d)
    // Await the response and process the result
    private static void OnCloudAddingDatas(ExecuteCloudScriptResult result)
    {
        // Cloudscript returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        Debug.Log(PlayFab.SimpleJson.SerializeObject(result));
        Debug.Log(PlayFab.SimpleJson.SerializeObject(result.FunctionResult));

        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in Cloud Script

        Debug.Log((string)messageValue);
    }

    private static void OnErrorShared(PlayFabError error)
    {

    }
}
