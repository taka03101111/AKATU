using TMPro;
using UnityEngine;

public class PlayerPointTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pointText;

    private PlayerPointCounter targetCounter;

    void Start()
    {
        if (pointText == null)
        {
            pointText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (targetCounter == null)
        {
            FindLocalPlayerPointCounter();
        }

        UpdatePointText();
    }

    public void SetPlayerPointCounter(
        PlayerPointCounter pointCounter
    )
    {
        if (pointCounter == null)
        {
            return;
        }

        if (pointCounter.Object != null &&
            !pointCounter.Object.HasStateAuthority)
        {
            return;
        }

        targetCounter = pointCounter;
        UpdatePointText();
    }

    private void FindLocalPlayerPointCounter()
    {
        PlayerPointCounter[] counters =
            FindObjectsOfType<PlayerPointCounter>();

        foreach (PlayerPointCounter counter in counters)
        {
            if (counter == null)
            {
                continue;
            }

            if (!counter.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (counter.Object != null &&
                counter.Object.HasStateAuthority)
            {
                targetCounter = counter;
                return;
            }
        }
    }

    private void UpdatePointText()
    {
        if (pointText == null)
        {
            return;
        }

        if (targetCounter == null)
        {
            pointText.text = "Point : 0 / 4";
            return;
        }

        pointText.text =
            "Point : " +
            targetCounter.CurrentPointCount +
            " / " +
            targetCounter.TotalPointCount;
    }
}