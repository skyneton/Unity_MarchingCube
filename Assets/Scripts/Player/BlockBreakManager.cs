using UnityEngine;

public class BlockBreakManager : MonoBehaviour
{
    public ProgressManager blockBreakProgress;
    public bool IsBreaking { get; private set; }
    public float BreakingTime { get; private set; }

    public int distance = 6;
    private Block _block;

    // Update is called once per frame
    private void Update()
    {
        BreakingCheck();
        BreakedCheck();
        BreakProgressUpdate();
    }

    private void BreakingCheck()
    {
        if (!Input.GetMouseButton(0))
        {
            IsBreaking = false;
            return;
        }
        var block = Player.Instance.GetLookCanBreakBlock(distance);
        if(block.Type == BlockType.Air)
        {
            IsBreaking = false;
            return;
        }
        IsBreaking = true;
        BreakingTime += Time.deltaTime;
        if (block != _block)
        {
            BreakingTime = 0;
            print($"Breaking Start {block}");
        }
        _block = block;
    }

    private void BreakedCheck()
    {
        if (!IsBreaking) return;
        if (BreakingTime < _block.Strength) return;
        print("Breaked");
        Player.Instance.world.SetBlock(BlockType.Air, _block.Location);
        IsBreaking = false;
    }

    private void BreakProgressUpdate()
    {
        if (blockBreakProgress.gameObject.activeSelf != IsBreaking)
            blockBreakProgress.gameObject.SetActive(IsBreaking);
        if (!IsBreaking) return;
        blockBreakProgress.FillAmount = BreakingTime / _block.Strength;
    }
}
