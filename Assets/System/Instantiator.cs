using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BeyondGames.Utility;

class Instantiator : MonoBehaviour
{
  //TODO: remove singleton pattern?
  static Instantiator instance;
  private static Dictionary<string,Material> sharedMaterials = new Dictionary<string,Material>();

  void Awake ()
  {
    instance = this;
  }


  public static Object Instantiate (string path, string subComponentName,System.Type type)
  {
    return instance._Instantiate(path, subComponentName,type);
  }


  public Object _Instantiate(string path, string subComponentName, System.Type type){
    AssetBundle bundle;
    if (bundles.TryGetValue(path, out bundle))
    {
      return bundle.Load (subComponentName, type);
    }
    else
    {
      bundle = LoadBundle (path);

      if (bundle != null)
      {

        bundles[path] = bundle;
      }
      Profiler.BeginSample ("_InstantiateSub "+ subComponentName);
      var loadedSubObject = bundle.Load (subComponentName, type);
      Profiler.EndSample ();
      return loadedSubObject;
    }

  }


  public static GameObject Instantiate (string path, string name, Transform transform, Transform parent, GameObject objType = null)
  {
    return instance._Instantiate(path, name, transform, parent, objType);
  }


  public static void Clear ()
  {
    instance.bundles.Values.ToList().ForEach(bundle => {
      bundle.Unload(false);
    });
    instance.bundles.Clear();
  }

  GameObject _Instantiate (string path, string name, Transform transform, Transform parent, GameObject asset = null)
  {
    Profiler.BeginSample("_Instantiate "+name);
    if (asset == null)
    {
      AssetBundle bundle;
      if (bundles.TryGetValue(path, out bundle))
      {
        asset = bundle.mainAsset as GameObject;
      }
      else
      {
        asset = LoadAsset(path);
      }
    }

    GameObject go = GameObject.Instantiate(asset) as GameObject;
    if (transform != null)
    {
      go.transform.position = transform.position;
      go.transform.rotation = transform.rotation;
    }

    go.transform.parent = parent;
    go.name = name;
    if (Application.platform != RuntimePlatform.IPhonePlayer)
    {
      HackShaderRef(go);
    }

    var lowUnifiedShader = Shader.Find("BeyondGames/UnifiedModelLow");
    foreach (var r in go.GetComponentsInChildren<Renderer>())
    {

      Material sharedMaterial = null;
      if (sharedMaterials.TryGetValue (r.material.name, out sharedMaterial)) {
        r.material = sharedMaterial;
      } else {
        if (r.material != null && r.material.shader != null)
        {
          if (r.material.shader.name == "BeyondGames/UnifiedModel")
          {
            r.material.shader = lowUnifiedShader;
          }
        }
        sharedMaterials.Add (r.material.name, r.material);
        //r.material.name += "(shared)";
      }



    }




    Profiler.EndSample();
    return go;
  }

  AssetBundle LoadBundle(string path){
    var assetHash = ("Assets/" + path).GetMD5Hash();

    AssetBundle localBundle = null;
    if (!lodQueue.Contains(path))
    {
      var lodPath = Application.persistentDataPath + "/" + assetHash + ".unity3d";
      if (File.Exists(lodPath))
      {
        Debug.Log( "AssetBundle.CreateFromFile: " + lodPath);
        localBundle = AssetBundle.CreateFromFile(lodPath);
      }
      else
      {
        lodQueue.Add(path);
//        if (!isDownloading)
//        {
//          isDownloading = true;
//          StartCoroutine(DownloadLod(path, assetHash));
//        }
      }
    }

    if (localBundle == null)
    {
      var defaultPath = Application.streamingAssetsPath + "/Defaults";
      defaultPath = defaultPath + "/" + assetHash + ".unity3d";
      Debug.Log("AssetBundle.CreateFromFile: " + defaultPath);
      localBundle = AssetBundle.CreateFromFile(defaultPath);
    }
    return localBundle;

  }

  GameObject LoadAsset (string path)
  {
    Profiler.BeginSample("LoadAsset");
    GameObject asset;

    AssetBundle localBundle = LoadBundle(path);


    if (localBundle != null)
    {
      asset = localBundle.mainAsset as GameObject;
      bundles[path] = localBundle;
    }
    else
    {
      Debug.Log("Missing asset: " + path);
      asset = Resources.Load("Buildings/Grass") as GameObject;
    }

    Profiler.EndSample();
    return asset;
  }

//  IEnumerator DownloadLod (string path, string assetHash)
//  {
//    var fullCDNPath = cdnPath + "/" + platform[Application.platform] + "/lod" + lod + "/" + assetHash + ".unity3d";
//    Debug.Log("Downloading: " + path + " from: " + fullCDNPath);
//    using (var request = new WWW(fullCDNPath))
//    {
//      yield return request;
//      lodQueue.Remove(path);
//      if (request.error == null)
//      {
//        var lodPath = Application.persistentDataPath + "/" + assetHash + ".unity3d";
//        Debug.Log("Caching: " + lodPath);
//        File.WriteAllBytes(lodPath, request.bytes);
//        bundles[path].Unload(false);
//        bundles[path] = request.assetBundle;
//      }
//      else
//      {
//        Debug.Log(request.error + " " + path + " from: " + fullCDNPath);
//      }
//    }
//    if (lodQueue.Count > 0)
//    {
//      assetHash = ("Assets/" + lodQueue[0]).GetMD5Hash();
//      StartCoroutine(DownloadLod(lodQueue[0], assetHash));
//    }
//    else
//    {
//      isDownloading = false;
//    }
//  }

  //TODO: hate Unity for making me do this, remove some day
  void HackShaderRef (GameObject go)
  {
    Profiler.BeginSample("HackShaderRef");
    foreach (var r in go.GetComponentsInChildren<Renderer>())
    {
      foreach (var m in r.sharedMaterials)
      {
        if (m != null && m.shader != null)
        {
          var sn = m.shader.name;
          m.shader = Shader.Find(sn);
        }
      }
    }
    Profiler.EndSample();  
  }

  //TODO: data-drive, ideally from remote
  string cdnPath = "http://s3-us-west-1.amazonaws.com/beyondgames-cdn/bow/assets";
  //TODO: setup per device, 300 high quality, 200 mid quality, 100 low quality?
  int lod = 300;
  Dictionary<string,AssetBundle> bundles = new Dictionary<string,AssetBundle> ();
  List<string> lodQueue = new List<string> ();
  bool isDownloading = false;
  Dictionary<RuntimePlatform, string> platform = new Dictionary<RuntimePlatform, string>
  {
    { RuntimePlatform.WindowsEditor, "windows" },
    { RuntimePlatform.OSXEditor, "ios" },
    { RuntimePlatform.Android, "android" },
    { RuntimePlatform.IPhonePlayer, "ios" },
  };

}


