using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_Stats_Tab : MonoBehaviour
{
    [SerializeField] private List<Button> tabButtons;
    [SerializeField] private List<GameObject> scrollViews;

    private readonly Color activeColor = Color.white;                        
    private readonly Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    private void Start()
    {

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SelectTab(index));
        }


        SelectTab(0);
    }
    public void SelectTab(int tabIndex)
    {
        Debug.Log($"클릭 확인{tabIndex}");
        for (int i = 0; i < tabButtons.Count; i++)
        {
            bool isActive = (i == tabIndex);

            
            if (i < scrollViews.Count && scrollViews[i] != null)
                scrollViews[i].SetActive(isActive);

            
            Image tabImage = tabButtons[i].GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = isActive ? activeColor : inactiveColor;
            }

            //TextMeshProUGUI tabText = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            //if (tabText != null)
            //{
            //    tabText.color = isActive ? activeColor : inactiveColor;
            //}
        }
    }
}
