using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// This script was created to show the sample scene and is not intended to be used as a resource for actual assets.

[ExecuteInEditMode]
public class IconLayout : MonoBehaviour
{
    public bool set;
    public List<Sprite> _spriteList = new List<Sprite>();


    // Update is called once per frame
    void Update()
    {
        if(set)
        {
            set = false;

            //삭제 구문
            List<GameObject> tList = new List<GameObject>();
            for(var i = 0 ; i <  transform.childCount; i++)
            {
                GameObject tObj = transform.GetChild(i).gameObject;
                tList.Add(tObj);
            }

            foreach( var obj in tList)
            {
                DestroyImmediate(obj);
            }

            for(var i = 0 ; i < _spriteList.Count;i++)
            {
                GameObject tObj2 = new GameObject(_spriteList[i].name);
                Image tImg = tObj2.AddComponent<Image>();
                tImg.sprite = _spriteList[i];
                tObj2.transform.SetParent(this.transform);
            }
        }
    }
}
