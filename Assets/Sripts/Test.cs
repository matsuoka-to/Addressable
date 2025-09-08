using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using System.Threading.Tasks;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    [SerializeField]
    Button[] button;

    [SerializeField]
    Image image;

    [SerializeField]
    Text text;

    string[] files = new string[]
    {
        "potion_orange_128",
        "potion_pink_128",
        "potion_blue_128",
        "potion_red_128",
        "potion_violet_128",
        "potion_green_128",
        "chara00",
        "chara01",
        "chara02",
        "chara03",
        "chara04",
        "chara05",
        "background00_00",
        "background00_01",
        "background01_00",
        "background01_01",
        "background02_00",
        "background02_01",
        "background03_00",
        "background03_01",
        "background04_00",
        "background04_01",
        "background05_00",
        "background05_01",
    };

    class ResourceData
    {
        public AsyncOperationHandle handle;
        public long count;
    }
    private Dictionary<string, ResourceData> handles = new Dictionary<string, ResourceData>();

    private string catalogUrl = "https://matsuoka-to.github.io/unity-addressables/{0}/catalog_{1}.json";
    private bool isLoad;
    private int select;

    private void Start()
    {
        isLoad = false;
        select = 0;
        text.text = string.Format($"選択 : {select} | {files[select]}");

        for (var i = 0; i < button.Length; i++)
        {
            var id = i;
            button[i].onClick.AddListener(() => ButtonCallBack(id));
        }
    }

    private void OnDestroy()
    {
        for(var i = files.Length - 1; i >= 0; i--)
        {
            var key = files[i];
            if (handles.ContainsKey(key))
            {
                Addressables.Release(handles[key].handle);
                handles.Remove(key);
            }
        }
    }

    private async void ButtonCallBack(int id)
    {
        if(isLoad)
        {
            return;
        }
        isLoad = true;

        switch (id)
        {
            case 0:
            {
                var url = string.Format(catalogUrl, "Android", "0.1.0");
                await LoadRemoteCatalog(url);
            }
            break;

            case 1:
            {
                var url = string.Format(catalogUrl, "Android", "0.2.0");
                await LoadRemoteCatalog(url);
            }
            break;

            case 2:
                await LoadImageData(files[select]);
            break;

            case 3:
            {
                DataRelease(files[select]);
            }
            break;

            case 4:
            {
                select++;
                select = (int)Mathf.Clamp((float)select, 0.0f, (float)(files.Length - 1));

                text.text = string.Format($"選択 : {select} | {files[select]}");
            }
            break;

            case 5:
            {
                select--;
                select = (int)Mathf.Clamp((float)select, 0.0f, (float)(files.Length - 1));

                text.text = string.Format($"選択 : {select} | {files[select]}");
            }
            break;

            case 6:
            {
                Addressables.ClearResourceLocators();
                Caching.ClearCache();
            }
            break;
        }

        isLoad = false;
    }

    /// <summary>
    /// カタログを読み込む
    /// </summary>
    private async Task LoadRemoteCatalog(string url)
    {
        Debug.Log("カタログ読み込み開始: " + url);

        Addressables.ClearResourceLocators();
        Caching.ClearCache();

        var handle = Addressables.LoadContentCatalogAsync(url);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("カタログ読み込み成功: " + handle.Result.LocatorId);
        }
        else
        {
            Debug.LogError("カタログ読み込み失敗: " + handle.OperationException);
        }

        Addressables.Release(handle);
    }

    /// <summary>
    /// データを読み込む
    /// </summary>
    private async Task LoadImageData(string key)
    {
        if(handles.ContainsKey(key))
        {
            handles[key].count++;
            return;
        }

        var data = new ResourceData();
        handles.Add(key, data);

        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        await handle.Task;

        image.sprite = handle.Result;

        handles[key].handle = handle;
        handles[key].count = 1;
        handles[key] = data;
    }

    /// <summary>
    /// データを解放
    /// </summary>
    private void DataRelease(string key)
    {
        if (handles.ContainsKey(key))
        {
            handles[key].count--;
            if(handles[key].count == 0)
            {
                Addressables.Release(handles[key].handle);

                handles.Remove(key);
            }
        }
    }

    /// <summary>
    /// データを全て開放
    /// </summary>
    private void DataAllRelease()
    {
        for (var i = files.Length - 1; i >= 0; i--)
        {
            var key = files[i];
            if (handles.ContainsKey(key))
            {
                Addressables.Release(handles[key].handle);
                handles.Remove(key);
            }
        }
    }
}
