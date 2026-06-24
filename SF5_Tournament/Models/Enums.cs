namespace SF5_Tournament.Models;

/// <summary>Lifecycle stage of a tournament.</summary>
public enum TournamentStatus
{
    Draft = 0,
    GroupStage = 1,
    KnockoutStage = 2,
    Finished = 3
}

/// <summary>Which part of the tournament a match belongs to.</summary>
public enum MatchStage
{
    Group = 0,
    Semifinal = 1,
    ThirdPlace = 2,
    Final = 3
}
