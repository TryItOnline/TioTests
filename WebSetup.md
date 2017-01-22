# How to make the test available on web

 - Install https://github.com/mthenw/frontail
 - Install https://www.microsoft.com/net/core
 - Clone https://github.com/AndrewSav/TioTests.git
 - In the TioTests folder edit config.json make sure that "UseConsoleCodes" is false
 - In the TioTests folder run:

```
dotnet restore
dotnet build
```

And then setup the schedule to run on:


```
cat <<EOT >> /etc/systemd/system/frontail.service
[Unit]
Description=Runs frontail to monitor TIO test logs
After=network.target auditd.service

[Service]
ExecStart=/bin/sh -c 'frontail -p 80 -n 300 -t dark --ui-highlight --ui-highlight-preset /root/frontail.json /var/log/helloworld.backend.tryitonline.net.log'
WorkingDirectory=/root
Restart=always

[Install]
WantedBy=multi-user.target
EOT

cat <<EOT >> /etc/systemd/system/tiotests.service
[Unit]
Description=Runs TIO test for each language

[Service]
Type=simple
ExecStart=/bin/sh -c 'dotnet /root/TioTests/bin/Debug/netcoreapp1.0/TioTests.dll >> /var/log/helloworld.backend.tryitonline.net.log'
WorkingDirectory=/root/TioTests
EOT

cat <<EOT >> /etc/systemd/system/tiotests.timer
[Unit]
Description=Run tiotests.service every 30 minutes

[Timer]
OnCalendar=*:0/30

[Install]
WantedBy=timers.target
EOT

systemctl start tiotests.timer
systemctl enable tiotests.timer
systemctl enable frontail.service
systemctl start frontail.service
```