using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.Generic;

namespace LatestChatty.Classes
{
	//Ripped this from http://danielvaughan.orpius.com/post/Scroll-Based-Data-Loading-in-Windows-Phone-7.aspx
	public class ScrollViewerMonitor
	{
		public static DependencyProperty AtEndCommandProperty
				= DependencyProperty.RegisterAttached(
						"AtEndCommand", typeof(ICommand),
						typeof(ScrollViewerMonitor),
						new PropertyMetadata(OnAtEndCommandChanged));

		public static ICommand GetAtEndCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(AtEndCommandProperty);
		}

		public static void SetAtEndCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(AtEndCommandProperty, value);
		}


		public static void OnAtEndCommandChanged(
				DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			FrameworkElement element = (FrameworkElement)d;
			if (element != null)
			{
				element.Loaded -= element_Loaded;
				element.Loaded += element_Loaded;
			}
		}

		static void element_Loaded(object sender, RoutedEventArgs e)
		{
			FrameworkElement element = (FrameworkElement)sender;
			element.Loaded -= element_Loaded;
			ScrollViewer scrollViewer = FindChildOfType<ScrollViewer>(element);
			if (scrollViewer == null)
			{
				throw new InvalidOperationException("ScrollViewer not found.");
			}

			var listener = new DependencyPropertyListener();
			listener.ValueChanged
					+= delegate
					{
						bool atBottom = scrollViewer.VerticalOffset
																>= (scrollViewer.ScrollableHeight / 10) * 9;

						if (atBottom)
						{
							var atEnd = GetAtEndCommand(element);
							if (atEnd != null)
							{
								atEnd.Execute(null);
							}
						}
					};
			Binding binding = new Binding("VerticalOffset") { Source = scrollViewer };
			listener.Attach(scrollViewer, binding);
		}

		static T FindChildOfType<T>(DependencyObject root) where T : class
		{
			var queue = new Queue<DependencyObject>();
			queue.Enqueue(root);

			while (queue.Count > 0)
			{
				DependencyObject current = queue.Dequeue();
				for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
				{
					var child = VisualTreeHelper.GetChild(current, i);
					var typedChild = child as T;
					if (typedChild != null)
					{
						return typedChild;
					}
					queue.Enqueue(child);
				}
			}
			return null;
		}
	}


	//This was ripped from http://red-badger.com/Blog/author/stuartharris.aspx
	//I'm too lazy to write my own right now.  Google coding is how everyone does it now days anyway, right?
	public class DependencyPropertyListener
	{
		private readonly DependencyProperty dependencyProperty;

		private FrameworkElement targetElement;

		public DependencyPropertyListener()
		{
			this.dependencyProperty = DependencyProperty.RegisterAttached(
					Guid.NewGuid().ToString(),
					typeof(object),
					typeof(DependencyPropertyListener),
					new PropertyMetadata(null, this.OnValueChanged));
		}

		public event EventHandler<ValueChangedEventArgs> ValueChanged;

		public void Attach(FrameworkElement target, Binding binding)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}

			if (binding == null)
			{
				throw new ArgumentNullException("binding");
			}

			this.Detach();
			this.targetElement = target;
			this.targetElement.SetBinding(this.dependencyProperty, binding);
		}

		public void Detach()
		{
			if (this.targetElement != null)
			{
				this.targetElement.ClearValue(this.dependencyProperty);
				this.targetElement = null;
			}
		}

		private void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			EventHandler<ValueChangedEventArgs> handler = this.ValueChanged;
			if (handler != null)
			{
				handler(this, new ValueChangedEventArgs(e.OldValue, e.NewValue));
			}
		}
	}

	public class ValueChangedEventArgs : EventArgs
	{
		private readonly object newValue;

		private readonly object oldValue;

		public ValueChangedEventArgs(object oldValue, object newValue)
		{
			this.oldValue = oldValue;
			this.newValue = newValue;
		}

		public object NewValue
		{
			get
			{
				return this.newValue;
			}
		}

		public object OldValue
		{
			get
			{
				return this.oldValue;
			}
		}
	}
}
