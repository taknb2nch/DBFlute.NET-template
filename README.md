DBFlute.NET-template
=============

DBFlute.NETを使用するためのテンプレートプロジェクトです。


##設定、準備
ディレクトリ構成は以下のようになっています。  

/dbflute\_dfclient  
/doc  
/lib  
/mydbflute  
/src/MyTemplate/  

※mydbfluteディレクトリには、  
http://dbflute.net.sandbox.seasar.org/ja/environment/newest.html  
からダウンロードして解凍したものを配置してください。


VisualStudioのプロジェクトの既定の名前空間を"Aaa.Bbb.Ccc"とし、出力先を/src/MyTemplate/とした場合、変更する設定ファイルの箇所は以下のとおりです。

■basicInfoMap.dfprop  
database = 適宜  
targetLanguage = csharp  
packageBase = Aaa.Bbb.Ccc.DBFlute  
generateOutputDirectory = ../src/MyTemplate  
flatDirectoryPackage = Aaa.Bbb.Ccc.DBFlute  
omitDirectoryPackage = Aaa.Bbb.Ccc  

■databaseInfoMap.dfprop  
適宜  

■outsideSqlDefinitionMap.dfprop  
defaultPackage = Aaa.Bbb.Ccc (※この項目は存在しないので追加)


外出しSQLは上記の設定ですと、  
/src/MyTemplate/DBFlute/ExBhv/  
以下に、  
`[対応するBehaviorの名前]_select[業務名].sql`  
の形式で"埋め込まれたリソース"として保存しておきます。  
※select、update等の先頭は小文字にしてください。  

追加で以下の設定もしています。  

■commonColumnMap.dfprop  
共通項目の定義を追加。  
$$AccessContext$$とentityへのアクセスはプロパティになるので大文字に変更します。  

■classificationResource.dfprop  
区分値の定義の追加。  
このファイル自体は存在していないので追加します。  
classificationDefinitionMap.dfpropとclassificationDeploymentMap.dfpropでも定義できますが、  
利便性と管理のしやすさでこちらを使用しています。  

##ダウンロード方法
gitをお使いの方は、  
`$ git clone`  
そうでない方はライトナビの「Download ZIP」ボタンからお願いします。

##注意
Windows環境での使用を想定しているので、  
/srcおよび/docディレクトリ以下のファイルについては改行コードがCRLF、  
/dbflute\_dfclientおよび/mydbfluteディレクトリについてはそのまま（おそらくLF）になっています。  
gitを使用する場合は、  
`autocrlf = false`  
または  
`autosafe = true`  
にしておく必要があります。


##実行環境
.NET Framework 2.0  
PostgreSQL9.1  
S2Container.NET 1.3.19  
DBFlute.NET 0.8.9.55  


##実行方法  
1. dfsampleデータベースを作成します。  
2. databaseInfoMap.dfpropとApp.configのDB接続情報を修正します。  
3. /dbflute\_dfclient/replace-schema.batを実行します。  
4. VisualStudioから実行します。  
