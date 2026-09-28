namespace BDB.Price.Plugin.ViewModel.Data
{
    public class BrokerInfo
    {
        private string _bid;

        public string Bid 
        {
            get { return _bid; }
            set { _bid = value; }
        }

        private string _bidAmount;

        public string BidAmount
        {
            get { return _bidAmount; }
            set { _bidAmount = value; }
        }

        private string _bidCornerMarketText;

        public string BidCornerMarketText
        {
            get { return _bidCornerMarketText; }
            set { _bidCornerMarketText = value; }
        }

        private decimal _bidT1Qty;

        public decimal BidT1Qty
        {
            get { return _bidT1Qty; }
            set
            {
                _bidT1Qty = value;
            }
        }

        private string _bidCornerMarketText_T1;

        public string BidCornerMarketText_T1
        {
            get { return _bidCornerMarketText_T1; }
            set { _bidCornerMarketText_T1 = value; }
        }

        private string _ofr;

        public string Ofr
        {
            get { return _ofr; }
            set { _ofr = value; }
        }

        private string _ofrAmount;

        public string OfrAmount
        {
            get { return _ofrAmount; }
            set { _ofrAmount = value; }
        }

        private string _ofrCornerMarketText;

        public string OfrCornerMarketText
        {
            get { return _ofrCornerMarketText; }
            set { _ofrCornerMarketText = value; }
        }

        private decimal _ofrT1Qty;

        public decimal OfrT1Qty
        {
            get { return _ofrT1Qty; }
            set
            {
                _ofrT1Qty = value;
            }
        }

        private string _ofrCornerMarketText_T1;

        public string OfrCornerMarketText_T1
        {
            get { return _ofrCornerMarketText_T1; }
            set { _ofrCornerMarketText_T1 = value; }
        }

        private string _updateTime;

        public string UpdateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }

        private bool _isPermission = false;
        public bool IsPermission
        {
            get { return _isPermission; }
            set { _isPermission = value; }
        }
    }
}
