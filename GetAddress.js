"use strict";

//ページ上に入力された郵便番号から対応する住所を表示する関数
function Zip2Address() {
    const ZipControl = document.getElementById('ZipCode');
    const AddressControl = document.getElementById('Address');

    AddressControl.value = '';

    //郵便番号を読み取り、有効な郵便番号形式(7桁の半角数字)でなければアラートを表示する
    const ZipCode = ZipControl.value;
    if (ZipCode.match(/^\d{7}$/) == null) {
        alert("'" + ZipCode + "'は郵便番号ではありません。");
        return;
    }

    //セキュリティのためlambda関数を呼び出すURLをハードコードではなく、
    //JSONファイルから読み込むように変更
    let url = '';
    axios.get('Url.json')
    .then(function (response) {
        url = response.data.URL;

        //lambda関数を呼び出し、応答を組み立てる
        const option = {
            method: 'GET',
            params: { 'ZipCode': ZipCode }
        };
        axios.get(url, option)
        .then(function (response) {
            let Address = '';
            if (response.data.length == 1) {
                for (let Elem in response.data[0]) Address += response.data[0][Elem]; 
            }
            else {
                for (let i = 0; i < response.data.length; i++)
                {
                    let Line = response.data[i];
    
                    if (Address != '') Address += '\n'; 
                    for (let j = 0; j < Line.length; j++) Address += Line[j]; 
                }
            };
            AddressControl.value = Address;
        })
        ["catch"]
        (function (error) {
            ZipControl.value = '';
            alert(error.response.data);
        });
    })
    ["catch"]
    (function (error) {
        alert('設定読み込みに失敗しました');
        return;
    });
}
