using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script was created to show the sample scene and is not intended to be used as a resource for actual assets.
public class PageLayout : MonoBehaviour
{
    public Transform _obj;
    public float _maxValue;
    public float _stepValue;
    // Start is called before the first frame update

    public void SetMove(int num)
    {
        switch(num)
        {
            case 0: // 좌측으로 이동
            _obj.localPosition -= new Vector3(_stepValue,0,0);
            break;

            case 1: // 우측으로 이동
            _obj.localPosition += new Vector3(_stepValue,0,0);
            break;
        }

        if(_obj.localPosition.x >= 0) _obj.localPosition = new Vector3(0,0,0);
        if(_obj.localPosition.x <= _maxValue) _obj.localPosition = new Vector3(_maxValue,0,0);
    }
}
