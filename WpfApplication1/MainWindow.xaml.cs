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
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MainControl mainControl = new MainControl();
        ImageSearch imageSearch = new ImageSearch();
        RecentControl recentControl = new RecentControl();
        ComboBox combobox = new ComboBox();
        MySqlConnection conn;
        MySqlCommand cmd;
        List<Image> imageList;

        // 메인 창
        public MainWindow()
        {
            InitializeComponent();
            MainGrid.Children.Add(mainControl);

            mainControl.btn_image_search.Click += new RoutedEventHandler(btn_image_search_Click);
            mainControl.btn_recent.Click += new RoutedEventHandler(btn_recent_Click);

            imageSearch.btn_image_back.Click += btn_image_back_Click;
            imageSearch.btn_search.Click += btn_search_Click;

            recentControl.btn_recent_back.Click += btn_recent_back_Click;
            recentControl.btn_recent_delete.Click += btn_recent_delete_Click;

            String strConn = "Server=localhost;Database=ensharp;Uid=root;Pwd=xogus1696;";
            conn = new MySqlConnection(strConn);
            cmd = new MySqlCommand();

            // 이미지 썸네일을 저장하기 위한 List
            imageList = new List<Image>();

            imageSearch.wp.MouseDown += Image_MouseDown;
        }

        // [MainWindow] 이미지 검색 버튼을 눌렀을 때
        private void btn_image_search_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear(); // 메인그리드의 Children Clear
            MainGrid.Children.Add(imageSearch); //
        }

        // [ImageSearch] 뒤로가기 버튼을 눌렀을 때
        private void btn_image_back_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(mainControl);
        }

        // [ImageSearch] 검색 버튼을 눌렀을 때
        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            // 이미지 리스트 초기화 (안해주면 이미지 refresh가 안됨)
            imageList.Clear();

            // 버튼을 눌렀을때 시간 저장
            var dateNow = DateTime.Now;

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
            // imageCount : API로 이미지를 받아온 갯수
            // error : 에러체크 값
            XmlNodeList imageUrlList = doc.GetElementsByTagName("thumbnail");
            int imageCount = imageUrlList.Count;
            bool error = false;

            while(true)
            {
                // 이미지 출력설정갯수와 받아오는 이미지갯수가 다를경우
                if (imageCount != int.Parse(imageNum))
                {
                    // 에러로 판별 후 메세지박스와 함께 해당 갯수의 이미지만 띄워준다
                    error = true;
                    MessageBox.Show("이미지를 " + imageCount + "개 밖에 불러올수 없네요 :(");
                    for (int i = 0; i < imageCount; i++)
                    {
                        addImageInList(imageUrlList[i].InnerText, i);
                    }
                }
                break;
            }
            // 위에서 에러가 났을경우 수행안함
            if (!error)
            {
                // 썸네일을 Wrap Panel 에 뿌려줌
                for (int i = 0; i < Convert.ToInt32(imageNum); i++)
                {
                    addImageInList(imageUrlList[i].InnerText, i);
                }
            }

            // DB OPEN
            conn.Open();
            cmd.Connection = conn;

            // 중복된 Keyword 가 있는지 판별
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT EXISTS (SELECT * FROM data WHERE Keyword = @Keyword)";
            cmd.Parameters.Add("@Keyword", MySqlDbType.VarChar, 100);
            cmd.Parameters[0].Value = keyword;

            // 중복O : 1 반환
            // 중복X : 0 반환
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            // 중복이라면 DELETE 문 사용
            if (result == 1)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "DELETE FROM data WHERE Keyword = @keyword";
                cmd.Parameters.Add("@Keyword", MySqlDbType.VarChar, 100);
                cmd.Parameters[0].Value = keyword;
                cmd.ExecuteNonQuery();
            }

            // INSERT 문
            cmd.Parameters.Clear();
            cmd.CommandText = "INSERT INTO data(Keyword, SearchTime) VALUES(@Keyword, @SearchTime)";
            cmd.Parameters.Add("@Keyword", MySqlDbType.VarChar, 100);
            cmd.Parameters.Add("@SearchTime", MySqlDbType.VarChar, 100);
            cmd.Parameters[0].Value = keyword;
            cmd.Parameters[1].Value = Convert.ToString(dateNow);
            cmd.ExecuteNonQuery();

            conn.Close();
        }

        private void addImageInList(string imageUrl, int i)
        {
            Image image = new Image();
            image.Source = LoadImage(imageUrl);
            // Wrap Panel 에 뿌려주는 이미지의 크기조정
            image.Height = 100;
            image.Width = 100;
            imageList.Add(image);
            imageSearch.wp.Children.Add(imageList[i]);
        }

        // 더블클릭 이벤트 발생시
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImageWindow imagewindow = new ImageWindow();
            if (e.ClickCount == 2)
            {
                // 해당 이미지의 소스를 가져옴
                var clickedImage = (Image)e.OriginalSource;
                // 안해주면 Error
                Image newImage = new Image();
                newImage.Source = clickedImage.Source;

                // 해당 이미지의 Pixel Height 와 Width 를 추출
                var PixelHeight = ((System.Windows.Media.Imaging.BitmapSource)newImage.Source).PixelHeight;
                var PixelWidth = ((System.Windows.Media.Imaging.BitmapSource)newImage.Source).PixelWidth;

                // 사이즈에 맞게 윈도우창 크기와 WrapPanel 크기를 조정
                imagewindow.wp.Width = imagewindow.img_window.Width = 7 * PixelWidth;
                imagewindow.wp.Height = imagewindow.img_window.Height = 7 * PixelHeight;

                // 사진을 패널에 맞게 꽉 채움
                newImage.Stretch = Stretch.Fill;
                
                // 이미지를 보여줌
                // Topmost = 창을 가장위로 뜨게함
                imagewindow.wp.Children.Clear();
                // 위에서 Image로 따로 선언 안해주면 해당 에러 발생
                // "지정한 요소가 이미 다른 요소의 논리 자식입니다. 먼저 이 연결을 끊으십시오."
                imagewindow.wp.Children.Add(newImage);
                imagewindow.Topmost = true;
                imagewindow.Show();
            }
        }


        // [RecentControl] 최근 검색어 버튼을 눌렀을 때
        private void btn_recent_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(recentControl);
            selectFromTable();

        }

        // SELECT * FROM TABLE
        private void selectFromTable()
        {
            DataTable dt = new DataTable();
            using (conn)
            {
                conn.Open();
                string query = "SELECT * FROM data";
                using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    da.Fill(dt);
            }
            recentControl.dataGrid.ItemsSource = dt.DefaultView;
            conn.Close();
        }

        // [RecentControl] 삭제 버튼을 눌렀을 때
        // SET SQL_SAFE_UPDATES=0;
        private void btn_recent_delete_Click(object sender, RoutedEventArgs e)
        {
            conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM data";
            cmd.ExecuteNonQuery();

            conn.Close();

            recentControl.dataGrid.UpdateLayout();
            btn_recent_Click(null, null);
        }

        // [RecentControl] 뒤로가기 버튼을 눌렀을 때
        private void btn_recent_back_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(mainControl);
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
