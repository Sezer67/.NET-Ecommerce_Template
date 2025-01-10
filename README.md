İlişkiler

Swagger dokümantasyonu tekli tekli var ancak tek aPIGateway üzerinden bağlı değil.
Kontroller yapılırken Mikroservislerin portlarına Ocelot.json dan bakılabilir. Yada her mikroservisin kendi Properties>launchjson dan bakılabilir.

Şuan aktif olarak beraber çalışan 3 mikroservis var.
İstek atılırken Mikroservislerin Controller larından hangi verileri alması gerektiğini,
itek atılacak endpoint in başı için Ocelot un dosyası okunmalı.

Run&Debug adımında aşağıda ki 3 servis ayağa kaldırılıyor. launch.json okunabilir.

User Mikroservis
User var. JWT ve Redis kurulu ancak uygulama genelinde kullanılmıyor. Redis in cihazda kurulu olması gerek.

Product Mikroservis
Ürün kategorisi ve etiketleri tutuluyor. 

Cart Mikroservis
Sepet mantığıyla çalışıyor. Ürün ve kullanıcı id leri tutuluyor. Get isteğinde ürün mikroservisine istek atılıyor.


*** Redis indirilmeli (User Service)
brew install redis

Her Serviste çalıştırılmalı
dotnet ef migrations add InitialCreate
dotnet ef database update