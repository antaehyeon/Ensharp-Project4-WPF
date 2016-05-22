using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Web;
using System.Net;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MainControl mainControl = new MainControl();
        ImageSearch imageSearch = new ImageSearch();
        ComboBox combobox = new ComboBox();

        // 메인 창
        public MainWindow()
        {
            InitializeComponent();
            MainGrid.Children.Add(mainControl);

            mainControl.btn_image_search.Click += new RoutedEventHandler(btn_image_search_Click);
            imageSearch.btn_back.Click += btn_back_Click;
            imageSearch.btn_search.Click += btn_search_Click;                   
        }

        // 이미지 검색 버튼을 눌렀을 때
        private void btn_image_search_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear(); // 메인그리드의 Children Clear
            MainGrid.Children.Add(imageSearch); //
        }

        // [ImageSearch] 뒤로가기 버튼을 눌렀을 때
        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(mainControl);
        }

        // [ImageSearch] 검색 버튼을 눌렀을 때
        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            // wrapPanel 부분 초기화
            imageSearch.wp.Children.Clear();

            // keyword : 검색키워드
            // imageNum : 이미지출력갯수
            string keyword = "";
            string imageNum = "";

            // textBox에 입력된 문자 받아오는 부분
            keyword = imageSearch.textBox_search.Text;

            // 이미지 갯수 받아오는 부분
            ComboBoxItem currentItem = imageSearch.cb_image_number.SelectedItem as ComboBoxItem;
            imageNum = currentItem.Content.ToString();

            // XML DOCUMENT 데이터를 얻음
            // Keyword : 해당 검색 키워드
            // imageNum : 이미지 갯수
            XmlDocument doc = new XmlDocument();
            doc.Load("http://openapi.naver.com/search?key=4432e614518baff96f7dcc60d0fe5c88&query=" + keyword + "&target=image&start=1&display=" + imageNum);


            // 이미지 경로를 저장하기 위한 List
            XmlNodeList imageUrlList = doc.GetElementsByTagName("thumbnail");

            // 이미지를 저장하기 위한 List
            List<Image> imageList = new List<Image>();

            for (int i = 0; i < Convert.ToInt32(imageNum); i++)
            {
                Image image = new Image();
                image.Source = LoadImage(imageUrlList[i].InnerText);
                image.Height = 100;
                image.Width = 100;

                imageList.Add(image);

                imageSearch.wp.Children.Add(imageList[i]);
            }
        }

        // URL 주소를 이미지로 변환
        public BitmapImage LoadImage(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            WebClient wc = new WebClient();
            Byte[] MyData = wc.DownloadData(url);
            wc.Dispose();

            BitmapImage bimgTemp = new BitmapImage();
            bimgTemp.BeginInit();
            bimgTemp.StreamSource = new MemoryStream(MyData);
            bimgTemp.EndInit();

            return bimgTemp;
        }
    }
}
