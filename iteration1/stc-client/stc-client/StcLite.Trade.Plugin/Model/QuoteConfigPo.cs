using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Model
{
    public class QuoteConfigPo
    {


        //增加配置文件 当前显示模式
        public DisplayMode CurrentDisplayMode { get; set; }
        /// <summary>
        /// 切换前的显示模式
        /// </summary>
        public DisplayMode LastDisplayMode { get; set; }
        //增加配置文件 是否需要二次确认
        public bool SecondConfirm { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public int LastRows { get; set; }
        public int LastCols { get; set; }
        public string GroupId { get; set; }
        public List<QcPagePo> Pages { get; set; }
        public List<int> TenorIndexs { get; set; }
        public List<string> Brokers { get; set; }
        public string BestType { get; set; }
        public bool HideStar { get; set; }
        public decimal TknDefaultVolume { get; set; }
        public decimal GvnDefaultVolume { get; set; }
        public decimal AlertVolume { get; set; }
        public decimal ViewScale { get; set; }

        public QuoteConfigPo()
        {
            Pages = new List<QcPagePo>();
            TenorIndexs = new List<int>();
            BestType = "3";
            HideStar = false;

            TknDefaultVolume = 3;
            GvnDefaultVolume = 3;
            AlertVolume = 20;
            SecondConfirm = true;

            ViewScale = 125;
        }

        public static string ZipToBase64(List<QuoteConfigPo> po)
        {
            string str = JsonConvert.SerializeObject(po);

            return str;
            //Console.WriteLine("QuoteConfigPo ZipToBase64: {0}", str);
            //byte[] buf = com.adaptiveMQ2.utils.Utils.gzipStr(str);
            //string result = Convert.ToBase64String(buf);
            //return result;
        }

        public static List<QuoteConfigPo> Parse(string str)
        {
            //byte[] buf = Convert.FromBase64String(str);
            //string json = com.adaptiveMQ2.utils.Utils.ungzipByte(buf);
            //Console.WriteLine("QuoteConfigPo Parse: {0}", json);
            List<QuoteConfigPo> po = JsonConvert.DeserializeObject<List<QuoteConfigPo>>(str);
            return po;
        }

        public QuoteConfigPo RefreshLayout(int rows, int cols)
        {
            QuoteConfigPo po = new QuoteConfigPo();
            po.CurrentDisplayMode = CurrentDisplayMode;
            po.SecondConfirm = SecondConfirm;
            po.TknDefaultVolume = TknDefaultVolume;
            po.AlertVolume = AlertVolume;
            po.GvnDefaultVolume = GvnDefaultVolume;
            po.SecondConfirm = SecondConfirm;
            List<QcPanelPo> panelList = new List<QcPanelPo>();
            foreach (QcPagePo page in this.Pages)
            {
                panelList.AddRange(page.Panels);
            }
            int pagePanelCount = rows * cols;
            int pageCount = panelList.Count / pagePanelCount + (panelList.Count % pagePanelCount > 0 ? 1 : 0);
            int index = 0;
            for (int i = 0; i < pageCount; i++)
            {
                QcPagePo pagePo = new QcPagePo();
                pagePo.IsSeleted = (i == 0);
                pagePo.Index = i;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (index < panelList.Count)
                        {
                            QcPanelPo panelPo = panelList[index];
                            panelPo.Row = r;
                            panelPo.Col = c;
                            pagePo.Panels.Add(panelPo);
                        }
                        index++;
                    }
                }
                po.Pages.Add(pagePo);
            }
            if (po.Pages.Count == 0)
            {
                QcPagePo pagePo = new QcPagePo();
                pagePo.Index = 0;
                pagePo.IsSeleted = true;
                po.Pages.Add(pagePo);
            }
            return po;
        }

        /// <summary>
        /// 刷新界面溢出逻辑
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <returns></returns>
        public QuoteConfigPo RefreshLayoutOverFlow(int rows, int cols)
        {
            QuoteConfigPo po = new QuoteConfigPo();
            po.CurrentDisplayMode = CurrentDisplayMode;
            po.SecondConfirm = SecondConfirm;
            po.TknDefaultVolume = TknDefaultVolume;
            po.AlertVolume = AlertVolume;
            po.GvnDefaultVolume = GvnDefaultVolume;
            po.SecondConfirm = SecondConfirm;

            int pageCount = this.Pages.Count;

            for (int i = 0; i < pageCount; i++)
            {
                int index = 0;
                QcPagePo pagePo = new QcPagePo();
                pagePo.IsSeleted = (i == 0);
                pagePo.Index = i;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (index < this.Pages[i].Panels.Count)
                        {
                            QcPanelPo panelPo = this.Pages[i].Panels[index];
                            panelPo.Row = r;
                            panelPo.Col = c;
                            pagePo.Panels.Add(panelPo);
                        }
                        else
                        {
                            continue;
                        }
                        index++;
                    }
                }
                po.Pages.Add(pagePo);
            }
            if (po.Pages.Count == 0)
            {
                QcPagePo pagePo = new QcPagePo();
                pagePo.Index = 0;
                pagePo.IsSeleted = true;
                po.Pages.Add(pagePo);
            }
            return po;
        }
    }
}
