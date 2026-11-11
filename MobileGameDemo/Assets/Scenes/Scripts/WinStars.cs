using UnityEngine;
using UnityEngine.UI;

public class WinStars : MonoBehaviour
{
    [SerializeField] private Image[] starSlots;
    [SerializeField] private Sprite filledStar;
    [SerializeField] private Sprite emptyStar;

    public void ShowStars(int earnedStars)
    {
        for (int i = 0; i < starSlots.Length; i++)
        {
            if (starSlots[i] != null)
                starSlots[i].sprite = i < earnedStars ? filledStar : emptyStar;
        }
    }
}