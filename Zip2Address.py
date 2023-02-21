import boto3
from botocore.exceptions import ClientError
import json

def lambda_handler(event, context):
    try :
        error_response = {
            'Error': {
                'Code': ''
            }
        }

        ZipCode = event['queryStringParameters']['ZipCode']

        StatusCode = 200
        Body = []
        
        Valid = True

        #郵便番号パラメーターの存在チェック
        if ZipCode == '' :
            error_response['Error']['Code'] = '郵便番号がありません'
            raise ClientError(error_response, '')
        
        #郵便番号パラメーターが正しい形式であるかのチェック
        for c in ZipCode :
            if not (c in '0123456789') : Valid = False

        if len(ZipCode) != 7 :
            Valid = False
        
        if not Valid :
            error_response['Error']['Code'] = '無効な郵便番号です'
            raise ClientError(error_response, '')

        #郵便番号パラメーターから住所を取得
        UpperZipCode = ZipCode[:3]
        LowerZipCode = ZipCode[3:]
    
        s3_client = boto3.client('s3')
        response = s3_client.get_object(Bucket = '********', Key = UpperZipCode + '.JSON')
        ZipContents = json.loads(response['Body'].read().decode('utf-8_sig'))
        
        AddressItemList = ZipContents['Address']
        if (LowerZipCode in AddressItemList) :
            AddressItem = AddressItemList[LowerZipCode]
        else :
            error_response['Error']['Code'] = '郵便番号が見つかりません'
            raise ClientError(error_response, '')

        PrefAndMunicsCode = AddressItem['Code']                

        PrefAndMunicsItemList = ZipContents['PrefAndMunics']
        PrefAndMunicsItem = PrefAndMunicsItemList[str(PrefAndMunicsCode)]

        Pref = PrefAndMunicsItem['Pref']
        Munic = PrefAndMunicsItem['Munic']

        AddressListItems = AddressItem['Address']
        for AddressList in list(AddressListItems) :
            BodyItem = [ Pref, Munic ]
            for Address in list(AddressList) :
                BodyItem.append(Address)
            Body.append(BodyItem)

        # TODO implement
        return {
            'statusCode' : 200,
            'body' : json.dumps(Body, indent = 2, ensure_ascii=False)
        }

    except ClientError as e:
        return {
            'statusCode' : 400,
            'body' : e.response['Error']['Code']
        }
