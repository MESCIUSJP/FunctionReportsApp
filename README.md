# FunctionReportsApp

## 概要
FunctionReportsAppは、Azure Functionsを使用してHTTPリクエストを処理し、ActiveReports for .NETを用いてPDFを生成するアプリケーションです。このアプリケーションは、指定されたデータを基にレポートを作成し、PDF形式で出力します。
ソースコードはブログ記事「[Azure FunctionsとActiveReports for .NETでつくる帳票生成API](https://devlog.mescius.jp/activereports-azure-functions/)」内で解説している内容となります。

## 使用技術
- **Azure Functions**: サーバーレスコンピューティングサービスを利用して、HTTPリクエストを処理します。
- **GrapeCity ActiveReports**: レポートの生成とPDF出力に使用します。
- **.NET**: アプリケーションの開発に使用されています。

## セットアップ手順
1. 必要な.NET SDKをインストールします。
2. プロジェクトをクローンまたはダウンロードします。
3. 必要なNuGetパッケージを復元します。

## 使用方法
1. Azure Functionsをデプロイします。
2. HTTPリクエストを送信し、`datajson`パラメータを指定します。
3. レスポンスとして生成されたPDFを受け取ります。

## フォント
このアプリケーションでは、`ipag.ttf`フォントが埋め込まれて使用されています。
