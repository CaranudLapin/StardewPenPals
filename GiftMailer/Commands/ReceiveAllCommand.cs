﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using GiftMailer.Data;

namespace GiftMailer.Commands;

internal record ReceiveAllArgs();

internal class ReceiveAllCommand(Func<GiftDistributor> distributorFactory)
    : ICommand<ReceiveAllArgs>
{
    public string Name => "receiveall";

    public string Description =>
        "Make all NPCs receive their gifts immediately, ignoring mail schedules.";

    private static readonly int[] COLUMN_WIDTHS = [12, 12, 20, 14, 5];

    public void Execute(ReceiveAllArgs args)
    {
        var distributor = distributorFactory();
        var results = distributor.ReceiveAll();
        PrintResults(results, distributor.Context.Monitor);
    }

    public bool TryParseArgs(
        string[] args,
        [MaybeNullWhen(false)] out ReceiveAllArgs parsedArgs,
        [MaybeNullWhen(true)] out string error
    )
    {
        parsedArgs = new();
        error = null;
        return true;
    }

    private static char GetQualityChar(int quality)
    {
        return quality switch
        {
            1 => 'S',
            2 => 'G',
            4 => 'I',
            _ => '?',
        };
    }

    private void PrintResults(IEnumerable<GiftResult> results, IMonitor monitor)
    {
        var output = new StringBuilder();
        output.AppendLine("Gift shipment results:");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Top);
        output.AppendColumns(COLUMN_WIDTHS, "From", "To", "Gift", "Reaction", "Pts");
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Middle);
        foreach (var result in results)
        {
            var giftName = result.Gift.Name;
            if (result.Gift.Quality > 0)
            {
                giftName = "(" + GetQualityChar(result.Gift.Quality) + ") " + giftName;
            }
            output.AppendColumns(
                COLUMN_WIDTHS,
                result.From.Name,
                result.To.Name,
                giftName,
                result.Outcome,
                result.Points
            );
        }
        output.AppendBorderLine(COLUMN_WIDTHS, BorderLine.Bottom);
        monitor.Log(output.ToString(), LogLevel.Info);
    }
}
