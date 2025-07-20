namespace Energy8.Identity.UI.Runtime.ViewModels
{
    /// <summary>
    /// ViewModel для ChangeNameView.
    /// Содержит данные для отображения текущего и нового имени.
    /// </summary>
    public class ChangeNameViewModel
    {
        /// <summary>
        /// Текущее имя пользователя (для отображения)
        /// </summary>
        public string CurrentName { get; }
        
        /// <summary>
        /// Предзаполненное новое имя (опционально)
        /// </summary>
        public string PrefilledNewName { get; }
        
        /// <summary>
        /// Минимальная длина имени (для валидации в Presenter)
        /// </summary>
        public int MinNameLength { get; }
        
        /// <summary>
        /// Инструкция для пользователя
        /// </summary>
        public string Instructions { get; }
        
        public ChangeNameViewModel(
            string currentName = null,
            string prefilledNewName = null,
            int minNameLength = 3,  // Как в Legacy - по умолчанию > 3
            string instructions = null)
        {
            CurrentName = currentName;
            PrefilledNewName = prefilledNewName;
            MinNameLength = minNameLength;
            Instructions = instructions ?? "Enter your new name";
        }
        
        /// <summary>
        /// Пустая ViewModel по умолчанию (как Legacy)
        /// </summary>
        public static ChangeNameViewModel Default => new ChangeNameViewModel();
        
        /// <summary>
        /// ViewModel с текущим именем (типичное использование)
        /// </summary>
        public static ChangeNameViewModel WithCurrentName(string currentName) => 
            new ChangeNameViewModel(currentName);
    }
}
