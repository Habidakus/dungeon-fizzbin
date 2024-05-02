using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

public partial class Main : Node
{
    Random rnd = new Random((int)DateTime.Now.Ticks);
    private List<Player> _players = new List<Player>();
    private Deal? _deal = null;
    internal Deal Deal
    {
        get
        {
            if (_deal == null)
            {
                throw new Exception("Deal has not been created yet.");
            }

            return _deal;
        }
    }

    internal int Dealer { get; private set; }
    internal int InitialBetter { get { return (Dealer + 1) % _players.Count; } }
    internal int CurrentBetter { get; private set; }
    private double currentBetLimit = 1.0;
    private int BettingRound = 0;
    private int TableSize = 5;
    internal List<Species> SpeciesAtTable { get { return _players.Select(a => a.Species).ToList(); } }

    public static int HandNumber { get; private set; } = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    internal void StartFreshDeal()
    {
        _deal = new Deal();

        while (_players.Count < TableSize)
        {
            List<int> tablePositions = new List<int>() { 0, 1, 2, 3, 4 };
            int missingPositionID = tablePositions.Where(a => !_players.Select(b => b.PositionID).Contains(a)).First();
            Player player = new Player(missingPositionID, rnd, SpeciesAtTable, _deal);
            _players.Add(player);
        }
        _players.Sort((a,b) => a.PositionID.CompareTo(b.PositionID));

        HandNumber += 1;

        HUD hud = GetHUD();
        foreach (Player player in _players)
        {
            _deal.AddPlayer(hud, player);
        }

        _deal.Shuffle(_players, rnd);
        _deal.UpdateHUD(hud);

        const double anteAmount = 1;
        foreach (Player player in _players)
        {
            Deal.MoveMoneyToPot(player.Ante(hud, anteAmount), player);
        }

        Deal.UpdatePot(hud);

        BettingRound = 0;
        Dealer = 0; // Should advance each round
        CurrentBetter = InitialBetter;
        currentBetLimit = anteAmount + 1;

        Deal.Dump();
    }

    internal HUD GetHUD()
    {
        if (FindChild("HUD") is HUD hud)
        {
            return hud;
        }

        throw new Exception($"{Name} does not have child HUD");
    }

    internal state_machine GetStateMachine()
    {
        if (FindChild("StateMachine") is state_machine sm)
        {
            return sm;
        }

        throw new Exception($"{Name} does not have child state_machine");
    }

    private Player? NextPlayerToPassACard
    {
        get
        {
            if (Deal.PassCardsToLeftNeighbor > 0)
            {
                for (int i = 0; i < _players.Count; ++i)
                {
                    bool hasPassingCards = Deal.HasPassingCards(Deal.GetPlayerHand(_players[i]));
                    if (!hasPassingCards)
                    {
                        return _players[i];
                    }
                }
            }

            return null;
        }
    }

    internal bool SomeoneNeedsToPass(out int positionId)
    {
        if (Deal.PassCardsToLeftNeighbor > 0)
        {
            var player = NextPlayerToPassACard;
            if (player != null)
            {
                positionId = player.PositionID;
                return true;
            }
        }

        positionId = -1;
        return false;
    }

    internal void ForceSomeoneToPass()
    {
        Deal.DeterminePassCards(GetHUD(), NextPlayerToPassACard!, rnd);
        GetStateMachine().SwitchState("Play_Loop");
    }

    internal bool NeedsToResolvePassAndRiver(out int positionID)
    {
        return Deal.NeedsToProcessPassedCards(out positionID);
    }

    internal void ResolvePassAndRiver()
    {
        Deal.ResolvePassAndRiver(GetHUD());
        GetStateMachine().SwitchState("Play_Loop");
    }

    public bool SomeoneNeedsToDiscard(out int highlightPositionID)
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            int j = (InitialBetter + i) % _players.Count;
            if (!_players[j].HasDiscarded)
            {
                highlightPositionID = _players[j].PositionID;
                return true;
            }
        }

        highlightPositionID = -1;
        return false;
    }

    public void ForceSomeoneToDiscard()
    {
        for (int i = 0; i < _players.Count; ++i)
        {
            int j = (InitialBetter + i) % _players.Count;
            if (!_players[j].HasDiscarded)
            {
                Hand hand = Deal.GetPlayerHand(_players[j]);
                Deal.ApplyDiscardCost();
                _players[j].Discards = hand.SelectDiscards(0, Deal.MaxDiscard, Deal, rnd);
                if (_players[j].Discards != null)
                {
                    double penaltyForDiscard = _players[j].Discards!.Count * Deal.CostPerDiscard;
                    Deal.MoveMoneyToPot(penaltyForDiscard, _players[j]);
                    Deal.UpdatePot(GetHUD());
                }

                Deal.ReleaseDiscardCost();
                _players[j].HasDiscarded = true;

                GetStateMachine().SwitchState("Play_Animate_Discards");

                return;
            }
        }

        throw new Exception("There was no player awaiting discard");
    }

    public bool ProgressDiscardAnimation()
    {
        HUD hud = GetHUD();
        foreach (Player player in _players)
        {
            if (player.Discards != null && player.Discards.Count > 0)
            {
                Card? card = player.Discards.OrderBy(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, Deal.PixieCompare))).First();
                if (card == null)
                {
                    throw new Exception("Why is no card min?");
                }

                bool exposeDiscard = player.ExposedDiscardCount < Deal.DiscardsToReveal;
                List<int> playersWhoCanSeeThisDiscard = new List<int>();
                foreach (Player viewingPlayer in _players)
                {
                    if (exposeDiscard || Deal.GetPlayerHand(player).IsVisible(card, viewingPlayer))
                    {
                        playersWhoCanSeeThisDiscard.Add(viewingPlayer.PositionID);
                    }
                }

                player.ExposedDiscardCount += 1;
                Deal.MoveCardToDiscard(hud, player, playersWhoCanSeeThisDiscard, card);
                player.Discards.Remove(card);
                player.DiscardCount += 1;
                return true;
            }
        }

        if (Deal.ProgressReplaceDiscard(hud))
            return true;

        return false;
    }

    internal bool HasPostDiscard()
    {
        return Deal.RevealRightNeighborsHighestCards > 0 || Deal.NumberOfHighestRankingCardsToExpose > 0;
    }

    internal void PerformPostDiscord()
    {
        List<int> positionIDs = new List<int>();
        foreach (Hand hand in Deal._hands)
        {
            positionIDs.Add(hand.PositionID);
            int leftNeighbor = (hand.PositionID + 1 + _players.Count) % _players.Count;
            hand.RevealHighestCardsToOtherPlayer(GetHUD(), Deal.RevealRightNeighborsHighestCards, _players[leftNeighbor], Deal.PixieCompare);
        }

        Deal.ExposeHighestRankingCards(GetHUD());
        Deal.RevealRightNeighborsHighestCards = 0;
    }

    public bool SomeoneNeedsToBet(out int highlightPositionId)
    {
        if (Deal.Pot == 0)
        {
            highlightPositionId = -1;
            return false;
        }

        if (1 == _players.Where(a => !a.HasFolded).Count())
        {
            highlightPositionId = -1;
            return false;
        }

        int consider = _players.Count;
        while (consider > 0)
        {
            if (CurrentBetter == InitialBetter)
                ++BettingRound;

            if (_players[CurrentBetter].HasFolded)
            {
                consider -= 1;
                CurrentBetter = (CurrentBetter + 1) % _players.Count;
                continue;
            }

            if (_players[CurrentBetter].AmountBet < currentBetLimit)
            {
                highlightPositionId = _players[CurrentBetter].PositionID;
                return true;
            }
            else if (_players[CurrentBetter].AmountBet == currentBetLimit)
            {
                highlightPositionId = -1;
                return false;
            }
            else
            {
                throw new Exception($"Why is player #{CurrentBetter} got more bet ({_players[CurrentBetter].AmountBet}) than the current bet limit ({currentBetLimit})?");
            }
        }

        highlightPositionId = -1;
        return false;
    }

    public void ForceNextBet()
    {
        if (Deal.CostPerDiscard != 0)
        {
            throw new Exception("We should not be computing discard cost at this point in time");
        }

        HUD hud = GetHUD();
        DateTime start = DateTime.Now;
        var betAmount = Deal.ForceBetOrFold(_players[CurrentBetter], _players, rnd, hud, currentBetLimit, BettingRound);
        GD.Print($"Bet computation time: {(DateTime.Now - start).TotalSeconds:F2}");

        if (betAmount > 0)
        {
            Deal.MoveMoneyToPot(betAmount, _players[CurrentBetter]);
            Deal.UpdatePot(hud);
        }

        if (!_players[CurrentBetter].HasFolded)
            currentBetLimit = _players[CurrentBetter].AmountBet;

        CurrentBetter = (CurrentBetter + 1) % _players.Count;
    }

    public bool SomeoneNeedsToReveal(out int highlightPositionId)
    {
        if (1 == _players.Where(a => !a.HasFolded).Count())
        {
            // Only one player hasn't folded, they get to keep their hand secret.
            highlightPositionId = -1;
            return false;
        }

        for (int i = 0; i < _players.Count; ++i)
        {
            int position = (InitialBetter + i) % _players.Count;
            if (_players[position].HasFolded)
                continue;
            if (_players[position].HasRevealed)
                continue;

            highlightPositionId = _players[position].PositionID;
            return true;
        }

        highlightPositionId = -1;
        return false;
    }

    public void RevealHand()
    {
        if (!SomeoneNeedsToReveal(out int positionId))
        {
            throw new Exception("Can not reveal hand if no one needs to reveal.");
        }

        Player playerToRevealThisTime = _players[positionId];
        Hand revealedHand = Deal.GetPlayerHand(playerToRevealThisTime);
        GD.Print($"Revealing {revealedHand} from {playerToRevealThisTime}");
        Deal.Reveal(playerToRevealThisTime, GetHUD(), revealedHand.ScoreAsString());

        List<Hand> allHandsWhichHaveRevealed = _players.Where(p => p.HasRevealed && !p.HasFolded).Select(a => Deal.GetPlayerHand(a)).ToList();
        for (int i = 0; i< allHandsWhichHaveRevealed.Count - 1; ++i)
        {
            GetHUD().SetFeltToLost(allHandsWhichHaveRevealed[i].PositionID);
        }
    }

    private Hand GetWinningHand()
    {
        Hand? bestHand = null;
        foreach (Player player in _players)
        {
            if (player.HasFolded)
            {
                continue;
            }

            Hand hand = Deal.GetPlayerHand(player);
            if (bestHand == null || hand.CompareTo(bestHand) > 0)
            {
                bestHand = hand;
            }
        }

        if (bestHand == null)
        {
            throw new Exception("Why didn't we find a winning player?");
        }

        return bestHand;
    }

    internal bool NeedToDeclareWinner(out int highlightPositionId)
    {
        if (Deal.Pot > 0)
        {
            highlightPositionId = GetWinningHand().PositionID;
            return true;
        }
        else
        {
            highlightPositionId = -1;
            return false;
        }
    }

    private List<int>? PlayersWhoAreLeaving { get; set; } = null;

    public void AwardWinner()
    {
        Hand bestHand = GetWinningHand();
        Deal.MovePotToPlayer(bestHand._player);
        Deal.UpdatePot(GetHUD());
        GetHUD().HighlightPosition(-1);
        Deal.Dump();

        PlayersWhoAreLeaving = new List<int>();
        foreach (Player player in _players)
        {
            if (player.IsNPC)
            {
                if (100 + rnd.NextDouble() * 100 > player.Wallet)
                {
                    PlayersWhoAreLeaving.Add(player.PositionID);
                }
            }
        }
    }

    internal bool SomeoneNeedsToLeaveGame(out int highlightPositionId)
    {
        if (PlayersWhoAreLeaving == null || PlayersWhoAreLeaving.Count == 0)
        {
            highlightPositionId = -1;
            return false;
        }

        highlightPositionId = PlayersWhoAreLeaving.First();
        return true;
    }

    public void HavePlayerLeave()
    {
        if (PlayersWhoAreLeaving != null && PlayersWhoAreLeaving.Count > 0)
        {
            int playerIDToLeave = PlayersWhoAreLeaving[0];
            Player playerWhoIsLeaving = _players.Where(p => p.PositionID == playerIDToLeave).First();
            PlayersWhoAreLeaving.RemoveAt(0);
            GetHUD().PlayerLeaves(playerIDToLeave, playerWhoIsLeaving.Species.GetLeavingText(playerWhoIsLeaving));
            _players.Remove(playerWhoIsLeaving);
        }
        else
        {
            throw new Exception("Asked to dismiss a player and they're not here.");
        }
    }

    //private void Test()
    //{
    //    Deal deal = new Deal();
    //    deal.ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);

    //    // Sorted hands (pixie=False):

    //    Player player = new Player(0, rnd, new List<Species>(), deal);
    //    Player player1 = new Player(1, rnd, Species.Get("Elf"), deal);
    //    Player player2 = new Player(2, rnd, Species.Get("Centaur"), deal);
    //    Player player3 = new Player(3, rnd, Species.Get("Giant"), deal);
    //    Player player4 = new Player(4, rnd, Species.Get("Halfling"), deal);
    //    deal.AddPlayer(GetHUD(), player);
    //    deal.AddPlayer(GetHUD(), player1);
    //    deal.AddPlayer(GetHUD(), player2);
    //    deal.AddPlayer(GetHUD(), player3);
    //    deal.AddPlayer(GetHUD(), player4);

    //    Hand hand = new Hand(player);
    //    Suit spade = Suit.DefaultSuits[0];
    //    Suit heart = Suit.DefaultSuits[1];
    //    Suit diamond = Suit.DefaultSuits[2];
    //    Rank three = Rank.DefaultRanks[1];
    //    Rank five = Rank.DefaultRanks[3];
    //    Rank six = Rank.DefaultRanks[4];
    //    Rank seven = Rank.DefaultRanks[5];
    //    Rank eight = Rank.DefaultRanks[6];
    //    Rank nine = Rank.DefaultRanks[7];
    //    hand.AddCard(new Card(spade, five));
    //    hand.AddCard(new Card(spade, nine));
    //    hand.AddCard(new Card(heart, eight));
    //    hand.AddCard(new Card(diamond, seven));
    //    Card card_d3 = new Card(diamond, three);
    //    hand.AddCard(card_d3);
    //    hand.ComputeBestScore(minRank, maxRank, suitsCount, new List<Card>());
    //    //
    //    hand._cards.Remove(card_d3);
    //    hand.AddCard(new Card(diamond, six));
    //    deal._hands.Add(hand);
    //    deal.Dump();
    //    GD.Print("------------------");
    //}
}

//private void Test(Random rnd)
//{
//    Player player = new Player(0, rnd, SpeciesAtTable, _deal!);

//    Suit suitA = Suit.DefaultSuits[0];
//    Suit suitB = Suit.DefaultSuits[1];
//    Suit suitC = Suit.DefaultSuits[2];
//    Suit suitD = Suit.DefaultSuits[3];

//    List<Suit> suits = new List<Suit>()
//        {
//            suitA, suitB, suitC, suitD
//        };

//    Rank rank2 = Rank.DefaultRanks[0];
//    Rank rank3 = Rank.DefaultRanks[1];
//    Rank rank4 = Rank.DefaultRanks[2];
//    Rank rank5 = Rank.DefaultRanks[3];
//    Rank rank6 = Rank.DefaultRanks[4];
//    Rank rank7 = Rank.DefaultRanks[5];
//    Rank rank8 = Rank.DefaultRanks[6];
//    Rank rank9 = Rank.DefaultRanks[7];
//    Rank rank10 = Rank.DefaultRanks[8];
//    Rank rankJ = Rank.DefaultRanks[9];
//    Rank rankQ = Rank.DefaultRanks[10];
//    Rank rankK = Rank.DefaultRanks[11];
//    Rank rankA = Rank.DefaultRanks[12];

//    List<Rank> ranks = new List<Rank>()
//        {
//            rank2,
//            rank3,
//            rank4,
//            rank5,
//            rank6,
//            rank7,
//            rank8,
//            rank9,
//            rank10,
//            rankJ,
//            rankQ,
//            rankK,
//            rankA,
//        };

//    Rank.ExtractMinAndMax(ranks, suits, out int minRank, out int maxRank, out int suitsCount);

//    { // A 9 4 3 2
//        Hand hand = new Hand(player);

//        //Card cardAce = new Card(suitA, rankA);
//        //Card cardFour = new Card(suitC, rank4);
//        //Card cardThree = new Card(suitD, rank3);

//        hand.AddCard(new Card(suitA, rankA));
//        hand.AddCard(new Card(suitB, rankA));
//        hand.AddCard(new Card(suitC, rankQ));
//        hand.AddCard(new Card(suitA, rank9));
//        hand.AddCard(new Card(suitA, rank3));
//        hand.ComputeBestScore(minRank, maxRank, suitsCount);

//        GD.Print($"Evaluating {hand}");

//        List<Card> availableCards = new List<Card>();
//        foreach (Rank rank in ranks)
//        {
//            foreach (Suit suit in suits)
//            {
//                Card c = new Card(suit, rank);
//                bool addCard = true;
//                foreach (Card card in hand._cards)
//                {
//                    if (c.CompareTo(card) == 0)
//                    {
//                        addCard = false;
//                    }
//                }

//                if (addCard)
//                {
//                    availableCards.Add(c);
//                }
//            }
//        }

//        List<Tuple<AggregateValue, string, TimeSpan>> choices = new List<Tuple<AggregateValue, string, TimeSpan>>();
//        foreach (int i in new List<int>() { 2, 3, 3, 3, 3 })
//        {
//            DateTime start = DateTime.Now;
//            Tuple<AggregateValue, List<Card>> discard = hand.SelectDiscards(i, availableCards, minRank, maxRank, suitsCount, rnd);
//            TimeSpan dur = DateTime.Now - start;
//            StringBuilder cardsAsText = new StringBuilder();
//            if (discard.Item2.Count == 0)
//            {
//                cardsAsText.Append("nothing");
//            }
//            else
//            {
//                foreach (Card card in discard.Item2)
//                {
//                    if (cardsAsText.Length > 0)
//                        cardsAsText.Append(", ");
//                    cardsAsText.Append(card.ToString());
//                }
//            }

//            choices.Add(Tuple.Create(discard.Item1, $"Discarding {i} cards: trying for {discard.Item1.GetDesc()} by discarding {cardsAsText}", dur));
//        }

//        choices.Sort((a, b) => a.Item1.CompareTo(b.Item1));
//        choices.Reverse();
//        foreach (var choice in choices)
//        {
//            GD.Print($"{choice.Item2} time={choice.Item3.TotalSeconds:F2}");
//        }
//    }
//}

//private void Test1()
//{
//    Player player = new Player(0, rnd, SpeciesAtTable, _deal!);
//    List<Hand> hands = new List<Hand>();
//    Suit suitA = Suit.DefaultSuits[0];
//    Suit suitB = Suit.DefaultSuits[1];
//    Suit suitC = Suit.DefaultSuits[2];
//    Suit suitD = Suit.DefaultSuits[3];
//    Rank rank2 = Rank.DefaultRanks[0];
//    Rank rank3 = Rank.DefaultRanks[1];
//    Rank rank4 = Rank.DefaultRanks[2];
//    Rank rank5 = Rank.DefaultRanks[3];
//    Rank rank6 = Rank.DefaultRanks[4];
//    Rank rank7 = Rank.DefaultRanks[5];
//    Rank rank8 = Rank.DefaultRanks[6];
//    Rank rank9 = Rank.DefaultRanks[7];
//    Rank rank10 = Rank.DefaultRanks[8];
//    Rank rankJ = Rank.DefaultRanks[9];
//    Rank rankQ = Rank.DefaultRanks[10];
//    Rank rankK = Rank.DefaultRanks[11];
//    Rank rankA = Rank.DefaultRanks[12];
//    { // straight
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitA, rank2));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitA, rankQ));
//        hand.AddCard(new Card(suitA, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Nothing
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank2));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitA, rankJ));
//        hand.AddCard(new Card(suitA, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Nothing
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank2));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitC, rankK));
//        hand.AddCard(new Card(suitA, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Nothing
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank2));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitD, rankA));
//        hand.AddCard(new Card(suitA, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Two of a kind
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank4));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitA, rankJ));
//        hand.AddCard(new Card(suitC, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Two Pair
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank4));
//        hand.AddCard(new Card(suitD, rankJ));
//        hand.AddCard(new Card(suitA, rankJ));
//        hand.AddCard(new Card(suitC, rankQ));
//        hand.AddCard(new Card(suitA, rankQ));
//        hands.Add(hand);
//    }
//    { // Three of a kind
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank5));
//        hand.AddCard(new Card(suitA, rank5));
//        hand.AddCard(new Card(suitC, rank5));
//        hand.AddCard(new Card(suitC, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // Full House
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank3));
//        hand.AddCard(new Card(suitA, rank3));
//        hand.AddCard(new Card(suitC, rank3));
//        hand.AddCard(new Card(suitC, rank8));
//        hand.AddCard(new Card(suitA, rank8));
//        hands.Add(hand);
//    }
//    { // Four of a kind
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank6));
//        hand.AddCard(new Card(suitA, rank6));
//        hand.AddCard(new Card(suitC, rank6));
//        hand.AddCard(new Card(suitC, rank6));
//        hand.AddCard(new Card(suitA, rank9));
//        hands.Add(hand);
//    }
//    { // flush
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitB, rank2));
//        hand.AddCard(new Card(suitC, rank3));
//        hand.AddCard(new Card(suitA, rankA));
//        hand.AddCard(new Card(suitA, rank4));
//        hand.AddCard(new Card(suitD, rank5));
//        hands.Add(hand);
//    }
//    { // flush
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitA, rankJ));
//        hand.AddCard(new Card(suitB, rank10));
//        hand.AddCard(new Card(suitC, rank9));
//        hand.AddCard(new Card(suitA, rank8));
//        hand.AddCard(new Card(suitD, rank7));
//        hands.Add(hand);
//    }
//    { // straight flush
//        Hand hand = new Hand(player);
//        hand.AddCard(new Card(suitC, rank8));
//        hand.AddCard(new Card(suitC, rank4));
//        hand.AddCard(new Card(suitC, rank5));
//        hand.AddCard(new Card(suitC, rank6));
//        hand.AddCard(new Card(suitC, rank7));
//        hands.Add(hand);
//    }
//    foreach (Hand hand in hands)
//    {
//        hand.ComputeBestScore(rank2._strength, rankK._strength, 4);
//    }
//    hands.Sort();
//    GD.Print("Worst to best:");
//    foreach (Hand hand in hands)
//    {
//        GD.Print(hand.ToString());
//    }
//}
