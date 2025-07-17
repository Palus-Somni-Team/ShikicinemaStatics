# ShikicinemaStatics

## Процесс сборки

Делаем рестор пакетов
```sh
dotnet restore
```

Собираем проект c релизной конфигурацией (`-c Release`, еще можно `-с Debug`)
```sh
dotnet build -c Release --no-restore
```

Запускаем тесты (но сейчас их нет). Без пересборки проекта поэтому надо повторить флаг `-c`.
```sh
dotnet test -c Release --no-build
```

Т.к. это веб проект лучше вызвать публикацию, опять же без билда.
```sh
dotnet publish -c Release --no-build
```

Далее перед запуском проекта необходимо обратить внимание на файл appsettings.json.
В файле лежат все настройки.

Если файл с настройками не менялся шаги для запуска такие:
```sh
cd ShikicinemaStatics/bin/Release/net8.0/publish/
mkdir statics
dotnet ShikicinemaStatics.dll
```
