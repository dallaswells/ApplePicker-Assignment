public sealed class GameRules
{
    public const int BasketCount = 4;
    public bool IsPlaying { get; private set; }
    public int BasketsRemaining { get; private set; }
    public int Round { get; private set; }
    public int Score { get; private set; }
    public void Begin() { IsPlaying = true; BasketsRemaining = 4; Round = 1; Score = 0; }
    public void CatchApple() { if (IsPlaying) Score += 100; }
    public void MissApple()
    {
        if (!IsPlaying) return;
        BasketsRemaining--;
        if (BasketsRemaining == 0) IsPlaying = false; else Round++;
    }
    public void CatchBomb() { if (IsPlaying) IsPlaying = false; }
}
