using System;
using System.Collections.Generic;
using System.Numerics;

namespace RaceTo21
{
    public class CardTable
    {
        public CardTable()
        {
            Console.WriteLine("Setting Up Table...");
        }

        /* Shows the name of each player and introduces them by table position.
         * Is called by Game object.
         * Game object provides list of players.
         * Calls Introduce method on each player object.
         */
        public void ShowPlayers(List<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Introduce(i+1); // List is 0-indexed but user-friendly player positions would start with 1...
            }
        }

        /* Gets the user input for number of players.
         * Is called by Game object.
         * Returns number of players to Game object.
         */
        public int GetNumberOfPlayers()
        {
            Console.Write("How many players? ");
            string response = Console.ReadLine();
            int numberOfPlayers;
            while (int.TryParse(response, out numberOfPlayers) == false
                || numberOfPlayers < 2 || numberOfPlayers > 8)
            {
                Console.WriteLine("Invalid number of players.");
                Console.Write("How many players? ");
                response = Console.ReadLine();
            }
            return numberOfPlayers;
        }

        /* Gets the name of a player
         * Is called by Game object
         * Game object provides player number
         * Returns name of a player to Game object
         */
        public string GetPlayerName(int playerNum)
        {
            Console.Write("What is the name of player# " + playerNum + "? ");
            string response = Console.ReadLine();
            while (response.Length < 1)
            {
                Console.WriteLine("Invalid name.");
                Console.Write("What is the name of player# " + playerNum + "? ");
                response = Console.ReadLine();
            }
            return response;
        }

        /* Gets the bet of a player
         * Is called by Game object
         * Game object provides player
         * Returns potential bet of player to Game object
         */
        public int GetPlayerBet(Player player) 
        {
            Console.Write(player.name + ", how many dollars would you like to bet? You currently have $" + player.bank + ". ");
            string response = Console.ReadLine();
            int potentialBet;

            bool betSuccess = int.TryParse(response, out potentialBet);

            while(betSuccess == false || potentialBet < 1 || potentialBet > player.bank)
            {
                Console.WriteLine("Invalid amount.");
                Console.Write(player.name + ", how many dollars would you like to bet? You currently have $" + player.bank + ". ");
                response = Console.ReadLine();
                betSuccess = int.TryParse(response, out potentialBet);
                potentialBet = int.Parse(response);
            }
            return potentialBet;
        }

        /* Shows the bet of a player
         * Is called by Game object
         * Game object provides player and bet
         */
        public void ShowBet(Player player, int bet)
        {
            Console.WriteLine(player.name + " bet $" + bet + ".");
        }

        /* Shows the pot for current round
         * Is called by Game object
         * Game object provides pot
         */
        public void ShowPot(int pot)
        {
            Console.WriteLine("The winner of this round will receive $" + pot + ".");
        }

        /* Asks players if they would like to draw a card
         * Is called by Game object
         * Game object provides player
         * Returns true to draw a card or false to not draw
         */
        public bool OfferACard(Player player)
        {
            while (true)
            {
                if (player.cards.Count == 0)
                {
                    Console.WriteLine("Dealing first card for " + player.name +"...");
                    return true;
                }
                else
                {
                    Console.Write(player.name + ", do you want a card? (Y/N) ");
                    string response = Console.ReadLine();
                    if (response.ToUpper().StartsWith("Y"))
                    {
                        return true;
                    }
                    else if (response.ToUpper().StartsWith("N"))
                    {
                        return false;
                    }
                    else
                    {
                    Console.WriteLine("Please answer Y(es) or N(o)!");
                    }
                }
                
            }
        }

        /* Shows the cards of a player
         * Is called by Game object and ShowHands method
         * Game object provides player
         */
        public void ShowHand(Player player)
        {
            if (player.cards.Count > 0)
            {
                Console.Write(player.name + " has: ");

                bool isFirst = true; //ChatGPT helped develop the idea for using a boolean trigger for after the first card

                foreach (Card card in player.cards)
                {
                    if (!isFirst)
                    {
                        Console.Write(", ");
                    }
                    else
                    {
                        isFirst = false;
                    }
                    Console.Write(card.name);
                }
                Console.Write(" = " + player.score + "/21 ");
                if (player.status != PlayerStatus.active)
                {
                    Console.Write("(" + player.status.ToString().ToUpper() + ")");
                }
                Console.WriteLine();
            }
        }

        /* Shows the hands of all players
         * Is called by Game object
         * Game object provides list of players
         * Calls ShowHand method for each player
         */
        public void ShowHands(List<Player> players)
        {
            foreach (Player player in players)
            {
                ShowHand(player);
            }
        }

        /* Announces the winner of the game
         * Is called by Game object
         * Game object provides player and pot
         */
        public void AnnounceWinner(Player player, int pot) 
        {
            if (player != null)
            {
                Console.WriteLine(player.name + " wins $" + pot + "!");
            }
            else
            {
                Console.WriteLine("Everyone busted!"); // shouldn't be possible
            }
        }

        /* Asks players if they would like another round
         * Is called by Game object
         * Returns true to play another round or false if not
         */
        public bool AnotherRound()
        {
            while (true)
            {
                Console.Write("Would you like to play another game? (Y/N) ");
                string response = Console.ReadLine();
                if (response.ToUpper().StartsWith("Y"))
                {
                    return true;
                }
                else if (response.ToUpper().StartsWith("N"))
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Please answer Y(es) or N(o)!");
                }
            }
        }
    }
}