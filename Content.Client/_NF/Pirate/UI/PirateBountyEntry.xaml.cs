using Content.Client.Message;
using Content.Shared._NF.Pirate;
using Content.Shared._NF.Pirate.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._NF.Pirate.UI;

[GenerateTypedNameReferences]
public sealed partial class PirateBountyEntry : BoxContainer
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public Action? OnLabelButtonPressed;
    public Action? OnSkipButtonPressed;

    public TimeSpan EndTime;
    public TimeSpan UntilNextSkip;

    public bool Accepted;

    public PirateBountyEntry(PirateBountyData bounty, bool accepted, TimeSpan untilNextSkip)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        UntilNextSkip = untilNextSkip;
        Accepted = accepted;

        if (!_prototype.TryIndex<PirateBountyPrototype>(bounty.Bounty, out var bountyPrototype))
            return;

        if (bountyPrototype.SpawnChest)
            AcceptButton.Label.Text = Loc.GetString("pirate-bounty-console-accept-button-chest");
        else
            AcceptButton.Label.Text = Loc.GetString("pirate-bounty-console-accept-button-label");

        var items = new List<string>();
        foreach (var entry in bountyPrototype.Entries)
        {
            items.Add(Loc.GetString("pirate-bounty-console-manifest-entry",
                ("amount", entry.Amount),
                ("item", Loc.GetString(entry.Name))));
        }
        ManifestLabel.SetMarkup(Loc.GetString("pirate-bounty-console-manifest-label", ("item", string.Join(", ", items))));
        RewardLabel.SetMarkup(Loc.GetString("pirate-bounty-console-reward-label", ("reward", bountyPrototype.Reward)));
        DescriptionLabel.SetMarkup(Loc.GetString("pirate-bounty-console-description-label", ("description", Loc.GetString(bountyPrototype.Description))));
        IdLabel.SetMarkup(Loc.GetString("pirate-bounty-console-id-label", ("id", bounty.Id)));

        AcceptButton.OnPressed += _ => OnLabelButtonPressed?.Invoke();
        SkipButton.OnPressed += _ => OnSkipButtonPressed?.Invoke();
    }

    private void UpdateSkipButton(float deltaSeconds)
    {
        if (Accepted)
        {
            SkipButton.Label.Text = Loc.GetString("pirate-bounty-console-skip-button-accepted");
            SkipButton.Disabled = true;
            return;
        }

        UntilNextSkip -= TimeSpan.FromSeconds(deltaSeconds);
        if (UntilNextSkip > TimeSpan.Zero)
        {
            SkipButton.Label.Text = UntilNextSkip.ToString("mm\\:ss");
            SkipButton.Disabled = true;
            return;
        }

        SkipButton.Label.Text = Loc.GetString("pirate-bounty-console-skip-button-text");
        SkipButton.Disabled = false;
    }

    private void UpdateAcceptButton()
    {
        AcceptButton.Disabled = Accepted;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        UpdateSkipButton(args.DeltaSeconds);
        UpdateAcceptButton(); // Ideally, shouldn't be run every frame.
    }
}
