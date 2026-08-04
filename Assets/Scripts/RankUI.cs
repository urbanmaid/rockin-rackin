using System.Collections.Generic;
using UnityEngine;

public class RankUI : MonoBehaviour
{
    // Object with this class will have vertical layout group

    public RankUIElement rankUIElement;
    public int rankUICount = 6;
    private readonly List<RankUIElement> generatedElements = new();

    public void GenerateRankBoard()
    {
        GenerateRankBoard(RankManager.LoadRecords());
    }

    public void GenerateRankBoard(List<RankElement> element)
    {
        if(element == null || rankUIElement == null)
        {
            return;
        }

        ClearGeneratedElements();
        rankUIElement.gameObject.SetActive(false);

        int displayCount = Mathf.Min(rankUICount, element.Count);
        for(int i = 0 ; i < displayCount ; i++)
        {
            RankUIElement uiElement = Instantiate(rankUIElement, transform);
            uiElement.gameObject.SetActive(true);
            uiElement.SetRecordText(element[i]);
            generatedElements.Add(uiElement);
        }
    }

    private void ClearGeneratedElements()
    {
        for (int i = generatedElements.Count - 1; i >= 0; i--)
        {
            if (generatedElements[i] != null)
            {
                Destroy(generatedElements[i].gameObject);
            }
        }

        generatedElements.Clear();
    }
}