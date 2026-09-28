using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace BDB.Python.Plugin.ViewModel
{
    public class PythonViewModel : BaseViewModel
    {
        public ICommand DownloadCommand { get; set; }

        public PythonViewModel()
        {
            DownloadCommand = new ComRelayCommand<System.Windows.Controls.Button>(OnDownloadClick);
        }

        public void OnDownloadClick(System.Windows.Controls.Button control)
        {
            //创建一个保存文件式的对话框
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();                              //设置这个对话框的起始保存路径
            sfd.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sfd.RestoreDirectory = true;
            sfd.FileName = $"pystc-latest-cp39-cp39-win_amd64";
            //设置保存的文件的类型，注意过滤器的语法
            sfd.Filter = "whl Documents|*.whl";
            sfd.DefaultExt = ".whl";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            string promptTxt = string.Empty;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string url = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pythonAddin/pystc-latest-cp39-cp39-win_amd64.whl");
                //Copy到新文件下
                FileInfo file = new FileInfo(url);
                if (file.Exists)
                {
                    //true 为覆盖已存在的同名文件，false 为不覆盖
                    file.CopyTo(sfd.FileName, true);
                    Window ownerWindow = Window.GetWindow(control);
                    CommonUtils.Prompt2Second("下载插件", "下载成功。", ownerWindow);
                }
                else
                {
                    Window ownerWindow = Window.GetWindow(control);
                    CommonUtils.Prompt2Second("下载插件","下载失败，未找到原文件。", ownerWindow);
                }
            }
        }

        /// <summary>
        /// 拷贝文件到另一个文件夹下
        /// </summary>
        /// <param name="sourceName">源文件路径</param>
        /// <param name="folderPath">目标路径（目标文件夹）</param>
        public void CopyToFile(string sourceName, string folderPath)
        {
            //例子：
            //源文件路径
            //string sourceName = @"D:\Source\Test.txt";
            //目标路径:项目下的NewTest文件夹,(如果没有就创建该文件夹)
            //string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NewTest");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            //当前文件如果不用新的文件名，那么就用原文件文件名
            string fileName = Path.GetFileName(sourceName);
            //这里可以给文件换个新名字，如下：
            //string fileName = string.Format("{0}.{1}", "newFileText", "txt");

            //目标整体路径
            string targetPath = Path.Combine(folderPath, fileName);

            
        }
    }
}
