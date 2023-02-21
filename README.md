# 　Zipcode2Address

## 郵便番号を与えて住所を返すモジュールのデモです

このモジュールはAWSのlambdaサービスで動作しています。
lambdaサービスで使用するデータを用意するのがC#で書かれた郵便番号ユーティリティです。
以下それぞれについて説明して行きます。

## lambdaサービス

このサービスは郵便番号をパラメータとして
https://tahprpe7ixyrlnde7kccfiewj40afjyi.lambda-url.ap-northeast-3.on.aws/?ZipCode=1000001
のように呼び出すと、対応する住所をjson形式で
[
  [
    "東京都",
    "千代田区",
    "千代田"
  ]
]
と返します。

角括弧が二重になっているのは、ごく少数ですが一つの郵便番号に複数の住所がぶら下がっている場合があり、
それに対応するためです。

また事業所郵便番号簿にも対応しており、0018612を与えると、
[
  [
    "北海道",
    "札幌市北区",
    "北二十四条西",
    "６丁目１番１号",
    "札幌市北区役所"
  ]
]
と返します。

このサービスは Zip2Address.pyのPythonコードによって実現されています。

郵便番号上3桁でAWSのS3上に格納されている郵便番号ファイル群の中から郵便番号ファイルを選び、
下4桁で対応する住所を選びます。

## 郵便番号データ作成ユーティリティ

このユーティリティはlambdaサービスで使用する郵便番号データファイル(001.JSON〜999.JSON)を作成するものです。
ユーティリティ実行前に、
[郵便番号住所データダウンロードページ](https://www.post.japanpost.jp/zipcode/dl/kogaki-zip.html)から
[全国一括](https://www.post.japanpost.jp/zipcode/dl/kogaki/zip/ken_all.zip)のリンクをクリックしてダウンロードしたken_all.zipを解凍して
出てきたKEN_ALL.CSVと、
[事業所郵便番号データダウンロードページ](https://www.post.japanpost.jp/zipcode/dl/jigyosyo/index-zip.html)から
[最新データのダウンロード](https://www.post.japanpost.jp/zipcode/dl/jigyosyo/zip/jigyosyo.zip)のリンクをクリックしてダウンロードしたjigyosho.zipを解凍して
出てきたJIGYOSHO.CSVをダウンロードフォルダに保存しておきます。

＃＃python郵政省が公開している郵便番号データから郵便番号→住所の変換モジュールを作ってみました
