using System;
using Gtk;
using Cairo;
using Gdk;

namespace WebServer.GUI.GTK
{
    public partial class TemplateWindow : Gtk.Window
    {
        private global::Gtk.Fixed gridMain;
        private global::Gtk.DrawingArea border;
        private global::Gtk.DrawingArea titlebar;
        private global::Gtk.DrawingArea borderleft;
        private global::Gtk.DrawingArea bordertop;
        private global::Gtk.DrawingArea borderright;
        private global::Gtk.DrawingArea borderbottom;

        //Buttons
        private const int ButtonOffSet = 10;
        private const int ButtonWidth = 32;
        private const int ButtonHeight = 24;
        //*** Close Button ***
        private const double CrossLineWidth = 1.5;
        private const int CrossLineLength = 8;
        private const double CrossLineOffsetX = 12.5;
        private const int CrossLineOffsetY = 8;

        //*** Minimize Button ***
        private const double MinimizeLineWidth = 1.5;
        private const int MinimizeLineLength = 8;
        private const double MinimizeLineOffsetX = 12.5;
        private const double MinimizeLineOffsetY = 12.5;

        private Pango.Layout titleLayout;

        private enum ButtonStates { Inside, DownInside, Outside, DownOutside }
        private ButtonStates _closeState;
        private Cairo.Point _closeStart;
        private ButtonStates _minimizeState;
        private Cairo.Point _minimizeStart;

        private bool _titleBarMouseDown;

        protected global::Gtk.Fixed GridMain
        {
            get
            {
                return this.gridMain;
            }
        }

        private bool _showMinimize = true;
        public bool ShowMinimize
        {
            set
            {
                _showMinimize = value;
            }
            get
            {
                return _showMinimize;
            }
        }

        public TemplateWindow(string title)
            : base(Gtk.WindowType.Toplevel)
        {
            this.Decorated = false;

            global::Stetic.Gui.Initialize(this);
            // Widget CrossPlatformWebServer.TemplateWindow
            //this.WidthRequest = 550;
            //this.HeightRequest = 350;
            this.Name = "CrossPlatformWebServer.TemplateWindow";
            this.Title = global::Mono.Unix.Catalog.GetString("MainWindow");
            this.WindowPosition = ((global::Gtk.WindowPosition)(3));

            // Container child CrossPlatformWebServer.TemplateWindow.Gtk.Container+ContainerChild
            this.gridMain = new global::Gtk.Fixed();
            this.gridMain.Name = "gridMain";
            this.gridMain.HasWindow = false;
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.border = new global::Gtk.DrawingArea();
            this.border.Name = "border";
            this.gridMain.Add(this.border);
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.titlebar = new global::Gtk.DrawingArea();
            this.titlebar.Name = "titlebar";
            this.gridMain.Add(this.titlebar);
            global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.gridMain[this.titlebar]));
            w2.X = 1;
            w2.Y = 1;
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.borderleft = new global::Gtk.DrawingArea();
            this.borderleft.Name = "borderleft";
            this.gridMain.Add(this.borderleft);
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.bordertop = new global::Gtk.DrawingArea();
            this.bordertop.Name = "bordertop";
            this.gridMain.Add(this.bordertop);
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.borderright = new global::Gtk.DrawingArea();
            this.borderright.Name = "borderright";
            this.gridMain.Add(this.borderright);
            // Container child gridMain.Gtk.Fixed+FixedChild
            this.borderbottom = new global::Gtk.DrawingArea();
            this.borderbottom.Name = "borderbottom";
            this.gridMain.Add(this.borderbottom);
            this.Add(this.gridMain);

            this._closeState = ButtonStates.Outside;
            this._minimizeState = ButtonStates.Outside;
            
            _closeStart = new Cairo.Point(this.WidthRequest - ButtonOffSet - ButtonWidth, ButtonOffSet);
            _minimizeStart = new Cairo.Point(this.WidthRequest - ButtonOffSet - ButtonWidth - ButtonWidth, 
                                             ButtonOffSet);

            w2 = null;

            this.ModifyBg(StateType.Normal, new Gdk.Color(255, 255, 255));
            this.titlebar.WidthRequest = this.WidthRequest - 2;
            this.titlebar.HeightRequest = 60;
            this.titlebar.AddEvents((int)Gdk.EventMask.ButtonPressMask |
                                     (int)Gdk.EventMask.PointerMotionMask |
                                     (int)Gdk.EventMask.ButtonReleaseMask);
            this.titlebar.ButtonPressEvent += TitlebarButtonPressEvent;
            this.titlebar.ExposeEvent += OnTitlebarExpose;
            this.titlebar.MotionNotifyEvent += OnTitlebarMotionNotifyEvent;
            this.titlebar.ButtonReleaseEvent += OnTitlebarButtonReleaseEvent;

            titleLayout = new Pango.Layout(this.PangoContext);
            titleLayout.Alignment = Pango.Alignment.Left;
            titleLayout.FontDescription = new Pango.FontDescription();

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    titleLayout.FontDescription.Family = "Segoe UI Light";
                    break;
                case PlatformID.Unix:
                    titleLayout.FontDescription.Family = "Ubuntu";
                    break;
                default:
                    //Unknown OS, use default font family
                    break;
            }
            titleLayout.FontDescription.Size = 17000;
            titleLayout.SetText(title.ToLower());

            borderleft.WidthRequest = 1;
            borderleft.HeightRequest = this.HeightRequest;
            borderleft.ModifyBg(StateType.Normal, new Gdk.Color(65, 177, 225));

            bordertop.WidthRequest = this.WidthRequest;
            bordertop.HeightRequest = 1;
            bordertop.ModifyBg(StateType.Normal, new Gdk.Color(65, 177, 225));

            borderright.WidthRequest = 1;
            borderright.HeightRequest = this.HeightRequest;
            borderright.ModifyBg(StateType.Normal, new Gdk.Color(65, 177, 225));
            w2 = ((global::Gtk.Fixed.FixedChild)(this.gridMain[this.borderright]));
            w2.X = this.WidthRequest - 1;
            w2.Y = 0;

            borderbottom.WidthRequest = this.WidthRequest;
            borderbottom.HeightRequest = 1;
            borderbottom.ModifyBg(StateType.Normal, new Gdk.Color(65, 177, 225));
            w2 = ((global::Gtk.Fixed.FixedChild)(this.gridMain[this.borderbottom]));
            w2.X = 0;
            w2.Y = this.HeightRequest - 1;

            if ((this.Child != null))
            {
                this.Child.ShowAll();
            }
            this.Show();
            this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
        }

        protected void ShowObjects()
        {
            if ((this.Child != null))
            {
                this.Child.ShowAll();
            }
            this.Show();
        }

        protected void OnTitlebarExpose(object sender, ExposeEventArgs args)
        {
            RedrawControls(ButtonStates.Outside, ButtonStates.Outside, true);
        }

        protected void OnTitlebarButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            _titleBarMouseDown = args.Event.Button == 1 && args.Event.Type == EventType.ButtonPress;
            ButtonStates tempCloseState = _closeState;
            ButtonStates tempMinimizeState = _minimizeState;

            this.RedrawControls(GetState(args.Event.X, args.Event.Y, _closeStart, _titleBarMouseDown, _closeState),
                    GetState(args.Event.X, args.Event.Y, _minimizeStart, _titleBarMouseDown, _minimizeState),
                    false);

            if (tempCloseState == ButtonStates.DownInside && !_titleBarMouseDown)
            {
                this.CloseApp();
            }
            else if (tempMinimizeState == ButtonStates.DownInside && !_titleBarMouseDown)
            {
                this.MinimizeApp();
            }

        }

        protected void OnTitlebarMotionNotifyEvent(object sender, MotionNotifyEventArgs args)
        {
            this.RedrawControls(GetState(args.Event.X, args.Event.Y, _closeStart, _titleBarMouseDown, _closeState),
                                GetState(args.Event.X, args.Event.Y, _minimizeStart, _titleBarMouseDown, _minimizeState),
                                false);
        }

        protected void TitlebarButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            _titleBarMouseDown =  args.Event.Button == 1 && args.Event.Type == EventType.ButtonPress;
            ButtonStates closeState = GetState(args.Event.X, args.Event.Y, _closeStart, _titleBarMouseDown, _closeState);
            ButtonStates minimizeState = GetState(args.Event.X, args.Event.Y, _minimizeStart, _titleBarMouseDown, _minimizeState);
            this.RedrawControls(closeState, minimizeState, false);
            if (closeState != ButtonStates.DownInside &&
                minimizeState != ButtonStates.DownInside)
            {
                _titleBarMouseDown = false;
                this.BeginMoveDrag(1, (int)args.Event.XRoot, (int)args.Event.YRoot, args.Event.Time);
            }
        }

        private static ButtonStates GetState(double argsX, double argsY, Cairo.Point point, bool mouseDown, ButtonStates previousState)
        {
            ButtonStates result = ButtonStates.Outside;
            bool inside = argsX >= point.X && argsX <= point.X + ButtonWidth &&
                    argsY >= point.Y && argsY <= point.Y + ButtonHeight;
            if (inside)
            {
                result = ButtonStates.Inside;
                if (mouseDown)
                {
                    result = ButtonStates.DownInside;
                }
            }
            else
            {
                if (mouseDown && previousState == ButtonStates.Inside)
                {
                    result = ButtonStates.DownOutside;
                }
            }
            return result;
        }

        private void RedrawControls(ButtonStates closeState, ButtonStates minimizeState, bool initDraw)
        {
            bool mustRedraw = _closeState != closeState || (_minimizeState != minimizeState && _showMinimize) || initDraw;
  
            //Background
            if (mustRedraw)
            {
                using (Context ctx = Gdk.CairoHelper.Create(titlebar.GdkWindow))
                {
                    Cairo.Gradient pat = new Cairo.LinearGradient(0, 0, 0, this.titlebar.HeightRequest);

                    pat.AddColorStop(0, new Cairo.Color(0.81, 0.96, 0.99));
                    pat.AddColorStop(1, new Cairo.Color(1, 1, 1));

                    ctx.SetSource(pat);
                    ctx.Operator = Operator.Source;
                    ctx.Paint();
                }

                titlebar.GdkWindow.DrawLayout(titlebar.Style.TextGC(StateType.Normal), 10, 10, titleLayout);
            }

            Cairo.Context cr = Gdk.CairoHelper.Create(titlebar.GdkWindow);

            //Close Button
            if (mustRedraw)
            {
                switch (closeState)
                {
                    case ButtonStates.DownInside:
                        cr.SetSourceRGB(1.0, 0, 0);
                        break;
                    case ButtonStates.DownOutside:
                    case ButtonStates.Inside:
                        cr.SetSourceRGB(0.86, 0.86, 0.86);
                        break;
                    case ButtonStates.Outside:
                        cr.SetSourceRGBA(0, 0, 0, 0);
                        break;
                }
                cr.Rectangle(_closeStart.X, _closeStart.Y, ButtonWidth, ButtonHeight);
                cr.Fill();

                cr.LineWidth = CrossLineWidth;
                cr.SetSourceRGB(0.2, 0.2, 0.2);
                cr.MoveTo(_closeStart.X + CrossLineOffsetX, _closeStart.Y + CrossLineOffsetY);
                cr.LineTo(_closeStart.X + CrossLineOffsetX + CrossLineLength, _closeStart.Y + CrossLineOffsetY + CrossLineLength);
                cr.Stroke();

                cr.LineWidth = CrossLineWidth;
                cr.MoveTo(_closeStart.X + CrossLineOffsetX + CrossLineLength, _closeStart.Y + CrossLineOffsetY);
                cr.LineTo(_closeStart.X + CrossLineOffsetX, _closeStart.Y + CrossLineOffsetY + CrossLineLength);
                cr.Stroke();
            }
            _closeState = closeState;
            //Minimize
            if (mustRedraw && _showMinimize)
            {
                switch (minimizeState)
                {
                    case ButtonStates.DownInside:
                        cr.SetSourceRGB(0.105, 0.63, 0.886);
                        break;
                    case ButtonStates.DownOutside:
                    case ButtonStates.Inside:
                        cr.SetSourceRGB(0.86, 0.86, 0.86);
                        break;
                    case ButtonStates.Outside:
                        cr.SetSourceRGBA(0, 0, 0, 0);
                        break;
                }
                cr.Rectangle(_minimizeStart.X, _minimizeStart.Y, ButtonWidth, ButtonHeight);
                cr.Fill();

                cr.LineWidth = MinimizeLineWidth;
                cr.SetSourceRGB(0, 0, 0);
                cr.MoveTo(_minimizeStart.X + MinimizeLineOffsetX, _minimizeStart.Y + MinimizeLineOffsetY);
                cr.LineTo(_minimizeStart.X + MinimizeLineOffsetX + MinimizeLineLength, _minimizeStart.Y + MinimizeLineOffsetY);
                cr.Stroke();
            }

            if (_showMinimize)
            {
                _minimizeState = minimizeState;
            }
        }

        protected void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            this.CloseApp();
            a.RetVal = true;
        }

        private void MinimizeApp()
        {
            this.Hide();
        }

        protected virtual void CloseApp()
        {
            Application.Quit();
        }
    }
}
