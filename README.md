Docker Commands: 

```
docker pull mcr.microsoft.com/mssql/server

docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong()Password" \
   -e "MSSQL_PID=Developer" \
   -p 1433:1433 --name invoiceserver --hostname invoiceserver \
   -d \
   mcr.microsoft.com/mssql/server:2022-latest
```

![image](https://github.com/user-attachments/assets/76c7f49a-b42a-4979-8b76-18aaf2d358a6)
