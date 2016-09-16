using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System.Collections.Generic;

public class CloudScripts{

    //The function pass a hash that could help to do an e-mail verification by passing it in the URL.
    //then catch it on a webpage with the Javascript SDK of Playfab to validate the player on the Server.
    public static void AddVerificationsData(string hash, string PlayFabId)
    {
        ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddVerificationsData", // Arbitrary function name, must exist in the cloudscript in playfab
                                                   // to find it login on playfab select the wanted game then server. 
            FunctionParameter = new { playerID = PlayFabId,
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
    
    // Success callback of the request.
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

    //Error callback of the request
    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }
}
