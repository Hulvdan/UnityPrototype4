using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
/// <summary>
///     This is a generic Singleton implementation for Monobehaviours.
///     Create a derived class where the type T is the script you want to "Singletonize"
///     Upon loading it will call DontDestroyOnLoad on the gameobject where this script is contained
///     so it persists upon scene changes.
/// </summary>
/// <remarks>
///     DO NOT REDEFINE Awake() Start() or OnDestroy() in derived classes. EVER.
///     Instead, use protected virtual methods:
///     SingletonAwakened()
///     SingletonStarted()
///     SingletonDestroyed()
///     to perform the initialization/cleanup: those methods are guaranteed to only be called once in
///     the
///     entire lifetime of the MonoBehaviour
/// </remarks>
public class SingletonMB<T> : MonoBehaviour where T : MonoBehaviour {
    /// <summary>
    ///     <c>true</c> if this Singleton Awake() method has already been called by Unity; otherwise,
    ///     <c>false</c>.
    /// </summary>
    public static bool isAwakened { get; private set; }

    /// <summary>
    ///     <c>true</c> if this Singleton Start() method has already been called by Unity; otherwise,
    ///     <c>false</c>.
    /// </summary>
    public static bool isStarted { get; private set; }

    /// <summary>
    ///     <c>true</c> if this Singleton OnDestroy() method has already been called by Unity; otherwise,
    ///     <c>false</c>.
    /// </summary>
    public static bool isDestroyed { get; private set; }

    /// <summary>
    ///     Global access point to the unique instance of this class.
    /// </summary>
    public static T instance {
        get {
            if (_instance == null) {
                if (isDestroyed) {
                    return null;
                }

                _instance = FindExistingInstance() ?? CreateNewInstance();
            }

            return _instance;
        }
    }

    #region Singleton Implementation

    /// <summary>
    ///     Holds the unique instance for this class
    /// </summary>
    static T _instance;

    /// <summary>
    ///     Finds an existing instance of this singleton in the scene.
    /// </summary>
    static T FindExistingInstance() {
        var existingInstances = FindObjectsOfType<T>();

        // No instance found
        if (existingInstances == null || existingInstances.Length == 0) {
            return null;
        }

        return existingInstances[0];
    }

    /// <summary>
    ///     If no instance of the T MonoBehaviour exists, creates a new GameObject in the scene
    ///     and adds T to it.
    /// </summary>
    static T CreateNewInstance() {
        var containerGo = new GameObject("__" + typeof(T).Name + " (Singleton)");
        return containerGo.AddComponent<T>();
    }

    #endregion

    #region Singleton Life-time Management

    /// <summary>
    ///     Unity3D Awake method.
    /// </summary>
    /// <remarks>
    ///     This method will only be called once even if multiple instances of the
    ///     singleton MonoBehaviour exist in the scene.
    ///     You can override this method in derived classes to customize the initialization of your
    ///     MonoBehaviour
    /// </remarks>
    protected virtual void SingletonAwakened() {
    }

    /// <summary>
    ///     Unity3D Start method.
    /// </summary>
    /// <remarks>
    ///     This method will only be called once even if multiple instances of the
    ///     singleton MonoBehaviour exist in the scene.
    ///     You can override this method in derived classes to customize the initialization of your
    ///     MonoBehaviour
    /// </remarks>
    protected virtual void SingletonStarted() {
    }

    /// <summary>
    ///     Unity3D OnDestroy method.
    /// </summary>
    /// <remarks>
    ///     This method will only be called once even if multiple instances of the
    ///     singleton MonoBehaviour exist in the scene.
    ///     You can override this method in derived classes to customize the initialization of your
    ///     MonoBehaviour
    /// </remarks>
    protected virtual void SingletonDestroyed() {
    }

    /// <summary>
    ///     If a duplicated instance of a Singleton MonoBehaviour is loaded into the scene
    ///     this method will be called instead of SingletonAwakened(). That way you can customize
    ///     what to do with repeated instances.
    /// </summary>
    /// <remarks>
    ///     The default approach is delete the duplicated MonoBehaviour
    /// </remarks>
    protected virtual void NotifyInstanceRepeated() {
        Destroy(GetComponent<T>());
    }

    #endregion

    #region Unity3d Messages - DO NOT OVERRRIDE / IMPLEMENT THESE METHODS in child classes!

    void Awake() {
        var thisInstance = GetComponent<T>();

        // Initialize the singleton if the script is already in the scene in a GameObject
        if (_instance == null) {
            _instance = thisInstance;
            DontDestroyOnLoad(_instance.gameObject);
        }

        else if (thisInstance != _instance) {
            PrintWarn(string.Format(
                "Found a duplicated instance of a Singleton with type {0} in the GameObject {1}",
                GetType(), gameObject.name));

            NotifyInstanceRepeated();

            return;
        }

        if (!isAwakened) {
            PrintLog(string.Format(
                "Awake() Singleton with type {0} in the GameObject {1}",
                GetType(), gameObject.name));

            SingletonAwakened();
            isAwakened = true;
        }
    }

    void Start() {
        // do not start it twice
        if (isStarted) {
            return;
        }

        PrintLog(string.Format(
            "Start() Singleton with type {0} in the GameObject {1}",
            GetType(), gameObject.name));

        SingletonStarted();
        isStarted = true;
    }

    void OnDestroy() {
        // Here we are dealing with a duplicate so we don't need to shut the singleton down
        if (this != _instance) {
            return;
        }

        // Flag set when Unity sends the message OnDestroy to this Component.
        // This is needed because there is a chance that the GO holding this singleton
        // is destroyed before some other object that also access this singleton when is being destroyed.
        // As the singleton instance is null, that would create both a new instance of this
        // MonoBehaviourSingleton and a brand new GO to which the singleton instance is attached to..
        //
        // However as this is happening during the Unity app shutdown for some reason the newly created GO
        // is kept in the scene instead of being discarded after the game exists play mode.
        // (Unity bug?)
        isDestroyed = true;

        PrintLog(string.Format(
            "Destroy() Singleton with type {0} in the GameObject {1}",
            GetType(), gameObject.name));
        SingletonDestroyed();
    }

    #endregion

    #region Debug Methods (available in child classes)

    /// <summary>
    ///  Set this to true either by code or in the inspector to print trace log messages
    /// </summary>
    [FormerlySerializedAs("PrintTrace")]
    [Header("Debug")]
    public bool printTrace;

    protected void PrintLog(string str, params object[] args) {
        Print(Debug.Log, printTrace, str, args);
    }

    protected void PrintWarn(string str, params object[] args) {
        Print(Debug.LogWarning, printTrace, str, args);
    }

    protected void PrintError(string str, params object[] args) {
        Print(Debug.LogError, printTrace, str, args);
    }

    void Print(Action<string> call, bool doPrint, string str, params object[] args) {
        if (doPrint) {
            call(string.Format(
                    "<b>[{0}][{1}] {2} </b>",
                    Time.frameCount,
                    GetType().Name.ToUpper(),
                    string.Format(str, args)
                )
            );
        }
    }

    #endregion
}
}
