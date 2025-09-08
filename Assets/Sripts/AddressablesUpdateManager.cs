using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressablesUpdateManager : MonoBehaviour
{
    // インスペクターで設定する古いカタログのURL
    [SerializeField]
    private string oldCatalogUrl = "https://matsuoka-to.github.io/unity-addressables/Android/catalog_0.1.0.json";

    // 最新のアセットをロードする際に使うキー
    private const string AssetKey = "YourAssetName"; // ここをロードしたいアセットのキーに置き換えてください

    // カタログ更新があるかチェック
    private IEnumerator Start()
    {
        Debug.Log("Addressables初期化中...");
        var init = Addressables.InitializeAsync();
        yield return init;

        if (init.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Addressables初期化失敗: {init.OperationException}");
            yield break;
        }

        Debug.Log("古いカタログをロード中: " + oldCatalogUrl);
        // 古いカタログを強制的にロード
        var catalogLoadHandle = Addressables.LoadContentCatalogAsync(oldCatalogUrl);
        yield return catalogLoadHandle;

        if (catalogLoadHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"古いカタログロード失敗: {catalogLoadHandle.OperationException}");
            Addressables.Release(catalogLoadHandle);
            yield break;
        }
        Debug.Log("古いカタログロード成功.");

        Debug.Log("カタログ更新チェック中...");
        // カタログ更新チェック
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var catalogsToUpdate = checkHandle.Result;
            Debug.Log($"更新可能なカタログの数: {catalogsToUpdate.Count}");

            if (catalogsToUpdate.Count > 0)
            {
                Debug.Log($"更新が必要なカタログが見つかりました: {string.Join(", ", catalogsToUpdate)}");

                // カタログ更新
                var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
                yield return updateHandle;

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("カタログ更新完了.");
                }
                else
                {
                    Debug.LogError($"カタログ更新失敗: {updateHandle.OperationException}");
                }
                Addressables.Release(updateHandle);
            }
            else
            {
                Debug.Log("カタログは最新です。");
            }
        }
        else
        {
            Debug.LogError($"カタログ更新チェック失敗: {checkHandle.OperationException}");
        }

        Addressables.Release(checkHandle);
        Addressables.Release(catalogLoadHandle);
    }
}

