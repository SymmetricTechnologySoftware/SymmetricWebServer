using System;
using Gdk;
using Gtk;

namespace WebServer.GUI.GTK
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MetroButton : Gtk.Bin
	{
		private global::Gtk.Fixed fixed1;
		private global::Gtk.DrawingArea drawingarea1;
		private global::Gtk.EventBox eventbox1;
		private global::Gtk.Image image1;

		public Pixbuf NormalImage { private set; get; }
		public Pixbuf MouseDown { private set; get; }
		public Pixbuf Highlighted { private set; get; }


		public MetroButton (global::Gdk.Pixbuf normalImage,
			global::Gdk.Pixbuf mousedown)
			: this(normalImage, mousedown, null)
		{

		}

		public MetroButton (global::Gdk.Pixbuf normalImage,
			global::Gdk.Pixbuf mousedown,
			global::Gdk.Pixbuf highlighted)
		{

			global::Stetic.Gui.Initialize (this);
			// Widget CrossPlatformWebServer.MetroButton
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CrossPlatformWebServer.MetroButton";
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();

			this.NormalImage = normalImage;
			this.MouseDown = mousedown;
			this.Highlighted = highlighted;

			//fixed
			this.fixed1 = new global::Gtk.Fixed ();
			this.fixed1.Name = "fixed1";
			this.fixed1.HasWindow = false;
			this.Add (this.fixed1);
			// Container child fixed1.Gtk.Fixed+FixedChild
			if (highlighted == null) {
				this.drawingarea1 = new global::Gtk.DrawingArea ();
				this.drawingarea1.Name = "drawingarea1";
				this.drawingarea1.ExposeEvent += OnExpose;
				this.fixed1.Add (this.drawingarea1);
				global::Gtk.Fixed.FixedChild w1 = ((global::Gtk.Fixed.FixedChild)(this.fixed1 [this.drawingarea1]));
				w1.X = 1;
				w1.Y = 1;
			}
			// Container child fixed1.Gtk.Fixed+FixedChild
			this.eventbox1 = new global::Gtk.EventBox ();
			//this.eventbox1.VisibleWindow = false;
			this.eventbox1.ModifyBg (StateType.Normal, new Color(65, 177, 225));
			this.eventbox1.Name = "eventbox1";
			this.eventbox1.ButtonPressEvent += new global::Gtk.ButtonPressEventHandler (this.OnEventbox1ButtonPressEvent);
			this.eventbox1.ButtonReleaseEvent += new global::Gtk.ButtonReleaseEventHandler (this.OnEventbox1ButtonReleaseEvent);
			this.eventbox1.EnterNotifyEvent += new global::Gtk.EnterNotifyEventHandler (this.OnEventbox1EnterNotifyEvent);
			this.eventbox1.LeaveNotifyEvent += new global::Gtk.LeaveNotifyEventHandler (this.OnEventbox1LeaveNotifyEvent);
			// Container child eventbox1.Gtk.Container+ContainerChild
			this.image1 = new global::Gtk.Image ();
			this.image1.Name = "image1";
			this.image1.Pixbuf = this.NormalImage;
			this.eventbox1.Add (this.image1);
			this.fixed1.Add (this.eventbox1);
			global::Gtk.Fixed.FixedChild w3 = ((global::Gtk.Fixed.FixedChild)(this.fixed1 [this.eventbox1]));
			w3.X = 2;
			w3.Y = 2;

			this.WidthRequest = normalImage.Width;
			this.HeightRequest = normalImage.Height;
		}

		private void OnExpose(object sender, ExposeEventArgs args)
		{
			DrawingArea area = (DrawingArea) sender;
			Cairo.Context cr =  Gdk.CairoHelper.Create(area.GdkWindow);
			cr.LineWidth = 2;
			cr.SetSourceRGB(0.7, 0.2, 0.0);
		}

		protected override void OnSizeRequested (ref Requisition requisition)
		{
			base.OnSizeRequested (ref requisition);

			if (this.drawingarea1 != null) {
				this.drawingarea1.WidthRequest = this.WidthRequest;
				this.drawingarea1.HeightRequest = this.HeightRequest;
			}
          
			this.eventbox1.WidthRequest = this.WidthRequest - 2;
			this.eventbox1.HeightRequest = this.HeightRequest - 2;
		}

		protected void OnEventbox1EnterNotifyEvent (object o, EnterNotifyEventArgs args)
		{
			if (Highlighted == null) {
				Gdk.Color col = new Gdk.Color (65, 177, 225);
				drawingarea1.ModifyBg (StateType.Normal, col);
			} else {
				image1.Pixbuf = this.Highlighted;
			}
		}

		protected void OnEventbox1LeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
		{
			if (Highlighted == null) {
				Gdk.Color col = new Gdk.Color (255, 255, 255);
				drawingarea1.ModifyBg (StateType.Normal, col);
			} else {
				image1.Pixbuf = this.NormalImage;
			}
		}

		protected void OnEventbox1ButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			image1.Pixbuf = this.MouseDown;
		}

		protected void OnEventbox1ButtonReleaseEvent(object o,  ButtonReleaseEventArgs args)
		{
			if (((Gdk.EventButton)args.Event).Type == Gdk.EventType.ButtonRelease) 
			{
				double x = this.Allocation.X + ((Gdk.EventButton)args.Event).X;
				double y = this.Allocation.Y + ((Gdk.EventButton)args.Event).Y;
				if ((x >= this.Allocation.X) &&
					(x <= this.Allocation.X + this.WidthRequest) &&
					(y >= this.Allocation.Y) &&
					(y <= this.Allocation.Y + this.HeightRequest))
				{
                    if (Clicked != null)
                    {
                        Clicked(o, new EventArgs());
                    }
				}	 
			}
			image1.Pixbuf = this.NormalImage;
		}

        public event EventHandler Clicked;
	}
}

