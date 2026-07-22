using Fusion;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNameLabel : NetworkBehaviour
{
    [Header("Label Setting")]
    [SerializeField] private Vector3 labelOffset = new Vector3(0.0f, 2.2f, 0.0f);
    [SerializeField] private float fontSize = 4.0f;

    private TextMeshPro labelText;
    private Transform labelTransform;

    [Networked]
    private int PlayerNumber { get; set; }

    public override void Spawned()
    {
        CreateLabelIfNeeded();

        if (Object.HasStateAuthority)
        {
            int playerId = Object.InputAuthority.PlayerId;

            if (playerId <= 0)
            {
                playerId = Object.StateAuthority.PlayerId;
            }

            if (playerId <= 0)
            {
                playerId = 1;
            }

            PlayerNumber = playerId;
        }

        UpdateLabelText();
    }

    public override void Render()
    {
        UpdateLabelText();
    }

    private void LateUpdate()
    {
        if (labelTransform == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        labelTransform.LookAt(
            labelTransform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up
        );
    }

    private void CreateLabelIfNeeded()
    {
        if (labelText != null)
        {
            return;
        }

        GameObject labelObject = new GameObject("PlayerNameLabel");
        labelObject.transform.SetParent(transform);
        labelObject.transform.localPosition = labelOffset;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one;

        labelTransform = labelObject.transform;

        labelText = labelObject.AddComponent<TextMeshPro>();
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSize = fontSize;
        labelText.text = "Player";
    }

    private void UpdateLabelText()
    {
        if (labelText == null)
        {
            CreateLabelIfNeeded();
        }

        if (PlayerNumber <= 0)
        {
            labelText.text = "Player";
        }
        else
        {
            labelText.text = "Player " + PlayerNumber;
        }
    }
}