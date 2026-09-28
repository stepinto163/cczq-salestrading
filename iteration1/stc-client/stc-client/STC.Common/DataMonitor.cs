using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Price;
using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Common
{
    public class DataMonitor
    {

        private Thread CalcDataThread = null;

        public void Start()
        {
            if (CalcDataThread == null)
            {
                CalcDataThread = new Thread(SetData);//CalcData);
                CalcDataThread.IsBackground = true;
                CalcDataThread.Start();
            }
        }


        private bool isFirst = true;

        public void SetData()
        {

            int i = 0;
            while (true)
            {
                if (isFirst)
                {
                    Thread.Sleep(25000);
                    isFirst = false;
                }
                else
                {
                    Thread.Sleep(100);
                }

                Random random = new Random();
                // MarketDataSnapshotFullRefresh ctPrice = STC.XBond.Plugin.Test.TestDataGenerator.GetCTPrice(this.SecurityID);
                string topic = string.Empty;
                MarketDataSnapshotFullRefresh ctPrice = new MarketDataSnapshotFullRefresh();
                if (i % 3 == 0)
                {
                    ctPrice.CCDCValuation = "3.2";
                    ctPrice.ClOrdID = "1623117498307";
                    ctPrice.MDBookType = "401";
                    ctPrice.MarketIndicator = "4";
                    ctPrice.SecurityID = "220210.IB";
                    ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                    ctPrice.Symbol = "15国开10";
                    ctPrice.VenueTypeGW = "ct";

                    topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, ctPrice.SecurityID, string.Empty, string.Empty);
                    List<GNoMDEntries> list = new List<GNoMDEntries>();
                    
                    for (int j = 0; j < 12; j++)
                    {
                        GNoMDEntries data1 = new GNoMDEntries();
                        data1.FirmFlag = "1";
                        data1.InstitutionID = string.Empty;
                        data1.InstitutionName = string.Empty;
                        data1.InternalFlag = "1";
                        data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                        data1.MDEntryID = Guid.NewGuid().ToString("N");
                        data1.MDEntryPx = random.NextDouble().ToString();
                        data1.MDEntrySize = "2000";
                        data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                        data1.MDEntryType = (j%2)==1?"0":"1";
                        data1.MDPriceLevel = "1";
                        data1.MDReqID = string.Empty;
                        data1.NetPrice = "100.7239";
                        data1.PartySubID = "193";
                        data1.PartySubName = "严晓莉";
                        data1.RoleName = "管理员";
                        if (j % 4 == 0)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 1)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 2)
                        {
                            data1.SettlType = "3";
                        }
                        else if (j % 4 == 3)
                        {
                            data1.SettlType = "1";
                        }

                        data1.TraderID = string.Empty;
                        data1.TraderName = string.Empty;
                        data1.VenueTypeGW = "XBOND";
                        data1.Venues = "ODM";
                        data1.WholeFlag = "0";
                        data1.Yield = "4";

                        list.Add(data1);
                    }

                    ctPrice.NoMDEntries = list;

                }
                else if (i % 3 == 1)
                {
                    ctPrice.CCDCValuation = "3.2";
                    ctPrice.ClOrdID = "1623117498307";
                    ctPrice.MDBookType = "401";
                    ctPrice.MarketIndicator = "4";
                    ctPrice.SecurityID = "220025.IB";
                    ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                    ctPrice.Symbol = "15国开10";
                    ctPrice.VenueTypeGW = "ct";

                    topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, ctPrice.SecurityID, string.Empty, string.Empty);

                    List<GNoMDEntries> list = new List<GNoMDEntries>();

                    for (int j = 0; j < 12; j++)
                    {
                        GNoMDEntries data1 = new GNoMDEntries();
                        data1.FirmFlag = "1";
                        data1.InstitutionID = string.Empty;
                        data1.InstitutionName = string.Empty;
                        data1.InternalFlag = "1";
                        data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                        data1.MDEntryID = Guid.NewGuid().ToString("N");
                        data1.MDEntryPx = random.NextDouble().ToString();
                        data1.MDEntrySize = "2000";
                        data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                        data1.MDEntryType = (j % 2) == 1 ? "0" : "1";
                        data1.MDPriceLevel = "1";
                        data1.MDReqID = string.Empty;
                        data1.NetPrice = "100.7239";
                        data1.PartySubID = "193";
                        data1.PartySubName = "严晓莉";
                        data1.RoleName = "管理员";
                        if (j % 4 == 0)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 1)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 2)
                        {
                            data1.SettlType = "3";
                        }
                        else if (j % 4 == 3)
                        {
                            data1.SettlType = "1";
                        }
                        data1.TraderID = string.Empty;
                        data1.TraderName = string.Empty;
                        data1.VenueTypeGW = "XBOND";
                        data1.Venues = "ODM";
                        data1.WholeFlag = "0";
                        data1.Yield = "4";

                        list.Add(data1);
                    }

                    ctPrice.NoMDEntries = list;
                }
                else if (i % 3 == 2)
                {
                    ctPrice.CCDCValuation = "3.2";
                    ctPrice.ClOrdID = "1623117498307";
                    ctPrice.MDBookType = "401";
                    ctPrice.MarketIndicator = "4";
                    ctPrice.SecurityID = "200210.IB";
                    ctPrice.SendingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff");
                    ctPrice.Symbol = "15国开10";
                    ctPrice.VenueTypeGW = "ct";

                    topic = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, ctPrice.SecurityID, string.Empty, string.Empty);

                    List<GNoMDEntries> list = new List<GNoMDEntries>();

                    for (int j = 0; j < 12; j++)
                    {
                        GNoMDEntries data1 = new GNoMDEntries();
                        data1.FirmFlag = "1";
                        data1.InstitutionID = string.Empty;
                        data1.InstitutionName = string.Empty;
                        data1.InternalFlag = "1";
                        data1.MDEntryDate = DateTime.Now.ToString("yyyyMMdd");
                        data1.MDEntryID = Guid.NewGuid().ToString("N");
                        data1.MDEntryPx = random.NextDouble().ToString();
                        data1.MDEntrySize = "2000";
                        data1.MDEntryTime = DateTime.Now.ToString("hh:mm:ss.fff");
                        data1.MDEntryType = (j % 2) == 1 ? "0" : "1";
                        data1.MDPriceLevel = "1";
                        data1.MDReqID = string.Empty;
                        data1.NetPrice = "100.7239";
                        data1.PartySubID = "193";
                        data1.PartySubName = "严晓莉";
                        data1.RoleName = "管理员";
                        if (j % 4 == 0)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 1)
                        {
                            data1.SettlType = "2";
                        }
                        else if (j % 4 == 2)
                        {
                            data1.SettlType = "3";
                        }
                        else if (j % 4 == 3)
                        {
                            data1.SettlType = "1";
                        }
                        data1.TraderID = string.Empty;
                        data1.TraderName = string.Empty;
                        data1.VenueTypeGW = "XBOND";
                        data1.Venues = "ODM";
                        data1.WholeFlag = "0";
                        data1.Yield = "4";

                        list.Add(data1);
                    }

                    ctPrice.NoMDEntries = list;
                }

                i++;


                MDHandler.Instance.Instance_OnCTPrice(topic, ctPrice);


                foreach (GNoMDEntries gNoMDEntries in ctPrice.NoMDEntries)
                {
                    if (ctPrice.SecurityID == "200210.IB")
                        Console.WriteLine("SecurityId:" + ctPrice.SecurityID + ", side:" + gNoMDEntries.MDEntryType + ", price:" + gNoMDEntries.MDEntryPx + ", amount:" + gNoMDEntries.MDEntrySize);
                }


                if (i % 100 == 0)
                {
                    Thread.Sleep(10000);
                }

            }
        }

    }
}
