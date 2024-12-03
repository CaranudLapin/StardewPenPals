using StardewValley.BellsAndWhistles;
using StardewValley.Quests;

namespace PenPals.UI;

/// <summary>
/// Details about a quest involving the delivery of an item to an NPC.
/// </summary>
/// <remarks>
/// This can be related to an <see cref="ItemDeliveryQuest"/> or a different type of quest with
/// similar mechanics, such as <see cref="FishingQuest"/>.
/// </remarks>
/// <param name="Title">The formatted title, used in tooltips.</param>
/// <param name="Text">The formatted text, which is an abbreviated version of the quest description
/// and shown in the tooltip body.</param>
/// <param name="RequiredItemId">ID of the item that must be delivered to complete the quest.</param>
public record ItemQuestInfo(string Title, string Text, string RequiredItemId)
{
    /// <summary>
    /// Attempts to resolve the delivery info for a quest. Requires a compatible quest type.
    /// </summary>
    /// <param name="quest">The active quest.</param>
    /// <param name="who">The current player; used for inventory checks.</param>
    /// <param name="npc">The NPC who would receive the gift.</param>
    /// <returns>The delivery details for the specified <paramref name="quest"/>, or <c>null</c> if
    /// the quest is not a compatible type or cannot yet be completed.</returns>
    public static ItemQuestInfo? TryFromQuest(Quest quest, Farmer who, NPC npc)
    {
        var (itemId, itemCount) = quest switch
        {
            FishingQuest fq when IsCompletable(fq, npc) => (fq.ItemId.Value, 1),
            ItemDeliveryQuest dq when IsCompletable(dq, npc) => (dq.ItemId.Value, dq.number.Value),
            _ => ("", 0),
        };
        if (string.IsNullOrEmpty(itemId) || itemCount == 0)
        {
            return null;
        }
        var item = ItemRegistry.Create(itemId, itemCount);
        var heldCount = who.Items.CountId(itemId);
        var description = I18n.GiftMailMenu_Tooltip_Quest_Description(
            itemCount,
            itemCount > 1 ? Lexicon.makePlural(item.DisplayName) : item.DisplayName,
            heldCount
        );
        return new(quest.questTitle, description, itemId);
    }

    private static bool IsCompletable(FishingQuest quest, NPC npc)
    {
        if (quest.numberFished.Value < quest.numberToFish.Value)
        {
            return false;
        }
        // Willy fallback is hardcoded in FishingQuest logic.
        string targetNpcName = !string.IsNullOrEmpty(quest.target.Value)
            ? quest.target.Value
            : "Willy";
        return npc.Name == targetNpcName;
    }

    private static bool IsCompletable(ItemDeliveryQuest quest, NPC npc)
    {
        // In NPC.tryToReceiveActiveObject, vanilla logic checks that ItemDeliveryQuest is accepted
        // but not FishingQuest. Without knowing the reasons why, just do the same thing here to
        // avoid unexpected issues.
        return quest.accepted.Value && npc.Name == quest.target.Value;
    }
}
