﻿using Bit.iOS.Core.Controllers;
using Bit.iOS.Core.Utilities;
using System;
using UIKit;

namespace Bit.iOS.Core.Views
{
    public class FormEntryTableViewCell : ExtendedUITableViewCell, ISelectable
    {
        public FormEntryTableViewCell(
            string labelName = null,
            bool useTextView = false,
            nfloat? height = null,
            bool useLabelAsPlaceholder = false)
            : base(UITableViewCellStyle.Default, nameof(FormEntryTableViewCell))
        {
            var descriptor = UIFontDescriptor.PreferredBody;
            var pointSize = descriptor.PointSize;

            if(labelName != null && !useLabelAsPlaceholder)
            {
                Label = new UILabel
                {
                    Text = labelName,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.FromDescriptor(descriptor, 0.8f * pointSize),
                    TextColor = ThemeHelpers.MutedColor
                };

                ContentView.Add(Label);
            }

            if(useTextView)
            {
                TextView = new UITextView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = UIFont.FromDescriptor(descriptor, pointSize),
                    TextColor = ThemeHelpers.TextColor,
                    TintColor = ThemeHelpers.TextColor,
                    BackgroundColor = ThemeHelpers.BackgroundColor
                };

                ContentView.Add(TextView);
                ContentView.AddConstraints(new NSLayoutConstraint[] {
                    NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Leading, 1f, 15f),
                    NSLayoutConstraint.Create(ContentView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, TextView, NSLayoutAttribute.Trailing, 1f, 15f),
                    NSLayoutConstraint.Create(ContentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, TextView, NSLayoutAttribute.Bottom, 1f, 10f)
                });

                if(labelName != null && !useLabelAsPlaceholder)
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, Label, NSLayoutAttribute.Bottom, 1f, 10f));
                }
                else
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 10f));
                }

                if(height.HasValue)
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, height.Value));
                }
            }
            else
            {
                TextField = new UITextField
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    BorderStyle = UITextBorderStyle.None,
                    Font = UIFont.FromDescriptor(descriptor, pointSize),
                    ClearButtonMode = UITextFieldViewMode.WhileEditing,
                    TextColor = ThemeHelpers.TextColor,
                    TintColor = ThemeHelpers.TextColor,
                    BackgroundColor = ThemeHelpers.BackgroundColor
                };

                if(useLabelAsPlaceholder)
                {
                    TextField.Placeholder = labelName;
                }

                ContentView.Add(TextField);
                ContentView.AddConstraints(new NSLayoutConstraint[] {
                    NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Leading, 1f, 15f),
                    NSLayoutConstraint.Create(ContentView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, TextField, NSLayoutAttribute.Trailing, 1f, 15f),
                    NSLayoutConstraint.Create(ContentView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, TextField, NSLayoutAttribute.Bottom, 1f, 10f)
                });

                if(labelName != null && !useLabelAsPlaceholder)
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, Label, NSLayoutAttribute.Bottom, 1f, 10f));
                }
                else
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 10f));
                }

                if(height.HasValue)
                {
                    ContentView.AddConstraint(
                        NSLayoutConstraint.Create(TextField, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, height.Value));
                }
            }

            if(labelName != null && !useLabelAsPlaceholder)
            {
                ContentView.AddConstraints(new NSLayoutConstraint[] {
                    NSLayoutConstraint.Create(Label, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Leading, 1f, 15f),
                    NSLayoutConstraint.Create(Label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContentView, NSLayoutAttribute.Top, 1f, 10f),
                    NSLayoutConstraint.Create(ContentView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, Label, NSLayoutAttribute.Trailing, 1f, 15f)
                });
            }
        }

        public UILabel Label { get; set; }
        public UITextField TextField { get; set; }
        public UITextView TextView { get; set; }

        public void Select()
        {
            if(TextView != null)
            {
                TextView.BecomeFirstResponder();
            }
            else if(TextField != null)
            {
                TextField.BecomeFirstResponder();
            }
        }
    }
}
