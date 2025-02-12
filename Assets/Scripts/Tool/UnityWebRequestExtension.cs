using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

public static class UnityWebRequestExtension {
    public static Task<UnityWebRequest.Result> SendWebRequestTask(this UnityWebRequest request) {
        var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
        var operation = request.SendWebRequest();
        operation.completed += _ => {
            if (request.isNetworkError || request.isHttpError) {
                tcs.SetException(new Exception(request.error));
            } else {
                tcs.SetResult(request.result);
            }
            request.Dispose();
        };
        return tcs.Task;
    }
}
