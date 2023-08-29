if [[ "$PROXY_IP" ]]; then
    sed -i "s/##PROXY_IP##/$PROXY_IP/g" wwwroot/js/site.js
fi

dotnet ./sampleWebService.dll