using _25CardGameMAD.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace _25CardGameMAD
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public Deck deck1 = new Deck();

        Card dragged = null;
        Card playerPlayedCard = null;
        Card oppPlayedCard = null;
        Card trumpCard = null;

        ObservableCollection<Card> playerHand = new ObservableCollection<Card>();
        ObservableCollection<Card> oppHand = new ObservableCollection<Card>();

        List<Card> oppPlayable = new List<Card>();

        List<Card> playerPlayable = new List<Card>();
        List<Card> playerTrumps = new List<Card>();

        Boolean playerDealer = false;
        Boolean playerLead = true;
        Boolean playerTurn = true;

        int playerScore = 0;
        int oppScore = 0;

        int playerGames = 0;
        int oppGames = 0;

        int tricksPlayed = 0;

        int trumpCardSuit = 0;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public GamePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void mygridview_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            dragged = e.Items.ElementAt(0) as Card;
            e.Data.SetText(dragged.Text);
        }

        private async void myimage_Drop(object sender, DragEventArgs e)
        {
            var View = e.Data.GetView();
            String ans = await View.GetTextAsync();
            dropText.Text = " ";
            if (dragged != null)
            {
                //lead
                //wait for opponent to play
                if (playerLead == true)
                {
                    playerHand.Remove(dragged);
                    playerPlayedCardImg.Source = dragged.getImage();
                    playerPlayedCard = dragged;
                    dragged = null;
                    pointArrowUp();
                    oppPlayCard();
                }
                //follow
                //check for winner
                if (playerLead == false)
                {
                    //checks if you have a suit the same as what the opponent played
                    checkPlayable();
                    //if you have a similaiar suit, you must play it, or play a trump card
                    if (playerPlayable.Count() != 0)
                    {
                        if ((dragged.getSuitValue() == oppPlayedCard.getSuitValue()) || dragged.getSuitValue() == trumpCardSuit)
                        {
                            playerHand.Remove(dragged);
                            playerPlayedCardImg.Source = dragged.getImage();
                            playerPlayedCard = dragged;
                            dragged = null;
                        }
                        else
                        {
                            var message = new Windows.UI.Popups.MessageDialog("You must follow suit OR play a trump card");
                            await message.ShowAsync();
                        }
                    }
                    else
                    {
                        playerHand.Remove(dragged);
                        playerPlayedCardImg.Source = dragged.getImage();
                        playerPlayedCard = dragged;
                        dragged = null;
                    }
                    trickWinner();
                }
            }
        }


        private void dealPlayersCards()
        {
            playerHand.Clear();

            for (int i = 0; i < 5; i++)
            {
                Card currCard = deck1.DealCard();
                currCard.Text = currCard.ToString();
                currCard.Image = currCard.getImage();
                playerHand.Add(currCard);
            }
            playerGridView.ItemsSource = null;
            playerGridView.ItemsSource = playerHand;
        }

        private void dealOppCards()
        {
            oppHand.Clear();
            for (int i = 0; i < 5; i++)
            {
                Card currCard = deck1.DealCard();
                currCard.Text = currCard.ToString();
                currCard.Image = currCard.getImage();
                oppHand.Add(currCard);
            }
            oppGridView.ItemsSource = null;
            oppGridView.ItemsSource = oppHand;
        }

        public void dealTrumpCard()
        {
            trumpCard = deck1.DealCard();
            trumpCardFront.Source = new BitmapImage(new Uri("ms-appx:/Cards/" + trumpCard.getCardNo() + ".png", UriKind.RelativeOrAbsolute));
            FlipCard.Begin();
            trumpCardSuit = trumpCard.getSuitValue();
        }

        private void playedCard_drag_enter(object sender, DragEventArgs e)
        {
            dropText.Text = "+";
        }

        private void playedCard_drag_leave(object sender, DragEventArgs e)
        {
            dropText.Text = " ";
        }

        private async void oppPlayCard()
        {
            if (tricksPlayed < 5)
            {
                await Task.Delay(1000);

                //lead trick
                if (playerLead == false)
                {
                    oppPlayRandom();
                    await Task.Delay(500);
                    pointArrowDown();
                }

                //follow trick
                if (playerLead == true)
                {
                    oppFollow();
                    await Task.Delay(500);
                    trickWinner();
                }
            }
        }

        private void oppFollow()
        {
            oppPlayable.Clear();

            for (int i = 0; i < oppHand.Count; i++)
            {
                if ((oppHand.ElementAt(i).getSuitValue() == trumpCardSuit) || (oppHand.ElementAt(i).getSuitValue() == playerPlayedCard.getSuitValue()))
                {
                    oppPlayable.Add(oppHand.ElementAt(i));
                }
            }

            if (oppPlayable.Count != 0)
            {
                Random rnd = new Random();
                int randCard = rnd.Next(0, oppPlayable.Count);
                oppPlayedCard = oppPlayable.ElementAt(randCard);
                oppHand.Remove(oppPlayedCard);
                oppPlayedCardImg.Source = new BitmapImage(new Uri("ms-appx:/Cards/" + oppPlayedCard.getCardNo() + ".png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                oppPlayRandom();
            }
        }

        private void oppPlayRandom()
        {
            Random rnd = new Random();
            int randCard = rnd.Next(0, oppHand.Count);
            oppPlayedCard = oppHand.ElementAt(randCard);
            oppHand.Remove(oppPlayedCard);
            oppPlayedCardImg.Source = new BitmapImage(new Uri("ms-appx:/Cards/" + oppPlayedCard.getCardNo() + ".png", UriKind.RelativeOrAbsolute));
        }
        private void trickWinner()
        {
            if ((playerPlayedCard != null) & (oppPlayedCard != null))
            {
                //if one player plays ace of hearts
                if ((playerPlayedCard.getSuitValue() == 1 && playerPlayedCard.getFaceValue() == 1) ||
                    (oppPlayedCard.getSuitValue() == 1 && oppPlayedCard.getFaceValue() == 1))
                {
                    sameSuit();
                }

                //one player plays a trump card
                else if ((playerPlayedCard.getSuitValue() == trumpCardSuit && oppPlayedCard.getSuitValue() != trumpCardSuit) ||
                    (oppPlayedCard.getSuitValue() == trumpCardSuit && playerPlayedCard.getSuitValue() != trumpCardSuit))
                {
                    oneTrump();
                }

                //two tump cards are played
                else if ((playerPlayedCard.getSuitValue() == trumpCardSuit) && (oppPlayedCard.getSuitValue() == trumpCardSuit))
                {
                    sameSuit();
                }

                //no trump cards are played - (different suits)
                else if ((playerPlayedCard.getSuitValue() != trumpCardSuit) && (oppPlayedCard.getSuitValue() != trumpCardSuit)
                    && (playerPlayedCard.getSuitValue() != oppPlayedCard.getSuitValue()))
                {
                    noTrumps();
                }

                //no trump cards are played - (same suits)
                else if ((playerPlayedCard.getSuitValue() != trumpCardSuit) && (oppPlayedCard.getSuitValue() != trumpCardSuit)
                   && (playerPlayedCard.getSuitValue() == oppPlayedCard.getSuitValue()))
                {
                    sameSuit();
                }
            }
        }

        //One of the players plays a trump card
        private void oneTrump()
        {
            //player has trump card
            if (playerPlayedCard.getSuitValue() == trumpCardSuit)
            {
                playerWinTrick();
            }

            //opponent has trump card
            else
            {
                oppWinTrick();
            }
        }

        //neither player plays a trump card
        private void noTrumps()
        {
            if (playerLead == true)
            {
                playerWinTrick();
            }

            if (playerLead == false)
            {
                oppWinTrick();
            }
        }

        //players play same suit - (not trumps)
        private void sameSuit()
        {
            setCardValue(playerPlayedCard);
            setCardValue(oppPlayedCard);

            if (playerPlayedCard.Value < oppPlayedCard.Value)
            {
                playerWinTrick();
            }

            if (playerPlayedCard.Value > oppPlayedCard.Value)
            {
                oppWinTrick();
            }
        }


        private void Fly_Out_Storyboard_Complete(object sender, object e)
        {
            tricksPlayed++;

            playerPlayedCardImg.Source = null;
            playerCardBack.Source = null;
            oppPlayedCardImg.Source = null;
            oppCardBack.Source = null;

            playerPlayedCard = null;
            oppPlayedCard = null;

            winSound.Play();
            txtPlayer.Text = playerScore.ToString();
            txtOpp.Text = oppScore.ToString();

            oppPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            oppCardBack.Visibility = Windows.UI.Xaml.Visibility.Visible;

            playerPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            playerCardBack.Visibility = Windows.UI.Xaml.Visibility.Visible;



            //end game
            if (oppScore == 25)
            {
                clearScreen();
                changeDealer();
                oppGames += 1;
                oppGamesTxt.Text = oppGames.ToString();
            }

            else if (playerScore == 25)
            {
                clearScreen();
                changeDealer();
                playerGames += 1;
                playerGamesTxt.Text = playerGames.ToString();
            }

            else if (playerHand.Count == 0 && oppHand.Count == 0)
            {
                clearScreen();
                changeDealer();
                startNewGame();
            }

            //continue game
            else if (playerLead == true)
            {
                pointArrowDown();
            }

            else
            {
                pointArrowUp();
                oppPlayCard();
            }

        }

        private async void playerWinTrick()
        {

            playerLead = true;
            await Task.Delay(500);
            playerCardBack.Source = new BitmapImage(new Uri("ms-appx:/Assets/backofcard1.png", UriKind.RelativeOrAbsolute));
            PlayerWinFlyout.Begin();

            playerScore += 5;
        }

        private void oppWinTrick()
        {
            oppCardBack.Source = new BitmapImage(new Uri("ms-appx:/Assets/backofcard1.png", UriKind.RelativeOrAbsolute));
            playerLead = false;
            OppWinFlyout.Begin();

            oppScore += 5;
        }

        //setCardValue() -> Assigns a value to each card in the deck based on what trump card was dealt. Number 1 (5 of trumps) being the best card in the deck
        private void setCardValue(Card currCard)
        {

            int suit = currCard.getSuitValue();
            int face = currCard.getFaceValue();

            //5 of trumps
            if ((suit == trumpCardSuit) && (face == 5))
            {
                currCard.Value = 1;
            }

            //jack of trumps
            else if ((suit == trumpCardSuit) && (face == 11))
            {
                currCard.Value = 2;
            }

            //Ace of hearts
            else if ((suit == 1) && (face == 1))
            {
                currCard.Value = 3;
            }

            //Ace of trumps 
            else if ((suit == trumpCardSuit) && (face == 1))
            {
                currCard.Value = 4;
            }

            //King of trumps 
            else if ((suit == trumpCardSuit) && (face == 13))
            {
                currCard.Value = 5;
            }

            //Queen of trumps 
            else if ((suit == trumpCardSuit) && (face == 12))
            {
                currCard.Value = 6;
            }

            //if trumps are RED set for 10 - 2
            else if (((trumpCardSuit == 1) || (trumpCardSuit == 3)) && suit == trumpCardSuit)
            {
                int j = 7;
                for (int i = 10; i > 1; i--)
                {
                    if ((suit == trumpCardSuit) && (face == i))
                    {
                        currCard.Value = j;
                    }
                    j++;
                }
            }

            //if trumps are BLACK set value for 10 - 2
            else if (((trumpCardSuit == 2) || (trumpCardSuit == 4)) && suit == trumpCardSuit)
            {
                int j = 7;
                for (int i = 2; i < 11; i++)
                {
                    if ((suit == trumpCardSuit) && (face == i))
                    {
                        currCard.Value = j;
                    }
                    j++;
                }
            }

            //for cards that are not trumps - RED
            else if ((suit == 1) || (suit == 3))
            {
                int j = 16;
                for (int i = 13; i > 0; i--)
                {
                    if ((suit != trumpCardSuit) && (face == i))
                    {
                        currCard.Value = j;
                    }
                    j++;
                }
            }

            //for cards that are not trumps - BLACK
            else if ((suit == 2) || (suit == 4))
            {
                int i;
                int j;

                //face cards (K,Q,J)
                j = 16;
                for (i = 13; i > 10; i--)
                {
                    if ((suit != trumpCardSuit) && (face == i))
                    {
                        currCard.Value = j;
                    }
                    j++;
                }


                j = 19;
                for (i = 1; i < 11; i++)
                {
                    if ((suit != trumpCardSuit) && (face == i))
                    {
                        currCard.Value = j;
                    }
                    j++;
                }
            }


            else currCard.Value = 99;
        }//end of setvalue

        private void Start_Game_Click(object sender, RoutedEventArgs e)
        {
            startNewGame();
        }

        private void startNewGame()
        {
            shuffleSound.Play();
            if (oppScore == 25 || playerScore == 25)
            {
                oppScore = 0;
                playerScore = 0;
                txtOpp.Text = oppScore.ToString();
                txtPlayer.Text = playerScore.ToString();
            }

            trumpCardBack.Source = new BitmapImage(new Uri("ms-appx:/Assets/backofcard1.png", UriKind.RelativeOrAbsolute));
            tricksPlayed = 0;
            deck1.Shuffle();
            dealPlayersCards();
            dealOppCards();
            dealTrumpCard();
            StartButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            playedCard.Visibility = Windows.UI.Xaml.Visibility.Visible;
            dealerImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            arrowImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            oppPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            playerPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (playerLead == false)
            {
                oppPlayCard();
            }

        }

        private void pointArrowUp()
        {
            if (playerTurn == true)
            {
                arrowStoryBoard.Begin();
                playerTurn = false;
                playerGridView.CanDragItems = false;
                playedCard.Background = null;
                dropTxtLbl.Text = " ";
            }
        }

        private void pointArrowDown()
        {
            if (playerTurn == false)
            {
                arrowStoryBoard_Rev.Begin();
                playerTurn = true;
                playerGridView.CanDragItems = true;
                playedCard.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                dropTxtLbl.Text = "Drop Here";
            }

        }

        public void checkPlayable()
        {
            playerTrumps.Clear();
            playerPlayable.Clear();

            for (int i = 0; i < playerHand.Count; i++)
            {
                if (playerHand.ElementAt(i).getSuitValue() == trumpCardSuit)
                {
                    playerTrumps.Add(playerHand.ElementAt(i));
                }

                if (playerHand.ElementAt(i).getSuitValue() == oppPlayedCard.getSuitValue())
                {
                    playerPlayable.Add(playerHand.ElementAt(i));
                }
            }
        }

        private void clearScreen()
        {
            StartButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            trumpCardFront.Source = null;
            trumpCardBack.Source = null;
            playerHand.Clear();
            oppHand.Clear();
            txtOpp.Text = oppScore.ToString();
            txtPlayer.Text = playerScore.ToString();
            oppPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            playerPlayedCardImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            playedCard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            dealerImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            arrowImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            tricksPlayed = 5;
        }

        private void changeDealer()
        {
            if (playerDealer == true)
            {
                Dealer_anim_Rev.Begin();
                playerLead = true;
                playerDealer = false;
                pointArrowDown();
            }

            else if (playerDealer == false)
            {
                Dealer_anim.Begin();
                playerLead = false;
                playerDealer = true;
                pointArrowUp();
            }
        }

    }
}
