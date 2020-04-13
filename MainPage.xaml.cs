using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading.Tasks;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace P3EstebanSanRoman
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<VMDron> ListaDrones { get; } = new ObservableCollection<VMDron>();
        Dictionary<uint, Windows.UI.Xaml.Input.Pointer> pointers;
        //Para mostrar información
        private int SelMos = -1;
        //Para seleccionar con Ratón
        private int SelInd = -1;
        //Click con boton derecho
        private bool RotbotDer = false;
        //Puntero anterior
        PointerPoint ptrPtAnt;

        CoreCursor cursorHand = null;
        CoreCursor cursorPin = null;
        CoreCursor cursorBeforePointerEntered = null;

        //Para manejar los mandos
        private readonly object myLock = new object();
        private List<Gamepad> myGamepads = new List<Gamepad>();
        private Gamepad mainGamepad = null;
        private GamepadReading reading, prereading;
        //Maneja el Timer
        //Timer de la Vista y el Controlador
        DispatcherTimer GameTimer;
        

        public MainPage()
        {
            this.InitializeComponent();
            cursorHand = new CoreCursor(CoreCursorType.Hand, 0);
            cursorPin = new CoreCursor(CoreCursorType.Pin, 0);
            Gamepad.GamepadAdded += (object sender, Gamepad e) =>
            {
                lock (myLock)
                {
                    int indexRemoved = myGamepads.IndexOf(e);
                    if (indexRemoved > -1)
                    {
                        if (mainGamepad == myGamepads[indexRemoved])
                        {
                            mainGamepad = null;
                        }
                        myGamepads.RemoveAt(indexRemoved);
                    }
                }
            };
            //pointers = new Dictionary<uint, Windows.UI.Xaml.Input.Pointer>();
            //MiCanvas.PointerMoved += new PointerEventHandler(MiCanvas_PointerMoved);
            //MiCanvas.PointerReleased += new PointerEventHandler(MiCanvas_PointerReleased);
            //MiCanvas.PointerPressed += new PointerEventHandler(MiCanvas_PointerPressed);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Cosntruye las listas de ModelView a partir de la lista Modelo 
            if (ListaDrones != null)
                foreach (Dron dron in Model.GetAllDrones())
                {
                    VMDron VMitem = new VMDron(dron);
                    ListaDrones.Add(VMitem);
                    VMitem.CCImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    MiCanvas.Children.Add(VMitem.CCImg);
                    MiCanvas.Children.Last().SetValue(Canvas.LeftProperty, VMitem.X - 25);
                    MiCanvas.Children.Last().SetValue(Canvas.TopProperty, VMitem.Y - 25);
                }
            base.OnNavigatedTo(e);
            GameTimerSetup();
        }

        public void GameTimerSetup()
        {
            GameTimer = new DispatcherTimer();
            //GameTimer.Tick += GameTimer_Tick; //dispatcherTimer_Tick;
            GameTimer.Interval = new TimeSpan(10000);
            GameTimer.Start();
        }
        
        private void ImageGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            VMDron Item = e.ClickedItem as VMDron;
            SelMos = Item.Id;
            //ImagenSel.Source = Item.Img.Source;
            //SelDatos.Text = Item.Explicacion;
            //Canvas.SetLeft(ImagenC, Item.X);
            //Canvas.SetTop(ImagenC, Item.Y);
            //ImagenC.Source = Item.Img.Source;
            //SelInd = Item.Id;
            MuestraInfo();
        }

        private void MiCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            cursorBeforePointerEntered = Window.Current.CoreWindow.PointerCursor;
            ptrPtAnt = e.GetCurrentPoint(MiCanvas);
            if (ptrPtAnt.Properties.IsRightButtonPressed)
            {
                RotbotDer = true;
                Window.Current.CoreWindow.PointerCursor = cursorHand;
            }
            else
            {
                RotbotDer = false;
                Window.Current.CoreWindow.PointerCursor = cursorPin;
            }
            Image Fuente = e.OriginalSource as Image;
            ContentControl cc = Fuente.Parent as ContentControl;
            if (Fuente != null)
            {
                SelInd = MiCanvas.Children.IndexOf(cc);
                SelMos = SelInd;
                MuestraInfo();
            }
        }

        private void MiCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(ImagenC);
            if (SelInd >= 0)
            {
                if (RotbotDer)
                {
                    ListaDrones[SelInd].Angulo += (int)ptrPt.Position.X - (int)ptrPtAnt.Position.X;
                    ListaDrones[SelInd].Rotation.Angle = ListaDrones[SelInd].Angulo;
                    ptrPtAnt = ptrPt;
                }
                else
                {
                    ListaDrones[SelInd].X = (int)ptrPt.Position.X;
                    ListaDrones[SelInd].Y = (int)ptrPt.Position.Y;
                    MiCanvas.Children[SelInd].SetValue(Canvas.LeftProperty, ListaDrones[SelInd].X - 25);
                    MiCanvas.Children[SelInd].SetValue(Canvas.TopProperty, ListaDrones[SelInd].Y - 25);
                    MuestraInfo();
                }
            }
        }

        private void MiCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SelInd = -1;
            Window.Current.CoreWindow.PointerCursor = cursorBeforePointerEntered;
        }

        private void MiCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int FocInd = -1;
            ContentControl cc = e.OriginalSource as ContentControl;
            if(e.OriginalSource.GetType() == cc.GetType())
            {
                FocInd = MiCanvas.Children.IndexOf(cc);
            }
            if (FocInd >= 0)
            {
                int X = (int)ListaDrones[FocInd].X;
                int Y = (int)ListaDrones[FocInd].Y;
                int Angulo = (int)ListaDrones[FocInd].Angulo;
                switch (e.Key)
                {
                    case VirtualKey.A:
                    case VirtualKey.GamepadRightThumbstickLeft:
                        X -= 10;
                        break;
                    case VirtualKey.D:
                    case VirtualKey.GamepadRightThumbstickRight:
                        X += 10;
                        break;
                    case VirtualKey.W:
                    case VirtualKey.GamepadRightThumbstickUp:
                        Y += 10;
                        break;
                    case VirtualKey.S:
                    case VirtualKey.GamepadRightThumbstickDown:
                        Y += 10;
                        break;
                    case VirtualKey.Q:
                    case VirtualKey.GamepadX:
                        Angulo += 1;
                        break;
                    case VirtualKey.E:
                    case VirtualKey.GamepadY:
                        Angulo -= 1;
                        break;
                }

                ListaDrones[FocInd].X = X;
                ListaDrones[FocInd].Y = Y;
                ListaDrones[FocInd].Angulo = Angulo;

                MiCanvas.Children[FocInd].SetValue(Canvas.LeftProperty, X - 25);
                MiCanvas.Children[FocInd].SetValue(Canvas.TopProperty, Y - 25);
                ListaDrones[FocInd].Rotation.Angle = Angulo;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ImageGridView.SelectAll();
            SelMos = 0;
            MuestraInfo();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ImageGridView.SelectedIndex = -1;
            SelMos = -1;
            SelInd = -1;
            MuestraInfo();
        }
        private void Zoom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImageGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (VMDron item in e.AddedItems)
            {
                item.CCImg.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            foreach (VMDron item in e.RemovedItems)
            {
                item.CCImg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void MuestraInfo()
        {
            if (SelMos >= 0)
            {
                VMDron Sel = ListaDrones[SelMos];
                SelDatos.Text = "Id: " + Sel.Id + ", Nombre: " 
                    + Sel.Nombre + ", Estado: " + Sel.Estado + " /n, REF: " 
                    + Sel.RX.ToString() + "," + Sel.RY.ToString() + ", POS: " 
                    + Sel.X.ToString() + "," + Sel.Y.ToString() + ", ANGLE: " 
                    + Sel.Angulo + ", ROT: " + Sel.Rotation;
                //SelExp.Source = Sel.Imagen.Source;
            }
        }
    }
}
