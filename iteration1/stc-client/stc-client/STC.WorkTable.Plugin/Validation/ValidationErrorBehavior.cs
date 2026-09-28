using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace STC.WorkTable.Plugin.Validation
{
    public class ValidationErrorBehavior : Behavior<FrameworkElement>
    {
        int _errorCount = 0;
        protected override void OnAttached()
        {
            this.AssociatedObject.AddHandler(System.Windows.Controls.Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(this.OnValidationError));
        }

        private IValidationErrorHandler GetHandler()
        {
            if (this.AssociatedObject.DataContext is IValidationErrorHandler)
            {
                return this.AssociatedObject.DataContext as IValidationErrorHandler;
            }
            return null;
        }

        void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            try
            {
                var handler = GetHandler();
                var element = e.OriginalSource as UIElement;

                if (handler == null || element == null)
                {
                    return;
                }

                if (handler.ErrorList == null) handler.ErrorList = new Dictionary<UIElement, int>();

                if (!handler.ErrorList.ContainsKey(this.AssociatedObject))
                {
                    handler.ErrorList.Add(this.AssociatedObject, 0);
                }
                if (e.Action == ValidationErrorEventAction.Added)
                {
                    _errorCount++;
                    handler.ErrorList[AssociatedObject]++;
                }
                else if (e.Action == ValidationErrorEventAction.Removed)
                {
                    _errorCount--;
                    handler.ErrorList[AssociatedObject]--;

                }
                handler.IsValid = _errorCount == 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
