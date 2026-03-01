FROM httpd:alpine

# 必要なモジュール（proxy, proxy_http, headers）のコメントアウトを外して有効化
RUN sed -i '/LoadModule proxy_module/s/^#//g' /usr/local/apache2/conf/httpd.conf && \
    sed -i '/LoadModule proxy_http_module/s/^#//g' /usr/local/apache2/conf/httpd.conf && \
    sed -i '/LoadModule headers_module/s/^#//g' /usr/local/apache2/conf/httpd.conf

# 設定ファイルをコンテナ内にコピー
COPY senlink.conf /usr/local/apache2/conf/extra/senlink.conf

# メイン設定ファイルから senlink.conf を読み込むように追記
RUN echo "Include conf/extra/senlink.conf" >> /usr/local/apache2/conf/httpd.conf