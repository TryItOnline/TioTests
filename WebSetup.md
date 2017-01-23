# How to make the test available on web

 - Install https://github.com/mthenw/frontail
 - Install https://www.microsoft.com/net/core
 - Clone https://github.com/AndrewSav/TioTests.git
 - In the TioTests folder run:

```
dotnet restore
dotnet build
```

And then setup the schedule to run on:


```
rm -f /etc/systemd/system/frontail.service
cat <<EOT >> /etc/systemd/system/frontail.service
[Unit]
Description=Frontails job that serves TIO test logs on the web
After=network.target auditd.service

[Service]
ExecStart=/bin/sh -c 'frontail -p 80 -n 300 -t dark --ui-highlight --ui-highlight-preset /root/frontail.json /var/log/helloworld.backend.tryitonline.net.log'
WorkingDirectory=/root
Restart=always

[Install]
WantedBy=multi-user.target
EOT

rm -f /etc/systemd/system/tiotests.service
cat <<EOT >> /etc/systemd/system/tiotests.service
[Unit]
Description=Try-it-online test suite for every language

[Service]
Type=simple
ExecStart=/bin/sh -c 'dotnet /root/TioTests/bin/Debug/netcoreapp1.0/TioTests.dll -d off >> /var/log/helloworld.backend.tryitonline.net.log'
WorkingDirectory=/root/TioTests
EOT

rm -f /etc/systemd/system/tiotests.timer
cat <<EOT >> /etc/systemd/system/tiotests.timer
[Unit]
Description=Timer for Try-it-online test suite for every language

[Timer]
OnCalendar=07:00

[Install]
WantedBy=timers.target
EOT

systemctl start tiotests.timer
systemctl enable tiotests.timer
systemctl enable frontail.service
systemctl start frontail.service
systemctl daemon-reload
```

## How to update the server when sources in git changed

In TioSetup directory:
```
git pull
dotnet build
```

## How to kick off tests manually (outside of configured schedule)

```
systemctl start tiotests.service
````
