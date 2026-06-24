namespace SF5_Tournament.Models;

public class UpdateScoreViewModel
{
    public Guid MatchId { get; set; }
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
}
