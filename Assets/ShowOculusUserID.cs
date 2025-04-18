using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

public class ShowOculusUserID : MonoBehaviour
{
    void Start()
    {
        if (!Core.IsInitialized())
        {
            Core.Initialize();
            Debug.Log("🔧 Oculus Core Initialized");
        }

        Users.GetLoggedInUser().OnComplete(OnGetUser);
    }

    void OnGetUser(Message<User> msg)
    {
        if (!msg.IsError)
        {
            Debug.Log($"👤 [Oculus] 현재 로그인된 유저 ID: {msg.Data.ID}");
            Debug.Log($"🧑‍💼 닉네임: {msg.Data.OculusID}");
        }
        else
        {
            Debug.LogError("❌ Oculus 유저 정보를 불러오지 못했습니다: " + msg.GetError().Message);
        }
    }
}
