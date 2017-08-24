using UnityEngine;

public static class ChipContainer
{
    public static Transform ChipParent { get; private set; }
    private static LerpInformation<Vector3> chipPullAwayLerpInformation;

    public static void Initialize()
    {
        if (ChipParent != null)
        {
            Object.Destroy(ChipParent.gameObject);
        }

        ChipParent = new GameObject("Chips").transform;
    }

    public static void Update()
    {
        HandleChipPullAwayLerp();
    }

    private static void HandleChipPullAwayLerp()
    {
        if (chipPullAwayLerpInformation == null || chipPullAwayLerpInformation.TimeLeft <= 0)
        {
            chipPullAwayLerpInformation = null;
            return;
        }

        ChipParent.position = chipPullAwayLerpInformation.Step(Time.deltaTime);
    }

    public static void TakeChips()
    {
        Vector3 destination = ChipParent.position + new Vector3(1f, 5, -ChipParent.position.z);
        chipPullAwayLerpInformation = new LerpInformation<Vector3>(ChipParent.position, destination, 0.4f, Vector3.Lerp);
    }

    public static void ReturnChips()
    {
        Vector3 destination = ChipParent.position + new Vector3(2f, -10, -ChipParent.position.z);
        chipPullAwayLerpInformation = new LerpInformation<Vector3>(ChipParent.position, destination, 0.4f, Vector3.Lerp);
    }
}