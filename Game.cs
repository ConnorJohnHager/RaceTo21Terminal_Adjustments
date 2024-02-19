using System;
using System.Collections.Generic;
using System.Numerics;

namespace RaceTo21
{
    public class Game
    {
        int numberOfPlayers; // number of players in current game
        List<Player> players = new List<Player>(); // list of objects containing player data
        List<Player> removedPlayers = new List<Player>(); // list of players to be removed at end of round
        CardTable cardTable; // object in charge of displaying game information
        Deck deck = new Deck(); // deck of cards
        int currentPlayer = 0; // current player on list
        public Task nextTask; // keeps track of game state
        public int bet; // bet that each player makes for the round
        public int pot; // total bets for the round to be given to winner
        public int busted = 0; // counting how many players have busted

        public Game(CardTable c)
        {
            cardTable = c;
            deck.Shuffle();
            deck.ShowAllCards();
            nextTask = Task.GetNumberOfPlayers;
        }

        /* Adds a player to the current game
         * Called by DoNextTask() method
         */
        public void AddPlayer(string n)
        {
            players.Add(new Player(n));
        }

        /* Figures out what task to do next in game
         * as represented by field nextTask
         * Calls methods required to complete task
         * then sets nextTask.
         */
        public void DoNextTask()
        {
            Console.WriteLine("================================"); // this line should be elsewhere right?
            if (nextTask == Task.GetNumberOfPlayers)
            {
                numberOfPlayers = cardTable.GetNumberOfPlayers();
                nextTask = Task.GetNames;
            }
            else if (nextTask == Task.GetNames)
            {
                for (var count = 1; count <= numberOfPlayers; count++)
                {
                    var name = cardTable.GetPlayerName(count);
                    AddPlayer(name); // NOTE: player list will start from 0 index even though we use 1 for our count here to make the player numbering more human-friendly
                }
                nextTask = Task.IntroducePlayers;
            }
            else if (nextTask == Task.IntroducePlayers)
            {
                cardTable.ShowPlayers(players);
                nextTask = Task.GetBets;
            }
            else if (nextTask == Task.GetBets)
            {
                foreach (Player player in players)
                {
                    bet = cardTable.GetPlayerBet(player);
                    player.bank -= bet;
                    pot += bet;
                    cardTable.ShowBet(player, bet);
                }
                    cardTable.ShowPot(pot);
                    nextTask = Task.PlayerTurn; 
            }
            else if (nextTask == Task.PlayerTurn)
            {
                cardTable.ShowHands(players);
                Player player = players[currentPlayer];
                if (player.status == PlayerStatus.active)
                {
                    if (cardTable.OfferACard(player))
                    {
                        Card card = deck.DealTopCard();
                        player.cards.Add(card);
                        player.score = ScoreHand(player);
                        if (player.score > 21)
                        {
                            player.status = PlayerStatus.bust;
                            busted++;
                            nextTask = Task.CheckForEnd;
                        }
                        else if (player.score == 21)  // Triggers automatic win when someone gets 21
                        {
                            player.status = PlayerStatus.win;
                            Player winner = DoFinalScoring();
                            cardTable.ShowHands(players);
                            cardTable.AnnounceWinner(winner, pot);
                            winner.bank += pot;
                            nextTask = Task.CheckForNextRound;
                        }
                        else
                        {
                            nextTask = Task.CheckForEnd;
                        }
                    }
                    else
                    {
                        player.status = PlayerStatus.stay;
                        nextTask = Task.CheckForEnd;
                    }

                }
                else
                {
                    nextTask = Task.CheckForEnd;
                }
            }
            else if (nextTask == Task.CheckForEnd)
            {
                if (busted == players.Count - 1)
                {
                    AllButOneBust(players);
                    Player winner = DoFinalScoring();
                    cardTable.ShowHands(players);
                    cardTable.AnnounceWinner(winner, pot);
                    winner.bank += pot;
                    nextTask = Task.CheckForNextRound;
                }
                else if (!CheckActivePlayers())
                {
                    Player winner = DoFinalScoring();
                    cardTable.ShowHands(players);
                    cardTable.AnnounceWinner(winner, pot);
                    winner.bank += pot;
                    nextTask = Task.CheckForNextRound;
                }
                else
                {
                    currentPlayer++;
                    if (currentPlayer > players.Count - 1)
                    {
                        currentPlayer = 0; // back to the first player...
                    }
                    nextTask = Task.PlayerTurn;
                }
            }
            else if (nextTask == Task.CheckForNextRound)
            {
                removedPlayers = cardTable.AnotherRound(players);

                if (removedPlayers.Count > 0)
                {
                    for (int i = 0; i < removedPlayers.Count; i++)
                    {
                        players.Remove(removedPlayers[i]);
                    }
                }
                if (players.Count > 1)
                {
                    ResetRound();
                }
                else
                {
                    Console.WriteLine("Sorry, not enough players.");
                    nextTask = Task.GameOver;
                }
            }
            else 
            {
                Console.WriteLine("I'm sorry, I don't know what to do now!"); //Shouldn't get here
                nextTask = Task.GameOver;
            }
        }

        public int ScoreHand(Player player)
        {
            int score = 0;
            foreach (Card card in player.cards)
            {
                string faceValue = card.ID.Remove(card.ID.Length - 1);
                switch (faceValue)
                {
                    case "K":
                    case "Q":
                    case "J":
                        score = score + 10;
                        break;
                    case "A":
                        score = score + 1;
                        break;
                    default:
                        score = score + int.Parse(faceValue);
                        break;
                }
            }
            return score;
        }

        public bool CheckActivePlayers()
        {

            foreach (var player in players)
            {
                if (player.status == PlayerStatus.active)
                {
                    return true; // at least one player is still going!
                }
            }
            return false; // everyone has stayed or busted, or someone won!
        }

        public void AllButOneBust(List<Player> players)
        {
            foreach (Player player in players)
            {
                if (player.score < 21)
                {
                    player.status = PlayerStatus.win;
                }
                else
                {
                    continue;
                }
            }
        }

        public Player DoFinalScoring()
        {
            int highScore = 0;
            foreach (var player in players)
            {
                if (player.status == PlayerStatus.win) // someone hit 21
                {
                    return player;
                }
                if (player.status == PlayerStatus.stay) // still could win...
                {
                    if (player.score > highScore)
                    {
                        highScore = player.score;
                    }
                }
                // if busted don't bother checking!
            }
            if (highScore > 0) // someone scored, anyway!
            {
                // find the FIRST player in list who meets win condition
                return players.Find(player => player.score == highScore);
            }
            return null; // everyone must have busted because nobody won!
        }

        public void ResetRound()
        {
            removedPlayers.Clear();

            foreach (Player player in players)
            {
                player.cards.Clear();
                player.score = 0;
                player.status = PlayerStatus.active;
            }

            deck = new Deck();
            deck.Shuffle();
            pot = 0;
            currentPlayer = 0;
            busted = 0;

            nextTask = Task.GetBets;
        }
    }
}
