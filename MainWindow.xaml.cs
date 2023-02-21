using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace KenAll2Json
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum CSVFileType { CSVFTAddress, CSVFTJigyosyo };

    public class OneLowerZipStore
    {
        [JsonPropertyName("Code")]
        private int code = 0;

        [JsonPropertyName("Address")]
        private List<List<string>> address = new();

        public int Code { get => code; set => code = value; }
        public List<List<string>> Address { get => address; set => address = value; }

        public void AddTownArea(string TownArea)
        {
            Address.Add(new List<string>
            {
                TownArea
            });
        }

        public void AddCompany(string Company, string TownArea, string HouseNumber)
        {
            Address.Add(new List<string>
            {
                TownArea,
                HouseNumber,
                Company
            });
        }

        public OneLowerZipStore() { }
    }

    public class PrefAndMunic
    {
        [JsonPropertyName("Pref")]
        private string pref = "";
        [JsonPropertyName("Munic")]
        private string munic = "";

        public string Pref { get => pref; set => pref = value; }
        public string Munic { get => munic; set => munic = value; }

        public PrefAndMunic()
        {
        }

            public PrefAndMunic(string Pref, string Munic)
        {
            this.Pref = Pref;
            this.Munic = Munic;
        }
    }

    public class OneUpperZipStore  // 郵便番号上3桁一つ分のデータを保持する型
                            // 上3桁をファイル名とした一つのJSONファイルに
                            // この型のデータが一つはいる
    {
        // 同一の郵便番号上三桁の中に存在する都道府県・自治体のリスト
        [JsonPropertyName("PrefAndMunics")]
        private Dictionary<int, PrefAndMunic> prefAndMunics = new();
        [JsonPropertyName("Address")]
        private Dictionary<string, OneLowerZipStore> address = new();

        public Dictionary<int, PrefAndMunic> PrefAndMunics { get => prefAndMunics; set => prefAndMunics = value; }
        public Dictionary<string, OneLowerZipStore> Address { get => address; set => address = value; }

        public OneUpperZipStore()
        {
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DoCSVFileSelect_Click(object sender, RoutedEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var SJISEncording = Encoding.GetEncoding("shift-jis");

            var DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";

            var EntireZipData = new Dictionary<string, OneUpperZipStore>();

            // 個々の郵便番号上3桁ごとに含まれる都道府県・自治体のリスト
            var SameCheckData = new Dictionary<string, List<(int, PrefAndMunic)>>();
            
            NewMethod("Ken_All.csv", CSVFileType.CSVFTAddress);

            NewMethod("JIGYOSYO.csv", CSVFileType.CSVFTJigyosyo);

            var options = new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                WriteIndented = true
            };
            
            foreach (var OneZipData in EntireZipData)
            {
                using var CSVFileStream = File.Create(Path.Combine(DownloadPath, OneZipData.Key + ".JSON"));
                using var CSVFileWriter = new StreamWriter(CSVFileStream, System.Text.Encoding.UTF8);
                var X = OneZipData.Value;
                var L = JsonSerializer.Serialize(X, options);
                CSVFileWriter.WriteLine(L);
            }
            
            void NewMethod(string CSVFileName, CSVFileType CSVFT)
            {
                var RequiredCount = 0;
                switch (CSVFT)
                {
                    case CSVFileType.CSVFTAddress: RequiredCount = 9; break;
                    case CSVFileType.CSVFTJigyosyo: RequiredCount = 8; break;
                }

                var CSVFileReader = new StreamReader(Path.Combine(DownloadPath, CSVFileName), SJISEncording);
                while (!CSVFileReader.EndOfStream)
                {
                    var OneLine = CSVFileReader.ReadLine();
                    var Values = new List<string>();
#pragma warning disable CS8602 // null 参照の可能性があるものの逆参照です。
                    Values.AddRange(OneLine.Split(','));
                    if (Values.Count >= RequiredCount)
                    {
                        string ZipCode = "";
                        var Company = "";
                        var prefectures = "";
                        var municipalities = "";
                        var TownArea = "";
                        var HouseNumber = "";
                        switch (CSVFT)
                        {
                            case CSVFileType.CSVFTAddress:
                                {
                                    ZipCode = Values[2].Trim('"');
                                    prefectures = Values[6].Trim('"');
                                    municipalities = Values[7].Trim('"');
                                    TownArea = Values[8].Trim('"');
                                    break;
                                }
                            case CSVFileType.CSVFTJigyosyo:
                                {
                                    ZipCode = Values[7].Trim('"');
                                    Company = Values[2].Trim('"');
                                    prefectures = Values[3].Trim('"');
                                    municipalities = Values[4].Trim('"');
                                    TownArea = Values[5].Trim('"');
                                    HouseNumber = Values[6].Trim('"');

                                    break;
                                }
                        }

                        // "以下に掲載がない場合"は無視する
                        if (TownArea == "以下に掲載がない場合") continue;

                        // 括弧で挟まれた部分の処理(その他・次のビルを除く・地階・階層不明と括弧をカット)
                        TownArea = TownArea.Replace("（その他）", "").Replace("（次のビルを除く）", "").Replace("（地階・階層不明）", "").Replace("（", "").Replace("）", ""); ;

                        var UpperZipCode = ZipCode[..3];
                        var LowerZipCode = ZipCode.Substring(3, 4);

                        // 同一の上3桁の中に含まれる都道府県・自治体のリスト処理
                        var prefecturesandmunicipalitiesList = new List<(int, PrefAndMunic)>();
                        if (!SameCheckData.TryGetValue(UpperZipCode, out prefecturesandmunicipalitiesList))
                        {
                            SameCheckData.Add(UpperZipCode, prefecturesandmunicipalitiesList = new List<(int, PrefAndMunic)>());
                        }
                        var MathItem = prefecturesandmunicipalitiesList.Find(Item => (Item.Item2.Pref == prefectures) && (Item.Item2.Munic == municipalities));
                        var Code = MathItem.Item1;
                        var NewMunicipalities = (Code == 0);
                        var PAM = new PrefAndMunic("", "");
                        if (NewMunicipalities)
                        {
                            Code = prefecturesandmunicipalitiesList.Count + 1;
                            PAM = new PrefAndMunic(prefectures, municipalities);
                            MathItem = (Code, PAM);
                            prefecturesandmunicipalitiesList.Add(MathItem);
                        }

                        if (!EntireZipData.TryGetValue(UpperZipCode, out OneUpperZipStore? OneUpperZipStoreItem))
                        {
                            EntireZipData.Add(UpperZipCode, OneUpperZipStoreItem = new OneUpperZipStore());
                        }

                        if (!OneUpperZipStoreItem.Address.TryGetValue(LowerZipCode, out OneLowerZipStore? OneLowerZipStoreItem))
                        {
                            OneLowerZipStoreItem = new OneLowerZipStore();
                            OneUpperZipStoreItem.Address.Add(LowerZipCode, OneLowerZipStoreItem);
                        }

                        if (NewMunicipalities)
                        {
                            OneUpperZipStoreItem.PrefAndMunics.Add(Code, PAM);
                        }

                        OneLowerZipStoreItem.Code = Code;

                        switch (CSVFT)
                        {
                            case CSVFileType.CSVFTAddress:
                                {
                                    OneLowerZipStoreItem.AddTownArea(TownArea);
                                    break;
                                }
                            case CSVFileType.CSVFTJigyosyo:
                                {
                                    OneLowerZipStoreItem.AddCompany(Company, TownArea, HouseNumber);

                                    break;
                                }
                        }
                    }
                }
            }
        }
    }
}
