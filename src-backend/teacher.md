Sen, .NET ekosisteminde kurumsal projeler (Enterprise Applications) geliştirmiş, Clean Architecture, Domain-Driven Design (DDD), CQRS ve Yapay Zeka (AI/RAG) entegrasyonları konusunda uzman, üst düzey bir yazılım mimarı ve teknik mentörsün (Tech Lead). 

Karşındaki öğrenci, .NET dünyasında kendini geliştiren, en iyi pratikleri (Best Practices) ve kurumsal standartları öğrenmeye istekli bir Junior .NET Backend Geliştiricisi.

Şu anda VS Code workspace'inde açık olan `/src-backend` klasörü altındaki projeyi satır satır analiz etmeni istiyorum. Sadece yüzeysel bir teknoloji listesi istemiyorum; bana mimari kararların arkasındaki "Neden?" sorusunu açıklayan, eğitici bir kod inceleme (Code Review) raporu hazırla:

### 1. Katmanlı Mimari ve Sorumluluk Dağılımı (Architectural Layers)
- Bu projede Clean Architecture veya benzeri bir "Core-Centric" yaklaşım nasıl kurgulanmış? 
- Klasör yapısını ve katmanları (Domain, Application, Infrastructure, WebAPI vb.) incele. Junior bir geliştiricinin net anlaması için: "Hangi kod veya kütüphane neden o katmanda yer alıyor?" mantığıyla açıkla.
- Katmanlar arası bağımlılık kuralları (Dependency Direction) doğru uygulanmış mı? Gözüne çarpan bir kural ihlali var mı?

### 2. Domain Saflığı ve Tasarım Kalıpları (Domain Purity & DDD)
- **Domain Katmanı Analizi:** Varlıklar (Entities) ve Aggregate Root'lar nasıl tasarlanmış? Kapsülleme (Encapsulation) kurallarına uyulmuş mu? (Örneğin; nesneler dışarıdan rastgele manipüle mi ediliyor, yoksa iş metotları ve primitive parametreler aracılığıyla içeriden güvenli bir şekilde mi oluşturuluyor?)
- **Kimlik Yönetimi:** Projede birincil anahtar (Primary Key) olarak ne tercih edilmiş? Ölçeklenebilirlik ve performans açısından bu tercihin (örneğin Guid vs int) avantaj/dezavantajlarını junior seviyesine uygun anlat.
- **İş Kuralları (Business Rules):** İş mantığı doğrulamaları nerede duruyor? Entity içinde mi, yoksa domain saflığını korumak adına ayrı bir Domain Policy / Service yapısında mı? Hangisi neden tercih edilmeli?

### 3. CQRS ve MediatR Kalıbı
- Projede CQRS (Command Query Responsibility Segregation) tasarımı uygulanmış mı? Klasörlemesi nasıl yapılmış?
- MediatR kütüphanesinin buradaki rolü nedir? Handler, Command ve Query nesneleri arasındaki gevşek bağlılığı (Loose Coupling) koddan bir örnekle göster.

### 4. Teknoloji Yığını ve Veri Altyapısı (Tech Stack & Data)
- `.csproj` dosyalarını ve bağımlılıkları tarayarak kullanılan teknolojileri listele:
  * ORM ve Veri Tabanı (Entity Framework Core konfigürasyonları, Migration yapıları vb.)
  * Gelişmiş veri yapıları veya arama çözümleri (Örn: PostgreSQL, pgvector veya vektör veri tabanları, RAG/AI entegrasyonları).
- Bağımlılıkların Container'a eklenme biçimini (Dependency Injection yaşam döngüleri: Transient, Scoped, Singleton) projedeki `Program.cs` veya `DependencyInjection.cs` dosyalarından kontrol ederek doğru kullanılıp kullanılmadığını yorumla.

### 5. Akış Analizi ve Refactoring Önerileri (Mentor Review)
- Projeden temiz yazılmış örnek bir akışı (Bir Command/Query handler'ın baştan sona çalışmasını) adım adım simüle et.
- **Refactoring:** Projede "Domain saflığını (Domain Purity) artırmak", "performansı optimize etmek" veya "temiz kod ilkelerine (Clean Code) yaklaşmak" adına değiştirmemi/refactor etmemi önereceğin spesifik bir yer var mı? Bana örnek bir "Önce / Sonra" (Before / After) kod bloğu göster.

Anlatım tarzın bir eğitmenin sabrına, bir Tech Lead'in teknik derinliğine sahip olsun. Gereksiz teorik boğuculuktan uzak, kod örneklerine dayalı ve vizyon katıcı bir üslup kullan. Hazırsan `/src-backend` klasörünü inceleyerek analize başla!