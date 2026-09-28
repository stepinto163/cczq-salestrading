using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BDB.Trade.Plugin.View.Chat
{
    class ChatBubbleSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var u = container as FrameworkElement;

            ChatMessage message = item as ChatMessage;

            if (message.MsgType == MsgType.Trader)
                return u.FindResource("chatBroker") as DataTemplate;
            else if (message.MsgType == MsgType.Broker)
                return u.FindResource("chatSelf") as DataTemplate;
            else
                return u.FindResource("chatTime") as DataTemplate;
        }
    }
}
