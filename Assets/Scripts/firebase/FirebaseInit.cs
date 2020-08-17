using UnityEngine;
//using Firebase;

namespace Pandora.Analytics
{
    public class FirebaseInit : MonoBehaviour
    {
        void Start()
        {/*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseModule.app = Firebase.FirebaseApp.DefaultInstance;
                }
                else
                {
                    UnityEngine.Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                }
            });*/
        }


    }
}