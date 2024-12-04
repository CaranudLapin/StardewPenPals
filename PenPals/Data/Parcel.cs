namespace PenPals.Data;

/// <summary>
/// Wrapper for an outgoing gift.
/// </summary>
/// <param name="Gift">The gift object.</param>
/// <param name="QuestId">ID of the quest, if any, to complete using the
/// <paramref name="Gift"/>.</param>
public record Parcel(SObject Gift, string? QuestId);
