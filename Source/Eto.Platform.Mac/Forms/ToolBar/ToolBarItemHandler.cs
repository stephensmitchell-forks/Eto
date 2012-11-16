using System;
using System.Reflection;
using Eto.Forms;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Eto.Drawing;
using MonoMac.ObjCRuntime;
using Eto.Platform.Mac.Drawing;
using sd = System.Drawing;

namespace Eto.Platform.Mac
{
	interface IToolBarBaseItemHandler
	{
		string Identifier { get; }
		NSToolbarItem Control { get; }
		bool Selectable { get; }
		void ControlAdded(ToolBarHandler toolbar);
	}
	
	interface IToolBarItemHandler : IToolBarBaseItemHandler
	{
		void OnClick();
		bool Enabled { get; }
	}
	
	class ToolBarItemHandlerTarget : NSObject
	{
		public IToolBarItemHandler Handler { get; set; }
		
		[Export("validateToolbarItem:")]
		public bool ValidateToolbarItem(NSToolbarItem item)
		{
			return Handler.Enabled;
		}
		
		[Export("action")]
		public bool action()
		{
			Handler.OnClick();
			return true;
		}
	}

	public abstract class ToolBarItemHandler<T, W> : WidgetHandler<T, W>, IToolBarItem, IToolBarItemHandler
		where T: NSToolbarItem
		where W: ToolBarItem
	{
		Icon icon;
        Image image;
		bool enabled = true;
		public virtual string Identifier { get; set; }
		
		public ToolBarItemHandler()
		{
			this.Identifier = Guid.NewGuid().ToString();
		}

		public override T CreateControl ()
		{
			return (T)new NSToolbarItem(this.Identifier);
		}

		public override void Initialize ()
		{
			base.Initialize ();
			Control.Target = new ToolBarItemHandlerTarget{ Handler = this };
			Control.Action = new Selector("action");
			Control.Autovalidates = false;
			Control.Enabled = enabled;
			Control.ToolTip = this.ToolTip ?? string.Empty;
	
			if (icon != null) Control.Image = (NSImage)icon.ControlObject;
			Control.Label = this.Text;
		}
		
		public virtual void ControlAdded(ToolBarHandler toolbar)
		{
		}
		
		public virtual void InvokeButton()
		{
		}
		
		public string Text {
			get { return Control.Label; }
			set { Control.Label = value ?? string.Empty; }
		}

		public string ToolTip {
			get { return Control.ToolTip; }
			set { Control.ToolTip = value ?? string.Empty; }
		}

		public Icon Icon
		{
			get { return icon; }
			set
			{
				this.icon = value;
				if (this.icon != null)
					Control.Image = ((IImageSource)icon.Handler).GetImage ();
				else 
					//Control = null; // grr. NRE in monomac
					Control.Image = new NSImage(new sd.SizeF (1, 1));
			}
		}

        public Image Image
        {
            get { return image; }
            set
            {
                this.image = value;
            }
        }
		
		public virtual bool Enabled
		{
			get { return (Control != null) ? Control.Enabled : enabled; }
			set { if (Control != null) Control.Enabled = value; enabled = value; }
		}
	
		public virtual bool Selectable { get; set; }
		
		public void OnClick()
		{
			this.InvokeButton();
		}

		NSToolbarItem IToolBarBaseItemHandler.Control
		{
			get { return (NSToolbarItem)this.Control; }
		}
	}


}
