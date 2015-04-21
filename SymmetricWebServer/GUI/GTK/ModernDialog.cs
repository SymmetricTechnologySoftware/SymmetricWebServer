using Gtk;
using System;

namespace WebServer.GUI.GTK
{
    public partial class ModernDialog : TemplateWindow
    {
        public enum MessageBoxButton
        {
            // Summary:
            //     The message box displays an OK button.
            OK = 0,
            //
            // Summary:
            //     The message box displays OK and Cancel buttons.
            OKCancel = 1,
            //
            // Summary:
            //     The message box displays Yes, No, and Cancel buttons.
            YesNoCancel = 3,
            //
            // Summary:
            //     The message box displays Yes and No buttons.
            YesNo = 4,
        }

        public enum MessageBoxResult
        {
            // Summary:
            //     The message box returns no result.
            None = 0,
            //
            // Summary:
            //     The result value of the message box is OK.
            OK = 1,
            //
            // Summary:
            //     The result value of the message box is Cancel.
            Cancel = 2,
            //
            // Summary:
            //     The result value of the message box is Yes.
            Yes = 6,
            //
            // Summary:
            //     The result value of the message box is No.
            No = 7,
        }

        private ResponseType _response;
        private MessageBoxButton _buttonType;
        private TextView text;
        private Button btn1;
        private Button btn2;

        public ModernDialog(string title, Window owner) :
            base(title)
        {
            this.WidthRequest = 350;
            this.HeightRequest = 180;
            this.ShowMinimize = false;
            this.WindowPosition = WindowPosition.None;
            this.KeepAbove = true;
            this.Modal = true;
            _response = ResponseType.None;

            if (owner != null)
            {
                int root_x, root_y;
                owner.GetPosition(out root_x, out root_y);
                this.Move(root_x + (owner.WidthRequest / 2) - (this.WidthRequest / 2),
                          root_y + (owner.HeightRequest / 2) - (this.HeightRequest / 2));
            }

            this.text = new global::Gtk.TextView();
            this.text.WrapMode = Gtk.WrapMode.Word;
            this.text.CanFocus = true;
            this.text.Editable = false;
            this.text.WidthRequest = (int)(this.WidthRequest * 0.8);
            this.GridMain.Add(this.text);
            global::Gtk.Fixed.FixedChild w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[this.text]));
            w2.X = 50;
            w2.Y = 70;

            this.btn1 = new Button();
            this.btn1.Clicked += btn1_Clicked; 
            this.btn1.WidthRequest = 100;
            this.btn1.HeightRequest = 30;
            this.GridMain.Add(this.btn1);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[btn1]));
            w2.X = 75;
            w2.Y = 120;

            this.btn2 = new Button();
            this.btn2.Clicked += btn2_Clicked;
            this.btn2.WidthRequest = 100;
            this.btn2.HeightRequest = 30;
            this.GridMain.Add(this.btn2);
            w2 = ((global::Gtk.Fixed.FixedChild)(this.GridMain[btn2]));
            w2.X = 175;
            w2.Y = 120;

            this.ShowObjects();
        }

        private void btn1_Clicked(object sender, EventArgs e)
        {
            switch (_buttonType)
            {
                case MessageBoxButton.YesNo:
                    _response = ResponseType.Yes;
                    break;
                case MessageBoxButton.OK:
                    _response = ResponseType.Ok;
                    break;
            }
            base.CloseApp();
        }

        private void btn2_Clicked(object sender, EventArgs e)
        {
            switch (_buttonType)
            {
                case MessageBoxButton.YesNo:
                    _response = ResponseType.No;
                    base.CloseApp();
                    break;
            }
        }

        protected override void CloseApp()
        {
            _response = ResponseType.Close;
            base.CloseApp();
        }

        public static ResponseType ShowMessage(string text, string title, MessageBoxButton button, Window owner)
        {
            global::Gtk.Fixed.FixedChild w2 = null;
            ModernDialog win = new ModernDialog(title, owner);
            win.text.Buffer.Text = text;
            win._buttonType = button;
            switch (button)
            {
                case MessageBoxButton.YesNo:
                    win.btn1.Label = "Yes";
                    win.btn2.Label = "No";
                    break;
                case MessageBoxButton.OK:
                    win.btn1.Label = "Ok";
                    w2 = ((global::Gtk.Fixed.FixedChild)(win.GridMain[win.btn1]));
                    w2.X = 75 + (win.btn1.WidthRequest / 2);
                    w2.Y = 120;
                    win.btn2.Visible = false;
                    break;
            }
            win.Show();
            Application.Run();
            win.Hide();
            try
            {
                return win._response;
            }
            finally
            {
                win = null;
            }
        }
    }
}

