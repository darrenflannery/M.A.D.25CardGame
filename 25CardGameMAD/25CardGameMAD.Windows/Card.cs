using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace _25CardGameMAD
{
    public class Card
    {
        private string face;
        private string suit;
        BitmapImage img;
        int cardNo;
        int suitValue;
        int faceValue;

        public Card(string cardFace, string cardSuit, int cardNo, int suitNo, int faceNo)
        {
            face = cardFace;
            suit = cardSuit;
            img = new BitmapImage(new Uri("ms-appx:/Cards/" + cardNo + ".png", UriKind.RelativeOrAbsolute));
            this.cardNo = cardNo;
            suitValue = suitNo;
            faceValue = faceNo;
        }

        public int Value { get; set; }

        public BitmapImage Image { get; set; }

        public String Text { get; set; }

        public override string ToString()
        {
            return face + " of " + suit;
        }

        public BitmapImage getImage()
        {
            return img;
        }

        public int getCardNo()
        {
            return cardNo;
        }

        public int getSuitValue()
        {
            return suitValue;
        }

        public int getFaceValue()
        {
            return faceValue;
        }

    }
}
