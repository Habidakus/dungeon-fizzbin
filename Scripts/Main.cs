using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable

public partial class Main : Node
{
    Random rnd = new Random((int)DateTime.Now.Ticks);
    private List<Player> _players = new List<Player>();
    private Deal? _deal = null;

    internal int Dealer { get; private set; }
    internal int InitialBetter { get { return (Dealer + 1) % _players.Count; } }
    internal int CurrentBetter { get; private set; }
    private double currentBetLimit = 1.0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        for (int i = 0; i < 5; ++i)
        {
            _players.Add(new Player(i));
        }

        Dealer = 0; // Should advance each round
        CurrentBetter = InitialBetter;
        currentBetLimit = 1.0; // $1.00
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void Test(Random rnd)
    {
        Player player = new Player(0);
        
        Suit suitA = Suit.DefaultSuits[0];
        Suit suitB = Suit.DefaultSuits[1];
        Suit suitC = Suit.DefaultSuits[2];
        Suit suitD = Suit.DefaultSuits[3];

        List<Suit> suits = new List<Suit>()
        {
            suitA, suitB, suitC, suitD
        };

        Rank rank2 = Rank.DefaultRanks[0];
        Rank rank3 = Rank.DefaultRanks[1];
        Rank rank4 = Rank.DefaultRanks[2];
        Rank rank5 = Rank.DefaultRanks[3];
        Rank rank6 = Rank.DefaultRanks[4];
        Rank rank7 = Rank.DefaultRanks[5];
        Rank rank8 = Rank.DefaultRanks[6];
        Rank rank9 = Rank.DefaultRanks[7];
        Rank rank10 = Rank.DefaultRanks[8];
        Rank rankJ = Rank.DefaultRanks[9];
        Rank rankQ = Rank.DefaultRanks[10];
        Rank rankK = Rank.DefaultRanks[11];
        Rank rankA = Rank.DefaultRanks[12];

        List<Rank> ranks = new List<Rank>()
        {
            rank2,
            rank3,
            rank4,
            rank5,
            rank6,
            rank7,
            rank8,
            rank9,
            rank10,
            rankJ,
            rankQ,
            rankK,
            rankA,
        };

        Rank.ExtractMinAndMax(ranks, out int minRank, out int maxRank);

        { // A 9 4 3 2
            Hand hand = new Hand(player);

            //Card cardAce = new Card(suitA, rankA);
            //Card cardFour = new Card(suitC, rank4);
            //Card cardThree = new Card(suitD, rank3);

            hand.AddCard(new Card(suitA, rankA));
            hand.AddCard(new Card(suitB, rankA));
            hand.AddCard(new Card(suitC, rankQ));
            hand.AddCard(new Card(suitA, rank9));
            hand.AddCard(new Card(suitA, rank3));
            hand.ComputeBestScore(minRank, maxRank);

            GD.Print($"Evaluating {hand}");

            List<Card> availableCards = new List<Card>();
            foreach(Rank rank in ranks)
            {
                foreach(Suit suit in suits)
                {
                    Card c = new Card(suit, rank);
                    bool addCard = true;
                    foreach (Card card in hand._cards)
                    {
                        if (c.CompareTo(card) == 0)
                        {
                            addCard = false;
                        }
                    }

                    if (addCard)
                    {
                        availableCards.Add(c);
                    }
                }
            }

            //List<Tuple<Hand, Card, Card>> sortedList = new List<Tuple<Hand, Card, Card>>();
            //foreach (Card discardCard in new List<Card>() { cardNine, cardTwo })
            //{
            //    foreach (Card replacement in availableCards)
            //    {
            //        Hand potentialHand = hand.CloneWithDiscard(discardCard, replacement);
            //        potentialHand.ComputeBestScore(minRank, maxRank);
            //        sortedList.Add(Tuple.Create(potentialHand, discardCard, replacement));
            //    }
            //}

            //sortedList.Sort((a,b) => {
            //    return a.Item1.CompareTo(b.Item1);
            //});

            //foreach (var entry in sortedList)
            //{
            //    GD.Print($"Replacing {entry.Item2} for {entry.Item1}: {entry.Item1.GetDesc()}");
            //}

            List<Tuple<AggregateValue, string, TimeSpan>> choices = new List<Tuple<AggregateValue, string, TimeSpan>>();
            foreach (int i in new List<int>(){ 2, 3, 3, 3, 3})
            {
                DateTime start = DateTime.Now;
                Tuple<AggregateValue, List<Card>> discard = hand.SelectDiscards(i, availableCards, minRank, maxRank, rnd);
                TimeSpan dur = DateTime.Now - start;
                StringBuilder cardsAsText = new StringBuilder();
                if (discard.Item2.Count == 0)
                {
                    cardsAsText.Append("nothing");
                }
                else
                {
                    foreach (Card card in discard.Item2)
                    {
                        if (cardsAsText.Length > 0)
                            cardsAsText.Append(", ");
                        cardsAsText.Append(card.ToString());
                    }
                }

                choices.Add(Tuple.Create(discard.Item1, $"Discarding {i} cards: trying for {discard.Item1.GetDesc()} by discarding {cardsAsText}", dur));
            }

            choices.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            choices.Reverse();
            foreach (var choice in choices)
            {
                GD.Print($"{choice.Item2} time={choice.Item3.TotalSeconds:F2}");
            }
        }
    }

    private void Test1()
    {
        Player player = new Player(0);
        List<Hand> hands = new List<Hand>();
        Suit suitA = Suit.DefaultSuits[0];
        Suit suitB = Suit.DefaultSuits[1];
        Suit suitC = Suit.DefaultSuits[2];
        Suit suitD = Suit.DefaultSuits[3];
        Rank rank2 = Rank.DefaultRanks[0];
        Rank rank3 = Rank.DefaultRanks[1];
        Rank rank4 = Rank.DefaultRanks[2];
        Rank rank5 = Rank.DefaultRanks[3];
        Rank rank6 = Rank.DefaultRanks[4];
        Rank rank7 = Rank.DefaultRanks[5];
        Rank rank8 = Rank.DefaultRanks[6];
        Rank rank9 = Rank.DefaultRanks[7];
        Rank rank10 = Rank.DefaultRanks[8];
        Rank rankJ = Rank.DefaultRanks[9];
        Rank rankQ = Rank.DefaultRanks[10];
        Rank rankK = Rank.DefaultRanks[11];
        Rank rankA = Rank.DefaultRanks[12];
        { // straight
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitA, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankQ));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitC, rankK));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Nothing
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitD, rankA));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Two of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank4));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Two Pair
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank4));
            hand.AddCard(new Card(suitD, rankJ));
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitC, rankQ));
            hand.AddCard(new Card(suitA, rankQ));
            hands.Add(hand);
        }
        { // Three of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank5));
            hand.AddCard(new Card(suitA, rank5));
            hand.AddCard(new Card(suitC, rank5));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // Full House
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank3));
            hand.AddCard(new Card(suitA, rank3));
            hand.AddCard(new Card(suitC, rank3));
            hand.AddCard(new Card(suitC, rank8));
            hand.AddCard(new Card(suitA, rank8));
            hands.Add(hand);
        }
        { // Four of a kind
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank6));
            hand.AddCard(new Card(suitA, rank6));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitA, rank9));
            hands.Add(hand);
        }
        { // flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitB, rank2));
            hand.AddCard(new Card(suitC, rank3));
            hand.AddCard(new Card(suitA, rankA));
            hand.AddCard(new Card(suitA, rank4));
            hand.AddCard(new Card(suitD, rank5));
            hands.Add(hand);
        }
        { // flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitA, rankJ));
            hand.AddCard(new Card(suitB, rank10));
            hand.AddCard(new Card(suitC, rank9));
            hand.AddCard(new Card(suitA, rank8));
            hand.AddCard(new Card(suitD, rank7));
            hands.Add(hand);
        }
        { // straight flush
            Hand hand = new Hand(player);
            hand.AddCard(new Card(suitC, rank8));
            hand.AddCard(new Card(suitC, rank4));
            hand.AddCard(new Card(suitC, rank5));
            hand.AddCard(new Card(suitC, rank6));
            hand.AddCard(new Card(suitC, rank7));
            hands.Add(hand);
        }
        foreach (Hand hand in hands)
        {
            hand.ComputeBestScore(rank2._strength, rankK._strength);
        }
        hands.Sort();
        GD.Print("Worst to best:");
        foreach (Hand hand in hands)
        {
            GD.Print(hand.ToString());
        }
    }

    internal void StartFreshDeal()
    {

        //Test(rnd);

        _deal = new Deal(_players, rnd);
        _deal.UpdateHUD(GetHUD());
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

    public bool SomeoneNeedsToDiscard()
    {
        foreach (Player player in _players)
        {
            if (!player.HasDiscarded)
            {
                return true;
            }
        }

        return false;
    }

    public void ForceSomeoneToDiscard()
    {
        if (_deal == null)
            throw new Exception("Can force someone to discard if there is no deal");

        for (int i = 0; i < _players.Count; ++i)
        {
            int j = (InitialBetter + i) % _players.Count;
            if (!_players[j].HasDiscarded)
            {
                Hand hand = _deal.GetPlayerHand(_players[j]);
                _players[j].Discards = hand.SelectDiscards(0, 3, _deal, rnd);
                _players[j].HasDiscarded = true;
                //hud.SetHandDiscards(hand);

                GetStateMachine().SwitchState("Play_Animate_Discards");

                return;
            }
        }

        throw new Exception("There was no player awaiting discard");
    }

    public bool ProgressDiscardAnimation()
    {
        if (_deal == null)
            throw new Exception("Can force someone to animate discard if there is no deal");

        HUD hud = GetHUD();
        foreach (Player player in _players)
        {
            if (player.Discards != null && player.Discards.Count > 0)
            {
                Card card = player.Discards.First();
                List<int> playersWhoCanSeeThisDiscard = new List<int>();
                foreach (Player viewingPlayer in _players)
                {
                    if (_deal.GetPlayerHand(player).IsVisible(card, viewingPlayer))
                    {
                        playersWhoCanSeeThisDiscard.Add(viewingPlayer.PositionID);
                    }
                }

                _deal.MoveCardToDiscard(hud, player, playersWhoCanSeeThisDiscard, card);
                player.Discards.RemoveAt(0);
                return true;
            }
        }

        if (_deal.ProgressReplaceDiscard(hud))
            return true;

        return false;
    }

    public bool SomeoneNeedsToBet()
    {
        int consider = _players.Count;

        while (consider > 0)
        {
            if (_players[CurrentBetter].HasFolded)
            {
                consider -= 1;
                CurrentBetter = (CurrentBetter + 1) % _players.Count;
                continue;
            }

            if (_players[CurrentBetter].AmountBet == currentBetLimit)
            {
                consider -= 1;
                CurrentBetter = (CurrentBetter + 1) % _players.Count;
                continue;
            }

            if (_players[CurrentBetter].AmountBet > currentBetLimit)
                throw new Exception($"Why is player #{CurrentBetter} got more bet ({_players[CurrentBetter].AmountBet}) than the current bet limit ({currentBetLimit})?");

            return true;
        }

        return false;
    }

    public void ForceNextBet()
    {
        if (_deal == null)
            throw new Exception("Can force someone to bet if there is no deal");

        _deal.ExtractMinAndMax(out int minRank, out int maxRank);

        Hand hand = _deal.GetPlayerHand(_players[CurrentBetter]);
        hand.ComputeBestScore(minRank, maxRank);

        double maxPercent = double.MinValue;
        foreach (Player player in _players)
        {
            if (player.PositionID != hand.PositionID)
            {
                if (!player.HasFolded)
                {
                    List<Card> unseenCards = _deal.AvailableCardsFromHandsView(hand);
                    double percent = _deal.WhatIsThePercentChanceOtherPlayerIsBetterThanOurHand(player, hand, unseenCards, rnd);
                    if (percent > maxPercent)
                        maxPercent = percent;
                }
            }
        }

        GD.Print($"Player #{_players[CurrentBetter].PositionID} believes they have {100.0 - maxPercent:F2}% chance of winning");

        if (maxPercent > 75)
        {
            _players[CurrentBetter].Fold(GetHUD());
        }
        else
        {
            double howMuchToBet = 3.0 - (maxPercent / 25.0);
            howMuchToBet = 10.0 * howMuchToBet * howMuchToBet / 9.0;
            _players[CurrentBetter].Bet(GetHUD(), howMuchToBet, currentBetLimit);
            if (!_players[CurrentBetter].HasFolded)
                currentBetLimit = _players[CurrentBetter].AmountBet;
        }
    }

    public bool SomeoneNeedsToReveal()
    {
        foreach (Player player in _players)
        {
            if (player.HasFolded)
                continue;

            if (!player.HasRevealed)
            {
                return true;
            }
        }

        return false;
    }

    public void RevealHand()
    {
        if (_deal == null)
            throw new Exception("Can force someone to reveal if there is no deal");

        for (int i = 0;i < _players.Count; ++i)
        {
            int position = (InitialBetter + i) % _players.Count;
            if (_players[position].HasFolded)
                continue;
            if (_players[position].HasRevealed)
                continue;

            Hand revealedHand = _deal.GetPlayerHand(_players[position]);
            _deal.Reveal(_players[position], GetHUD(), revealedHand.ScoreAsString());
            foreach(Player player in _players)
            {
                if (!player.HasRevealed)
                    continue;
                if (player.PositionID == position)
                    continue;
                
                Hand previouslyRevealedHand = _deal.GetPlayerHand(player);
                int comparison = previouslyRevealedHand.CompareTo(revealedHand);
                if (comparison > 0)
                {
                    GetHUD().SetFeltToLost(position);
                }
                else if (comparison < 0)
                {
                    GetHUD().SetFeltToLost(player.PositionID);
                }
                else
                {
                    throw new Exception($"How are hands {revealedHand} and {previouslyRevealedHand} equal?");
                }
            }

            return;
        }
    }

}
