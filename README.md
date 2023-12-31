# TradeSignals-Demo

## Описание проекта и сервисов

Для просмотра визуализации проекта пройдите по ссылке:
https://miro.com/app/board/uXjVNH8oNHE=/?share_link_id=495559540165

> [!IMPORTANT]
> **Внимание! данный продукт демонстрационный и не может быть использован в работе реального времени.**

Идея взята с моего предыдущего места работы. Готовый продукт: сервис сигналов на покупку или продажу рыночных активов. В качестве рыночных данных будет использован сервис имитации этих данных. В качестве алгоритма предсказания будут использованы заранее обученные нейронные сети. **Повторяю, они обучены лишь для демонстрации.** Промежуточный сервис будет получать новые рыночные данные, получать по ним предсказание от нейронных сетей, сверять с подписками из базы данных, и если данное положение в рынке является сигналом, отправляем готовый сигнал в базу данных всем пользователям в соответствии с подписками. API ворота будут проверять аутентификацию пользователей и админов, и давать взаимодействие с сервисом. Так же будет общий контроллер всех сервисов, будет следить за работой всех внутренних сервисов, запускать их и получать информацию о сбоях. Клиентская часть будет разработана двух видов, WPF приложение и WEB сайт. Так же будет готовая библиотека открытого API для использования сторонними приложениями.


## Информация о разработке

Весь проект строится на базе NET 8. Для всех сервисов кроме контроллера будет использован шаблон WinForms проекта, но без формы. Это даст нам работу процесса в фоне без регистраций и прочей канители с оригинальными сервисами windows так как это лишь демонстрация а не боевой проект. Контроллер будет консольным приложением, там же в консоле можно будет управлять сервисами. API Gateway будет использовать ASP.NET Core для веб запросов и TCP сокеты для библиотеки API что даст нам скорость обработки сигналов выше чем у WEB API. Это делается для того что бы клиент пользователя молниеносно обрабатывал входящие сигналы. WPF клиент будет использовать готовую API библиотеку и отображать полученную информацию. Web сайт, будет использовать REST запросы и отображать полученную информацию. Пока еще не решено каким будет сайт, одностраничным или многостраничным, как лучше на нем отобразить информацию и т.д.


> [!IMPORTANT]
> На данный момент проект на стадии разработки, готовые 
> сервисы можете посмотреть ниже



Прогресс разработки:
- [x] Контроллер для управления всеми сервисами(Controller)
- [x] Библиотека для внутренней коммуникации сервисов(Internal API)
- [x] Имитатор рыночных данных (Market Data)
- [x] Нейронные сети для предсказания рыночных данных(AI)[^1]
- [ ] База данных(Data base) - **В разработке**
- [ ] Проверка сигналов(Signal checker)
- [ ] Ворота для взаимодействия с сервером(API Gateway)
- [ ] Сборка всего сервера и тестирование работы всех узлов
- [ ] Библиотека для простого использования API(Open API)
- [ ] Web страница клиента(Web Client)
- [ ] WPF приложение клиента(WPF Client)


[^1]: Не используйте данную сеть для предсказания актуальных рыночных данных,
сети обучены только для демонстрации и не готовы работать в режиме реального времени.
