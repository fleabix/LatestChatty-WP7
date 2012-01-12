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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LatestChatty.Classes
{
	[DataContract]
	public class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetProperty<T>(string name, ref T property, T value)
		{
			return this.SetProperty(name, ref property, value, EqualityComparer<T>.Default);
		}

		protected bool SetProperty<T>(string name, ref T property, T value, IEqualityComparer<T>comparer )
		{
			if (!comparer.Equals(property, value))
			{
				property = value;
				if (this.PropertyChanged != null)
				{
					this.PropertyChanged(this, new PropertyChangedEventArgs(name));
				}
				return true;
			}
			return false;
		}
	}
}
