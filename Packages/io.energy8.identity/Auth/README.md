# Auth Module

Модуль аутентификации для системы Identity Energy8.

## Структура

### Core
- **Models/** - Модели данных для аутентификации
- **Providers/** - Провайдеры аутентификации (Native, WebGL)
  - **Native/** - Нативная аутентификация
  - **WebGL/** - WebGL аутентификация через Firebase

### Runtime
- Компоненты времени выполнения для аутентификации

### Editor
- Инструменты редактора для настройки аутентификации

## Использование

Модуль предоставляет абстракцию для работы с различными методами аутентификации в зависимости от платформы.

```csharp
IAuthProvider authProvider = new WebGLAuthProvider();
var result = await authProvider.SignInWithEmailAsync(email, password);
```
