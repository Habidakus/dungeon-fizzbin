using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

class Bits
{
    internal int Count;
    internal uint Value;

    public Bits()
    {
        Count = 0;
        Value = 0;
    }

    public Bits(int min, int maxNonInclusive)
    {
        Count = maxNonInclusive - min;
        for (int i = min; i < maxNonInclusive; ++i)
        {
            Value |= (uint)0x1 << (int)i;
        }
    }

    public int FirstInt
    {
        get
        {
            if (Count == 0)
            {
                throw new Exception("Bad bit collection, no first bit");
            }

            int retVal = 0;
            uint v = Value;
            while ((v & 0x00000001) != 0x00000001)
            {
                ++retVal;
                v >>= 1;
            }

            return retVal;
        }
    }

    public IEnumerable<Bits> EachBit
    {
        get
        {
            int c = Count;
            uint v = 0x1;
            while (c > 0)
            {
                if ((Value & v) == v)
                {
                    yield return new Bits() { Count = 1, Value = v };
                    --c;
                }

                v <<= 1;
            }

            yield break;
        }
    }

    public IEnumerable<int> EachInt
    {
        get
        {
            int c = Count;
            uint v = 0x1;
            int retVal = 0;
            while (c > 0)
            {
                if ((Value & v) == v)
                {
                    yield return retVal;
                    --c;
                }

                v <<= 1;
                ++retVal;
            }

            yield break;
        }
    }

    internal Bits OnlyBitsGreaterThan(int slot)
    {
        int cRead = Count;
        int cWrite = 0;
        uint v = 0x1;
        uint vIndex = 0;
        uint vWrite = 0;
        while (cRead > 0)
        {
            if ((Value & v) == v)
            {
                --cRead;

                if (vIndex > slot)
                {
                    ++cWrite;
                    vWrite |= v;
                }
            }

            ++vIndex;
            v <<= 1;
        }

        return new Bits() { Count = cWrite, Value = vWrite };
    }

    internal Bits With(int slot)
    {
        uint b = (uint)0x1 << slot;
        if ((Value & b) == b)
        {
            throw new Exception("Bit already set");
        }

        return new Bits() { Count = this.Count + 1, Value = this.Value | b };
    }

    internal void Set(int slot)
    {
        uint b = (uint)0x1 << slot;
        if ((Value & b) == b)
        {
            throw new Exception("Bit already set");
        }

        Count += 1;
        Value |= b;
    }

    internal void Split(out int firstInt, out Bits remainingBits)
    {
        firstInt = FirstInt;
        remainingBits = new Bits() { Count = this.Count - 1, Value = (this.Value & ~((uint)0x1 << firstInt)) };
    }
}

class ExposedCard
{
    internal bool CanBeDiscarded { get; private set; }
    internal List<int> PlayersWhoCanSee { get; private set; } = new List<int>();
    internal bool CanSee(int positionID) { return PlayersWhoCanSee.Contains(positionID); }
    internal bool CanAnyoneElseSee(int viewerID) { return PlayersWhoCanSee.Where(a => a != viewerID).Any(); }
    internal ExposedCard(int seer, bool canBeDiscarded)
    {
        CanBeDiscarded = canBeDiscarded;
        PlayersWhoCanSee.Add(seer);
    }
    internal ExposedCard(ExposedCard other)
    {
        CanBeDiscarded = other.CanBeDiscarded;
        PlayersWhoCanSee = new List<int>(other.PlayersWhoCanSee);
    }
    internal void Add(int seer, bool canBeDiscarded)
    {
        CanBeDiscarded &= canBeDiscarded;
        if (!CanSee(seer))
        {

            PlayersWhoCanSee.Add(seer);
        }
    }

    internal void Add(ExposedCard otherCard)
    {
        CanBeDiscarded &= otherCard.CanBeDiscarded;
        foreach (int seer in otherCard.PlayersWhoCanSee)
        {
            if (!CanSee(seer))
            {
                PlayersWhoCanSee.Add(seer);
            }
        }
    }
}

class ExposedCardHandler
{
    Dictionary<Card, ExposedCard>? _exposedCards = null;

    internal void CopyFrom(ExposedCardHandler otherHandler)
    {
        _exposedCards = otherHandler._exposedCards == null
            ? null
            : new Dictionary<Card, ExposedCard>(otherHandler._exposedCards);
    }

    internal void CopyCardFrom(ExposedCardHandler otherHandler, Card card)
    {
        if (otherHandler._exposedCards != null && otherHandler._exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            Add(card, exposedCard);
        }
    }

    internal void Add(Card card, ExposedCard otherCard)
    {
        if (_exposedCards == null)
        {
            _exposedCards = new Dictionary<Card, ExposedCard>();
        }

        if (_exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            exposedCard.Add(otherCard);
            _exposedCards[card] = exposedCard;
        }
        else
        {
            _exposedCards[card] = new ExposedCard(otherCard);
        }
    }

    internal void Add(Card card, int seer, bool canDiscard)
    {
        if (_exposedCards == null)
        {
            _exposedCards = new Dictionary<Card, ExposedCard>();
        }

        if (_exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            exposedCard.Add(seer, canDiscard);
            _exposedCards[card] = exposedCard;
        }
        else
        {
            _exposedCards[card] = new ExposedCard(seer, canDiscard);
        }
    }

    internal bool CanSee(Card card, int positionID)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            return exposedCard.CanSee(positionID);
        }

        return false;
    }

    internal bool IsVisibleToAnyoneElse(Card card, int viewerID)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            return exposedCard.CanAnyoneElseSee(viewerID);
        }

        return false;
    }

    internal bool BlocksDiscard(Card card)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            return !exposedCard.CanBeDiscarded;
        }

        return false;
    }

    internal void ForEachNPCThatCanViewCard(Card card, Action<int> functor)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            foreach (int positionID in exposedCard.PlayersWhoCanSee)
            {
                if (positionID > 0)
                {
                    functor(positionID);
                }
            }
        }
    }

    internal bool IsCardVisibleToEntireTable(Card card)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            return (exposedCard.PlayersWhoCanSee.Count > 3);
        }
        else
        {
            return false;
        }
    }

    internal IEnumerable<int> ObserversOtherThanOwnerAndNonNPC(Card card, int ownerPositionID, int nonNPCPositionID)
    {
        if (_exposedCards != null && _exposedCards.TryGetValue(card, out ExposedCard? exposedCard))
        {
            return exposedCard.PlayersWhoCanSee.Where(a => a != ownerPositionID && a != nonNPCPositionID);
        }
        else
        {
            return Enumerable.Empty<int>();
        }
    }
}

class Hand : IComparable<Hand>
{
    readonly internal Player _player;
    internal List<Card> _cards = new List<Card>();
    internal List<Card>? _passingCards = null;
    internal HandValue? _handValue = null;
    internal ExposedCardHandler _exposedCards = new ExposedCardHandler();

    public int PositionID { get { return _player.PositionID; } }
    public Player Player { get { return _player; } }
    public bool HasPassingCards { get { return _passingCards != null && _passingCards.Count > 0; } }
    public bool PixieCompare { get { return _player.Deal.PixieCompare; } }

    internal Hand(Player player)
    {
        _player = player;
    }

    internal bool IsVisible(Card card, Player viewer)
    {
        if (_player.HasRevealed)
        {
            return true;
        }

        if (_player.PositionID == viewer.PositionID)
        {
            return true;
        }

        if (_exposedCards.CanSee(card, viewer.PositionID))
        {
            return true;
        }

        return false;
    }

    internal bool HasVisibleCards(Player viewer)
    {
        return _cards.Where(a => IsVisible(a, viewer)).Any();
    }

    internal bool IsVisibleToAnyoneElse(Card card, int viewerID)
    {
        return _exposedCards.IsVisibleToAnyoneElse(card, viewerID);
    }

    public bool IsEveryCardVisible(Player viewer)
    {
        foreach (Card card in _cards)
        {
            if (!IsVisible(card, viewer))
            {
                return false;
            }
        }

        return true;
    }

    internal void AddCard(Card card)
    {
        _cards.Add(card);
        _handValue = null;
    }

    internal static bool IsStraight(List<Card> fiveCards, int minRank, int maxRank, bool pixieCompare)
    {
        if (minRank >= maxRank)
        {
            throw new Exception("Min and Max rank not accurately calculated");
        }

        if (fiveCards.Count < 5)
            return false;

        List<Card> cardsInOrder = fiveCards.OrderBy(a => a, Comparer<Card>.Create((a,b) => a.PixieCompareTo(b, pixieCompare))).ToList();
        bool retVal = true;
        for (int i = 1; retVal && i < cardsInOrder.Count; ++i)
        {
            if (!cardsInOrder[i - 1].Rank.IsNeighborTo(cardsInOrder[i].Rank, minRank, maxRank))
                retVal = false;
        }

        // If we didn't find a straight without considering that ACE can appear at the front of the sequence
        if (retVal == false && cardsInOrder.Last().Rank.Wraps)
        {
            if (!cardsInOrder.First().Rank.IsNeighborTo(cardsInOrder.Last().Rank, minRank, maxRank))
            {
                return false;
            }

            for (int i = 1; i < cardsInOrder.Count - 1; ++i)
            {
                if (!cardsInOrder[i - 1].Rank.IsNeighborTo(cardsInOrder[i].Rank, minRank, maxRank))
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            return retVal;
        }
    }

    internal static bool IsFlush(List<Card> fiveCards)
    {
        if (fiveCards.Count < 5)
        {
            return false;
        }

        Suit? suit = null;
        foreach (Card card in fiveCards)
        {
            if (suit == null)
            {
                suit = card.Suit;
            }
            else if (suit != card.Suit)
            {
                return false;
            }

        }

        return true;
    }

    internal static bool IsMinorHouse(List<Card> fiveCards)
    {
        Suit? primary = null;
        int pCount = 0;
        Suit? secondary = null;
        int sCount = 0;
        foreach (Card card in fiveCards)
        {
            if (primary == null)
            {
                primary = card.Suit;
                pCount = 1;
            }
            else if (card.Suit == primary)
            {
                ++pCount;
            }
            else if (secondary == null)
            {
                secondary = card.Suit;
                sCount = 1;
            }
            else if (secondary == card.Suit)
            {
                ++sCount;
            }
            else
            {
                return false;
            }
        }

        return (pCount + 1 == sCount) || (sCount + 1 == pCount);
    }

    internal void ComputeBestScore(int minRank, int maxRank, int suitsCount, List<Card> river)
    {
        Bits availableSlots = new Bits(0, _cards.Count + river.Count);

        _handValue = null;
        foreach(Bits combinations in AllCombinationsOfAvailableSlotsChoseY(5, availableSlots))
        {
            List<Card> fiveCards = new List<Card>();
            foreach (int i in combinations.EachInt)
            {
                fiveCards.Add(i < _cards.Count ? _cards[i] : river![i - _cards.Count]);
            }

            HandValue hv = ComputeBestScore(fiveCards, minRank, maxRank, suitsCount, PixieCompare);
            if (_handValue == null || hv.PixieCompareTo(_handValue, PixieCompare) > 0)
            {
                _handValue = hv;
            }
        }
    }

    internal static HandValue ComputeBestScore(List<Card> fiveCards, int minRank, int maxRank, int suitsCount, bool pixieCompare)
    {
        List<Card> cardsSortedHighestToLowest = fiveCards.OrderByDescending(a => a, Comparer<Card>.Create((a,b)=>a.PixieCompareTo(b, pixieCompare))).ToList();
        Card highCard = cardsSortedHighestToLowest.First();
        HandValue retVal = new HandValue(cardsSortedHighestToLowest);
        bool isStraight = IsStraight(fiveCards, minRank, maxRank, pixieCompare);
        bool isFlush = IsFlush(fiveCards);
        bool isMinorHouse = suitsCount > 4 ? IsMinorHouse(fiveCards) : false;
        if (isStraight)
        {
            if (isFlush)
            {
                Card lowCard = fiveCards.OrderBy(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare))).First();
                if (pixieCompare)
                {
                    if (lowCard.Rank.IsSixOrLower)
                        retVal = new HandValue(HandValue.HandRanking.RoyalFlush, cardsSortedHighestToLowest);
                    else
                        retVal = new HandValue(HandValue.HandRanking.StraightFlush, cardsSortedHighestToLowest);
                }
                else
                {
                    if (lowCard.Rank.IsTenOrHigher)
                        retVal = new HandValue(HandValue.HandRanking.RoyalFlush, cardsSortedHighestToLowest);
                    else
                        retVal = new HandValue(HandValue.HandRanking.StraightFlush, cardsSortedHighestToLowest);
                }
            }
            else if (isMinorHouse)
            {
                retVal = new HandValue(HandValue.HandRanking.Castle, cardsSortedHighestToLowest);
            }
            else
            {
                retVal = new HandValue(HandValue.HandRanking.Straight, cardsSortedHighestToLowest);
            }
        }
        else if (isFlush)
        {
            retVal = new HandValue(HandValue.HandRanking.Flush, cardsSortedHighestToLowest);
        }

        ExtractOfAKind(fiveCards, pixieCompare, out List<Card> ofAKind, out List<Card> remainder);
        if (ofAKind.Count > 0)
        {
            ofAKind.Sort(Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare)));
            ofAKind.Reverse();
            HandValue? possiblyBetter = null;
            if (ofAKind.Count == 5)
            {
                possiblyBetter = new HandValue(HandValue.HandRanking.FiveOfAKind, ofAKind);
            }
            else if (ofAKind.Count == 4)
            {
                List<Card> orderOfImportance = new List<Card>(ofAKind);
                orderOfImportance.AddRange(remainder.OrderByDescending(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare))).ToList());
                possiblyBetter = new HandValue(HandValue.HandRanking.FourOfAKind, orderOfImportance);
            }
            else
            {
                List<Card> orderOfImportance = new List<Card>(ofAKind);
                ExtractOfAKind(remainder, pixieCompare, out List<Card> secondOfAKind, out List<Card> remainderRemainder);
                orderOfImportance.AddRange(secondOfAKind.OrderByDescending(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare))));
                orderOfImportance.AddRange(remainderRemainder.OrderByDescending(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare))));

                if (ofAKind.Count == 3)
                {
                    if (secondOfAKind.Count > 0)
                    {
                        possiblyBetter = new HandValue(HandValue.HandRanking.FullHouse, orderOfImportance);
                    }
                    else
                    {
                        possiblyBetter = new HandValue(HandValue.HandRanking.ThreeOfAKind, orderOfImportance);
                    }
                }
                else if (ofAKind.Count == 2)
                {
                    if (isMinorHouse)
                    {
                        possiblyBetter = new HandValue(HandValue.HandRanking.Prison, cardsSortedHighestToLowest);
                    }
                    else if (secondOfAKind.Count > 0)
                    {
                        possiblyBetter = new HandValue(HandValue.HandRanking.TwoPairs, orderOfImportance);
                    }
                    else
                    {
                        possiblyBetter = new HandValue(HandValue.HandRanking.TwoOfAKind, orderOfImportance);
                    }
                }
            }

            if (possiblyBetter != null)
            {
                if (possiblyBetter.PixieCompareTo(retVal, pixieCompare) > 0)
                {
                    retVal = possiblyBetter;
                }
                else
                {
                    if (retVal._handRanking == HandValue.HandRanking.HighCard)
                    {
                        GD.Print($"HighCard better than {possiblyBetter._handRanking}?");
                    }
                }
            }
        }
        else if (isMinorHouse)
        {
            HandValue possiblyBetter = new HandValue(HandValue.HandRanking.Prison, cardsSortedHighestToLowest);
            if (possiblyBetter.PixieCompareTo(retVal, pixieCompare) > 0)
            {
                retVal = possiblyBetter;
            }
            else
            {
                if (retVal._handRanking == HandValue.HandRanking.HighCard)
                {
                    GD.Print($"HighCard better than {possiblyBetter._handRanking}?");
                }
            }
        }

        return retVal;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(_player.ToString());
        if (_handValue != null)
        {
            sb.Append($": ({ScoreAsString()})");
        }
        else
        {
            sb.Append(": (value unknown)");
        }

        sb.Append(CardsAsString());

        return sb.ToString();
    }

    internal string ScoreAsString()
    {
        if (_handValue != null)
        {
            return $"{_handValue}";
        }
        else
        {
            return "???";
        }
    }

    internal string CardsAsString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Card card in _cards.OrderByDescending(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, PixieCompare))))
        {
            sb.Append(" ");
            sb.Append(card.ToString());
        }

        return sb.ToString();
    }

    public int CompareTo(Hand? other)
    {
        if (other == null)
        {
            throw new Exception("Comparing to null hand");
        }

        if (_handValue == null)
        {
            throw new Exception($"Best Score not computed for {ToString()}");
        }
        
        if (other._handValue == null)
        {
            throw new Exception($"Best Score not computed for {other}");
        }

        return _handValue.PixieCompareTo(other._handValue, PixieCompare);
    }

    //internal void CountTypes(int minRank, int maxRank, out bool isFlush, out bool isStraight, out bool isMinorHouse, out int primaryOfAKind, out int secondaryOfAKind)
    //{
    //    isFlush = IsFlush;
    //    isStraight = IsStraight(minRank, maxRank);
    //    isMinorHouse = IsMinorHouse;
    //    ExtractOfAKind(_cards, PixieCompare, out List<Card> ofAKind, out List<Card> remainder);
    //    primaryOfAKind = ofAKind.Count;
    //    if (primaryOfAKind > 0)
    //    {
    //        ExtractOfAKind(remainder, PixieCompare, out List<Card> secondarySet, out List<Card> _);
    //        secondaryOfAKind = secondarySet.Count;
    //    }
    //    else
    //    {
    //        secondaryOfAKind = 0;
    //    }
    //}

    private static void ExtractOfAKind(List<Card> cards, bool pixieCompare, out List<Card> ofAKind, out List<Card> remainder)
    {
        Rank? bestRank = null;
        int bestCount = 0;
        Dictionary<Rank, List<Card>> buckets = new Dictionary<Rank, List<Card>>();
        foreach (Card card in cards)
        {
            if (buckets.TryGetValue(card.Rank, out List<Card>? cardsOfThisRank))
            {
                cardsOfThisRank.Add(card);
                buckets[card.Rank] = cardsOfThisRank;
                if (cardsOfThisRank.Count > bestCount)
                {
                    bestCount = cardsOfThisRank.Count;
                    bestRank = card.Rank;
                }
                // TODO: If we have a river and more than one hand thus share the same "high card" we need to break the tie
                else if (cardsOfThisRank.Count == bestCount && card.Rank.PixieCompareTo(bestRank, pixieCompare) > 0)
                {
                    bestRank = card.Rank;
                }
            }
            else
            {
                buckets[card.Rank] = new List<Card> { card };
            }
        }

        if (bestCount > 0)
        {
            ofAKind = buckets[bestRank!];
            remainder = new List<Card>();
            foreach (var entry in buckets)
            {
                if (entry.Key != bestRank)
                {
                    remainder.AddRange(entry.Value);
                }
            }
        }
        else
        {
            ofAKind = new List<Card>();
            remainder = new List<Card>(cards);
        }
    }

    internal Hand CloneWithDiscard(Card discardCard, Card replacementCard)
    {
        Hand retVal = new Hand(_player);
        retVal._exposedCards.CopyFrom(_exposedCards);
        foreach (Card card in _cards)
        {
            if (card == discardCard)
            {
                if (_exposedCards.BlocksDiscard(card))
                {
                    throw new Exception("Cannot discard card observed by others");
                }

                retVal.AddCard(replacementCard);
            }
            else
            {
                retVal.AddCard(card);
            }
        }

        return retVal;
    }

    internal Hand GeneratePotentialHand(Span<Card> unseenCards, Random rnd, Player? observer)
    {
        Hand retVal = new Hand(_player);
        foreach (Card card in _cards)
        {
            if (observer != null && IsVisible(card, observer))
            {
                retVal.AddCard(card);
                retVal._exposedCards.CopyCardFrom(_exposedCards, card);
            }
        }

        while (retVal._cards.Count < _cards.Count)
        {
            int cardIndex = rnd.Next() % unseenCards.Length;
            Card card = unseenCards[cardIndex];
            bool alreadyUsed = false;
            for (int i = 0; !alreadyUsed && i < retVal._cards.Count; ++i)
            {
                if (retVal._cards[i] == card)
                {
                    alreadyUsed = true;
                }
            }

            if (!alreadyUsed)
            {
                retVal.AddCard(card);
            }
        }

        return retVal;
    }

    internal static List<Bits> AllCombinationsOfAvailableSlotsChoseY(int choseY, Bits availableSlots)
    {
        if (choseY == 0)
        {
            throw new Exception("Why are we AllCombinationsOfAvailableSlotsChoseY() with zero discards?");
        }

        if (choseY > availableSlots.Count)
        {
            throw new Exception($"Asking for more discards than is available (slots={availableSlots.Count}, discards={choseY})");
        }

        List<Bits> retVal = new List<Bits>();

        if (choseY == availableSlots.Count)
        {
            retVal.Add(availableSlots);
            return retVal;
        }

        if (choseY == 1)
        {
            retVal.AddRange(availableSlots.EachBit);
        }
        else
        {
            foreach (int slot in availableSlots.EachInt)
            {
                Bits subSlots = availableSlots.OnlyBitsGreaterThan(slot);
                if (subSlots.Count >= choseY - 1)
                {
                    foreach (Bits subIter in AllCombinationsOfAvailableSlotsChoseY(choseY - 1, subSlots))
                    {
                        retVal.Add(subIter.With(slot));
                    }
                }
            }
        }

        return retVal;
    }

    private Tuple<bool, List<Hand>> GenerateHands(Bits discardIndices, Span<Card> availableCards, Random rnd, int sampleCutOff)
    {
        if (discardIndices.Count == 0)
        {
            throw new Exception("GenerateHands() with zero discard indicies");
        }

        int multiple = availableCards.Length;
        int count = 1;
        for (int i = 0; i < discardIndices.Count; ++i, --multiple)
        {
            count *= multiple;
        }

        if (count <= sampleCutOff)
        {
            return Tuple.Create(false, GenerateAllHands(discardIndices, availableCards));
        }
        else
        {
            return Tuple.Create(true, GenerateSampledHands(discardIndices, availableCards, rnd, sampleCutOff / 2));
        }
    }

    private void GeneratePullIndices(int cardCount, int discardCount, Random rnd, ref List<int> pullIndices)
    {
        for (int k = 0; k < discardCount; ++k)
        {
            bool matchFound = true;
            while (matchFound)
            {
                matchFound = false;
                pullIndices[k] = rnd.Next() % cardCount;
                for (int i = 0; i < k && !matchFound; ++i)
                {
                    if (pullIndices[i] == pullIndices[k])
                    {
                        matchFound = true;
                    }
                }
            }
        }
    }

    private List<Hand> GenerateSampledHands(Bits discardIndices, Span<Card> availableCards, Random rnd, int numberOfHandsToGenerate)
    {
        List<Hand> retVal = new List<Hand>();
        List<int> pullIndices = new List<int>();
        for (int k = 0; k < discardIndices.Count; ++k)
        {
            pullIndices.Add(k);
        }

        discardIndices.Split(out int firstDiscardIndex, out Bits remainingIndicies);

        for (int i = 0; i< numberOfHandsToGenerate; ++i)
        {
            GeneratePullIndices(availableCards.Length, discardIndices.Count, rnd, ref pullIndices);
            int pullIndex = 0;
            Hand clone = CloneWithDiscard(_cards[firstDiscardIndex], availableCards[pullIndices[pullIndex]]);
            foreach (int discardIndex in remainingIndicies.EachInt)
            {
                if (_exposedCards.BlocksDiscard(_cards[discardIndex]))
                {
                    throw new Exception("Cannot discard card observed by others");
                }

                ++pullIndex;
                clone._cards[discardIndex] = availableCards[pullIndices[pullIndex]];
            }

            retVal.Add(clone);
        }

        return retVal;
    }

    private List<Hand> GenerateAllHands(Bits discardIndices, Span<Card> availableCards)
    {
        List<Hand> retVal = new List<Hand>();
        if (discardIndices.Count == 1)
        {
            foreach (Card card in availableCards)
            {
                retVal.Add(CloneWithDiscard(_cards[discardIndices.FirstInt], card));
            }
        }
        else
        {
            discardIndices.Split(out int initialDiscard, out Bits subIndicies);
            foreach (Card card in availableCards)
            {
                Hand subHand = CloneWithDiscard(_cards[initialDiscard], card);
                Card[] remainingCards = availableCards.ToArray().Where(a => a != card).ToArray();
                retVal.AddRange(subHand.GenerateAllHands(subIndicies, remainingCards));
            }
        }

        return retVal;
    }

    internal Tuple<AggregateValue, List<Card>> SelectDiscards(Bits discardIndices, Span<Card> availableCards, int minRank, int maxRank, int suitsCount, double costToDiscard, Random rnd, int sampleCutOff)
    {
        if (discardIndices.Count == 0)
        {
            throw new Exception("SelectDiscards() with zero discard indicies");
        }

        Tuple<bool, List<Hand>> generatedHands = GenerateHands(discardIndices, availableCards, rnd, sampleCutOff);
        Span<Hand> handSpan = generatedHands.Item2.ToArray().AsSpan();
        SortedSet<Hand> sortedHands = new SortedSet<Hand>();

        // We can't visit every possible hand, but we can sample them nearly randomly. We pick some reasonably large
        // prime to iterate over the list by (guaranteeing no duplicates, and with luck avoiding cluster sampling), and
        // then for 25 milliseconds we iterate over various hands and see what their scores were.
        Stopwatch stopwatch = Stopwatch.StartNew();
        int spanIndex = 0;
        int size = handSpan.Length;
        const int inc = 1627; // prime
        for (int i = 0; i < size; ++i)
        {
            if (stopwatch.ElapsedMilliseconds > 25)
            {
                break;
            }

            Hand hand = handSpan[spanIndex];
            spanIndex = (spanIndex + inc) % size;
            hand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
            sortedHands.Add(hand);
        }

        List<Card> discards = new List<Card>();
        foreach (int index in discardIndices.EachInt)
        {
            discards.Add(_cards[index]);
        }

        AggregateValue aggValue = new AggregateValue(_player, costToDiscard, (double)_player.Deal.MinimumHandToWinPot);
        aggValue.Add(sortedHands.ToArray(), generatedHands.Item1);

        return Tuple.Create(aggValue, discards);
    }

    internal Bits GetViableDiscardSlots(Player? observingPlayer)
    {
        Bits retVal = new Bits();
        for (int i = 0; i<_cards.Count; ++i)
        {
            if (observingPlayer == null || !IsVisible(_cards[i], observingPlayer))
            {
                retVal.Set(i);
            }
        }

        return retVal;
    }

    internal HandValue ApplyRandomDiscard(
        int        noOfDiscards,
        Span<Card> availableCards,
        int        minRank,
        int        maxRank,
        int        suitsCount,
        Random     rnd,
        Player?    observingPlayer)
    {
        if (noOfDiscards == 0)
        {
            if (_handValue == null)
            {
                throw new Exception("We shouldn't be applying random discard to un-evaluated hand");
            }

            return _handValue;
        }

        const int iterations = 16;
        HandValue? retVal = null;
        if (noOfDiscards == 1)
        {
            AggregateValue? bestCardToDiscardValue = null;
            Card? bestCardToDiscard = null;
            int availableCardsCount = availableCards.Length;
            foreach (Card discardCard in _cards)
            {
                if (observingPlayer != null && IsVisible(discardCard, observingPlayer))
                {
                    continue;
                }

                AggregateValue aggValue = new AggregateValue(_player, noOfDiscards * _player.Deal.CostPerDiscard, (double)_player.Deal.MinimumHandToWinPot);
                SortedSet<Hand> sortedHands = new SortedSet<Hand>();
                for (int i = 0; i < iterations; ++i)
                {
                    Hand potentialHand = CloneWithDiscard(discardCard, availableCards[rnd.Next() % availableCardsCount]);
                    potentialHand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
                    sortedHands.Add(potentialHand);
                }

                aggValue.Add(sortedHands.ToArray(), sampled: true);

                if (bestCardToDiscardValue == null || aggValue.CompareTo(bestCardToDiscardValue) > 0)
                {
                    bestCardToDiscardValue = aggValue;
                    bestCardToDiscard = discardCard;
                }
            }

            Hand sampleHand = CloneWithDiscard(bestCardToDiscard!, availableCards[rnd.Next() % availableCardsCount]);
            sampleHand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
            retVal = sampleHand._handValue;
        }
        else
        {
            AggregateValue? bestCardsToDiscardValue = null;
            Bits? bestCardsToDiscard = null;
            Bits viableSlots = GetViableDiscardSlots(observingPlayer);
            int cardsToCosiderAsDiscards = noOfDiscards;
            if (observingPlayer != null)
            {
                cardsToCosiderAsDiscards = Math.Min(cardsToCosiderAsDiscards, _cards.Count - this.CardsVisibleToSeer(observingPlayer.PositionID));
            }
            List<Bits> iterList = AllCombinationsOfAvailableSlotsChoseY(cardsToCosiderAsDiscards, viableSlots);
            if (iterList.Count == 0)
            {
                throw new Exception($"GenerateUniqueDiscardSelections(discards={noOfDiscards} slots={string.Join(',', viableSlots)}) found no valid combos");
            }

            foreach (Bits iter in iterList)
            {
                AggregateValue aggValue = new AggregateValue(_player, noOfDiscards * _player.Deal.CostPerDiscard, (double)_player.Deal.MinimumHandToWinPot);
                SortedSet<Hand> sortedHands = new SortedSet<Hand>();
                List<Hand> hands = GenerateSampledHands(iter, availableCards, rnd, iterations);
                foreach (Hand hand in hands)
                {
                    hand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
                    sortedHands.Add(hand);
                }

                aggValue.Add(sortedHands.ToArray(), sampled: true);

                if (bestCardsToDiscardValue == null || aggValue.CompareTo(bestCardsToDiscardValue) > 0)
                {
                    bestCardsToDiscardValue = aggValue;
                    bestCardsToDiscard = iter;
                }
            }

            Hand sampleHand = GenerateSampledHands(bestCardsToDiscard!, availableCards, rnd, 1).First();
            sampleHand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
            retVal = sampleHand._handValue;
        }

        return retVal!;
    }

    internal Tuple<AggregateValue, List<Card>> SelectDiscards(int noOfDiscards, Span<Card> availableCards, int minRank, int maxRank, int suitsCount, Random rnd)
    {
        if (noOfDiscards == 0)
        {
            return Tuple.Create(new AggregateValue(_player, this, 0, (double)_player.Deal.MinimumHandToWinPot), new List<Card>());
        }

        List<Card> retVal = new List<Card>();

        if (noOfDiscards == 1)
        {
            AggregateValue best = new AggregateValue(_player, 0 * _player.Deal.CostPerDiscard, (double)_player.Deal.MinimumHandToWinPot);
            foreach (Card discardCard in _cards)
            {
                SortedSet<HandValue> scoreList = new SortedSet<HandValue>(Comparer<HandValue>.Create((a,b)=>a.PixieCompareTo(b, PixieCompare)));
                foreach (Card replacement in availableCards)
                {
                    Hand potentialHand = CloneWithDiscard(discardCard, replacement);
                    potentialHand.ComputeBestScore(minRank, maxRank, suitsCount, _player.Deal._river);
                    scoreList.Add(potentialHand._handValue!);
                }

                if (scoreList.Count > 0)
                {
                    AggregateValue aggValue = new AggregateValue(_player, 1 * _player.Deal.CostPerDiscard, (double)_player.Deal.MinimumHandToWinPot);
                    aggValue.Add(scoreList.ToArray(), false);

                    if (best.UpdateIfBetter(aggValue))
                    {
                        retVal = new List<Card> { discardCard };
                    }
                }
            }

            return Tuple.Create(best, retVal);
        }
        else
        {
            AggregateValue best = new AggregateValue(_player, 0, (double)_player.Deal.MinimumHandToWinPot);
            const int sampleCutOff = 45000;
            Bits viableSlots = GetViableDiscardSlots(null);
            double costToDiscard = noOfDiscards * _player.Deal.CostPerDiscard;
            foreach (Bits iter in AllCombinationsOfAvailableSlotsChoseY(noOfDiscards, viableSlots))
            {
                Tuple<AggregateValue, List<Card>> subBest = SelectDiscards(iter, availableCards, minRank, maxRank, suitsCount, costToDiscard, rnd, sampleCutOff);
                if (best.UpdateIfBetter(subBest.Item1))
                {
                    retVal = subBest.Item2;
                }
            }

            return Tuple.Create(best, retVal);
        }
    }

    public void SetAsidePassCards(int numberOfCards, Deal actualDeal, Random rnd, HUD hud, Player destination, Action<int> confirmingPassCardsDetermined)
    {
        if (actualDeal.CostPerDiscard != 0)
        {
            throw new Exception("We should have no cost per discard while passing");
        }

        if (_passingCards != null && _passingCards.Count > 0)
        {
            throw new Exception($"Why is {_player.Name} passing again? HasPassingCards={HasPassingCards}");
        }

        if (_player.IsNPC)
        {
            Task.Run(() =>
            {
                _passingCards = SelectDiscards(numberOfCards, numberOfCards, actualDeal, rnd);
                confirmingPassCardsDetermined(PositionID);
            });
        }
        else
        {
            hud.EnableCardSelection_Passing(PositionID, numberOfCards, destination.Name);
            Task.Run(() =>
            {
                List<string> cardsAsText = hud.HavePlayerSelectCardsToPassOrDiscard(this).Result;
                _passingCards = new List<Card>();
                foreach (string cardText in cardsAsText)
                {
                    Card[] matchingCards = _cards.Where(a => a.ToString().CompareTo(cardText) == 0).ToArray();
                    if (matchingCards.Count() == 1)
                    {
                        _passingCards.Add(matchingCards[0]);
                    }
                    else if (matchingCards.Count() > 1)
                    {
                        throw new Exception($"Expected 1 card match to hand ({this}) but got {string.Join(',', matchingCards.Select(a => a.ToString()))}");
                    }
                    else
                    {
                        throw new Exception($"Expected 1 card match to hand ({this}) but got zero");
                    }
                }
                confirmingPassCardsDetermined(PositionID);
            });
        }
    }

    internal void SetAsidePassCards_Post(Random rnd, HUD hud, double delay, Player destination, Player nonNPCPlayer)
    {
        if (_passingCards == null)
        {
            throw new Exception($"Why is {this} passing cards null to {destination}?");
        }

        if (!_player.IsNPC)
        {
            hud.DisableCardSelection(PositionID);
        }

        foreach (Card card in _passingCards)
        {
            bool isVisible = IsVisible(card, nonNPCPlayer);
            int cardIndex = _cards.FindIndex(a => a == card);
            hud.FlingCard(PositionID, destination.PositionID, delay, rnd, card, cardIndex, isVisible);
        }

        foreach (Card card in _passingCards)
        {
            _cards.Remove(card);
        }

        if (_passingCards.Count > 0)
        {
            _handValue = null;
        }
    }

    internal List<Card> SelectDiscards(int minDiscards, int maxDiscards, Deal actualDeal, Random rnd)
    {
        actualDeal.ExtractMinAndMax(out int minRank, out int maxRank, out int suitsCount);
        Card[] availableCards = actualDeal.AvailableCardsFromHandsView(this).ToArray();
        List<Card> retVal = new List<Card>();
        AggregateValue best = new AggregateValue(_player, actualDeal.CostPerDiscard * 0.0, (double)_player.Deal.MinimumHandToWinPot);
        for (int discards = minDiscards; discards <= maxDiscards; ++discards)
        {
            (AggregateValue bestScoreWithThisDiscardCount, List<Card> cardsToDiscard) = SelectDiscards(discards, availableCards, minRank, maxRank, suitsCount, rnd);
            if (best.UpdateIfBetter(bestScoreWithThisDiscardCount))
            {
                retVal = cardsToDiscard;
            }
        }

        //if (best._hopefulValue != null)
        //{
        //    StringBuilder cardsToText = new StringBuilder();
        //    foreach (Card card in retVal)
        //    {
        //        if (cardsToText.Length > 0)
        //        {
        //            cardsToText.Append(" ");
        //        }
        //        cardsToText.Append(card.ToString());
        //    }
        //    GD.Print($"Player {_player.PositionID} replaces {cardsToText} trying for {best.GetDesc()}");
        //}

        return retVal;
    }

    internal void RevealHighestCardsToOtherPlayer(HUD hud, int revealRightNeighborsHighestCards, Player viewingPlayer, bool pixieCompare)
    {
        if (revealRightNeighborsHighestCards > 0)
        {
            List<Card> cardsToReveal = _cards.OrderBy(a => a, Comparer<Card>.Create((a, b) => a.PixieCompareTo(b, pixieCompare))).TakeLast(revealRightNeighborsHighestCards).ToList();
            foreach (Card card in cardsToReveal)
            {
                _exposedCards.Add(card, viewingPlayer.PositionID, canDiscard: false);
                int[] playersWhoCanSeeOtherThanOwnerAndNonNPC =
                    ObserversOtherThanOwnerAndNonNPC(card, _player.Deal.NonNPCPlayer.PositionID).ToArray();
                hud.ExposeCardToOtherPlayer(PositionID, card, viewingPlayer, playersWhoCanSeeOtherThanOwnerAndNonNPC);
            }
        }
    }

    internal int CardsVisibleToSeer(int seer)
    {
        int retVal = 0;
        foreach (Card card in _cards)
        {
            if (_exposedCards.CanSee(card, seer))
            {
                ++retVal;
            }
        }

        return retVal;
    }

    internal void ForEachNPCThatCanViewCard(Card card, Action<int> functor)
    {
        _exposedCards.ForEachNPCThatCanViewCard(card, functor);
    }

    internal bool IsCardVisibleToEntireTable(Card card)
    {
        return _exposedCards.IsCardVisibleToEntireTable(card);
    }

    internal IEnumerable<int> ObserversOtherThanOwnerAndNonNPC(Card card, int nonNPCPositionID)
    {
        return _exposedCards.ObserversOtherThanOwnerAndNonNPC(card, _player.PositionID, nonNPCPositionID);
    }
}

class AggregateValue : IComparable<AggregateValue>
{
    readonly Player _player;
    private HandValue? _hopefulValue = null;
    internal double _normalizedWealth;
    readonly private double _minHandWorth;
    internal double DiscardCost { get; private set; } = 0;
    private bool PixieCompare { get { return _player.Deal.PixieCompare; } }
    internal Dictionary<HandValue.HandRanking, Tuple<double, int>> _normalizedByRank = new Dictionary<HandValue.HandRanking, Tuple<double, int>>();

    public AggregateValue(Player player, Hand hand, double discardCost, double minHandWorth)
    {
        _player = player;
        SetHopefulValue(hand._handValue);

        _minHandWorth = minHandWorth;
        _normalizedWealth = 0;
        DiscardCost = discardCost;
        if (hand._handValue == null)
        {
            throw new Exception("Initializing with scoreless hand");
        }

        AddAggreageWorth(hand._handValue, 1);
    }

    internal AggregateValue(Player player, double discardCost, double minHandWorth)
    {
        _player = player;
        _normalizedWealth = 0;
        _minHandWorth = minHandWorth;
        DiscardCost = discardCost;
    }

    internal string DictText
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var pair in _normalizedByRank)
            {
                if (!first)
                {
                    sb.Append(", ");
                }

                first = false;
                sb.Append($"{pair.Key} ${pair.Value.Item1 - DiscardCost:F2} {pair.Value.Item2}");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    internal string GetDesc()
    {
        if (_hopefulValue != null)
        {
            return $"{_hopefulValue.GetDesc()} ${_normalizedWealth - DiscardCost:F2} {DictText}";
        }
        else
        {
            return $"() ${_normalizedWealth - DiscardCost:F2} {DictText}";
        }
    }

    internal void Add(Hand[] sortedHands, bool sampled)
    {
        _normalizedWealth = 0;

        // Best values are at the back
        int i = sortedHands.Length * 11 / 12;
        SetHopefulValue(sortedHands.ElementAt(i)._handValue);

        // If we're reading sampled values then we can poison the results by having some bad edge cases.
        int maxReads = sortedHands.Length;
        if (sampled)
        {
            maxReads = (int)Math.Round(1999 * maxReads / 2000f);
        }

        for (i = 0; i < maxReads; ++i)
        {
            AddAggreageWorth(sortedHands.ElementAt(i)._handValue, maxReads);
        }
    }

    internal void Add(HandValue[] sortedValues, bool sampled)
    {
        _normalizedWealth = 0;

        // Best values are at the back
        int i = sortedValues.Length * 11 / 12;
        SetHopefulValue(sortedValues.ElementAt(i));

        // If we're reading sampled values then we can poison the results by having some bad edge cases.
        int maxReads = sortedValues.Length;
        if (sampled)
        {
            maxReads = (int)Math.Round(999 * maxReads / 1000f);
        }

        for (i = 0; i < maxReads; ++i)
        {
            AddAggreageWorth(sortedValues.ElementAt(i), maxReads);
        }
    }

    private void SetHopefulValue(HandValue? handValue)
    {
        if (_hopefulValue != null)
        {
            throw new Exception("Clobbering AggregateValue");
        }

        _hopefulValue = handValue;
    }

    private void AddAggreageWorth(HandValue? handValue, int setSize)
    {
        if (handValue == null)
        {
            return;
        }

        double highCardValue = (handValue._highCard.FractionalValue / Card.MaxFractionalValue);
        double handWorth = handValue.Worth < _minHandWorth ? 0 : handValue.Worth;
        double totalValue = handWorth + highCardValue - (DiscardCost * HandValue.MinWorth * 10);
        _normalizedWealth += totalValue / (double)setSize;

        if (_normalizedByRank.TryGetValue(handValue._handRanking, out Tuple<double, int>? value))
        {
            _normalizedByRank[handValue._handRanking] = Tuple.Create((totalValue / (double)setSize) + value.Item1, value.Item2 + 1);
        }
        else
        {
            _normalizedByRank[handValue._handRanking] = Tuple.Create((totalValue / (double)setSize), 1);
        }
    }

    public int CompareTo(AggregateValue? other)
    {
        if (other == null)
        {
            return 1;
        }
        
        if (_normalizedWealth != other._normalizedWealth)
        {
            return _normalizedWealth.CompareTo(other._normalizedWealth);
        }

        if (_hopefulValue == null)
        {
            throw new Exception($"Aggregate value [{GetDesc()}] doesn't have computed value");
        }

        return _hopefulValue.PixieCompareTo(other._hopefulValue, PixieCompare);
    }

    internal bool UpdateIfBetter(AggregateValue other)
    {
        if (_normalizedWealth < other._normalizedWealth)
        {
            _hopefulValue = other._hopefulValue;
            _normalizedWealth = other._normalizedWealth;
            _normalizedByRank = other._normalizedByRank;
            DiscardCost = other.DiscardCost;
            return true;
        }

        return false;
    }

}

class HandValue /*: IComparable<HandValue>*/
{
    internal enum HandRanking
    {
        FiveOfAKind = 3898434 * 4, // Only possible with more suits
        RoyalFlush = 3898434,
        StraightFlush = 433154,
        Castle = 75656, // Only allowed when 5+ suits
        FourOfAKind = 24984,
        FullHouse = 4158,
        Flush = 3047, // LESS COMMON with more suits
        Straight = 1523,
        ThreeOfAKind = 278,
        Prison = 146, // Ignores pairs if present, only if 5+ suits
        TwoPairs = 120,
        TwoOfAKind = 8,
        HighCard = 6, // Maybe just lower this?
    };

    internal HandRanking _handRanking;
    internal Card _highCard;
    internal Double _fractionalValue;
    internal static double MinWorth { get { return (double)HandRanking.HighCard; } }
    internal double Worth { get { return (double)_handRanking; } }

    internal string GetDesc()
    {
        return $"{ToString()} ({_highCard}) ({_fractionalValue:F2})";
    }

    public static string GetPlayerFacingTextForHandRanking(HandRanking handRanking)
    {
        switch (handRanking)
        {
            case HandRanking.FiveOfAKind: return "Five of a Kind";
            case HandRanking.RoyalFlush: return "Royal Flush";
            case HandRanking.StraightFlush: return "Straight Flush";
            case HandRanking.FourOfAKind: return "Four of a Kind";
            case HandRanking.Castle: return "Castle";
            case HandRanking.FullHouse: return "Full House";
            case HandRanking.Flush: return "Flush";
            case HandRanking.Straight: return "Straight";
            case HandRanking.ThreeOfAKind: return "Three of a Kind";
            case HandRanking.TwoPairs: return "Two Pair";
            case HandRanking.Prison: return "Prison";
            case HandRanking.TwoOfAKind: return "A Pair";
            case HandRanking.HighCard: return "High Card";
            default:
                return $"MISSING \"{handRanking}\"";
        }
    }

    public override string ToString()
    {
        return GetPlayerFacingTextForHandRanking(_handRanking);
    }

    internal HandValue(List<Card> cards)
    {
        _handRanking = HandRanking.HighCard;
        _highCard = cards.First();
        _fractionalValue = ComputeFractionalValue(cards);
    }

    internal HandValue(HandRanking ranking, List<Card> cards)
    {
        _handRanking = ranking;
        _highCard = cards.First();
        _fractionalValue = ComputeFractionalValue(cards);
    }

    private double ComputeFractionalValue(List<Card> cards)
    {
        double total = 0;
        foreach (Card card in cards)
        {
            total *= Card.MaxFractionalValue;
            total += card.FractionalValue;
        }

        return total;
    }

    /// <summary>
    /// Best scores are sorted to the back
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int PixieCompareTo(HandValue? other, bool pixieCompare)
    {
        if (other == null)
        {
            return -1;
        }

        if (_handRanking != other._handRanking)
        {
            return (_handRanking > other._handRanking) ? 1 : -1;
        }

        int comp = _highCard.PixieCompareTo(other._highCard, pixieCompare);
        if (comp != 0)
        {
            return comp;
        }

        return _fractionalValue.CompareTo(other._fractionalValue);
    }
}