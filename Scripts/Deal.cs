using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

class DiscardCards
{
    internal Card Card { get; set; }
    internal List<int> PlayersWhoCanSeeThis = new List<int>();
    internal int PlayerWhoDiscardedThis { get; set; }
};

class Deal
{
    internal List<Suit> _suits = new List<Suit>();
    internal List<Rank> _ranks = new List<Rank>();
    internal List<Card> _drawPile = new List<Card>();
    internal List<Hand> _hands = new List<Hand>();
    internal List<DiscardCards> _discards = new List<DiscardCards> ();

    public Player NonNPCPlayer {
        get
        {
            return _hands[0].Player;
        }
    }

    internal Deal(List<Player> players, Random rnd)
    {
        _suits.AddRange(Suit.DefaultSuits);

        //_suits.RemoveAll(suit => suit._color != Suit.SuitColor.Red);
        //_suits.Add(Suit.Skull);
        //_suits.Add(Suit.Swords);

        _ranks.AddRange(Rank.DefaultRanks);

        //_ranks.Add(Rank.Sadness);
        //_ranks.Add(Rank.Ankh);
        _ranks.Add(Rank.Saturn);
        //_ranks.Add(Rank.Jupiter);

        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);

        foreach (Suit suit in _suits)
        {
            foreach (Rank rank in _ranks)
            {
                Card card = new Card(suit, rank);
                _drawPile.Add(card);
            }
        }

        _drawPile = _drawPile.Shuffle(rnd).ToList();

        //Test(rnd, players[0]);
        foreach (Player player in players)
        {
            Hand hand = new Hand(player);
            for (int i = 0; i < 5; ++i)
            {
                hand.AddCard(_drawPile.First());
                _drawPile.RemoveAt(0);
            }

            hand.ComputeBestScore(minRank, maxRank);

            _hands.Add(hand);
        }
    }

    private void Test(Random rnd, Player player)
    {
        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);
        int flush = 0;
        int straight = 0;
        int straightFlush = 0;
        Dictionary<Tuple<int, int>, int> kindCount = new Dictionary<Tuple<int, int>, int>();
        for (int i = 0; i < 1250000; ++i)
        {
            var newPile = _drawPile.Shuffle(rnd).ToList();
            Hand hand = new Hand(player);
            for (int j = 0; j < 5; ++j)
            {
                hand.AddCard(newPile[j]);
            }

            hand.CountTypes(minRank, maxRank, out bool isFlush, out bool isStraight, out int primaryOfAKind, out int secondaryOfAKind);
            if (isFlush)
                ++flush;
            if (isStraight)
                ++straight;
            if (isStraight && isFlush)
                ++straightFlush;
            var key = Tuple.Create(primaryOfAKind, secondaryOfAKind);
            if (kindCount.TryGetValue(key, out int count))
            {
                kindCount[key] = count + 1;
            }
            else
            {
                kindCount[key] = 1;
            }
        }

        GD.Print($"Flush: {flush}");
        GD.Print($"Straight: {straight}");
        GD.Print($"Straight Flush: {straightFlush}");
        foreach (var entry in kindCount)
        {
            GD.Print($"{entry.Key}: {entry.Value}");
        }
    }

    private void Test2(Random rnd, Player player)
    {
        Rank.ExtractMinAndMax(_ranks, out int minRank, out int maxRank);
        Hand? bestHand = null;
        Hand? worstHand = null;
        for (int i = 0; i < 25000; ++i)
        {
            var newPile = _drawPile.Shuffle(rnd).ToList();
            Hand hand = new Hand(player);
            for (int j = 0; j < 5; ++j)
            {
                hand.AddCard(newPile[j]);
            }

            hand.ComputeBestScore(minRank, maxRank);
            if (bestHand == null || hand.CompareTo(bestHand) > 0)
            {
                bestHand = hand;
            }
            if (worstHand == null || hand.CompareTo(worstHand) < 0)
            {
                worstHand = hand;
            }
        }

        GD.Print($"Best Hand: {bestHand}");
        GD.Print($"Worst Hand: {worstHand}");
    }

    public void Dump()
    {
        GD.Print("Sorted hands:");

        _hands.Sort();
        _hands.Reverse();

        foreach(Hand hand in _hands)
        {
            GD.Print(hand);
        }
    }

    internal void UpdateHUD(HUD hud)
    {
        foreach (Hand hand in _hands)
        {
            hud.SetVisibleHand(hand, NonNPCPlayer);
        }
    }

    internal void Reveal(Player player, HUD hud, string description)
    {
        player.HasRevealed = true;
        hud.SetVisibleHand(GetPlayerHand(player), NonNPCPlayer);
        hud.SetBetAmount(player.PositionID, player.AmountBet, description);
    }

    internal List<Card> AvailableCardsFromHandsView(Hand viewHand)
    {
        List<Card> retVal = new List<Card>();
        retVal.AddRange(_drawPile);
        foreach (Hand hand in _hands)
        {
            if (hand == viewHand)
                continue;
            foreach (Card card in hand._cards)
            {
                if (!hand.IsVisible(card, viewHand.Player))
                {
                    retVal.Add(card);
                }
            }
        }

        return retVal;
    }

    internal Hand GetPlayerHand(Player player)
    {
        return _hands.First(a => a._player == player);
    }

    internal void MoveCardToDiscard(HUD hud, Player player, List<int> playersWhoCanSeeThisDiscard, Card card)
    {
        Hand hand = GetPlayerHand(player);
        hand._cards.Remove(card);
        _discards.Add(new DiscardCards() { Card = card, PlayerWhoDiscardedThis = player.PositionID, PlayersWhoCanSeeThis = playersWhoCanSeeThisDiscard });
        hud.SetVisibleHand(hand, NonNPCPlayer);
        hud.MoveCardToDiscard(player.PositionID, card, playersWhoCanSeeThisDiscard.Contains(NonNPCPlayer.PositionID));
    }

    internal bool ProgressReplaceDiscard(HUD hud)
    {
        foreach (Hand hand in _hands)
        {
            if (hand._cards.Count < 5)
            {
                Card card = _drawPile.First();
                _drawPile.RemoveAt(0);
                hand._cards.Add(card);
                hud.SetVisibleHand(hand, NonNPCPlayer);
                return true;
            }
        }

        return false;
    }

    internal void ExtractMinAndMax(out int minRank, out int maxRank)
    {
        Rank.ExtractMinAndMax(_ranks, out minRank, out maxRank);
    }

    internal double WhatIsThePercentChanceOtherPlayerIsBetterThanOurHand(Player otherPlayer, Hand ourHand, List<Card> unseenCards, Random rnd)
    {
        ExtractMinAndMax(out int minRank, out int maxRank);
        Hand otherHand = GetPlayerHand(otherPlayer);

        int theyWin = 0;
        const int numHandsToCreate = 1000;
        for (int i = 0; i < numHandsToCreate; ++i)
        {
            Hand potentialHand = otherHand.GeneratePotentialHand(unseenCards, rnd);
            potentialHand.ComputeBestScore(minRank, maxRank);

            // #TODO: The hand now needs to discard the same amount that the other player discarded.

            if (ourHand.CompareTo(potentialHand) < 0)
            {
                theyWin += 1;
            }
        }

        return 100.0 * theyWin / numHandsToCreate;
    }
}

public static class ExtensionMethods
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
    {
        return source.Select(a => Tuple.Create(rnd.Next(), a)).ToList().OrderBy(a => a.Item1).Select(a => a.Item2);
    }
}