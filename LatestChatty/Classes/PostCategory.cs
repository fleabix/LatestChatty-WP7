﻿using System;
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

namespace LatestChatty.Classes
{
    public enum PostCategory
    {
        ontopic,
        stupid,
        offtopic,
        nws,
        political,
        interesting,
        informative
    }

    public class PostCategoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PostCategory pc = (PostCategory)value;
            switch (pc)
            {
                case PostCategory.offtopic:
                    return new SolidColorBrush(Color.FromArgb(0xff, 244, 244, 3));
                case PostCategory.stupid:
                    return new SolidColorBrush(Color.FromArgb(0xff, 137, 190, 64));
                case PostCategory.nws:
                    return new SolidColorBrush(Color.FromArgb(0xff, 255, 0, 0));
                case PostCategory.political:
                    return new SolidColorBrush(Color.FromArgb(0xff, 238, 147, 36));
                case PostCategory.informative:
                    return new SolidColorBrush(Color.FromArgb(0xff, 71, 169, 215));
                default:
                    return new SolidColorBrush(Color.FromArgb(0xff, 0xB0, 0xB0, 0xB0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
