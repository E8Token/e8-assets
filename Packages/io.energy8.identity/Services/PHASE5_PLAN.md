# Phase 5: Service Layer Refactor

## 🎯 Задача фазы  
Рефакторинг монолитных сервисов IIdentityService и IUserService с применением тех же принципов, что и в контроллерах.

## 🔍 Проблемы текущих сервисов
- **IIdentityService**: Монолитный сервис с 15+ методами
- **IUserService**: Смешивает HTTP, кэширование и бизнес-логику
- **Tight Coupling**: Сервисы жестко связаны друг с другом
- **Отсутствие тестируемости**: Сложно мокать и тестировать

## 📋 План рефакторинга

### Week 1: Service Analysis & Design
- [ ] Анализ IIdentityService (аналог CONTROLLER_ANALYSIS.md)
- [ ] Анализ IUserService и зависимых сервисов
- [ ] Дизайн новой сервисной архитектуры
- [ ] Создание интерфейсов для специализированных сервисов

### Week 2: Core Services Extraction
- [ ] AuthenticationService (только авторизация)
- [ ] UserProfileService (только профиль пользователя)
- [ ] EmailVerificationService (только email верификация)
- [ ] ProviderLinkingService (только связывание провайдеров)

### Week 3: HTTP & Data Layer
- [ ] HttpClientService (HTTP абстракция)
- [ ] CacheService (кэширование данных)
- [ ] ConfigurationService (конфигурация)
- [ ] Integration testing

## 🎯 Ожидаемый результат
```csharp
// ДО: Монолитные сервисы
IIdentityService (15+ методов)
IUserService (10+ методов)

// ПОСЛЕ: Специализированные сервисы  
IAuthenticationService (авторизация)
IUserProfileService (профиль) 
IEmailVerificationService (email)
IProviderLinkingService (провайдеры)
```
