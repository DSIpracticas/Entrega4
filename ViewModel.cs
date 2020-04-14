using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace P3EstebanSanRoman
{
    public class VMDron : Dron
    {
        public Image Img;
        public ContentControl CCImg;
        public int Zoom;
        public RotateTransform Rotation;
        public int Angulo;

        public VMDron(Dron dron)
            {
            Id = dron.Id;
            Nombre = dron.Nombre;
            Imagen = dron.Imagen;
            Explicacion = dron.Explicacion;
            Estado = dron.Estado;
            X = dron.X;
            Y = dron.Y;
            RX = dron.RX;
            RY = dron.RY;
            //Rotation
            Angulo = 0;
            Rotation = new RotateTransform();
            Rotation.Angle = Angulo;
            Rotation.CenterX = 25;
            Rotation.CenterY = 25;
            //Imagen
            Img = new Image();
            string s = System.IO.Directory.GetCurrentDirectory() + "\\" + dron.Imagen;
            Img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(s));
            Img.Width = 50;
            Img.Height = 50;
            CCImg = new ContentControl();
            CCImg.RenderTransform = Rotation;
            CCImg.Content = Img;
            CCImg.UseSystemFocusVisuals = true;
            CCImg.Visibility = Windows.UI.Xaml.Visibility.Visible;//.Collapsed;
            //Mapa.Children.Add(CCImg);
            //Mapa.Children.Last().SetValue(Canvas.LeftProperty, X - 25);
            //Mapa.Children.Last().SetValue(Canvas.TopProperty, Y - 25);
        }
    }
}
