using System;
using System.Collections.Generic;
using System.Text;

namespace STC.Common.Model.Fix
{
    public class FixMessage : BaseMessage
    {
        public FixMessage()
        {
            StandardHeader = new Component.StandardHeader();
            StandardTrailer = new Component.StandardTrailer();
        }

        public Component.StandardHeader StandardHeader
        {
            get { return this.Get(MessageDictionary.Fields.StandardHeader) as Component.StandardHeader; }
            set { this.Put(MessageDictionary.Fields.StandardHeader, value);}
        }
        public Component.StandardTrailer StandardTrailer
        {
            get { return this.Get(MessageDictionary.Fields.StandardTrailer) as Component.StandardTrailer; }
            set { this.Put(MessageDictionary.Fields.StandardTrailer, value); }
        }
    }
}
