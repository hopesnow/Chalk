using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/** ********************************************************************************
 * @brief   Preロードするプレハブ
 ***********************************************************************************/
public class PreloadPrefab<T> : MonoBehaviour where T : PreloadPrefab<T>
{
    [HideInInspector] public T origin;
    private List<PrefabStatus> loadedLists = new List<PrefabStatus>();
    private Transform parentObj = null;

    /** ********************************************************************************
     * @summary 読み込み処理
     ***********************************************************************************/
    public void Load(Transform trans, int num, bool isUsing = false)
    {
        for (int i = 0; i < num; i++)
        {
            T tmp = Instantiate(this) as T;
            tmp.transform.SetParent(trans);
            tmp.origin = this as T;
            var status = new PrefabStatus(tmp, isUsing);
            loadedLists.Add(status);
        }

        this.parentObj = trans;
    }

    /** ********************************************************************************
     * @summary 読み込み処理
     ***********************************************************************************/
    public void Load(int num, bool isUsing = false)
    {
        Load(null, num, isUsing);
    }

    /** ********************************************************************************
     * @summary 親のオブジェクトを設定する
     ***********************************************************************************/
    public void SetParentObj(Transform trans)
    {
        this.parentObj = trans;
    }

    /** ********************************************************************************
     * @summary 読み込み処理
     ***********************************************************************************/
    public T GetPrefab(Vector3 pos, Quaternion rot)
    {
        T tmp = GetPrefab();
        tmp.transform.position = pos;
        tmp.transform.rotation = rot;
        return tmp;
    }

    /** ********************************************************************************
     * @summary 読み込み処理
     ***********************************************************************************/
    public T GetPrefab(Transform trans, bool worldPositionStay = false)
    {
        T tmp = GetPrefab();
        tmp.transform.SetParent(trans, worldPositionStay);
        return tmp;
    }

    /** ********************************************************************************
     * @summary 読み込み処理
     ***********************************************************************************/
    public T GetPrefab()
    {
        var status = loadedLists.FirstOrDefault(l => !l.isUsing);

        if (null != status)
        {
            if (null != status.prefab)
            {
                status.prefab.gameObject.SetActive(true);
                status.isUsing = true;
            }
            else
            {
                loadedLists.Remove(status);
                return GetPrefab();
            }
        }
        else
        {
            var prefab = Instantiate(this) as T;
            prefab.name = string.Format("{0}({1})", GetType(), loadedLists.Count);
            prefab.origin = this as T;
            status = new PrefabStatus(prefab, true);
            loadedLists.Add(status);
        }

        return status.prefab;
    }

    /** ********************************************************************************
     * @summary 解放処理
     ***********************************************************************************/
    virtual public void Release()
    {
        if (this.origin != null)
        {
            this.origin.Release(this as T);
        }
    }

    /** ********************************************************************************
     * @summary 解放処理
     ***********************************************************************************/
    public void Release(T target)
    {
        if (null != this.origin)
        {
            this.origin.Release(target);
        }
        else
        {
            var loadedObj = loadedLists.FirstOrDefault(status => status.prefab == target);
            if (null != loadedObj)
            {
                if (null == this.parentObj || null != this.parentObj.gameObject)
                {
                    loadedObj.prefab.transform.SetParent(this.parentObj);
                    loadedObj.prefab.gameObject.SetActive(false);
                    loadedObj.isUsing = false;
                }
            }
        }
    }

    /** ********************************************************************************
     * @summary 全解放処理
     ***********************************************************************************/
    public void UnloadAll()
    {
        foreach (PrefabStatus status in loadedLists)
        {
            status.Unload();
        }

        loadedLists.Clear();
    }

    /** ********************************************************************************
     * @brief   プレハブ状態
     * @author  Masashi Tagami (m-tagami@landho.co.jp)
     ***********************************************************************************/
    public class PrefabStatus
    {
        public T prefab;
        public bool isUsing;

        /** ********************************************************************************
         * @summary コンストラクタ
         ***********************************************************************************/
        public PrefabStatus(T prefab, bool isUsing)
        {
            this.prefab = prefab;
            this.isUsing = isUsing;
            this.prefab.gameObject.SetActive(isUsing);
        }

        /** ********************************************************************************
         * @summary 解放処理
         ***********************************************************************************/
        public void Unload()
        {
            if (this.prefab != null)
            {
                Destroy(this.prefab.gameObject);
                this.prefab = null;
            }
        }
    }
}