using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : Singleton<SkinManager>
{
    // ��Ų���� �����ϰ� ��Ų�� ���µǾ����� Ȯ�� �ؾ���
    // ���µ� ��Ų���� �����ϰ�, ��ݵ� ��Ų�� ����
    [SerializeField]
    private List<GameObject> slimeSkin;

    private bool[] isSkinOpen;
    private GameObject selectedSkin;
    protected override void Awake()
    {
        base.Awake();
        isSkinOpen = new bool[slimeSkin.Count];   
    }

    protected override void Start()
    {
        base.Start();

        GameData gameData = SaveManager.Instance.LoadPlayerData();

        for(int i = 0; i < slimeSkin.Count; i++)
        {
            //isSkinOpen[i] = gameData.openSkin[i];
            isSkinOpen[i] = true;
        }   
    }

    public int GetSkinCount()
    {
        return slimeSkin.Count;
    }

    public List<GameObject> GetOpenSlime()
    {
        List<GameObject> openSkinList = new List<GameObject>();
      
        for (int i = 0; i < slimeSkin.Count; i++)
        {
            if (isSkinOpen[i])
            {
                openSkinList.Add(slimeSkin[i]);
            }
        }

        return openSkinList;    
    }

    public List<GameObject> GetCloseSlime()
    {
        List<GameObject> closeSkinList = new List<GameObject>();

        for (int i = 0; i < slimeSkin.Count; i++)
        {
            if (!isSkinOpen[i])
            {
                closeSkinList.Add(slimeSkin[i]);
            }
        }

        return closeSkinList;
    }

    public void SelectSkin(GameObject skin)
    {
        selectedSkin = skin;
    }

    public GameObject GetSelectSkin()
    { 
        return selectedSkin; 
    }
}
