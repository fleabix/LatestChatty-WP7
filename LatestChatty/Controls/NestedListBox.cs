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
using LatestChatty.Classes;

namespace LatestChatty.Controls
{
	public class NestedListBoxRoot : ListBox
	{
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NestedListBoxItem();
		}
	}

	public class NestedListBox : ListBox
	{
		public NestedListBox()
		{
			this.DefaultStyleKey = typeof(NestedListBox);
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new NestedListBoxItem();
		}
	}

	public class NestedListBoxItem : ListBoxItem
	{
		public NestedListBoxItem()
		{
			this.DefaultStyleKey = typeof(NestedListBoxItem);
			CoreServices.Instance.SelectedCommentChanged += SelectedCommentChanged;
		}

		public override void OnApplyTemplate()
		{
			((HyperlinkButton)GetTemplateChild("SelectionButton")).Click += new RoutedEventHandler(NestedListBoxItem_Click);
		}

		public void NestedListBoxItem_Click(object sender, RoutedEventArgs e)
		{
			CoreServices.Instance.SetCurrentSelectedComment((Comment)DataContext);
		}

		public void SelectedCommentChanged(Comment c)
		{
			if (c.id == ((Comment)this.DataContext).id)
			{
				if (CoreServices.Instance.SelectedCommentHighlight != null)
				{
					CoreServices.Instance.SelectedCommentHighlight.Visibility = Visibility.Collapsed;
				}

				Rectangle rect = GetTemplateChild("SelectedFill") as Rectangle;

				if (rect != null)
				{
					CoreServices.Instance.SelectedCommentHighlight = rect;
					CoreServices.Instance.SelectedCommentHighlight.Visibility = Visibility.Visible;
				}
				else
				{
					CoreServices.Instance.SelectedCommentHighlight = null;
				}
			}
		}

		public NestedListBoxItem GetChild(Comment c)
		{
			NestedListBox List = GetTemplateChild("List") as NestedListBox;

			NestedListBoxItem item = (NestedListBoxItem)(List.ItemContainerGenerator.ContainerFromItem(c));
			if (item != null)
			{
				return item;
			}
			else
			{
				for (int i = 0; i < List.Items.Count; i++)
				{
					item = (NestedListBoxItem)(List.ItemContainerGenerator.ContainerFromIndex(i));
					item = item.GetChild(c);
					if (item != null)
					{
						return item;
					}
				}
			}
			return null;
		}
	}
}
